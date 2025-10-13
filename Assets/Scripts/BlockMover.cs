using UnityEngine;

/// <summary>
/// Controls the horizontal movement and dropping of the block.
/// Also handles collision detection to determine if the block was placed successfully or if the game is over.
/// </summary>
public class BlockMover : MonoBehaviour
{
    // --- Public Variables (adjustable in Unity Inspector) ---
    [Tooltip("Speed of horizontal oscillation.")]
    public float moveSpeed = 3f;
    [Tooltip("Max distance from the center point for oscillation.")]
    public float moveRange = 4f;

    // --- Private Variables ---
    private bool isMoving = true;
    private GameManager gameManager;
    private Rigidbody2D rb;

    void Awake()
    {
        // Get component references
        rb = GetComponent<Rigidbody2D>();

        // Find the single instance of the GameManager in the scene
        gameManager = FindObjectOfType<GameManager>();

        // Ensure the Rigidbody and movement are properly initialized
        if (rb != null)
        {
            // Start with gravity disabled for horizontal movement
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            Debug.LogError("BlockMover requires a Rigidbody2D component on the same GameObject.");
        }

        if (gameManager == null)
        {
            Debug.LogError("BlockMover could not find a GameManager instance in the scene.");
        }
    }

    void Update()
    {
        if (isMoving)
        {
            // Continuous left-right movement using Sine wave for smooth oscillation
            Vector3 newPos = transform.position;
            // Mathf.Sin returns a value between -1 and 1
            newPos.x = Mathf.Sin(Time.time * moveSpeed) * moveRange;
            transform.position = newPos;

            // Check for user input (Mouse Click or Touch)
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                DropBlock();
            }
        }
    }

    /// <summary>
    /// Stops horizontal movement and enables gravity, causing the block to fall.
    /// </summary>
    void DropBlock()
    {
        isMoving = false;
        // Enable physics: block now falls due to gravity
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    /// <summary>
    /// Called when the block collides with another object.
    /// </summary>
    /// <param name="collision">Collision details.</param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check to make sure the block is actually falling and hasn't already been stabilized
        if (isMoving == false)
        {
            // The block has landed. Stop all physics movement immediately.
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            // Convert back to Kinematic to make it a stable, permanent part of the tower
            rb.bodyType = RigidbodyType2D.Kinematic;

            // Check what the block landed on using tags
            if (collision.gameObject.CompareTag("Ground"))
            {
                // Logic for falling on the initial ground (Game Over condition)
                Debug.Log("Game Over: Block hit the Ground!");
                if (gameManager != null)
                {
                    gameManager.GameOver();
                }
            }
            else if (collision.gameObject.CompareTag("Block"))
            {
                // Logic for successfully landing on another block
                Debug.Log("Block Placed: Ready for New Stack!");
                if (gameManager != null)
                {
                    // Notify the manager to spawn the next block
                    gameManager.BlockPlaced(this.gameObject);
                }
            }
            // Note: Ensure your Ground and Block prefabs have the correct tags set in the Inspector.
        }
    }
}
