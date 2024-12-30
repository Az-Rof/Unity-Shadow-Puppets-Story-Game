using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Traps : MonoBehaviour
{
     public Transform[] waypoints; // Array untuk menyimpan titik-titik tujuan
     public int currentWaypoint = 0; // Indeks titik tujuan saat ini
     private float idleTimer = 0f; // Penghitung waktu untuk perilaku idle
     private float idleDuration = 3f; // Durasi idle dalam detik
     public bool isIdle = true; // Status apakah sedang idle atau tidak
     private float speed = 2f; // Kecepatan pergerakan


     PlayerLivesManager playerLivesManager;
     void Start()
     {

     }
     void OnTriggerEnter2D(Collider2D collision)
     {
          GameObject player = GameObject.FindWithTag("Player");
          if (player != null && collision.CompareTag("Player"))
          {
               playerLivesManager = player.GetComponent<PlayerLivesManager>();
               if (collision.gameObject.tag == "Player")
               {
                    playerLivesManager.ReduceLives();
               }
          }
     }

     void patrolling()
     {
          // Jika NPC sedang idle
          if (isIdle)
          {
               idleTimer += Time.deltaTime; // Tambahkan waktu ke penghitung idle

               // Jika durasi idle telah tercapai
               if (idleTimer >= idleDuration)
               {
                    isIdle = false; // Atur status NPC menjadi tidak idle
                    idleTimer = 0f; // Reset penghitung idle
               }
               GetComponent<SpriteRenderer>().flipX = false;
          }
          else if (!isIdle) // Jika NPC tidak idle
          {
               // Dapatkan posisi titik tujuan saat ini
               Vector2 targetPosition = waypoints[currentWaypoint].position;

               // Hitung arah pergerakan NPC
               Vector2 direction = targetPosition - (Vector2)transform.position;

               // Ubah arah menghadap NPC berdasarkan arah pergerakan
               if (direction.x > 0)
               {
                    GetComponent<SpriteRenderer>().flipX = false;// Menghadap ke kanan
               }
               else if (direction.x < 0)
               {
                    GetComponent<SpriteRenderer>().flipX = true;// Menghadap ke kiri
               }
               // Gerakkan NPC menuju titik tujuan
               transform.position = new Vector2(Mathf.MoveTowards(transform.position.x, targetPosition.x, speed * Time.deltaTime), transform.position.y);
               //GetComponent<Rigidbody2D>().velocity = direction * speed;


               // Jika NPC telah mencapai titik tujuan
               if (Vector2.Distance(transform.position, targetPosition) < 1f)
               {
                    isIdle = true; // Atur status NPC menjadi idle
                    currentWaypoint++; // Pindah ke titik tujuan berikutnya

                    // Jika telah mencapai titik tujuan terakhir, kembali ke titik awal
                    if (currentWaypoint >= waypoints.Length)
                    {
                         currentWaypoint = 0;
                    }
               }
          }
     }
}
