using UnityEngine;

/// <summary>
/// 可推动的箱子。推动本身靠 Rigidbody2D 物理，不需要代码；
/// 这个脚本只负责掉进 DeadPlane 后回到原位。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PushableBoxBehaviour : MonoBehaviour, IResettable
{
    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void ResetObject()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0.0f;
        transform.SetPositionAndRotation(startPosition, startRotation);
    }
}
