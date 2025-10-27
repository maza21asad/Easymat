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

    private int blockCount = 0;
    private GameManagerUI uiManager;

    private float windForce = 0f; // Horizontal wind force
    public GameObject windArrow;



    public void Initialize(BlockManager blockManager, bool autoDrop = false)
    {
        manager = blockManager;
        rb = GetComponent<Rigidbody2D>();
        uiManager = FindObjectOfType<GameManagerUI>();

        if (rb == null)
        {
            Debug.LogError("Block missing Rigidbody2D!");
            return;
        }

        startPos = transform.position;

        // Random wind force between -1.5 to 1.5 (left or right)
        windForce = Random.Range(-3f, 3f);

        // Debug wind direction in console
        if (windForce > 0)
            Debug.Log($"Wind is blowing RIGHT with force {windForce}");
        else if (windForce < 0)
            Debug.Log($"Wind is blowing LEFT with force {windForce}");
        else
            Debug.Log("No wind for this block.");

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
            // Rotate arrow based on wind direction
            if (windForce > 0) windArrow.transform.rotation = Quaternion.Euler(0, 0, 0);   // right
            else if (windForce < 0) windArrow.transform.rotation = Quaternion.Euler(0, 0, 180); // left
            else windArrow.SetActive(false); // no wind
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

        // Apply horizontal wind force
        rb.AddForce(new Vector2(windForce, 0f), ForceMode2D.Impulse);

        // Debug when block is dropped
        if (windForce > 0)
            Debug.Log($"Dropped block: wind pushed RIGHT with force {windForce}");
        else if (windForce < 0)
            Debug.Log($"Dropped block: wind pushed LEFT with force {windForce}");
        else
            Debug.Log("Dropped block: no wind applied.");


        if (windArrow != null) windArrow.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasLanded && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Block")))
        {
            hasLanded = true;
            blockCount++;

            Debug.Log($"Block {blockCount} landed.");

            if (blockCount > 10 && collision.gameObject.CompareTag("Ground"))
            {
                if (uiManager != null) uiManager.ShowGameOver();
            }

            manager.OnBlockLanded(this.transform);
        }
    }
}
