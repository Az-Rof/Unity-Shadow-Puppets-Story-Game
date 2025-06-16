using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Serialized Fields
    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float idleDuration = 3f;

    [Header("Enemy AI")]
    [SerializeField] private float suspicionThreshold = 1.5f;
    [SerializeField] private BoxCollider2D suspicionZone;

    [Header("Combat")]
    [SerializeField] private LayerMask playerLayerMask = -1;
    #endregion

    #region Private Fields
    // Core Components
    private CharacterStats stats;
    private Rigidbody2D rb;
    private TrailRenderer tr;
    private Animator animator;
    private Transform playerTransform;
    private PlayerController playerController;

    // Patrol State
    private int currentWaypoint = 0;
    private float idleTimer = 0f;
    private bool isIdle = true;

    // AI State
    private EnemyState currentState = EnemyState.Patrolling;
    private float suspicionLevel = 0f;
    private bool isSuspicious = false;
    private bool wasSuspicious = false;
    private Vector2 lastKnownPlayerPosition;

    // Combat State
    private float lastAttackTime = 0f;
    private float lastDashTime = -10f;
    private float lastJumpTime = -10f;
    private bool isActionInProgress = false;

    // Animation State
    private bool isWalking = false;
    private bool isGrounded = true;

    // Cached Values
    private Vector3 originalScale;
    private WaitForSeconds dashDuration;
    private WaitForSeconds jumpMidAir;
    private WaitForSeconds jumpLanding;

    // Animation Hash IDs for performance
    private int hashIsWalking;
    private int hashIsIdle;
    private int hashAttack;
    private int hashDash;
    private int hashJump;
    private int hashIsGrounded;
    private int hashSpeed;

    // Constants
    private const float WAYPOINT_REACH_DISTANCE = 0.1f;
    private const float SUSPICION_MOVE_SPEED_MULTIPLIER = 1.15f;
    private const float DASH_DURATION = 0.3f;
    private const float JUMP_MID_AIR_TIME = 0.5f;
    private const float JUMP_LANDING_TIME = 0.3f;
    private const float GROUND_CHECK_DISTANCE = 0.25f;
    #endregion

    #region Enums
    public enum EnemyState
    {
        Patrolling,
        Suspicious,
        Combat,
        ActionInProgress
    }

    private enum CombatAction
    {
        Attack = 0,
        Dash = 1,
        Jump = 2
    }
    #endregion

    #region Properties
    public CharacterStats Stats
    {
        get => stats;
        set => stats = value;
    }

    public bool IsActionInProgress => isActionInProgress;
    public EnemyState CurrentState => currentState;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponents();
        CacheWaitForSeconds();
        CacheAnimationHashes();
    }

    private void Start()
    {
        InitializeEnemy();
        ValidateSetup();
    }

    private void Update()
    {
        if (!EnsurePlayerReference()) return;

        UpdateGroundCheck();
        UpdateStateMachine();
        UpdateAnimations();
    }

    private void OnDrawGizmosSelected()
    {
        DrawDebugGizmos();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
        tr = GetComponent<TrailRenderer>();
        animator = GetComponent<Animator>();

        if (suspicionZone == null)
            suspicionZone = GetComponent<BoxCollider2D>();

        originalScale = transform.localScale;
    }

    private void CacheWaitForSeconds()
    {
        dashDuration = new WaitForSeconds(DASH_DURATION);
        jumpMidAir = new WaitForSeconds(JUMP_MID_AIR_TIME);
        jumpLanding = new WaitForSeconds(JUMP_LANDING_TIME);
    }

    private void CacheAnimationHashes()
    {
        hashIsWalking = Animator.StringToHash("IsWalking");
        hashIsIdle = Animator.StringToHash("IsIdle");
        hashAttack = Animator.StringToHash("Attack");
        hashDash = Animator.StringToHash("Dash");
        hashJump = Animator.StringToHash("Jump");
        hashIsGrounded = Animator.StringToHash("IsGrounded");
        hashSpeed = Animator.StringToHash("Speed");
    }

    private void InitializeEnemy()
    {
        FindPlayerReference();
        currentState = waypoints.Length > 0 ? EnemyState.Patrolling : EnemyState.Combat;
    }

    private void ValidateSetup()
    {
        if (stats == null)
            Debug.LogError($"{name}: CharacterStats component not found!");

        if (waypoints.Length == 0)
            Debug.LogWarning($"{name}: No waypoints assigned for patrol!");

        if (suspicionZone == null)
            Debug.LogError($"{name}: No suspicion zone assigned and no BoxCollider2D found!");

        if (animator == null)
            Debug.LogError($"{name}: Animator component not found!");
    }
    #endregion

    #region Player Reference Management
    private bool EnsurePlayerReference()
    {
        if (playerTransform == null)
        {
            FindPlayerReference();
        }
        return playerTransform != null;
    }

    private void FindPlayerReference()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
        }
    }
    #endregion

    #region Ground Check
    private void UpdateGroundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, GROUND_CHECK_DISTANCE);
        isGrounded = hit.collider != null;
    }
    #endregion

    #region Animation System
    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Update basic movement parameters
        float currentSpeed = rb.velocity.magnitude;
        animator.SetFloat(hashSpeed, currentSpeed);
        animator.SetBool(hashIsGrounded, isGrounded);

        // Update walking state based on movement and current state
        bool shouldBeWalking = (currentSpeed > 0.1f) &&
                              (currentState == EnemyState.Patrolling || currentState == EnemyState.Suspicious) &&
                              !isActionInProgress;

        if (isWalking != shouldBeWalking)
        {
            isWalking = shouldBeWalking;
            animator.SetBool(hashIsWalking, isWalking);
        }

        // Update idle state
        bool shouldBeIdle = currentSpeed < 0.1f &&
                           !isActionInProgress &&
                           isGrounded &&
                           (currentState == EnemyState.Patrolling || currentState == EnemyState.Suspicious);

        animator.SetBool(hashIsIdle, shouldBeIdle);

        // Debug animation state
        if (Application.isEditor)
        {
            DebugAnimationState();
        }
    }

    private void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(hashAttack);
        }
    }

    private void TriggerDashAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(hashDash);
        }
    }

    private void TriggerJumpAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(hashJump);
        }
    }

    private void DebugAnimationState()
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // Uncomment for debugging
            // Debug.Log($"Current Animation: {stateInfo.shortNameHash}, Speed: {rb.velocity.magnitude}, Walking: {isWalking}");
        }
    }
    #endregion

    #region State Machine
    private void UpdateStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolling();
                break;
            case EnemyState.Suspicious:
                HandleSuspicious();
                break;
            case EnemyState.Combat:
                HandleCombat();
                break;
            case EnemyState.ActionInProgress:
                // Wait for action to complete
                break;
        }

        UpdateSuspicion();
        CheckForStateTransitions();
    }

    private void HandlePatrolling()
    {
        if (waypoints.Length == 0) return;

        if (isIdle)
        {
            UpdateIdleTimer();
        }
        else
        {
            MoveToWaypoint();
        }
    }

    private void HandleSuspicious()
    {
        MoveToLastKnownPosition();
    }

    private void HandleCombat()
    {
        FacePlayer();
        ExecuteAI();
    }

    private void CheckForStateTransitions()
    {
        if (isActionInProgress)
        {
            currentState = EnemyState.ActionInProgress;
            return;
        }

        float distanceToPlayer = GetDistanceToPlayer();

        // Transition to combat if player is in attack range
        if (distanceToPlayer <= stats.attackRange)
        {
            currentState = EnemyState.Combat;
        }
        // Transition to suspicious if suspicion threshold is met
        else if (isSuspicious)
        {
            currentState = EnemyState.Suspicious;
        }
        // Return to patrolling
        else
        {
            currentState = EnemyState.Patrolling;

            // Reset waypoint if just became unsuspicious
            if (wasSuspicious && waypoints.Length > 0)
            {
                currentWaypoint = Random.Range(0, waypoints.Length);
            }
        }
    }
    #endregion

    #region Patrol Behavior
    private void UpdateIdleTimer()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            isIdle = false;
            idleTimer = 0f;
        }
    }

    private void MoveToWaypoint()
    {
        Vector2 targetPosition = waypoints[currentWaypoint].position;

        // Move towards waypoint
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            stats.speed * Time.deltaTime
        );

        // Face movement direction
        FaceDirection(targetPosition.x - transform.position.x);

        // Check if reached waypoint
        if (Vector2.Distance(transform.position, targetPosition) < WAYPOINT_REACH_DISTANCE)
        {
            ReachWaypoint();
        }
    }

    private void ReachWaypoint()
    {
        isIdle = true;
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }
    #endregion

    #region Suspicion System
    private void UpdateSuspicion()
    {
        if (playerTransform == null) return;

        bool playerInZone = IsPlayerInSuspicionZone();

        if (playerInZone)
        {
            suspicionLevel += Time.deltaTime;
            lastKnownPlayerPosition = playerTransform.position;
        }
        else
        {
            suspicionLevel -= Time.deltaTime;
        }

        suspicionLevel = Mathf.Clamp(suspicionLevel, 0, suspicionThreshold);

        wasSuspicious = isSuspicious;
        isSuspicious = suspicionLevel >= suspicionThreshold;
    }

    private bool IsPlayerInSuspicionZone()
    {
        return suspicionZone != null &&
               suspicionZone.bounds.Contains(playerTransform.position);
    }

    private void MoveToLastKnownPosition()
    {
        Vector2 direction = (lastKnownPlayerPosition - (Vector2)transform.position).normalized;
        Vector2 newVelocity = new Vector2(
            direction.x * stats.speed * SUSPICION_MOVE_SPEED_MULTIPLIER,
            rb.velocity.y
        );

        rb.velocity = newVelocity;
        FaceDirection(direction.x);
    }
    #endregion

    #region Combat System
    private void ExecuteAI()
    {
        if (isActionInProgress) return;

        List<CombatAction> availableActions = GetAvailableActions();
        if (availableActions.Count == 0) return;

        CombatAction selectedAction = availableActions[Random.Range(0, availableActions.Count)];
        ExecuteAction(selectedAction);
    }

    private List<CombatAction> GetAvailableActions()
    {
        List<CombatAction> actions = new List<CombatAction>();
        float currentTime = Time.time;

        // Melee attack is always available
        if (stats.GetActionCost("Attack") <= stats.currentStamina)
            actions.Add(CombatAction.Attack);

        // Dash if cooldown is ready
        if (currentTime >= lastDashTime + stats.dashCooldown &&
            stats.GetActionCost("Dash") <= stats.currentStamina)
            actions.Add(CombatAction.Dash);

        // Jump if cooldown is ready
        if (currentTime >= lastJumpTime + stats.jumpCooldown &&
            stats.GetActionCost("Jump") <= stats.currentStamina)
            actions.Add(CombatAction.Jump);

        return actions;
    }

    private void ExecuteAction(CombatAction action)
    {
        switch (action)
        {
            case CombatAction.Attack:
                MeleeAttack();
                break;
            case CombatAction.Dash:
                StartCoroutine(PerformDash());
                lastDashTime = Time.time;
                break;
            case CombatAction.Jump:
                StartCoroutine(PerformJump());
                lastJumpTime = Time.time;
                break;
        }
    }

    private void MeleeAttack()
    {
        if (Time.time < lastAttackTime + stats.attackCooldown) return;
        if (playerController == null) return;

        TriggerAttackAnimation();
        PlaySFX("AttackedPlayer");
        playerController.TakeDamage((int)stats.attackPower);

        lastAttackTime = Time.time;
        stats.TakeAction(stats.GetActionCost("Attack"));
    }

    private IEnumerator PerformDash()
    {
        isActionInProgress = true;

        TriggerDashAnimation();
        Vector2 dashDirection = GetDirectionToPlayer();
        rb.velocity = dashDirection * stats.dashPower;

        PlaySFX("Dash");

        yield return dashDuration;

        rb.velocity = Vector2.zero;
        stats.TakeAction(stats.GetActionCost("Dash"));
        isActionInProgress = false;
    }

    private IEnumerator PerformJump()
    {
        isActionInProgress = true;

        TriggerJumpAnimation();
        Vector2 jumpDirection = new Vector2(GetDirectionToPlayer().x, 1).normalized;
        rb.velocity = new Vector2(jumpDirection.x * stats.speed, stats.jumpPower);

        PlaySFX("Jump");

        yield return jumpMidAir;

        rb.velocity = new Vector2(rb.velocity.x, -stats.jumpPower);

        yield return jumpLanding;

        rb.velocity = Vector2.zero;
        stats.TakeAction(stats.GetActionCost("Jump"));
        isActionInProgress = false;
    }
    #endregion

    #region Utility Methods
    private float GetDistanceToPlayer()
    {
        return playerTransform != null ?
               Vector2.Distance(transform.position, playerTransform.position) :
               float.MaxValue;
    }

    private Vector2 GetDirectionToPlayer()
    {
        return playerTransform != null ?
               (playerTransform.position - transform.position).normalized :
               Vector2.zero;
    }

    private void FacePlayer()
    {
        if (playerTransform != null)
        {
            FaceDirection(playerTransform.position.x - transform.position.x);
        }
    }

    private void FaceDirection(float directionX)
    {
        float scaleX = directionX < 0 ? -1 : 1;
        transform.localScale = new Vector3(
            scaleX * Mathf.Abs(originalScale.x),
            originalScale.y,
            originalScale.z
        );
    }

    private void PlaySFX(string sfxName)
    {
        try
        {
            AudioManager.Instance?.PlaySFX(sfxName);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to play SFX '{sfxName}': {ex.Message}");
        }
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int damage)
    {
        stats?.TakeDamage(damage);
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypoint = 0;
    }

    public void SetSuspicionZone(BoxCollider2D newZone)
    {
        suspicionZone = newZone;
    }

    // Animation Event Methods (called from Animation Events)
    public void OnAttackHit()
    {
        // Called during attack animation at the moment of impact
        // Damage is already applied in MeleeAttack(), this is for additional effects
        Debug.Log("Attack hit registered!");
    }

    public void OnDashComplete()
    {
        // Called when dash animation completes
        Debug.Log("Dash animation complete!");
    }

    public void OnJumpLanding()
    {
        // Called when jump landing animation completes
        Debug.Log("Jump landing complete!");
    }
    #endregion

    #region Debug Visualization
    private void DrawDebugGizmos()
    {
        if (stats == null) return;

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);

        // Draw patrol waypoints
        DrawWaypointGizmos();

        // Draw suspicion zone
        if (suspicionZone != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(suspicionZone.bounds.center, suspicionZone.bounds.size);
        }

        // Draw ground check
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * GROUND_CHECK_DISTANCE);

        // Draw current state
        DrawStateGizmo();
    }

    private void DrawWaypointGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.blue;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawSphere(waypoint.position, 0.2f);
            }
        }

        // Draw patrol path
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[i] != null && waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }
    }

    private void DrawStateGizmo()
    {
        Vector3 textPosition = transform.position + Vector3.up * 2f;

#if UNITY_EDITOR
        UnityEditor.Handles.Label(textPosition, $"State: {currentState}\nSuspicion: {suspicionLevel:F1}\nWalking: {isWalking}\nGrounded: {isGrounded}");
#endif
    }
    #endregion
}