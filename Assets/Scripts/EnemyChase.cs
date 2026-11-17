using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public Transform player;

    public float detectionRadius = 10f;
    public float loseRadius = 30f;
    public float speed = 3f;
    public float turnSpeed = 5f;

    private bool chasing = false;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // Start chasing
        if (!chasing && distance <= detectionRadius)
        {
            chasing = true;
        }

        // Stop chasing
        if (chasing && distance >= loseRadius)
        {
            chasing = false;
        }

        Vector3 direction;

        if (chasing)
        {
            // Face the player
            direction = player.position - transform.position;

            direction.y = 0;

            RotateTowards(direction);

            // Move in the direction the cone is pointing
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        else
        {
            // Face away from the player
            direction = transform.position - player.position;

            direction.y = 0;

            RotateTowards(direction);
        }
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        CellularAutomataCave cave = FindFirstObjectByType<CellularAutomataCave>();

        if (cave != null)
        {
            cave.PlayerDied(other.gameObject);
        }
            
    }
}