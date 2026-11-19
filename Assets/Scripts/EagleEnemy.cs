using UnityEngine;

/// <summary>
/// Enemy type 3: a flying eagle.
/// No gravity. Patrols left/right between startX - patrolRange and startX + patrolRange
/// at its start height. When PlayerDetection sees the player INSIDE that area,
/// it dives at the player. After a hit it flies back up before it can dive again.
/// </summary>
public class EagleEnemy : EnemyBehaviour
{
    [Header("Eagle Flight")]
    public float patrolRange = 4.0f;        // how far left/right of the start position
    public float returnSpeed = 2.0f;        // how fast it climbs back to patrol height
    public float retreatTime = 1.0f;        // seconds flying back up after hitting the player

    [Header("Debug (read only)")]
    public string currentState;             // Patrol / Dive / Retreat / "Sees player but out of range"

    private Vector2 startPosition;
    private float retreatTimer;

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0.0f;             // flying: no gravity
        startPosition = transform.position;
    }

    /// <summary>Only chase when the player is seen AND is above the eagle's patrol area.</summary>
    private bool ShouldDive
    {
        get
        {
            if (!CanSeePlayer || retreatTimer > 0.0f) return false;
            float playerOffset = detection.PlayerPosition.x - startPosition.x;
            return Mathf.Abs(playerOffset) <= patrolRange;
        }
    }

    protected override void Move()
    {
        if (retreatTimer > 0.0f)
        {
            retreatTimer -= Time.fixedDeltaTime;
        }

        if (retreatTimer > 0.0f) currentState = "Retreat";
        else if (ShouldDive) currentState = "Dive";
        else if (CanSeePlayer) currentState = "Sees player but out of range";
        else currentState = "Patrol";

        if (ShouldDive)
        {
            // Dive straight at the player
            Vector2 toPlayer = detection.PlayerPosition - rb.position;
            rb.velocity = toPlayer.normalized * CurrentSpeed;
        }
        else
        {
            // Patrol (or retreat): fly sideways and climb back to the start height
            float yVelocity = (startPosition.y - rb.position.y) * returnSpeed;
            rb.velocity = new Vector2(direction * moveSpeed, yVelocity);
        }
    }

    // Turn at the edges of the patrol range or at a wall (no ledges in the air)
    protected override bool ShouldTurnAround()
    {
        if (ShouldDive) return false;       // diving: Move() steers toward the player

        float offset = rb.position.x - startPosition.x;
        bool pastRight = offset > patrolRange && direction > 0;
        bool pastLeft = offset < -patrolRange && direction < 0;
        return pastRight || pastLeft || IsWallAhead();
    }

    protected override bool ShouldTurnToPlayer()
    {
        return ShouldDive && base.ShouldTurnToPlayer();
    }

    // After hurting the player, fly back up instead of sitting on top of them
    protected override void OnHitTarget(IDamageable target)
    {
        base.OnHitTarget(target);
        retreatTimer = retreatTime;
    }

    // The eagle flies, so it has no ledge check: only draw the wall ray and the patrol range
    protected override void OnDrawGizmos()
    {
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        if (bodyCollider != null)
        {
            if (!Application.isPlaying) direction = startMovingRight ? 1 : -1;
            Bounds b = bodyCollider.bounds;
            Vector2 wall = new Vector2(b.center.x + direction * b.extents.x, b.center.y);
            Gizmos.color = IsWallAhead() ? Color.red : Color.green;
            Gizmos.DrawLine(wall, wall + Vector2.right * direction * wallRayLength);
        }

        // Cyan line = patrol range
        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center + Vector2.left * patrolRange, center + Vector2.right * patrolRange);
    }
}
