using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;

    // Input System for Mobile Controls
    InputSystem_Actions inputActions;

    // Pause Menu
    GameObject pausePanel;

    // Character Stats
    CharacterStats stats;
    public CharacterStats Stats
    {
        get { return stats; }
        set { stats = value; }
    }

    // Player Attack Variables
    private float lastAttackTime = 0f;

    [SerializeField] bool onground;
    [SerializeField] bool isjump;

    [SerializeField] float jumpCooldownTimer = 0f;
    public LayerMask groundLayer;

    // Dashing
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] float dashingTime = 0.2f;
    [SerializeField] private TrailRenderer tr;

    // Wall Jump
    [SerializeField] private float wallSlideSpeed = 0.75f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.25f;
    private bool isWallSliding = false;
    private bool isWallJumping = false;
    private float wallJumpDirection = 0f;
    private float wallJumpTime = 0.2f;
    private float wallJumpTimer;

    [SerializeField] private bool isJumping = false;

    // Boolean part of cutscene even if the player was going to death
    // [SerializeField] public bool isCutscene;

    // Stats
    public Slider healthSlider, staminaSlider; // Sliders for UI representation

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        tr = GetComponent<TrailRenderer>();
        stats = GetComponent<CharacterStats>();

        // Initialize controllers and UI elements
        getController();
    }

    void Start()
    {

        // Initialize sliders
        healthSlider.maxValue = stats.maxHealth;
        healthSlider.value = stats.currentHealth;
        staminaSlider.maxValue = stats.maxStamina;
        staminaSlider.value = stats.currentStamina;

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
        Pause();
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        // Debug wall jump state
        if (isWallJumping)
        {
            Debug.Log($"Wall Jumping - Velocity: {rb.velocity}, Direction: {wallJumpDirection}");
        }
    }

    private void getController()
    {
        // Get the Input System
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        // Efficient way to get the actions
        InputSystem_Actions.PlayerActions input;
        input = inputActions.Player;

        // Set up the input actions
        input.Move.performed += ctx => movement();
        input.Jump.performed += ctx => handleJump();
        input.Dash.performed += ctx => Dashing();
        input.Attack.performed += ctx => Attack();

        // Add Listener to Pause Button
        // Get the Pause instance (assuming it's a component on a GameObject
        // Initialize pause panel
        pausePanel = GameObject.Find("Canvas").transform.Find("GUI").Find("Pause").gameObject;
        pausePanel.SetActive(false); // Ensure the pause panel is initially inactive
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
        // Jika sedang dashing, coroutine Dash() yang mengontrol pergerakan.
        if (isDashing)
        {
            // Update animator untuk dash
            animator.SetFloat("yMove", MathF.Abs(rb.velocity.y));
            return;
        }

        // Jika sedang wall jumping, JANGAN ubah velocity sama sekali
        if (isWallJumping)
        {
            // Hanya update animator berdasarkan kecepatan saat ini
            animator.SetFloat("hMove", Mathf.Abs(rb.velocity.x) > 0.01f ? 1f : 0f);
            animator.SetFloat("yMove", MathF.Abs(rb.velocity.y));
            Debug.Log($"Wall Jump Active - Current Velocity: {rb.velocity}");
            return; // KELUAR SEBELUM MENGUBAH VELOCITY
        }

        float h = 0f;
        if (inputActions != null)
        {
            h = inputActions.Player.Move.ReadValue<Vector2>().x;
        }
        else
        {
            h = Input.GetAxis("Horizontal");
        }

        // Pergerakan normal
        if (h != 0)
        {
            // Update facing direction
            if (h < 0)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else if (h > 0)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }

            // Apply horizontal movement
            rb.velocity = new Vector2(h * stats.speed, rb.velocity.y);
            animator.SetFloat("hMove", MathF.Abs(h));
        }
        else
        {
            // Stop horizontal movement when no input
            rb.velocity = new Vector2(0f, rb.velocity.y);
            animator.SetFloat("hMove", 0f);
        }

        // Update vertical animation
        animator.SetFloat("yMove", MathF.Abs(rb.velocity.y));
    }

    void Pause()
    {
        if (pausePanel != null && inputActions != null)
        {
            if (inputActions.Player.PauseGame.triggered)
            {
                if (pausePanel.activeSelf)
                {
                    pausePanel.SetActive(false);
                    Time.timeScale = 1f; // Resume the game
                }
                else
                {
                    pausePanel.SetActive(true);
                    Time.timeScale = 0f; // Pause the game
                }
            }
        }
    }
    void jump()
    {
        if (!isJumping && !isjump && onground && jumpCooldownTimer <= 0f && stats.GetActionCost("Jump") <= stats.currentStamina)
        {
            stats.TakeAction(stats.GetActionCost("Jump")); // Reduce stamina for jump action
            isGrounded(); // Check if the player is grounded before jumping
            isJumping = true;
            animator.SetBool("onGround", false);
            jumpCooldownTimer = stats.jumpCooldown;
            StartCoroutine(PrepareJump());
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
                transform.localScale = new Vector3(1f, 1f, 1f); // Hadap ke kanan
            }
            else if (Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer))
            {
                transform.localScale = new Vector3(-1f, 1f, 1f); // Hadap ke kiri
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
        if (isWallSliding && stats.GetActionCost("Jump") <= stats.currentStamina)
        {
            Debug.Log("Wall Jump Initiated");

            // Konsumsi stamina untuk wall jump
            stats.TakeAction(stats.GetActionCost("Jump"));

            float wallDirection = 0f;
            if (Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer))
            {
                wallDirection = -1f; // Wall is on the right, jump to left
                Debug.Log("Wall on right, jumping left");
            }
            else if (Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer))
            {
                wallDirection = 1f; // Wall is on the left, jump to right
                Debug.Log("Wall on left, jumping right");
            }

            if (wallDirection != 0)
            {
                // Set state wall jump
                isWallJumping = true;
                wallJumpDirection = wallDirection;

                // Hentikan wall sliding
                isWallSliding = false;
                animator.SetBool("wallSliding", false);

                // Update scale menghadap arah jump
                transform.localScale = new Vector3(wallDirection, 1f, 1f);

                // Stop any existing wall jump coroutine
                StopCoroutine(nameof(PerformWallJump));

                // Start wall jump coroutine
                StartCoroutine(PerformWallJump(wallDirection));

                // Play sound effect
                AudioManager.Instance.PlaySFX("Jump");
            }
        }
    }

    IEnumerator PerformWallJump(float direction)
    {
        Debug.Log($"Starting Wall Jump - Direction: {direction}");

        // Set initial wall jump velocity dengan force yang lebih kuat
        Vector2 wallJumpVelocity = new Vector2(direction * stats.speed * 2f, stats.jumpPower);
        rb.velocity = wallJumpVelocity;

        Debug.Log($"Wall Jump Velocity Set: {rb.velocity}");

        // Maintain velocity during wall jump duration
        float timer = 0f;
        while (timer < wallJumpTime)
        {
            // Pastikan velocity horizontal tetap terjaga selama wall jump
            if (isWallJumping)
            {
                rb.velocity = new Vector2(direction * stats.speed * 2f, rb.velocity.y);
                Debug.Log($"Maintaining Wall Jump - Timer: {timer:F2}, Velocity: {rb.velocity}");
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Wall Jump Completed");
        // End wall jump
        isWallJumping = false;
    }

    void StopWallJumping()
    {
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
        if (canDash && !isDashing && stats.GetActionCost("Dash") <= stats.currentStamina)
        {
            stats.TakeAction(stats.GetActionCost("Dash"));
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
        rb.velocity = new Vector2(transform.localScale.x * stats.dashPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false; // End the dash
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(stats.dashCooldown - dashingTime);
        canDash = true;
    }

    // Method to take damage from the enemy (implemented in CharacterStats)
    // This method will be called when the player takes damage
    public void TakeDamage(int damage)
    {
        stats.TakeDamage(damage); // Lanjut ke karakter stats
    }

    // Method to handle player attack
    public void Attack()
    {

        // Trigger attack animation
        animator.SetTrigger("isAttack");
        
        if (Time.time >= lastAttackTime + stats.attackCooldown)
        {
            // Define the direction the player is facing
            Vector2 attackDirection = transform.localScale.x < 0 ? Vector2.left : Vector2.right;

            // Define the attack box center and size
            Vector2 boxCenter = (Vector2)transform.position + attackDirection * (stats.attackRange / 2f);
            Vector2 boxSize = new Vector2(stats.attackRange, 1f); // 1f is height of the line area

            // Find all enemies in the attack area
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, LayerMask.GetMask("Enemy"));

            bool attacked = false;

            foreach (Collider2D enemyCollider in hitEnemies)
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    try
                    {
                        AudioManager.Instance.PlaySFX("Attack");
                    }
                    catch (System.Exception)
                    {
                        Debug.LogWarning("AudioManager.Instance not found or PlaySFX method failed.");
                    }

                    enemy.TakeDamage((int)stats.attackPower);
                    attacked = true;
                    // Trigger attack animation

                    // Log the attack for debugging
                    Debug.Log(gameObject.name + " attacked " + enemy.gameObject.name + " for " + (int)stats.attackPower + " damage.");
                }
            }
            if (attacked)
            {
                lastAttackTime = Time.time;
            }
        }
    }
    void sliderUpdate()
    {
        healthSlider.value = stats.currentHealth; // Update health slider
        staminaSlider.value = stats.currentStamina; // Update stamina slider
    }
    // On-Enable and On-Disable methods for Input System
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    // Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        if (stats == null) return;
        Gizmos.color = Color.red;
        Vector2 attackDirection = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        Vector2 boxCenter = (Vector2)transform.position + attackDirection * (stats.attackRange / 2f);
        Vector2 boxSize = new Vector2(stats.attackRange, 1f); // 1f is height of the line area
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}