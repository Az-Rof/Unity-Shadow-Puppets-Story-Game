using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spirit : MonoBehaviour
{
    public Transform player; // Referensi ke player
    public float speed = 5.0f; // Kecepatan AI
    public float rotationSpeed = 2.0f; // Kecepatan rotasi AI
    public float distance = 2.0f; // Jarak AI dari player
    public float randomDistance = 5.0f; // Jarak random AI dari player

    private Vector3 offset; // Offset posisi AI dari player
    private float angle = 0; // Sudut AI dari player

    void Start()
    {
        // Inisialisasi offset posisi AI dari player
        offset = transform.position - player.position;
    }

    void Update()
    {
        // Periksa apakah player sedang bergerak
        if (player.GetComponent<Rigidbody2D>() != null && player.GetComponent<Rigidbody2D>().velocity.magnitude > 0.1f)
        {
            // Ikuti player dari belakang dengan mengelilingi player
            FollowPlayerFromBehind();
        }
        else
        {
            // Kelilingi player
            CircleAroundPlayer();
        }
    }

    void FollowPlayerFromBehind()
    {
        // Hitung posisi AI yang diinginkan
        float randomX = Random.Range(-randomDistance, randomDistance);
        float randomY = Random.Range(-randomDistance, randomDistance);
        Vector3 desiredPosition = player.position + new Vector3(randomX, randomY, 0);

        // Pindahkan AI ke posisi yang diinginkan
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, speed * Time.deltaTime);

        // Rotasikan AI untuk menghadap player dari belakang
        float angle = Mathf.Atan2(player.position.y - transform.position.y, player.position.x - transform.position.x);
        angle += 180; // tambahkan 180 derajat untuk menghadap player dari belakang
        transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
    }

    void CircleAroundPlayer()
    {
        // Hitung posisi AI yang diinginkan
        float randomX = Random.Range(-randomDistance, randomDistance);
        float randomY = Random.Range(-randomDistance, randomDistance);
        Vector3 desiredPosition = player.position + new Vector3(randomX, randomY, 0);

        // Pindahkan AI ke posisi yang diinginkan
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, speed * Time.deltaTime);

        // Rotasikan AI untuk mengelilingi player
        angle += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

}