using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 挂在 UI 跳跃按钮（Image）上。手指按下就跳。
/// 用 IPointerDownHandler 而不是 Button.onClick，因为 onClick 要松手才触发，手感慢。
/// </summary>
public class JumpButtonBehaviour : MonoBehaviour, IPointerDownHandler
{
    public PlayerBehaviour player;

    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerBehaviour>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player != null)
        {
            player.RequestJump();
        }
    }
}
