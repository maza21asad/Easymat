using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasDropped = false;
    private bool hasLanded = false;
    private BlockManager manager;
    private static bool firstBlockGrounded = false; 
    private float windForce = 0f; 

    public GameObject windArrow; 

    // Public read-only property for wind
    public float WindForce { get { return windForce; } }

    public void Initialize(BlockManager blockManager, bool autoDrop = false)
    {
        manager = blockManager;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Block missing Rigidbody2D!");
            return;
        }

        // Apply wind logic if enabled
        if (manager.windEnabled)
        {
            windForce = Random.Range(-3f, 3f);
            if (windForce > 0)
                Debug.Log($"Wind blowing RIGHT with force {windForce}");
            else if (windForce < 0)
                Debug.Log($"Wind blowing LEFT with force {windForce}");
            else
                Debug.Log("Calm weather, no wind for this block.");
        }
        else
        {
            windForce = 0f;
            Debug.Log("Wind disabled — calm weather mode.");
        }

        // Before dropping, block stays attached to holder
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        if (!hasDropped && manager != null && manager.holder != null)
        {
            float yOffset = -0.7f;
            transform.position = new Vector3(
                manager.holder.position.x,
                manager.holder.position.y + yOffset,
                manager.holder.position.z
            );

            if (Input.GetMouseButtonDown(0))
                DropBlock();
        }
    }

    private void DropBlock()
    {
        if (hasDropped) return;

        hasDropped = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        // Apply wind impulse on drop
        if (manager.windEnabled && Mathf.Abs(windForce) > 0f)
        {
            rb.AddForce(new Vector2(windForce, 0f), ForceMode2D.Impulse);
            if (windForce > 0)
                Debug.Log($"Dropped block pushed RIGHT by wind {windForce}");
            else
                Debug.Log($"Dropped block pushed LEFT by wind {windForce}");
        }

        if (windArrow != null)
            windArrow.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Base block grounding
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!firstBlockGrounded)
            {
                firstBlockGrounded = true;
                rb.bodyType = RigidbodyType2D.Static;
                Debug.Log("First block grounded safely — base created.");
            }
            else
            {
                Debug.Log("Block touched ground — game over!");
                if (manager != null)
                    manager.EndGame();
            }
        }

        // Stacking logic
        if (!hasLanded && collision.gameObject.CompareTag("Block"))
        {
            hasLanded = true;
            if (manager != null)
                manager.OnBlockLanded(this.transform);
        }
    }
}
