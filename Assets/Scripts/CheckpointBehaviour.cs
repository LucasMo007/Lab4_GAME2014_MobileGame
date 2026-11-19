using UnityEngine;

/// <summary>
/// Checkpoint: when the player touches it, their respawn position moves here.
/// Needs a Collider2D with Is Trigger ticked.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CheckpointBehaviour : MonoBehaviour
{
    public Transform respawnPoint;   // optional custom respawn point; empty = use this checkpoint's position
    public bool isActivated;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated || !other.CompareTag("Player")) return;

        PlayerBehaviour player = other.GetComponent<PlayerBehaviour>();
        if (player == null) return;

        player.SetRespawnPoint(respawnPoint != null ? respawnPoint.position : transform.position);
        isActivated = true;

        // Simple visual feedback: turn green
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundFX.Checkpoint);
        }

        Debug.Log($"Checkpoint reached: {name}");
    }
}
