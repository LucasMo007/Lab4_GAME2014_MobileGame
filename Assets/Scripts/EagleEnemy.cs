using UnityEngine;

/// <summary>
/// Enemy type 3: a flying eagle.
/// No gravity. Patrols left/right between startX - patrolRange and startX + patrolRange
/// at its start height. When PlayerDetection sees the player, it dives toward
/// the player, but never leaves its patrol range.
/// </summary>
public class EagleEnemy : EnemyBehaviour
{
    [Header("Eagle Flight")]
    public float patrolRange = 4.0f;        // how far left/right of the start position
    public float returnSpeed = 2.0f;        // how fast it climbs back to patrol height

    private Vector2 startPosition;

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0.0f;             // flying: no gravity
        startPosition = transform.position;
    }

    protected override void Move()
    {
        if (CanSeePlayer)
        {
            // Dive at the player, clamped to the patrol area
            Vector2 target = detection.PlayerPosition;
            target.x = Mathf.Clamp(target.x, startPosition.x - patrolRange, startPosition.x + patrolRange);
            Vector2 toTarget = target - rb.position;
            rb.velocity = toTarget.normalized * CurrentSpeed;
        }
        else
        {
            // Patrol: fly sideways and drift back to the start height
            float yVelocity = (startPosition.y - rb.position.y) * returnSpeed;
            rb.velocity = new Vector2(direction * moveSpeed, yVelocity);
        }
    }

    // Turn at the edges of the patrol range or at a wall (no ledges in the air)
    protected override bool ShouldTurnAround()
    {
        if (CanSeePlayer) return false;     // chasing: Move() already keeps it inside the range

        float offset = rb.position.x - startPosition.x;
        bool pastRight = offset > patrolRange && direction > 0;
        bool pastLeft = offset < -patrolRange && direction < 0;
        return pastRight || pastLeft || IsWallAhead();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Cyan line = patrol range
        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center + Vector2.left * patrolRange, center + Vector2.right * patrolRange);
    }
}
