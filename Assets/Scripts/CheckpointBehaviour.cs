using UnityEngine;

/// <summary>
/// 检查点：玩家碰到后，复活位置更新为这里。
/// 需要 Collider2D 并勾选 Is Trigger。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CheckpointBehaviour : MonoBehaviour
{
    public Transform respawnPoint;   // 可选：自定义复活点，不填就用检查点自己的位置
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

        // 简单的视觉反馈：变绿
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
        }

        Debug.Log($"Checkpoint reached: {name}");
    }
}
