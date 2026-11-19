using UnityEngine;

/// <summary>
/// Gives an enemy "eyes".
///  1. Sensing: is the player inside the detection radius? (OverlapCircle on the Player layer)
///  2. Line of sight (LOS): is there a wall/ground between us? (Linecast on the Ground layer)
/// The enemy can only "see" the player when both are true.
/// </summary>
public class PlayerDetection : MonoBehaviour
{
    [Header("Sensing")]
    public float detectionRadius = 5.0f;
    public LayerMask playerLayerMask;       // Player layer
    public bool onlySeeInFront = false;     // true = ignore the player behind the enemy

    [Header("Line of Sight")]
    public LayerMask obstacleLayerMask;     // Ground layer (walls, tiles, box)

    [Header("Debug (read only)")]
    public bool playerInRange;
    public bool hasLineOfSight;

    private Transform player;

    /// <summary>True when the player is in range AND nothing blocks the view.</summary>
    public bool CanSeePlayer => playerInRange && hasLineOfSight;
    public Vector2 PlayerPosition => player != null ? (Vector2)player.position : (Vector2)transform.position;

    void Update()
    {
        // 1. Sensing: find the player inside the circle
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayerMask);
        playerInRange = hit != null;
        player = playerInRange ? hit.transform : null;

        if (playerInRange && onlySeeInFront && !IsInFront(player.position))
        {
            playerInRange = false;
        }

        // 2. LOS: a line from us to the player; if it hits Ground, the view is blocked
        hasLineOfSight = playerInRange &&
                         !Physics2D.Linecast(transform.position, player.position, obstacleLayerMask);
    }

    private bool IsInFront(Vector2 target)
    {
        // Our sprites face the way they walk using localScale.x, so use the walk direction
        EnemyBehaviour enemy = GetComponent<EnemyBehaviour>();
        float facing = enemy != null ? enemy.Direction : Mathf.Sign(transform.localScale.x);
        return Mathf.Sign(target.x - transform.position.x) == Mathf.Sign(facing);
    }

    void OnDrawGizmos()
    {
        // Yellow circle = sensing range (red when the player is inside it)
        Gizmos.color = playerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Line to the player: green = can see, red = view blocked
        if (playerInRange && player != null)
        {
            Gizmos.color = hasLineOfSight ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
