using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;


    // Character Stats
    CharacterStats stats;
    public CharacterStats Stats
    {
        get { return stats; }
        set { stats = value; }
    }

    // Controls
    public Joystick joystick;
    public Button jumpButton, dashButton, attackButton, pauseButton;

    [SerializeField] bool onground;
    [SerializeField] bool isjump;

    [SerializeField] float jumpCooldown = 0.5f;
    [SerializeField] float jumpCooldownTimer = 0f;
    public LayerMask groundLayer;

    // Dashing
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] float dashingPower = 100f;
    [SerializeField] float dashingTime = 0.2f;
    [SerializeField] float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer tr;

    // Wall Jump
    [SerializeField] private float wallJumpForce = 20f;
    [SerializeField] private float wallSlideSpeed = 0.3f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 5f;
    private bool isWallSliding = false;
    private bool isWallJumping = false;
    private float wallJumpDirection = 0f;
    private float wallJumpTime = 0.2f;
    private float wallJumpTimer;

    [SerializeField] private bool isJumping = false;

    // Stats
    public Slider healthSlider, staminaSlider; // Sliders for UI representation

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        tr = GetComponent<TrailRenderer>();
        stats = GetComponent<CharacterStats>();
    }

    void Start()
    {
        // Import all variables from CharacterStats
        float maxHealth = stats.maxHealth;
        float maxStamina = stats.maxStamina;
        float speed = stats.speed;
        float jumpPower = stats.jumpPower;
        float staminaRegenRate = stats.staminaRegenRate;
        float healthRegenRate = stats.healthRegenRate;

        float currentHealth = stats.currentHealth;
        float currentStamina = stats.currentStamina;

        // Initialize sliders
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;

    }

    void FixedUpdate()
    {
        isGrounded();
        movement();
        sliderUpdate();
    }

    void Update()
    {
        WallSlide();

        // Set Listener Button
        jumpButton.onClick.AddListener(handleJump);
        dashButton.onClick.AddListener(Dashing);
    }

    private bool IsTouchingWall()
    {
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        bool isTouchingWall = hitRight.collider != null || hitLeft.collider != null; // Check if the player is touching a wall
        isWallSliding = isTouchingWall; // Update the isWallSliding variable
        return isTouchingWall; // Return the result
    }

    void movement()
    {
        float h = joystick.Horizontal;

        if (!isDashing && h != 0)
        {
            if (h < 0)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (h > 0)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            Vector2 newVelocity = rb.velocity;
            newVelocity.x = h * stats.speed;
            rb.velocity = newVelocity;
            animator.SetFloat("hMove", MathF.Abs(h));
        }
        else
        {
            animator.SetFloat("hMove", 0);
        }
        animator.SetFloat("yMove", MathF.Abs(rb.velocity.y));
    }

    void jump()
    {
        if (!isJumping && !isjump && onground && jumpCooldownTimer <= 0f && stats.currentStamina >= 10) // Check for stamina
        {
            isJumping = true;
            AudioManager.Instance.PlaySFX("Jump");
            animator.SetBool("onGround", false);
            jumpCooldownTimer = jumpCooldown;
            stats.currentStamina -= 10; // Decrease stamina for jumping
            staminaSlider.value = stats.currentStamina; // Update stamina slider
            StartCoroutine(PrepareJump());
        }
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }
    }

    void WallSlide()
    {
        if (IsTouchingWall() && !onground && rb.velocity.y < 0)
        {
            isWallSliding = true;

            // Tentukan arah tembok
            if (Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer))
            {
                transform.localScale = new Vector3(-1f, 1f, 1f); // Hadap ke kanan
            }
            else if (Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer))
            {
                transform.localScale = new Vector3(1f, 1f, 1f); // Hadap ke kiri
            }

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
            animator.SetBool("wallSliding", true);
        }
        else
        {
            isWallSliding = false;
            animator.SetBool("wallSliding", false);
        }
    }

    public void WallJump()
    {
        if (isWallSliding)
        {
            float wallDirection = 0f;
            if (Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer))
            {
                wallDirection = -1f; // Wall is on the right
            }
            else if (Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer))
            {
                wallDirection = 1f; // Wall is on the left
            }

            // Update player scale to face the wall
            if (wallDirection != 0)
            {
                transform.localScale = new Vector3(wallDirection, 1f, 1f);
            }

            isWallJumping = true;
            wallJumpDirection = wallDirection; // Use the wall direction for jumping
            wallJumpTimer = wallJumpTime;

            Vector2 wallJumpVelocity = new Vector2(wallJumpDirection * wallJumpForce, stats.jumpPower);
            rb.velocity = wallJumpVelocity;
            CancelInvoke(nameof(StopWallJumping));
            Invoke(nameof(StopWallJumping), wallJumpTime);
        }
    }
    void StopWallJumping()
    {
        AudioManager.Instance.PlaySFX("Jump");
        isWallJumping = false;
    }

    IEnumerator PrepareJump()
    {
        AudioManager.Instance.PlaySFX("Jump");
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(0, stats.jumpPower), ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }

    void isGrounded()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 5f;
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        if (hit.collider != null)
        {
            onground = true;
            isjump = false;
            animator.SetBool("onGround", true);
        }
        else
        {
            isjump = true;
            onground = false;
            animator.SetBool("onGround", false);
        }
    }

    void handleJump()
    {
        if (isWallSliding == false && onground == true)
        {
            jump();
        }
        else if (isWallSliding == true)
        {
            WallJump();
        }
    }

    void Dashing()
    {
        if (canDash && stats.currentStamina >= 20) // Check for stamina
        {
            animator.SetTrigger("isDash");
            AudioManager.Instance.PlaySFX("Dash");
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(-transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        stats.currentStamina -= 20; // Decrease stamina for dashing
        staminaSlider.value = stats.currentStamina; // Update stamina slider
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false; // End the dash
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(dashingCooldown - dashingTime);
        canDash = true;
    }



    // Method to take damage from the enemy (implemented in CharacterStats)
    // This method will be called when the player takes damage
    public void TakeDamage(int damage)
    {
        stats.TakeDamage(damage); // Lanjut ke karakter stats
    }

    void sliderUpdate()
    {
        healthSlider.value = stats.currentHealth; // Update health slider
        staminaSlider.value = stats.currentStamina; // Update stamina slider
    }
    
}
