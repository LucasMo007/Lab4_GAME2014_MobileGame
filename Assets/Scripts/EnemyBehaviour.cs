using UnityEngine;

/// <summary>
/// Base enemy: walks back and forth on its platform and turns around
/// at a ledge or a wall. Hurts anything IDamageable it touches.
///
/// Flexible base for Lab 6: new enemies inherit from this class and
/// override Move(), ShouldTurnAround() or OnHitTarget().
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBehaviour : MonoBehaviour
{
    [Header("Patrol")]
    public float moveSpeed = 2.0f;
    public bool startMovingRight = false;
    public bool spriteFacesLeft = true;     // SunnyLand opossum art faces left

    [Header("Edge / Wall Detection")]
    public LayerMask groundLayerMask;       // Ground layer (tiles + box)
    public float ledgeRayLength = 0.5f;     // down from the front-bottom corner of the collider
    public float wallRayLength = 0.15f;     // forward from the front edge of the collider
    public float turnCooldown = 0.25f;      // stops flipping back and forth every frame

    [Header("Damage")]
    public int contactDamage = 1;

    protected Rigidbody2D rb;
    protected Collider2D bodyCollider;
    protected int direction;                // +1 = right, -1 = left
    private float turnTimer;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        direction = startMovingRight ? 1 : -1;
        UpdateFacing();
    }

    protected virtual void FixedUpdate()
    {
        if (turnTimer > 0.0f)
        {
            turnTimer -= Time.fixedDeltaTime;
        }
        else if (ShouldTurnAround())
        {
            TurnAround();
        }
        Move();
    }

    // ---------------- Patrol ----------------

    /// <summary>Default movement: walk at a constant speed. Override for other enemies.</summary>
    protected virtual void Move()
    {
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    /// <summary>True when there is no ground ahead (ledge) or a wall in front.</summary>
    protected virtual bool ShouldTurnAround()
    {
        return !IsGroundAhead() || IsWallAhead();
    }

    // The rays start from the collider's edges, so they work for any sprite size.
    private Vector2 LedgeRayOrigin()
    {
        Bounds b = bodyCollider.bounds;
        return new Vector2(b.center.x + direction * b.extents.x, b.min.y + 0.1f);
    }

    private Vector2 WallRayOrigin()
    {
        Bounds b = bodyCollider.bounds;
        return new Vector2(b.center.x + direction * b.extents.x, b.center.y);
    }

    protected bool IsGroundAhead()
    {
        return Physics2D.Raycast(LedgeRayOrigin(), Vector2.down, ledgeRayLength, groundLayerMask);
    }

    protected bool IsWallAhead()
    {
        return Physics2D.Raycast(WallRayOrigin(), Vector2.right * direction, wallRayLength, groundLayerMask);
    }

    protected void TurnAround()
    {
        direction = -direction;
        turnTimer = turnCooldown;
        UpdateFacing();
    }

    private void UpdateFacing()
    {
        // Flip the sprite so it looks the way it walks
        float facing = spriteFacesLeft ? -direction : direction;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facing;
        transform.localScale = scale;
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

    // Green = the ray hits Ground, red = it hits nothing.
    protected virtual void OnDrawGizmos()
    {
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        if (bodyCollider == null) return;
        if (!Application.isPlaying) direction = startMovingRight ? 1 : -1;

        Vector2 ledge = LedgeRayOrigin();
        Gizmos.color = IsGroundAhead() ? Color.green : Color.red;
        Gizmos.DrawLine(ledge, ledge + Vector2.down * ledgeRayLength);

        Vector2 wall = WallRayOrigin();
        Gizmos.color = IsWallAhead() ? Color.red : Color.green;
        Gizmos.DrawLine(wall, wall + Vector2.right * direction * wallRayLength);
    }
}
