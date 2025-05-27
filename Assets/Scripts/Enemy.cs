using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Patrol settings
    [Header("Patrol")]
    public Transform[] waypoints; // Array to store patrol points
    private int currentWaypoint = 0; // Index of the current waypoint
    private float idleTimer = 0f; // Timer for idle behavior
    private float idleDuration = 3f; // Duration of idle time
    public bool isIdle = true; // Status of whether the enemy is idle
    private GameObject player; // Reference to the player object

    // Importing character stats
    CharacterStats stats;
    public CharacterStats CharacterStats
    {
        get { return stats; }
        set { stats = value; }
    }

    Rigidbody2D rb;
    private TrailRenderer tr;

    [Header("Enemy AI")]
    public float suspicionLevel;
    public bool isSuspicious;
    private bool wasSuspicious = false;
    public float suspicionThreshold = 3f;
    public BoxCollider2D suspicionZone;
    Vector2 lastKnownPlayerPosition;

    // Attack variables
    [Header("Combat")]
    private float lastAttackTime = 0f;
    private float lastDashTime = -10f;
    private float lastJumpTime = -10f;
    private bool isActionInProgress = false;

    void getStats()
    {
        stats = GetComponent<CharacterStats>();
        if (tr != null)
        {
            tr = GetComponent<TrailRenderer>();
        }

        if (stats == null)
        {
            Debug.LogError("CharacterStats component not found on this GameObject.");
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Initialize CharacterStats
        getStats();
        player = GameObject.FindGameObjectWithTag("Player"); // Find the player object

        if (waypoints.Length == 0)
        {
            Debug.LogWarning(gameObject.name + ": No waypoints assigned for patrol!");
        }

        if (suspicionZone == null)
        {
            suspicionZone = GetComponent<BoxCollider2D>();
            if (suspicionZone == null)
            {
                Debug.LogError(gameObject.name + ": No suspicion zone assigned and no BoxCollider2D found!");
            }
        }
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
        }

        if (!isActionInProgress)
        {
            Sus();
            CheckForPlayer();
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        if (isIdle)
        {
            idleTimer += Time.deltaTime; // Increment idle timer

            // If idle duration has been reached
            if (idleTimer >= idleDuration)
            {
                isIdle = false; // Set enemy to not idle
                idleTimer = 0f; // Reset idle timer
            }
        }
        else
        {
            // Get the target position of the current waypoint
            Vector2 targetPosition = waypoints[currentWaypoint].position;

            // Move the enemy towards the target position
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, stats.speed * Time.deltaTime);

            // Flip based on movement direction
            transform.localScale = new Vector3(
                 (targetPosition.x < transform.position.x) ? -1 : 1,
                 transform.localScale.y,
                 transform.localScale.z
             );

            // If the enemy has reached the target position
            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                isIdle = true; // Set enemy to idle
                currentWaypoint++; // Move to the next waypoint

                // If the last waypoint has been reached, go back to the first
                if (currentWaypoint >= waypoints.Length)
                {
                    currentWaypoint = 0;
                }
            }
        }
    }

    void CheckForPlayer()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            // Check if the player is within attack range
            if (distanceToPlayer < stats.attackRange)
            {
                // Face the player
                transform.localScale = new Vector3(
                    (player.transform.position.x < transform.position.x) ? -1 : 1,
                    transform.localScale.y,
                    transform.localScale.z
                );

                // Execute AI actions
                AI();
                Debug.Log(gameObject.name + " AI is running!");
            }
        }
    }

    void Sus()
    {
        if (player != null)
        {
            // Increase suspicion level if player is in the suspicion zone
            if (suspicionZone.bounds.Contains(player.transform.position))
            {
                suspicionLevel += Time.deltaTime;
                lastKnownPlayerPosition = player.transform.position;
            }
            else
            {
                // Decrease suspicion level if player is not in the suspicion zone
                suspicionLevel -= Time.deltaTime;
            }

            // Clamp suspicion level between 0 and suspicionThreshold
            suspicionLevel = Mathf.Clamp(suspicionLevel, 0, suspicionThreshold);

            // NPC becomes suspicious if suspicion level reaches the threshold
            isSuspicious = suspicionLevel >= suspicionThreshold;

            // If NPC is suspicious, move to the last known player position
            if (isSuspicious)
            {
                Vector2 direction = new Vector2(lastKnownPlayerPosition.x - transform.position.x, lastKnownPlayerPosition.y - transform.position.y).normalized;
                rb.velocity = direction * stats.speed * 1.15f;
                transform.localScale = new Vector3(
                    (lastKnownPlayerPosition.x < transform.position.x) ? -1 : 1,
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else
            {
                if (!isActionInProgress)
                {
                    if (wasSuspicious && waypoints.Length > 0)
                    {
                        currentWaypoint = Random.Range(0, waypoints.Length);
                    }
                    Patrol();
                }
            }
        }
        wasSuspicious = isSuspicious;
    }

    void AI()
    {
        if (isActionInProgress) return;

        float currentTime = Time.time;
        bool canDash = currentTime >= lastDashTime + stats.dashCooldown;
        bool canJump = currentTime >= lastJumpTime + stats.jumpCooldown;

        // Build a list of available actions
        List<int> availableActions = new List<int>();

        // Melee attack is always available
        availableActions.Add(0);

        // Add dash if it's available
        if (canDash) availableActions.Add(1);

        // Add jump if it's available
        if (canJump) availableActions.Add(2);

        // Choose a random action from available ones
        int randomAction = availableActions[Random.Range(0, availableActions.Count)];

        switch (randomAction)
        {
            case 0:
                if (stats.GetActionCost("Attack") <= stats.currentStamina)
                {
                    MeleeAttack();
                }
                break;
            case 1:
                if (stats.GetActionCost("Dash") <= stats.currentStamina)
                {
                    StartCoroutine(Dash());
                    lastDashTime = currentTime;
                }
                break;
            case 2:
                if (stats.GetActionCost("Jump") <= stats.currentStamina)
                {
                    StartCoroutine(Jump());
                    lastJumpTime = currentTime;
                }
                break;
        }
    }
    void MeleeAttack()
    {
        // Check if enough time has passed since the last attack
        if (Time.time >= lastAttackTime + stats.attackCooldown)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                try
                {
                    AudioManager.Instance.PlaySFX("AttackedPlayer");
                }
                catch (System.Exception)
                {
                    Debug.LogWarning("AudioManager.Instance not found or PlaySFX method failed.");
                }

                playerController.TakeDamage((int)stats.attackPower); // Call the TakeDamage method on the player
                lastAttackTime = Time.time; // Update the last attack time
                                            // Debug.Log(gameObject.name + " attacked the player for " + (int)stats.attackPower + " damage.");
                stats.TakeAction(stats.GetActionCost("Attack"));
            }
        }
    }

    IEnumerator Dash()
    {
        isActionInProgress = true;
        Vector2 dashDirection = (player.transform.position - transform.position).normalized;
        rb.velocity = dashDirection * stats.dashPower;
        try
        {
            AudioManager.Instance.PlaySFX("Dash");
        }
        catch (System.Exception)
        {
            Debug.LogWarning("AudioManager.Instance not found or PlaySFX method failed.");
        }

        // Debug.Log(gameObject.name + " is dashing towards player!");

        yield return new WaitForSeconds(0.3f); // Dash duration
        rb.velocity = Vector2.zero;
        isActionInProgress = false;
        stats.TakeAction(stats.GetActionCost("Dash"));
    }

    IEnumerator Jump()
    {
        isActionInProgress = true;
        Vector2 jumpDirection = new Vector2((player.transform.position.x - transform.position.x), 1).normalized;
        rb.velocity = new Vector2(jumpDirection.x * stats.speed, stats.jumpPower);

        try
        {
            AudioManager.Instance.PlaySFX("Jump");
        }
        catch (System.Exception)
        {
            Debug.LogWarning("AudioManager.Instance not found or PlaySFX method failed.");
        }

        // Debug.Log(gameObject.name + " is performing JUMP!");

        yield return new WaitForSeconds(0.5f); // Wait mid-air
        rb.velocity = new Vector2(rb.velocity.x, -stats.jumpPower); // Simulate coming down faster

        yield return new WaitForSeconds(0.3f); // Wait for landing
        rb.velocity = Vector2.zero;
        isActionInProgress = false;
        stats.TakeAction(stats.GetActionCost("Jump"));
    }

    // Optional: Add visual debugging
    void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);

        // Draw patrol waypoints if assigned
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach (Transform waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawSphere(waypoint.position, 0.2f);
                }
            }

            // Draw lines between waypoints
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }

            // Close the loop
            if (waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
            {
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
            }
        }
    }
    // Method to take damage from the player (implemented in CharacterStats)
    // This method will be called when the player attacks the enemy
    public void TakeDamage(int damage)
    {
        stats.TakeDamage(damage); // Lanjut ke karakter stats
    }
}