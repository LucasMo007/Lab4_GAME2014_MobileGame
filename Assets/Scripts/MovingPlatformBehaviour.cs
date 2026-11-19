using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moving platform with 4 movement types:
///   Horizontal / Vertical / Diagonal  -> moves 'distance' from its start and back (ping-pong)
///   Custom                            -> follows the 'customPoints' path in a loop
/// Anything standing on top (player, box) is carried along.
///
/// Setup: Rigidbody2D (Kinematic) + BoxCollider2D, layer = Ground.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatformBehaviour : MonoBehaviour
{
    public enum MovementType { Horizontal, Vertical, Diagonal, Custom }

    [Header("Movement")]
    public MovementType movementType = MovementType.Horizontal;
    public float distance = 3.0f;           // for Horizontal / Vertical / Diagonal (negative = other way)
    public float speed = 2.0f;
    public float waitTime = 0.5f;           // pause at each end point

    [Header("Custom Path")]
    public Transform[] customPoints;        // empty children placed along the path

    private Rigidbody2D rb;
    private Collider2D platformCollider;
    private readonly List<Vector2> points = new List<Vector2>();
    private int targetIndex = 1;
    private float waitTimer;

    // Rigidbodies standing on the platform right now
    private readonly HashSet<Rigidbody2D> riders = new HashSet<Rigidbody2D>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        platformCollider = GetComponent<Collider2D>();
        BuildPath();
    }

    /// <summary>Turn the movement type into a list of world positions to visit.</summary>
    private void BuildPath()
    {
        Vector2 start = transform.position;
        points.Clear();
        points.Add(start);

        switch (movementType)
        {
            case MovementType.Horizontal:
                points.Add(start + Vector2.right * distance);
                break;
            case MovementType.Vertical:
                points.Add(start + Vector2.up * distance);
                break;
            case MovementType.Diagonal:
                points.Add(start + new Vector2(1.0f, 1.0f).normalized * distance);
                break;
            case MovementType.Custom:
                // Save the world positions now; the children move with the platform later
                foreach (Transform point in customPoints)
                {
                    if (point != null) points.Add(point.position);
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (points.Count < 2) return;

        if (waitTimer > 0.0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        // Step toward the current target
        Vector2 target = points[targetIndex];
        Vector2 newPosition = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        Vector2 delta = newPosition - rb.position;
        rb.MovePosition(newPosition);

        // Carry everything standing on top by the same amount
        foreach (Rigidbody2D rider in riders)
        {
            if (rider != null) rider.position += delta;
        }

        // Reached the target: wait, then go to the next point (loops back to 0)
        if (Vector2.Distance(newPosition, target) < 0.01f)
        {
            targetIndex = (targetIndex + 1) % points.Count;
            waitTimer = waitTime;
        }
    }

    // ---------------- Riders ----------------

    void OnCollisionStay2D(Collision2D collision)
    {
        Rigidbody2D other = collision.rigidbody;
        if (other == null || other.bodyType != RigidbodyType2D.Dynamic) return;

        // Only carry things whose feet are on top of the platform (not hitting the side/bottom)
        bool onTop = collision.collider.bounds.min.y >= platformCollider.bounds.max.y - 0.1f;
        if (onTop) riders.Add(other);
        else riders.Remove(other);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.rigidbody != null) riders.Remove(collision.rigidbody);
    }

    // ---------------- Debug ----------------

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        if (Application.isPlaying)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
            }
            return;
        }

        // Preview the path in the editor before pressing Play
        Vector2 start = transform.position;
        switch (movementType)
        {
            case MovementType.Horizontal: Gizmos.DrawLine(start, start + Vector2.right * distance); break;
            case MovementType.Vertical:   Gizmos.DrawLine(start, start + Vector2.up * distance); break;
            case MovementType.Diagonal:   Gizmos.DrawLine(start, start + new Vector2(1.0f, 1.0f).normalized * distance); break;
            case MovementType.Custom:
                Vector2 previous = start;
                foreach (Transform point in customPoints)
                {
                    if (point == null) continue;
                    Gizmos.DrawLine(previous, point.position);
                    previous = point.position;
                }
                Gizmos.DrawLine(previous, start);
                break;
        }
    }
}
