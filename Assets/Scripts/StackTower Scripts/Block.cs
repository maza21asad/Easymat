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
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Block prefab is missing a Rigidbody2D component!");
            return;
        }

        // Set the block's Tag to "Block" upon initialization if it wasn't already set
        // This is crucial for collision detection with other blocks.
        gameObject.tag = "Block";

        manager = blockManager;
        startPos = transform.position;

        // Kinematic/Movement logic is correct: 
        // First block (autoDrop=true) falls immediately.
        // Subsequent blocks (autoDrop=false) move horizontally.
        rb.isKinematic = !autoDrop;
        isMoving = !autoDrop;
    }

    void Update()
    {
        // Horizontal movement logic
        if (isMoving && !hasLanded)
        {
            float x = Mathf.PingPong(Time.time * moveSpeed, moveRange * 2) - moveRange;
            transform.position = new Vector3(startPos.x + x, transform.position.y, transform.position.z);
        }

        // Drop initiation (on mouse click/touch)
        if (isMoving && !hasLanded && Input.GetMouseButtonDown(0))
        {
            isMoving = false;
            rb.isKinematic = false; // Start falling
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // If this block has already landed and is static, skip this.
        if (hasLanded) return;

        // Check for collision with the Ground or another landed Block
        if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Block"))
        {
            Debug.Log($"Block landed on {collision.gameObject.tag}.");
            hasLanded = true;

            // 1. Stop all motion and make the current block a static platform.
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            // 2. Inform the manager to spawn the next block using this block's position.
            manager.OnBlockLanded(this.gameObject);

           
        }
    }
}
