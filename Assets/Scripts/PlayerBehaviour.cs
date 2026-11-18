using UnityEngine;

/// <summary>
/// 2D platformer player controller: movement, jumping, ground check,
/// sprite flipping, animation, checkpoint respawn and taking damage.
/// Works with the keyboard (PC) and the Joystick Pack joystick + jump button (mobile).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBehaviour : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    public float horizontalForce = 20.0f;
    public float maxSpeed = 5.0f;
    public float jumpForce = 12.0f;
    [Range(0.0f, 1.0f)]
    public float airControlFactor = 0.6f;   // fraction of move force applied in the air
    [Range(0.0f, 1.0f)]
    public float groundFriction = 0.85f;    // slowdown when no input (lower = stops faster)

    [Header("Ground Detection")]
    public Transform groundPoint;           // empty child at the player's feet
    public float groundRadius = 0.15f;
    public LayerMask groundLayerMask;       // Ground layer (level tiles + box)
    public bool isGrounded;

    [Header("Mobile Controls")]
    public Joystick leftJoystick;
    [Range(0.0f, 1.0f)]
    public float joystickDeadZone = 0.2f;

    [Header("Respawn")]
    public Transform spawnPoint;            // optional, defaults to the start position

    [Header("Health / Damage")]
    public int maxHealth = 3;
    public int currentHealth;
    public float invulnerableTime = 1.0f;   // no damage for this long after a hit
    public float hurtStunTime = 0.3f;       // no move input during knockback
    public Vector2 knockbackForce = new Vector2(6.0f, 6.0f);
    public CameraShakeBehaviour cameraShake;

    protected Rigidbody2D rb;
    private Vector3 currentRespawnPosition;
    private Animator animator;
    private bool jumpRequested;             // set by the keyboard or the jump button
    private SpriteRenderer spriteRenderer;
    private float invulnerableTimer;
    private float hurtTimer;

    // Animator parameter names (must match the Animator window exactly)
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int YVelocityHash = Animator.StringToHash("yVelocity");

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        currentRespawnPosition = (spawnPoint != null) ? spawnPoint.position : transform.position;

        if (leftJoystick == null)
        {
            leftJoystick = FindObjectOfType<Joystick>();
        }
        if (cameraShake == null)
        {
            cameraShake = FindObjectOfType<CameraShakeBehaviour>();
        }
    }

    void Update()
    {
        // Read key presses in Update so they are never missed
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }

        UpdateDamageTimers();
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

        // Use whichever input is stronger
        return Mathf.Abs(stick) > Mathf.Abs(keyboard) ? stick : keyboard;
    }

    /// <summary>
    /// Called by the UI jump button (JumpButtonBehaviour).
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
        if (hurtTimer > 0.0f) return;       // being knocked back: ignore input

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
    /// Called by the dead plane. Virtual so later labs can add damage, etc.
    /// </summary>
    public virtual void Respawn()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0.0f;
        transform.position = currentRespawnPosition;
    }

    // ---------------- Damage ----------------

    /// <summary>
    /// IDamageable: called by enemies. Lose health, get knocked back,
    /// shake the camera, then stay invulnerable for a short time.
    /// </summary>
    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (invulnerableTimer > 0.0f) return;   // still flashing from the last hit

        currentHealth -= amount;
        invulnerableTimer = invulnerableTime;
        hurtTimer = hurtStunTime;

        // Push away from whatever hit us
        float side = (transform.position.x >= sourcePosition.x) ? 1.0f : -1.0f;
        rb.velocity = new Vector2(side * knockbackForce.x, knockbackForce.y);

        if (cameraShake != null)
        {
            cameraShake.Shake();
        }

        if (currentHealth <= 0)
        {
            // Out of health: back to the last checkpoint with full health
            currentHealth = maxHealth;
            Respawn();
        }
    }

    private void UpdateDamageTimers()
    {
        if (hurtTimer > 0.0f) hurtTimer -= Time.deltaTime;

        if (invulnerableTimer > 0.0f)
        {
            invulnerableTimer -= Time.deltaTime;

            // Blink while invulnerable so the hit is easy to see
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = invulnerableTimer <= 0.0f || Mathf.FloorToInt(invulnerableTimer * 10.0f) % 2 == 0;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (groundPoint == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundPoint.position, groundRadius);
    }
}
