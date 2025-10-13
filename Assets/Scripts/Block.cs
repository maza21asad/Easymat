using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    public static bool isFirstBlock = true;
    public GameObject gameOverPanel;

    // NEW: Public reference to the Spawner to call its coroutine
    [HideInInspector] public BlockSpawner spawner;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Prevent repeated checks after the block has successfully stacked or failed.
        if (this.enabled == false) return;

        // -------------------------
        // 1. GROUND COLLISION (Game Over or First Block)
        // -------------------------
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (isFirstBlock)
            {
                isFirstBlock = false;
                Debug.Log("First block landed successfully.");

                // IMPORTANT: Tell the Spawner to check the landing and spawn the next one.
                spawner?.StartCoroutine(spawner.CheckBlockLanded(gameObject));

                this.enabled = false; // Block is set, disable its script
                return;
            }

            // If any subsequent block hits the ground, it's Game Over.
            HandleGameOver();
            this.enabled = false; // Block has failed, disable its script
            return;
        }

        // -------------------------
        // 2. BLOCK COLLISION (Successful Stack)
        // -------------------------
        if (collision.gameObject.CompareTag("Block"))
        {
            // CRITICAL: Stop the falling block immediately to prevent physics glitches
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
            }

            // Tell the Spawner to update the top block and spawn the next one.
            spawner?.StartCoroutine(spawner.CheckBlockLanded(gameObject));

            this.enabled = false; // Block is successfully stacked, disable its script
        }
    }

    private void HandleGameOver()
    {
        if (Time.timeScale == 0f) return;

        Debug.Log("Game Over Triggered! Block touched the ground.");

        // Game Over logic (Panel activation, TimeScale stop, etc.)
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        GameObject gameplayUI = GameObject.Find("GameplayUI");
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        spawner.enabled = false;
        Time.timeScale = 0f;
    }
}