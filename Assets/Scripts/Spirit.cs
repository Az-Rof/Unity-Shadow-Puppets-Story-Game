using UnityEngine;

public class SpiritFollower : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;
    public float rotationSpeed = 50f;
    public float orbitRadius = 2f;
    public float behindDistance = 3f;

    private float orbitAngle;

    void Update()
    {
        if (player == null) return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        bool isMoving = rb != null && rb.velocity.magnitude > 0.1f;

        if (isMoving)
        {
            FollowBehindPlayer(rb);
        }
        else
        {
            OrbitAroundPlayer();
        }
    }

    void FollowBehindPlayer(Rigidbody2D rb)
    {
        Vector2 moveDirection = rb.velocity.normalized;
        Vector3 targetPosition = player.position - (Vector3)(moveDirection * behindDistance);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Rotasi menghadap player
        Vector3 directionToPlayer = player.position - transform.position;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OrbitAroundPlayer()
    {
        orbitAngle += rotationSpeed * Time.deltaTime;
        float radians = orbitAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * orbitRadius;
        Vector3 targetPosition = player.position + offset;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Rotasi menghadap arah gerakan
        Vector3 direction = targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
