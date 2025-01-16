using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    // **Variables**
    Animator animator;
    public float attackRange = 2f; // Range of the attack
    public int damage = 10; // Damage dealt per attack
    public LayerMask enemyLayer; // Layer to detect enemies
    public Transform attackPoint; // Point from where the attack originates

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    // **Update Method**
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && this.gameObject.tag == "Player")
        {
            animator.SetTrigger("isAttack");
            PerformAttack();
        }
    }

    // **Attack Logic**
    void PerformAttack()
    {
        if (attackPoint == null) return;

        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        // Damage each enemy hit
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit: " + enemy.name);
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null) enemyHealth.TakeDamage(damage); // Assuming the enemy has a Health script
        }
    }

    // **Visualize Attack Range (Optional)**
    private void OnDrawGizmosSelected()
    {
        try
        {
            if (attackPoint == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error drawing gizmo: " + ex.Message);
        }
    }
}

