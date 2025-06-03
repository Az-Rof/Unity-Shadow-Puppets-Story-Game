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
    [SerializeField] private float suspicionThreshold = 3f;
    [SerializeField] private BoxCollider2D suspicionZone;
    
    [Header("Combat")]
    [SerializeField] private LayerMask playerLayerMask = -1;
    #endregion

    #region Private Fields
    // Core Components
    private CharacterStats stats;
    private Rigidbody2D rb;
    private TrailRenderer tr;
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

    // Cached Values
    private Vector3 originalScale;
    private WaitForSeconds dashDuration;
    private WaitForSeconds jumpMidAir;
    private WaitForSeconds jumpLanding;

    // Constants
    private const float WAYPOINT_REACH_DISTANCE = 0.1f;
    private const float SUSPICION_MOVE_SPEED_MULTIPLIER = 1.15f;
    private const float DASH_DURATION = 0.3f;
    private const float JUMP_MID_AIR_TIME = 0.5f;
    private const float JUMP_LANDING_TIME = 0.3f;
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
    }

    private void Start()
    {
        InitializeEnemy();
        ValidateSetup();
    }

    private void Update()
    {
        if (!EnsurePlayerReference()) return;

        UpdateStateMachine();
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

        PlaySFX("AttackedPlayer");
        playerController.TakeDamage((int)stats.attackPower);
        
        lastAttackTime = Time.time;
        stats.TakeAction(stats.GetActionCost("Attack"));
    }

    private IEnumerator PerformDash()
    {
        isActionInProgress = true;
        
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
        UnityEditor.Handles.Label(textPosition, $"State: {currentState}\nSuspicion: {suspicionLevel:F1}");
        #endif
    }
    #endregion
}