using System.Collections;
using UnityEngine;

public class JathayuScript : MonoBehaviour
{
    public enum BossState { Idle, Attacking, TakingDamage, SpecialMove }
    public BossState currentState = BossState.Idle;

    // Movement
    public float moveSpeed = 3f;
    public float chaseDistance = 10f;
    private Transform player;
    private Rigidbody2D rb;

    // Attack
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    private float lastAttackTime;
    public int attackDamage = 10;

    // Health
    public int maxHealth = 500;
    [SerializeField] private int currentHealth;
    [SerializeField] private int phase = 1;

    // Phase-specific attributes
    public GameObject projectilePrefab;
    public GameObject minionPrefab;
    public float phase2MoveSpeedMultiplier = 1.5f;
    public float phase3DamageMultiplier = 2f;
    public float shieldDuration = 3f;
    private bool isShieldActive = false;

    // References
    private Animator animator;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState();
                break;
            case BossState.Attacking:
                HandleAttackState();
                break;
            case BossState.TakingDamage:
                HandleDamageState();
                break;
            case BossState.SpecialMove:
                HandleSpecialMoveState();
                break;
        }

        CheckPhaseTransition();
    }

    void HandleIdleState()
    {
        if (Vector2.Distance(transform.position, player.position) < chaseDistance)
        {
            currentState = BossState.Attacking;
        }
    }

    void HandleAttackState()
    {
        // Move towards player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        // Attack if in range
        if (Vector2.Distance(transform.position, player.position) < attackRange &&
            Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        // Deal damage to player
        player.GetComponent<PlayerController>().TakeDamage(attackDamage);
        yield return new WaitForSeconds(0.5f);
    }

    void HandleDamageState()
    {
        // Play damage animation
        animator.SetTrigger("TakeDamage");
        currentState = BossState.Attacking;
    }

    void HandleSpecialMoveState()
    {
        // Implement special move based on phase
        switch (phase)
        {
            case 2:
                StartCoroutine(Phase2SpecialMove());
                break;
            case 3:
                StartCoroutine(Phase3SpecialMove());
                break;
        }
    }

    IEnumerator Phase2SpecialMove()
    {
        // Ranged projectile attack
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - transform.position).normalized;
        projectile.GetComponent<Rigidbody2D>().velocity = direction * 10f;

        yield return new WaitForSeconds(1f);
        currentState = BossState.Attacking;
    }

    IEnumerator Phase3SpecialMove()
    {
        // Summon minions and area attack
        for (int i = 0; i < 3; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + Random.insideUnitCircle * 2f;
            Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        }

        animator.SetTrigger("SpecialAttack");
        yield return new WaitForSeconds(2f);
        currentState = BossState.Attacking;
    }

    void CheckPhaseTransition()
    {
        if (phase == 1 && currentHealth <= maxHealth * 0.66f)
        {
            phase = 2;
            moveSpeed *= phase2MoveSpeedMultiplier;
            StartCoroutine(ActivateShield());
        }
        else if (phase == 2 && currentHealth <= maxHealth * 0.33f)
        {
            phase = 3;
            attackDamage = (int)(attackDamage * phase3DamageMultiplier);
        }
    }

    IEnumerator ActivateShield()
    {
        isShieldActive = true;
        yield return new WaitForSeconds(shieldDuration);
        isShieldActive = false;
    }

    void Die()
    {
        // Play death animation
        animator.SetTrigger("Die");
        // Disable boss
        this.enabled = false;
    }
    public void TakeDamage(int damage)
    {
        if (!isShieldActive)
        {
            currentHealth -= damage;
            currentState = BossState.TakingDamage;

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                CheckPhaseTransition(); // Panggil ini setiap kali menerima kerusakan
            }
        }
    }
}
