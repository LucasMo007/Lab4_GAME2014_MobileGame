using UnityEngine;

/// <summary>
/// 死亡区：放在关卡下方的一个很宽的 Trigger。
/// 玩家掉进来就回到最近的检查点；箱子等其他刚体掉进来就回到它们的起始位置。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DeadPlaneBehaviour : MonoBehaviour
{
    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBehaviour player = other.GetComponent<PlayerBehaviour>();
            if (player != null)
            {
                player.Respawn();
            }
            return;
        }

        IResettable resettable = other.GetComponent<IResettable>();
        if (resettable != null)
        {
            resettable.ResetObject();
        }
    }
}
