using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasLanded = false;
    private bool isMoving = false;
    private BlockManager manager;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float moveRange = 2.5f;

    private Vector3 startPos;

    public void Initialize(BlockManager blockManager, bool autoDrop = false)
    {
        manager = blockManager;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Block missing Rigidbody2D!");
            return;
        }

        gameObject.tag = "Block";
        startPos = transform.position;

        // Set up the Rigidbody2D initially
        rb.isKinematic = true; // Start as kinematic to allow movement via transform
        rb.gravityScale = 0f;

        if (autoDrop)
        {
            // First block drops immediately without lateral movement
            rb.isKinematic = false;
            rb.gravityScale = 1f;
            isMoving = false;
        }
        else
        {
            // Subsequent blocks move laterally
            isMoving = true;
        }
    }

    void Update()
    {
        if (isMoving && !hasLanded)
        {
            // Horizontal movement
            float x = Mathf.PingPong(Time.time * moveSpeed, moveRange * 2) - moveRange;
            transform.position = new Vector3(startPos.x + x, transform.position.y, transform.position.z);
        }

        // Drop the block on touch/click
        if (isMoving && !hasLanded && Input.GetMouseButtonDown(0))
        {
            rb.isKinematic = false; // Disable kinematic so physics (gravity) takes over
            rb.gravityScale = 1f;
            isMoving = false; // Stop horizontal movement
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded) return;

        if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Block"))
        {
            hasLanded = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f; // Prevent rotation/tilting
            rb.isKinematic = true; // ? CRITICAL FIX: Lock the block in place to prevent physics jittering

            // Notify the manager
            manager.OnBlockLanded(gameObject);
        }
    }
}