using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasLanded = false;
    private bool isMoving = false;
    private bool hasDropped = false;
    private float moveDirection = 1f;
    private float moveRange = 2.5f;
    private float moveSpeed = 2f;
    private Vector3 startPos;
    private BlockManager manager;
    private bool isFirstBlock = false;

    private static int blockCount = 0; // Track how many blocks have landed
    private GameManagerUI uiManager;   // Reference to UI manager

    public void Initialize(BlockManager blockManager, bool autoDrop = false)
    {
        manager = blockManager;
        rb = GetComponent<Rigidbody2D>();
        uiManager = FindObjectOfType<GameManagerUI>(); // ? Automatically find UI manager

        if (rb == null)
        {
            Debug.LogError("Block missing Rigidbody2D!");
            return;
        }

        startPos = transform.position;
        isFirstBlock = autoDrop;

        if (autoDrop)
        {
            // First block falls automatically
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            Debug.Log("First block auto-dropped.");
        }
        else
        {
            // Subsequent blocks move left-right before drop
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            isMoving = true;

            Debug.Log("New moving block spawned — waiting for player input to drop.");
        }
    }

    private void Update()
    {
        if (isMoving && !hasDropped)
        {
            transform.position += Vector3.right * moveDirection * moveSpeed * Time.deltaTime;

            if (Mathf.Abs(transform.position.x - startPos.x) >= moveRange)
                moveDirection *= -1;

            if (Input.GetMouseButtonDown(0))
                DropBlock();
        }
    }

    private void DropBlock()
    {
        hasDropped = true;
        isMoving = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        Debug.Log("Block dropped by player.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // The main condition (not landed yet, and collided with Ground or Block) remains the same
        if (!hasLanded && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Block")))
        {
            hasLanded = true;
            blockCount++;

            Debug.Log($"Block {blockCount} landed.");

            
            if (blockCount > 1 && collision.gameObject.CompareTag("Ground"))
            {
                if (uiManager != null)
                {
                    uiManager.ShowGameOver();
                    Debug.Log("Game Over triggered: Block landed directly on Ground.");
                }
                else
                {
                    Debug.LogError("UI Manager not found in scene!");
                }
            }

            // Inform the manager that a block landed (still needed for spawning logic)
            manager.OnBlockLanded(this.transform);
        }
    }
}
