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

    private float windForce = 0f; // Horizontal wind force
    public GameObject windArrow;

    public void Initialize(BlockManager blockManager, bool autoDrop = false)
    {
        manager = blockManager;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Block missing Rigidbody2D!");
            return;
        }

        startPos = transform.position;

        // Apply wind only if manager.windEnabled is true
        if (manager.windEnabled)
        {
            windForce = Random.Range(-3f, 3f);

            if (windForce > 0)
                Debug.Log($"??? Wind is blowing RIGHT with force {windForce}");
            else if (windForce < 0)
                Debug.Log($"??? Wind is blowing LEFT with force {windForce}");
            else
                Debug.Log("No wind for this block.");
        }
        else
        {
            windForce = 0f;
            Debug.Log("?? Wind disabled — calm weather mode (no wind yet).");
        }

        if (autoDrop)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            isMoving = true;
        }
    }

    public void ShowWindIndicator()
    {
        if (windArrow != null)
        {
            windArrow.SetActive(true);
            if (windForce > 0)
                windArrow.transform.rotation = Quaternion.Euler(0, 0, 0);    // right
            else if (windForce < 0)
                windArrow.transform.rotation = Quaternion.Euler(0, 0, 180);  // left
            else
                windArrow.SetActive(false); // no wind
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

        // Apply horizontal wind only if active
        if (manager.windEnabled && Mathf.Abs(windForce) > 0f)
        {
            rb.AddForce(new Vector2(windForce, 0f), ForceMode2D.Impulse);

            if (windForce > 0)
                Debug.Log($"?? Dropped block: wind pushed RIGHT with force {windForce}");
            else
                Debug.Log($"?? Dropped block: wind pushed LEFT with force {windForce}");
        }
        else
        {
            Debug.Log("?? Dropped block: calm weather (no wind applied).");
        }

        if (windArrow != null)
            windArrow.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Always check for ground collision
        if (collision.gameObject.CompareTag("Ground"))
        {
            // ? Allow only the very first block to touch ground safely
            if (manager != null && manager.GetBlockCount() > 1)
            {
                Debug.Log("?? Block touched the ground — GAME OVER!");
                manager.EndGame();
                return;
            }
        }

        // Normal stacking logic
        if (!hasLanded && collision.gameObject.CompareTag("Block"))
        {
            hasLanded = true;
            manager.OnBlockLanded(this.transform);
        }
    }
}
