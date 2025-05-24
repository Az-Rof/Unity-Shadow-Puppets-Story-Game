using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    // **Variables**
    CharacterStats characterStats; // Reference to CharacterStats script
    Animator animator;

    [SerializeField] float attackRange = 2f; 
    private int damage; // Damage dealt by the attack
    public LayerMask enemyLayer; // Layer to detect enemies

    private void Start()
    {
        // Initialize components
        animator = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        // Initialize attack power based on character stats
        InitiateAttack();
    }



    // **Update Method**
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && this.gameObject.tag == "Player")
        {
            // PerformAttack();
        }
        
    }



    // **Initialize Attack Power**
    void InitiateAttack()
    {
        // Initialize attack power based on character stats
        damage = (int)characterStats.attackPower; // Assuming attackPower is a float in CharacterStats
    }
}