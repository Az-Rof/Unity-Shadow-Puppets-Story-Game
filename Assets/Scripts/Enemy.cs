using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    // State machine for AI
    private enum AIState { Patrolling, Chasing, Combat }
    private AIState currentState;

    [Header("AI Components")]
    private CharacterStats stats;
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;
    public float patrolWaitTime = 3f;
    private int currentPatrolPointIndex = 0;
    private float waitTimer;
    private Vector3 startingPosition;

    [Header("Detection & Chase Settings")]
    public Transform player;
    [SerializeField] private BoxCollider2D suspicionZone;
    public float combatRange = 10f;
    public float chaseTimeout = 10f;
    private Vector3 lastKnownPlayerPosition;
    private float chaseTimer;

    [Header("Smart AI Settings")]
    [Range(0f, 1f)]
    public float dashChance = 0.6f;
    public float repositionTime = 4f;
    public float dashDuration = 0.2f;
    private float lastActionTimer;

    [Header("Combat Tracking")]
    private float lastAttackTime;
    private float lastDashTime;
    private float lastJumpTime;
    private bool isDashing = false;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundRaycastDistance = 1f;
    public float ledgeCheckDistance = 0.5f;
    private bool isGrounded;

    private Vector2 moveDirection;
    private bool isWaiting = false;

    void Start()
    {
        stats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        if (suspicionZone == null) Debug.LogError("Suspicion Zone BoxCollider2D tidak di-set di Inspector untuk " + gameObject.name);

        startingPosition = transform.position;
        SwitchState(AIState.Patrolling);
    }

    void Update()
    {
        if (isDashing) return;

        CheckIfGrounded();
        HandleStateMachine();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    void HandleStateMachine()
    {
        switch (currentState)
        {
            case AIState.Patrolling: Patrol(); break;
            case AIState.Chasing: Chase(); break;
            case AIState.Combat: Combat(); break;
        }
    }

    public void TakeDamage(int damage)
    {
        stats.TakeDamage(damage);
        AudioManager.Instance.PlaySFX("Attack");
    }

    #region State Logic & Actions

    void Patrol()
    {
        if (IsPlayerInDetectionRange()) { SwitchState(AIState.Chasing); return; }

        if (!isWaiting && isGrounded && moveDirection.x != 0 && IsNearLedge())
        {
            moveDirection = Vector2.zero;
            Flip(-transform.localScale.x);
            return;
        }

        if (patrolPoints.Count == 0)
        {
            float horizontalDistanceToStart = Mathf.Abs(transform.position.x - startingPosition.x);
            if (horizontalDistanceToStart > 0.5f)
            {
                MoveTowards(startingPosition, false);
            }
            else
            {
                moveDirection = Vector2.zero;
            }
            isWaiting = false;
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolPointIndex];

        float horizontalDistance = Mathf.Abs(transform.position.x - targetPoint.position.x);

        if (horizontalDistance < 0.5f)
        {
            moveDirection = Vector2.zero;
            isWaiting = true;
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                waitTimer = patrolWaitTime;
                currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Count;
                isWaiting = false;
            }
        }
        else
        {
            isWaiting = false;
            MoveTowards(targetPoint.position, false);
        }
    }

    void Chase()
    {
        isWaiting = false;

        if (isGrounded && IsNearLedge())
        {
            SwitchState(AIState.Patrolling);
            return;
        }

        if (IsPlayerInCombatRange()) { SwitchState(AIState.Combat); return; }
        if (!IsPlayerInDetectionRange())
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer > chaseTimeout) { SwitchState(AIState.Patrolling); chaseTimer = 0; return; }
        }
        else
        {
            chaseTimer = 0;
            lastKnownPlayerPosition = player.position;
        }
        MoveTowards(lastKnownPlayerPosition, false);
    }

    void Combat()
    {
        isWaiting = false;

        if (isGrounded && IsNearLedge() && (player.position - transform.position).normalized.x == Mathf.Sign(transform.localScale.x))
        {
            moveDirection = Vector2.zero;
        }
        else
        {
            HandleTacticalPositioning(Vector2.Distance(transform.position, player.position));
        }

        if (!IsPlayerInCombatRange() && IsPlayerInDetectionRange()) { SwitchState(AIState.Chasing); return; }
        if (!IsPlayerInDetectionRange()) { SwitchState(AIState.Patrolling); return; }

        lastActionTimer += Time.deltaTime;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= stats.attackRange && Time.time > lastAttackTime + stats.attackCooldown) { Attack(); }
        else if (Time.time > lastDashTime + stats.dashCooldown)
        {
            if (distanceToPlayer < 2.5f && Random.value < dashChance) { Dash(); }
        }

        if (player.position.y > transform.position.y + 1f && isGrounded && Time.time > lastJumpTime + stats.jumpCooldown) { Jump(); }

        if (lastActionTimer >= repositionTime && Time.time > lastDashTime + stats.dashCooldown) { Dash(); }
    }

    void HandleTacticalPositioning(float currentDistance)
    {
        float idealDistance = stats.attackRange * 0.8f;
        float comfortZone = 0.5f;

        if (currentDistance > stats.attackRange) { MoveTowards(player.position, true); }
        else if (currentDistance < idealDistance - comfortZone) { MoveTowards(transform.position - (player.position - transform.position), true); }
        else if (currentDistance > idealDistance + comfortZone) { MoveTowards(player.position, true); }
        else { moveDirection = Vector2.zero; }
    }

    void Attack()
    {
        float attackCost = stats.GetActionCost("Attack");
        if (stats.currentStamina >= attackCost)
        {
            lastAttackTime = Time.time;
            stats.TakeAction(attackCost);
            animator.SetTrigger("Attack");
            ResetActionTimer();
        }
    }

    public void DealDamageToPlayer()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= stats.attackRange)
        {
            CharacterStats playerStats = player.GetComponent<CharacterStats>();
            if (playerStats != null) playerStats.TakeDamage((int)stats.attackPower);
            AudioManager.Instance.PlaySFX("Attack");
        }
    }

    void Dash()
    {
        float dashCost = stats.GetActionCost("Dash");
        if (stats.currentStamina >= dashCost && !isDashing)
        {
            lastDashTime = Time.time;
            stats.TakeAction(dashCost);
            animator.SetTrigger("Dash");
            ResetActionTimer();
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        AudioManager.Instance.PlaySFX("Dash");
        isDashing = true;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float directionToPlayerX = Mathf.Sign(player.position.x - transform.position.x);
        float dashDirection;

        if (distanceToPlayer < 5f)
        {
            dashDirection = -directionToPlayerX;
        }
        else
        {
            dashDirection = directionToPlayerX;
        }

        if (dashDirection == 0)
        {
            dashDirection = (transform.localScale.x > 0) ? -1 : 1;
        }

        Flip(dashDirection);
        rb.velocity = new Vector2(dashDirection * stats.dashPower, 0f);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }

    void Jump()
    {
        float jumpCost = stats.GetActionCost("Jump");
        if (stats.currentStamina >= jumpCost)
        {
            lastJumpTime = Time.time;
            stats.TakeAction(jumpCost);
            animator.SetTrigger("Jump");
            AudioManager.Instance.PlaySFX("Jump");
            rb.AddForce(new Vector2(0, stats.jumpPower), ForceMode2D.Impulse);
            ResetActionTimer();
        }
    }

    #endregion

    #region Physics & Helpers

    void MoveTowards(Vector3 target, bool faceTarget)
    {
        moveDirection = (target - transform.position).normalized;
        if (faceTarget)
        {
            float directionToPlayer = player.position.x - transform.position.x;
            Flip(directionToPlayer);
        }
    }

    void ApplyMovement()
    {
        if (isDashing) return;

        if (currentState != AIState.Combat && moveDirection.x != 0) { Flip(moveDirection.x); }
        rb.velocity = new Vector2(moveDirection.x * stats.speed, rb.velocity.y);
    }

    void Flip(float directionX)
    {
        if (directionX > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (directionX < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void UpdateAnimator()
    {
        float horizontalMove = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("hMove", horizontalMove);

        bool isIdle = isGrounded && horizontalMove <= 0.01f && !isDashing;
        animator.SetBool("isIdle", isIdle);

        animator.SetBool("isGrounded", isGrounded);
    }

    void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    private bool IsNearLedge()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(direction * ledgeCheckDistance, 0);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, groundRaycastDistance, groundLayer);
        return hit.collider == null;
    }


    private void SwitchState(AIState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        ResetActionTimer();
        if (newState != AIState.Patrolling)
        {
            isWaiting = false;
        }
    }

    private void ResetActionTimer()
    {
        lastActionTimer = 0f;
    }

    private bool IsPlayerInDetectionRange()
    {
        return player != null && suspicionZone != null && suspicionZone.bounds.Contains(player.position);
    }

    private bool IsPlayerInCombatRange()
    {
        return player != null && Vector2.Distance(transform.position, player.position) < combatRange;
    }

    void OnDrawGizmosSelected()
    {
        if (suspicionZone != null) { Gizmos.color = Color.yellow; Gizmos.DrawWireCube(suspicionZone.bounds.center, suspicionZone.bounds.size); }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRaycastDistance);
        if (stats != null) { Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, stats.attackRange * 0.8f); }

        float direction = transform.localScale.x > 0 ? 1 : -1;
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(direction * ledgeCheckDistance, 0);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(raycastOrigin, raycastOrigin + Vector2.down * groundRaycastDistance);
    }

    #endregion
}