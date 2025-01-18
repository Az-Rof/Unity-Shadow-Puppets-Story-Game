using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100; // Maximum health of the enemy
    private int currentHealth; // Current health of the enemy
    public float patrolSpeed = 2f; // Speed of the enemy while patrolling
    public Transform[] waypoints; // Array to store patrol points
    private int currentWaypoint = 0; // Index of the current waypoint
    private float idleTimer = 0f; // Timer for idle behavior
    private float idleDuration = 3f; // Duration of idle time
    public bool isIdle = true; // Status of whether the enemy is idle
    public float attackRange = 1.5f; // Range within which the enemy can attack the player
    public int damage = 10; // Damage dealt to the player
    private GameObject player; // Reference to the player object
    private float attackCooldown = 1f; // Cooldown time between attacks
    private float lastAttackTime; // Time of the last attack

    public float suspicionLevel;
    public bool isSuspicious;
    private bool wasSuspicious = false;
    public float suspicionThreshold = 3f;
    public BoxCollider2D suspicionZone;
    Vector2 lastKnownPlayerPosition;
    public float speed = 3.0f;

    void Start()
    {
        currentHealth = maxHealth; // Initialize current health
        player = GameObject.FindGameObjectWithTag("Player"); // Find the player object
    }

    void Update()
    {
        Sus();
        CheckForPlayer();
    }

    void Patrol()
    {
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
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);

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
            // Check if the player is within attack range
            if (Vector2.Distance(transform.position, player.transform.position) < attackRange)
            {
                Attack();
            }
        }
    }

    void Sus()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
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
                GetComponent<Rigidbody2D>().velocity = direction * speed * 1.15f;
                GetComponent<SpriteRenderer>().flipX = direction.x < 0;
            }
            else
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                if (wasSuspicious)
                {
                    currentWaypoint = UnityEngine.Random.Range(0, waypoints.Length);
                }
                Patrol();
            }
        }
        wasSuspicious = isSuspicious;
    }

    void Attack()
    {
        // Check if enough time has passed since the last attack
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                AudioManager.Instance.PlaySFX("AttackedPlayer");
                playerController.TakeDamage(damage); // Call the TakeDamage method on the player
                lastAttackTime = Time.time; // Update the last attack time
                Debug.Log(gameObject.name + " attacked the player for " + damage + " damage."); // Debug log for attack
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // Reduce health by the damage amount
        if (currentHealth <= 0)
        {
            Die(); // Call the Die method if health drops to 0 or below
            AudioManager.Instance.PlaySFX("EnemyDeath"); // Play enemy death sound effect
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject); // Destroy the enemy game object
    }
}
