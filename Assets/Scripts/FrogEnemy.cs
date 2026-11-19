using UnityEngine;

/// <summary>
/// Enemy type 2: a frog that patrols by hopping.
/// Inherits patrol, ledge/wall checks, chase and damage from EnemyBehaviour;
/// only HOW it moves is different (override Move / ShouldTurnAround).
/// Animator needs a Bool parameter "isGrounded" (Idle <-> Jump).
/// </summary>
public class FrogEnemy : EnemyBehaviour
{
    [Header("Frog Hop")]
    public float hopInterval = 1.2f;        // seconds between hops
    public Vector2 hopVelocity = new Vector2(2.5f, 6.0f);
    public float hopLandingCheck = 1.5f;    // how far ahead the hop lands (ledge check)

    private float hopTimer;
    private Animator animator;
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");

    protected override void Start()
    {
        base.Start();                       // keep everything the base class sets up
        animator = GetComponent<Animator>();
        hopTimer = hopInterval;
    }

    protected override void Move()
    {
        bool grounded = IsGrounded();
        hopTimer -= Time.fixedDeltaTime;

        // On the ground between hops: stand still (no sliding)
        if (grounded && hopTimer < hopInterval - 0.2f)
        {
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }

        if (grounded && hopTimer <= 0.0f)
        {
            // Don't hop off the platform: if the landing spot has no ground, turn first
            if (!IsGroundAt(hopLandingCheck))
            {
                TurnAround();
            }

            float speedScale = CanSeePlayer ? chaseSpeedMultiplier : 1.0f;
            rb.velocity = new Vector2(direction * hopVelocity.x * speedScale, hopVelocity.y);
            hopTimer = CanSeePlayer ? hopInterval * 0.5f : hopInterval;   // hops faster when chasing
        }

        if (animator != null)
        {
            animator.SetBool(IsGroundedHash, grounded);
        }
    }

    // Only check walls, and only on the ground (in the air the ledge ray always misses)
    protected override bool ShouldTurnAround()
    {
        return IsGrounded() && IsWallAhead();
    }

    protected override bool ShouldTurnToPlayer()
    {
        return IsGrounded() && base.ShouldTurnToPlayer();
    }
}
