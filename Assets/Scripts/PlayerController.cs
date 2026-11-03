using System;
using UnityEngine;

public enum SizeMode { Micro, Grande }

[Serializable]
public class SizeSettings
{
    public float visualScale;
    public Vector2 colliderSize;
    public Vector2 colliderOffset;
    public float mass;
    public float moveSpeed;
    public float jumpForce;
}

public class PlayerController : MonoBehaviour
{
    // Lo escuchan el HUD (P4) y los objetos empujables (P3)
    public static event Action<SizeMode> OnSizeModeChanged;
    public SizeMode CurrentMode { get; private set; }
    public bool CanPush => CurrentMode == SizeMode.Grande;

    [Header("Modos de tamaño")]
    public SizeMode startMode = SizeMode.Grande;
    public SizeSettings grande = new SizeSettings
    {
        visualScale = 0.4f,
        colliderSize = new Vector2(1.25f, 1.96f),
        colliderOffset = new Vector2(0.04f, 0.48f),
        mass = 3f,
        moveSpeed = 5f,
        jumpForce = 12f
    };
    public SizeSettings micro = new SizeSettings
    {
        visualScale = 0.2f,
        colliderSize = new Vector2(0.62f, 0.98f),
        colliderOffset = new Vector2(0.02f, -0.01f),
        mass = 0.5f,
        moveSpeed = 4f,
        jumpForce = 8f
    };
    public LayerMask obstacleLayer;     

    [Header("Salto con peso")]
    public float fallMultiplier = 2f;
    public float lowJumpMultiplier = 2f;

    [Header("Suelo")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Visual")]
    public Transform visual;
    public float turnAngle = 60f;
    public float turnSpeed = 12f;

    [Header("Debug")]
    public bool debugKeys = true;

    Rigidbody2D rb;
    CapsuleCollider2D col;
    Animator anim;
    bool isGrounded;
    float moveInput;
    float targetY;
    float moveSpeed;
    float jumpForce;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<Animator>();
        targetY = 180f - turnAngle;
    }

    void Start()
    {
        ApplyMode(startMode);  
    }

    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftShift))
            TrySwitchSize();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        if (moveInput > 0.01f) targetY = 180f - turnAngle;
        if (moveInput < -0.01f) targetY = 180f + turnAngle;
        visual.localRotation = Quaternion.Slerp(
            visual.localRotation,
            Quaternion.Euler(0, targetY, 0),
            turnSpeed * Time.deltaTime);

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetBool("IsGrounded", isGrounded);

        if (debugKeys && Input.GetKeyDown(KeyCode.K)) PlayDeath();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        float g = Physics2D.gravity.y * rb.gravityScale;
        if (rb.velocity.y < 0)
            rb.velocity += Vector2.up * g * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            rb.velocity += Vector2.up * g * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    // ---------- Cambio de tamaño ----------
    void TrySwitchSize()
    {
        SizeMode target = CurrentMode == SizeMode.Micro ? SizeMode.Grande : SizeMode.Micro;

        if (target == SizeMode.Grande && !HasRoomToGrow())
        {
            Debug.Log("No hay espacio para crecer");
            return;
        }
        ApplyMode(target);
    }

    bool HasRoomToGrow()
    {
        Vector2 center = (Vector2)transform.position + grande.colliderOffset;
        Vector2 size = grande.colliderSize * 0.95f;   
        return Physics2D.OverlapCapsule(
            center, size, CapsuleDirection2D.Vertical, 0f, obstacleLayer) == null;
    }

    void ApplyMode(SizeMode mode)
    {
        CurrentMode = mode;
        SizeSettings s = mode == SizeMode.Micro ? micro : grande;

        visual.localScale = Vector3.one * s.visualScale;
        col.size = s.colliderSize;
        col.offset = s.colliderOffset;
        rb.mass = s.mass;
        moveSpeed = s.moveSpeed;
        jumpForce = s.jumpForce;

        OnSizeModeChanged?.Invoke(mode);
    }

    public void PlayDeath()
    {
        anim.SetTrigger("Death");
        rb.velocity = Vector2.zero;
        enabled = false;
    }
}