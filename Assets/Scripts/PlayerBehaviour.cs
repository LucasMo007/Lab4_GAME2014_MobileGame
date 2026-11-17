using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBehaviour : MonoBehaviour
{
    [Header("Movement")]
    public float horizontalForce = 20.0f;
    public float maxSpeed = 5.0f;
    public float jumpForce = 12.0f;
    [Range(0.0f, 1.0f)]
    public float airControlFactor = 0.6f;   
    [Range(0.0f, 1.0f)]
    public float groundFriction = 0.85f;    

    [Header("Ground Detection")]
    public Transform groundPoint;           
    public float groundRadius = 0.15f;
    public LayerMask groundLayerMask;       
    public bool isGrounded;

    [Header("Box (not walkable)")]
    public LayerMask boxLayerMask;          
    public float slideOffForce = 25.0f;     

    [Header("Mobile Controls")]
    public Joystick leftJoystick;
    [Range(0.0f, 1.0f)]
    public float joystickDeadZone = 0.2f;

    [Header("Respawn")]
    public Transform spawnPoint;            // optional, defaults to the start position

    protected Rigidbody2D rb;
    private Vector3 currentRespawnPosition;
    private Animator animator;
    private bool jumpRequested;             // set by the keyboard or the jump button

    // Animator parameter names (must match the Animator window exactly)
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
        // Read key presses in Update so they are never missed
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }
    }

    protected virtual void FixedUpdate()
    {
        CheckGrounded();
        SlideOffBox();
        Move();
        Jump();
        UpdateAnimator();
    }


    protected float GetHorizontalInput()
    {
        float keyboard = Input.GetAxisRaw("Horizontal");
        float stick = 0.0f;

        if (leftJoystick != null && Mathf.Abs(leftJoystick.Horizontal) > joystickDeadZone)
        {
            stick = leftJoystick.Horizontal;
        }

        // Use whichever input is stronger
        return Mathf.Abs(stick) > Mathf.Abs(keyboard) ? stick : keyboard;
    }

   
    public void RequestJump()
    {
        jumpRequested = true;
    }

   

    private void CheckGrounded()
    {
        if (groundPoint == null) return;
        isGrounded = Physics2D.OverlapCircle(groundPoint.position, groundRadius, groundLayerMask);
    }

    // The box is not a platform: if the feet touch it, push the player off the nearer side
    private void SlideOffBox()
    {
        if (groundPoint == null || isGrounded) return;

        Collider2D box = Physics2D.OverlapCircle(groundPoint.position, groundRadius, boxLayerMask);
        if (box == null) return;

        float side = Mathf.Sign(transform.position.x - box.bounds.center.x);
        if (side == 0.0f) side = 1.0f;
        rb.AddForce(Vector2.right * side * slideOffForce);
    }

    private void Move()
    {
        float x = GetHorizontalInput();

        if (Mathf.Abs(x) > 0.01f)
        {
            Flip(x);

            float force = horizontalForce * (isGrounded ? 1.0f : airControlFactor);
            rb.AddForce(Vector2.right * x * force);

            // Clamp horizontal speed
            Vector2 v = rb.velocity;
            v.x = Mathf.Clamp(v.x, -maxSpeed, maxSpeed);
            rb.velocity = v;
        }
        else if (isGrounded)
        {
            // No input on the ground: slow down instead of sliding
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
        jumpRequested = false;  // always clear, so a mid-air press doesn't jump on landing
    }

    private void Flip(float x)
    {
        // Flip with localScale so children (groundPoint) flip too
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (x > 0 ? 1.0f : -1.0f);
        transform.localScale = scale;
    }

   

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat(SpeedHash, Mathf.Abs(rb.velocity.x));
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetFloat(YVelocityHash, rb.velocity.y);
    }



    public void SetRespawnPoint(Vector3 position)
    {
        currentRespawnPosition = position;
    }


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
