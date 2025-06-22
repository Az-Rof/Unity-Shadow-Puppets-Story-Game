using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// CameraFollower.cs

public class CameraFollower : MonoBehaviour
{
    public Transform target; // Player
    public float smoothSpeed = 0.125f; // Speed smoothing againts player movement
    public Vector2 followOffset; // Offset camera and target
    public Vector2 deadZoneSize = new Vector2(2f, 1f); // Dead zone size for camera movement
    public float aheadLookDistance = 2f; // Distance in front of player that camera will look at

    public bool useBounds = false; // Use level boundaries?
    public Vector2 mapMinBounds, mapMaxBounds; // Minimum and maximum boundaries

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = CalculateTargetPosition();

        // Smooth camera movement
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothSpeed);

        // Apply boundaries if used
        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, mapMinBounds.x, mapMaxBounds.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, mapMinBounds.y, mapMaxBounds.y);
        }

        transform.position = smoothedPosition;
    }

    private Vector3 CalculateTargetPosition()
    {
        // Calculate target position considering dead zone and offset
        // Vector3 targetPosition = target.position + (Vector3)followOffset;
        Vector3 worldPosition = target.parent != null ? target.parent.TransformPoint(target.localPosition) : target.position;
        Vector3 targetPosition = worldPosition + (Vector3)followOffset;


        // Apply dead zone
        float deltaX = Mathf.Abs(targetPosition.x - transform.position.x);
        float deltaY = Mathf.Abs(targetPosition.y - transform.position.y);
        if (deltaX > deadZoneSize.x)
        {
            transform.position = new Vector3(targetPosition.x - (deltaX - deadZoneSize.x) * Mathf.Sign(targetPosition.x - transform.position.x), transform.position.y, transform.position.z);
        }

        if (deltaY > deadZoneSize.y)
        {
            transform.position = new Vector3(transform.position.x, targetPosition.y - (deltaY - deadZoneSize.y) * Mathf.Sign(targetPosition.y - transform.position.y), transform.position.z);
        }

        // Add a bit of lookahead in the direction of the player's movement (optional)
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetPosition.x += Mathf.Sign(targetRb.velocity.x) * aheadLookDistance;
        }

        targetPosition.z = transform.position.z; // Keep camera Z position
        return targetPosition;
    }

    // Optional function to draw boundaries (debug)
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3((mapMinBounds.x + mapMaxBounds.x) / 2, (mapMinBounds.y + mapMaxBounds.y) / 2, 0), new Vector3(mapMaxBounds.x - mapMinBounds.x, mapMaxBounds.y - mapMinBounds.y, 0));
        }
    }
}
