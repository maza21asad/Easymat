using UnityEngine;

/// <summary>
/// Manages the game state, block spawning, and panel activation (InGame/GameOver).
/// </summary>
public class GameManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public float moveSpeed = 3f;

    private GameObject currentBlock;
    private GameObject lastBlock;
    private bool isMovingRight = true;
    private bool isDropping = false;

    void Start()
    {
        SpawnNewBlock();
    }

    void Update()
    {
        if (currentBlock == null) return;

        if (!isDropping)
        {
            // Move left and right
            float moveDir = isMovingRight ? 1f : -1f;
            currentBlock.transform.position += Vector3.right * moveSpeed * moveDir * Time.deltaTime;

            // Reverse direction at edges
            if (Mathf.Abs(currentBlock.transform.position.x) > 3f)
                isMovingRight = !isMovingRight;

            // Drop when player clicks/taps
            if (Input.GetMouseButtonDown(0))
                DropBlock();
        }
    }

    void DropBlock()
    {
        isDropping = true;
        Rigidbody rb = currentBlock.GetComponent<Rigidbody>();
        rb.isKinematic = false; // Enable gravity

        // When it lands, spawn the next one
        Invoke(nameof(SpawnNewBlock), 0.8f);
    }

    void SpawnNewBlock()
    {
        // Raise spawn position
        Vector3 pos = spawnPoint.position;
        if (lastBlock != null)
            pos.y = lastBlock.transform.position.y + 1f;

        currentBlock = Instantiate(blockPrefab, pos, Quaternion.identity);
        currentBlock.GetComponent<Rigidbody>().isKinematic = true;
        isMovingRight = true;
        isDropping = false;

        lastBlock = currentBlock;
    }
}
