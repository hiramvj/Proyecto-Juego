using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    public float fallMultiplier = 2f;      
    public float lowJumpMultiplier = 2f;  

    [Header("Suelo")]
    public Transform groundCheck;      // objeto vacío en los pies
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Visual")]
    public Transform visual;           // el hijo con el modelo

    public float turnAngle = 60f;  
    public float turnSpeed = 12f; 
    float targetY = 120f;

    Rigidbody2D rb;
    Animator anim;
    bool isGrounded;
    float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        targetY = 180f - turnAngle;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        if (moveInput > 0.01f) targetY = 180f - turnAngle;
        if (moveInput < -0.01f) targetY = 180f + turnAngle;

        visual.localRotation = Quaternion.Slerp(
            visual.localRotation,
            Quaternion.Euler(0, targetY, 0),
            turnSpeed * Time.deltaTime);

        // Animator
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetBool("IsGrounded", isGrounded);

        // Prueba de muerte (borrar después)
        if (Input.GetKeyDown(KeyCode.K)) PlayDeath();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        float g = Physics2D.gravity.y * rb.gravityScale;

        if (rb.velocity.y < 0)
        {
            // Cayendo: más gravedad
            rb.velocity += Vector2.up * g * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * g * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    public void PlayDeath()
    {
        anim.SetTrigger("Death");
        rb.velocity = Vector2.zero;
        enabled = false;
    }
}