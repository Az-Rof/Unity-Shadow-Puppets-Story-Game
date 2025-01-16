using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // **Variables**
    public float moveSpeed = 3f; // Kecepatan gerak musuh
    public float attackRange = 2f; // Jarak serangan musuh
    public int attackDamage = 10; // Damage yang diberikan musuh
    public float attackCooldown = 2f; // Cooldown antara serangan
    public LayerMask playerLayer; // Layer untuk mendeteksi pemain

    private Transform player; // Referensi ke pemain
    private Health playerHealth; // Referensi ke script Health pemain
    private float lastAttackTime; // Waktu terakhir musuh menyerang
    private Health enemyHealth; // Referensi ke script Health musuh

    // **Initialization**
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Cari pemain berdasarkan tag
        playerHealth = player.GetComponent<Health>(); // Ambil komponen Health pemain
        enemyHealth = GetComponent<Health>(); // Ambil komponen Health musuh
    }

    // **Update Method**
    void Update()
    {
        if (player == null || enemyHealth.currentHealth <= 0)
            return; // Hentikan jika pemain tidak ada atau musuh mati

        // Bergerak menuju pemain
        MoveTowardsPlayer();

        // Cek jarak ke pemain dan serang jika dalam jangkauan
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            AttackPlayer();
        }
    }

    // **Move Towards Player**
    void MoveTowardsPlayer()
    {
        // Hitung arah menuju pemain
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime; // Bergerak ke arah pemain

        // Putar musuh agar menghadap pemain (opsional)
        transform.LookAt(player);
    }

    // **Attack Player**
    void AttackPlayer()
    {
        // Cek cooldown serangan
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Debug.Log("Enemy attacked the player!");
            playerHealth.TakeDamage(attackDamage); // Berikan damage ke pemain
            lastAttackTime = Time.time; // Reset waktu serangan terakhir
        }
    }

    // **Die Method (Optional)**
    public void Die()
    {
        Debug.Log("Enemy has died.");
        Destroy(gameObject); // Hancurkan musuh
    }
}