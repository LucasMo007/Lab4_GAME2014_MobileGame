using UnityEngine;

/// <summary>
/// Base enemy: walks back and forth on its platform and turns around
/// at a ledge or a wall. Hurts anything IDamageable it touches.
/// If it has a PlayerDetection component and sees the player,
/// it turns toward the player and moves faster (chase).
///
/// Child classes (FrogEnemy, EagleEnemy) override Move(),
/// ShouldTurnAround() or OnHitTarget() to behave differently.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBehaviour : MonoBehaviour
{
    [Header("Patrol")]
    public float moveSpeed = 2.0f;
    public bool startMovingRight = false;
    public bool spriteFacesLeft = true;     // SunnyLand enemy art faces left

    [Header("Edge / Wall Detection")]
    public LayerMask groundLayerMask;       // Ground layer (tiles + box + platforms)
    public float ledgeRayLength = 0.5f;     // down from the front-bottom corner of the collider
    public float wallRayLength = 0.15f;     // forward from the front edge of the collider
    public float turnCooldown = 0.25f;      // stops flipping back and forth every frame

    [Header("Chase (needs PlayerDetection)")]
    public float chaseSpeedMultiplier = 1.5f;

    [Header("Damage")]
    public int contactDamage = 1;

    protected Rigidbody2D rb;
    protected Collider2D bodyCollider;
    protected PlayerDetection detection;    // optional
    protected int direction;                // +1 = right, -1 = left
    private float turnTimer;

    public int Direction => direction;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        detection = GetComponent<PlayerDetection>();
        direction = startMovingRight ? 1 : -1;
        UpdateFacing();
    }

    protected virtual void FixedUpdate()
    {
        if (turnTimer > 0.0f)
        {
            turnTimer -= Time.fixedDeltaTime;
        }
        else if (ShouldTurnAround() || ShouldTurnToPlayer())
        {
            TurnAround();
        }
        Move();
    }

    // ---------------- Patrol ----------------

    /// <summary>Default movement: walk at a constant speed. Override for other enemies.</summary>
    protected virtual void Move()
    {
        rb.velocity = new Vector2(direction * CurrentSpeed, rb.velocity.y);
    }

    /// <summary>True when there is no ground ahead (ledge) or a wall in front.</summary>
    protected virtual bool ShouldTurnAround()
    {
        return !IsGroundAhead() || IsWallAhead();
    }

    protected void TurnAround()
    {
        direction = -direction;
        turnTimer = turnCooldown;
        UpdateFacing();
    }

    private void UpdateFacing()
    {
        // Flip the sprite so it looks the way it moves
        float facing = spriteFacesLeft ? -direction : direction;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facing;
        transform.localScale = scale;
    }

    // ---------------- Player detection / chase ----------------

    protected bool CanSeePlayer => detection != null && detection.CanSeePlayer;

    protected float CurrentSpeed => CanSeePlayer ? moveSpeed * chaseSpeedMultiplier : moveSpeed;

    /// <summary>Player seen behind us: turn to face them (but never walk off a ledge to do it).</summary>
    protected virtual bool ShouldTurnToPlayer()
    {
        if (!CanSeePlayer) return false;
        float dx = detection.PlayerPosition.x - transform.position.x;
        return Mathf.Abs(dx) > 0.5f && Mathf.Sign(dx) != direction;
    }

    // ---------------- Raycast helpers ----------------
    // The rays start from the collider's edges, so they work for any sprite size.

    /// <summary>Is there ground below a point 'extraDistance' in front of the collider?</summary>
    protected bool IsGroundAt(float extraDistance)
    {
        Bounds b = bodyCollider.bounds;
        Vector2 origin = new Vector2(b.center.x + direction * (b.extents.x + extraDistance), b.min.y + 0.1f);
        return Physics2D.Raycast(origin, Vector2.down, ledgeRayLength, groundLayerMask);
    }

    protected bool IsGroundAhead()
    {
        return IsGroundAt(0.0f);
    }

    protected bool IsWallAhead()
    {
        Bounds b = bodyCollider.bounds;
        Vector2 origin = new Vector2(b.center.x + direction * b.extents.x, b.center.y);
        return Physics2D.Raycast(origin, Vector2.right * direction, wallRayLength, groundLayerMask);
    }

    /// <summary>Standing on the ground right now? (used by the jumping frog)</summary>
    protected bool IsGrounded()
    {
        Bounds b = bodyCollider.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.05f);
        return Physics2D.Raycast(origin, Vector2.down, 0.15f, groundLayerMask);
    }

    // ---------------- Damage ----------------

    // Stay (not Enter) so standing against the enemy keeps hurting;
    // the player's invulnerability time stops it from hitting every frame.
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            OnHitTarget(target);
        }
    }

    protected virtual void OnHitTarget(IDamageable target)
    {
        target.TakeDamage(contactDamage, transform.position);
    }

    // ---------------- Debug ----------------

    // Green = the ray hits Ground, red = it hits nothing (ledge) or hits a wall.
    protected virtual void OnDrawGizmos()
    {
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        if (bodyCollider == null) return;
        if (!Application.isPlaying) direction = startMovingRight ? 1 : -1;

        Bounds b = bodyCollider.bounds;
        Vector2 ledge = new Vector2(b.center.x + direction * b.extents.x, b.min.y + 0.1f);
        Gizmos.color = IsGroundAhead() ? Color.green : Color.red;
        Gizmos.DrawLine(ledge, ledge + Vector2.down * ledgeRayLength);

        Vector2 wall = new Vector2(b.center.x + direction * b.extents.x, b.center.y);
        Gizmos.color = IsWallAhead() ? Color.red : Color.green;
        Gizmos.DrawLine(wall, wall + Vector2.right * direction * wallRayLength);
    }
}
