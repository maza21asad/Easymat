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

    public float WindForce => windForce;

    public void Initialize(BlockManager blockManager, bool autoDrop = false)
    {
        manager = blockManager;
        rb = GetComponent<Rigidbody2D>();

        if (manager.windEnabled)
            windForce = Random.Range(-3f, 3f);

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        if (!hasDropped)
        {
            if (Input.GetMouseButtonDown(0))
                DropBlock();
        }
    }

    private void DropBlock()
    {
        if (hasDropped) return;

        hasDropped = true;

        // ?? FIX: Unparent from holder
        transform.SetParent(null);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        if (manager.windEnabled && Mathf.Abs(windForce) > 0f)
            rb.AddForce(new Vector2(windForce, 0f), ForceMode2D.Impulse);

        if (windArrow != null)
            windArrow.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!firstBlockGrounded)
            {
                firstBlockGrounded = true;
                rb.bodyType = RigidbodyType2D.Static;
            }
            else
            {
                manager.EndGame();
            }
        }

        if (!hasLanded && collision.gameObject.CompareTag("Block"))
        {
            hasLanded = true;
            manager.OnBlockLanded(this.transform);
        }
    }
}
