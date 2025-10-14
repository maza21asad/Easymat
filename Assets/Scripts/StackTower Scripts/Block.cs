using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasLanded = false;
    private BlockManager manager;

    public void Initialize(BlockManager blockManager)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        manager = blockManager;
    }

    void Update()
    {
        // Drop the block on click/tap
        if (!hasLanded && Input.GetMouseButtonDown(0))
        {
            rb.isKinematic = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded) return;

        // Check for collision with the ground or another block
        if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Block"))
        {
            hasLanded = true;
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;

            // Notify the manager, passing THIS block's reference
            manager.OnBlockLanded(this.gameObject);
        }
    }
}