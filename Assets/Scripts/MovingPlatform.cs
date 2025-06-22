using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    Rigidbody2D rb;
    BoxCollider2D trigger;

    [Header("Platform Movement Settings")]
    public Transform[] waypoints;
    int currentWaypointIndex = 0;

    [Header("Movement Speed")]
    [SerializeField] float moveSpeed = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trigger = GetComponent<BoxCollider2D>();

        // Ensure the Rigidbody2D is set to Kinematic for moving platforms
        rb.isKinematic = true;

        // Ensure the BoxCollider2D is set as a trigger
        trigger.isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Set the player as a child of the platform
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Remove the player from the platform's parent
            collision.transform.SetParent(null);
        }
    }
    // Move the platform between waypoints
    private void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target.position, step);

        // Check if the platform has reached the target waypoint
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}