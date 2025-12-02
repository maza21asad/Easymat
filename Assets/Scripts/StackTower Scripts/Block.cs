using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasDropped = false;
    private bool hasLanded = false;
    private BlockManager manager;
    private static bool firstBlockGrounded = false;
    private float windForce = 0f;
    private int blockNumber;

    public GameObject windArrow;

    public float WindForce => windForce;

    public void Initialize(BlockManager blockManager, int blockCount, bool autoDrop = false, bool applyWind = false)
    {
        manager = blockManager;
        blockNumber = blockCount;
        rb = GetComponent<Rigidbody2D>();

        // Only apply wind if the manager explicitly says so for this block
        if (manager.windEnabled && applyWind)
        {
            // Randomly choose direction (left or right)
            if (Random.value > 0.5f)
                windForce = manager.rightForce;
            else
                windForce = manager.leftForce;

            // Ensure the wind arrow is visible if wind is applied
            if (windArrow != null)
                windArrow.SetActive(true);
        }
        else
        {
            windForce = 0f;
            // Ensure the wind arrow is invisible if no wind is applied
            if (windArrow != null)
                windArrow.SetActive(false);
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        // Reset drag to default values while being held (Kinematic)
        rb.drag = 0f;
        rb.angularDrag = 0.05f;

        // Blocks start with Z-rotation locked.
        manager.SetBlockZRotationConstraint(this.transform, true);
    }

    private void Update()
    {
        if (!hasDropped)
        {
            // Handle horizontal movement here if needed (e.g., using A/D keys or touch drag)

            if (Input.GetMouseButtonDown(0))
                DropBlock();
        }
    }

    private void DropBlock()
    {
        if (hasDropped) return;

        hasDropped = true;

        // Unparent from holder
        transform.SetParent(null);

        rb.bodyType = RigidbodyType2D.Dynamic;

        // Clear any residual velocity from the moving holder
        rb.velocity = Vector2.zero;

        // ?? CRITICAL CHANGE: Apply soft-fall mechanics
        if (blockNumber >= manager.gravityReductionStartBlock)
        {
            rb.gravityScale = manager.reducedGravityScale;
            // Greatly increase drag to slow down the fall and damp rotation before impact
            rb.drag = 5f; // Increase linear drag dramatically
            rb.angularDrag = 10f; // Increase angular drag dramatically
            Debug.Log($"Block {blockNumber} dropping softly with Gravity: {manager.reducedGravityScale}, Drag: {rb.drag}");
        }
        else
        {
            rb.gravityScale = 1f; // Default gravity for early blocks
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
        }

        // Apply the fixed wind force as an impulse
        if (manager.windEnabled && Mathf.Abs(windForce) > 0f)
            rb.AddForce(new Vector2(windForce, 0f), ForceMode2D.Impulse);

        if (windArrow != null)
            windArrow.SetActive(false);

        // Briefly slow down time when the block drops
        Time.timeScale = 0.8f;

        // Also call a routine on the manager to reset Time.timeScale shortly after drop
        manager.StartCoroutine(manager.ResetTimeScale(0.3f));

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!firstBlockGrounded)
            {
                firstBlockGrounded = true;
                rb.bodyType = RigidbodyType2D.Static;

                // Freeze Z-Rotation for the first block permanently on the ground
                manager.SetBlockZRotationConstraint(this.transform, true);
            }
            else
            {
                // This block missed the stack and hit the ground after the first block
                manager.EndGame();
            }
        }

        if (!hasLanded && collision.gameObject.CompareTag("Block"))
        {
            hasLanded = true;
            manager.OnBlockLanded(this.transform);

            // OPTIONAL: Make blocks static once landed to prevent further movement
            // rb.bodyType = RigidbodyType2D.Static;
        }
    }
}