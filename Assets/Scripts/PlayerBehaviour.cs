using UnityEngine;

/// <summary>
/// 2D 平台玩家控制：左右移动、跳跃、地面检测、翻转、动画、存档点复活。
/// 键盘（PC 测试）和 Joystick Pack 虚拟摇杆 + 跳跃按钮（手机）都能用。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBehaviour : MonoBehaviour
{
    [Header("Movement")]
    public float horizontalForce = 20.0f;   // 左右移动的力
    public float maxSpeed = 5.0f;           // 最大水平速度
    public float jumpForce = 12.0f;         // 跳跃冲量
    [Range(0.0f, 1.0f)]
    public float airControlFactor = 0.6f;   // 空中移动的力度比例
    [Range(0.0f, 1.0f)]
    public float groundFriction = 0.85f;    // 松开方向时的减速（越小停得越快）

    [Header("Ground Detection")]
    public Transform groundPoint;           // 放在角色脚底的空物体
    public float groundRadius = 0.15f;
    public LayerMask groundLayerMask;       // 只勾选 Ground 层
    public bool isGrounded;

    [Header("Mobile Controls")]
    public Joystick leftJoystick;           // 拖入 Fixed / Floating / Variable Joystick
    [Range(0.0f, 1.0f)]
    public float joystickDeadZone = 0.2f;

    [Header("Respawn")]
    public Transform spawnPoint;            // 初始出生点（可选，不填就用开局位置）

    protected Rigidbody2D rb;
    private Vector3 currentRespawnPosition;
    private Animator animator;
    private bool jumpRequested;             // 由跳跃按钮或键盘设置

    // Animator 参数名，必须和 Animator 窗口里的一模一样
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int YVelocityHash = Animator.StringToHash("yVelocity");

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentRespawnPosition = (spawnPoint != null) ? spawnPoint.position : transform.position;

        if (leftJoystick == null)
        {
            leftJoystick = FindObjectOfType<Joystick>();
        }
    }

    void Update()
    {
        // 按键在 Update 里读，FixedUpdate 里可能漏掉
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }
    }

    protected virtual void FixedUpdate()
    {
        CheckGrounded();
        Move();
        Jump();
        UpdateAnimator();
    }

    // ---------------- Input ----------------

    protected float GetHorizontalInput()
    {
        float keyboard = Input.GetAxisRaw("Horizontal");
        float stick = 0.0f;

        if (leftJoystick != null && Mathf.Abs(leftJoystick.Horizontal) > joystickDeadZone)
        {
            stick = leftJoystick.Horizontal;
        }

        // 哪个输入更大用哪个
        return Mathf.Abs(stick) > Mathf.Abs(keyboard) ? stick : keyboard;
    }

    /// <summary>
    /// 给 UI 跳跃按钮调用（见 JumpButtonBehaviour）。
    /// </summary>
    public void RequestJump()
    {
        jumpRequested = true;
    }

    // ---------------- Movement ----------------

    private void CheckGrounded()
    {
        if (groundPoint == null) return;
        isGrounded = Physics2D.OverlapCircle(groundPoint.position, groundRadius, groundLayerMask);
    }

    private void Move()
    {
        float x = GetHorizontalInput();

        if (Mathf.Abs(x) > 0.01f)
        {
            Flip(x);

            float force = horizontalForce * (isGrounded ? 1.0f : airControlFactor);
            rb.AddForce(Vector2.right * x * force);

            // 限制最大水平速度
            Vector2 v = rb.velocity;
            v.x = Mathf.Clamp(v.x, -maxSpeed, maxSpeed);
            rb.velocity = v;
        }
        else if (isGrounded)
        {
            // 没有输入且在地上：慢慢停下，防止滑冰
            rb.velocity = new Vector2(rb.velocity.x * groundFriction, rb.velocity.y);
        }
    }

    private void Jump()
    {
        if (jumpRequested && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        jumpRequested = false;
    }

    private void Flip(float x)
    {
        // 用 localScale 翻转，子物体（groundPoint）也会跟着翻
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (x > 0 ? 1.0f : -1.0f);
        transform.localScale = scale;
    }

    // ---------------- Animation ----------------

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat(SpeedHash, Mathf.Abs(rb.velocity.x));
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetFloat(YVelocityHash, rb.velocity.y);
    }

    // ---------------- Checkpoint / Respawn ----------------

    public void SetRespawnPoint(Vector3 position)
    {
        currentRespawnPosition = position;
    }

    /// <summary>
    /// 掉进死亡区时调用。virtual：Lab 6 可以重写，加扣血、受伤等。
    /// </summary>
    public virtual void Respawn()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0.0f;
        transform.position = currentRespawnPosition;
    }

    void OnDrawGizmos()
    {
        if (groundPoint == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundPoint.position, groundRadius);
    }
}
