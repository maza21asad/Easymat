using System.Collections;
using UnityEngine;
using Cinemachine;

public class BlockSpawner : MonoBehaviour
{
    public GameObject blockPrefab;
    public float dropHeight = 5f;
    public float moveSpeed = 5f;
    public CinemachineVirtualCamera vCam;
    public Transform cameraTarget;
    public float cameraOrthographicSize = 6f;
    public Transform groundTransform;

    [Header("Debugging Info")] // Added for Inspector clarity
    public float blockHalfHeight; // Made public for inspection

    private GameObject currentBlock;
    private GameObject topBlock;
    private bool isMovingRight = true;
    private float moveDirection = 0.5f;

    private void Start()
    {
        // 1. CALCULATE BLOCK DIMENSIONS ONCE
        BoxCollider2D blockCollider = blockPrefab.GetComponent<BoxCollider2D>();
        if (blockCollider != null)
        {
            // Calculate half height: (Collider size Y * Transform scale Y) / 2
            blockHalfHeight = (blockCollider.size.y * blockPrefab.transform.localScale.y) / 2f;
        }
        else
        {
            Debug.LogError("Block Prefab is missing a BoxCollider2D component!");
            blockHalfHeight = 0.5f;
        }

        SpawnBlock();
    }

    void Update()
    {
        if (currentBlock != null)
        {
            // 1. Block Movement
            float horizontalMovement = moveDirection * moveSpeed * Time.deltaTime;
            currentBlock.transform.position += new Vector3(horizontalMovement, 0, 0);

            // 2. Edge Detection
            if (currentBlock.transform.position.x > 3f || currentBlock.transform.position.x < -3f)
            {
                moveDirection *= -1;
            }

            // 3. Drop Input
            if (Input.GetMouseButtonDown(0))
            {
                DropBlock();
            }
        }

        UpdateCameraTarget();
    }

    void SpawnBlock()
    {
        float spawnY;

        // Use the calculated blockHalfHeight for accurate stacking
        if (topBlock == null)
        {
            // FIRST BLOCK: Spawn above the ground
            float groundTopY = groundTransform.position.y + (groundTransform.localScale.y / 2f);
            spawnY = groundTopY + blockHalfHeight + dropHeight;
        }
        else
        {
            // SUBSEQUENT BLOCKS: Spawn above the previous block
            float stackTopY = topBlock.transform.position.y + (2f * blockHalfHeight);
            spawnY = stackTopY + dropHeight;
        }

        Vector3 pos = new Vector3(0, spawnY, 0);

        currentBlock = Instantiate(blockPrefab, pos, Quaternion.identity, null);
        currentBlock.GetComponent<Rigidbody2D>().gravityScale = 0;

        // Pass necessary spawner reference to the instantiated block
        Block blockComponent = currentBlock.GetComponent<Block>();
        if (blockComponent != null)
        {
            blockComponent.spawner = this;
        }

        moveDirection = isMovingRight ? 1f : -1f;
        isMovingRight = !isMovingRight;
    }

    void DropBlock()
    {
        if (currentBlock != null)
        {
            Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
            rb.gravityScale = 1;

            // CRITICAL: Set currentBlock to null immediately so input is ignored while it drops
            // The next block spawn will be handled by the Block.cs collision.
            currentBlock = null;
        }
    }

    // This method is now called by Block.cs when a collision occurs.
    public IEnumerator CheckBlockLanded(GameObject block)
    {
        Rigidbody2D rb = block.GetComponent<Rigidbody2D>();

        // Wait a short duration to ensure physics settles (velocity near zero)
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => rb.velocity.sqrMagnitude < 0.1f);

        // Update the top block of the stack
        topBlock = block;

        // Wait briefly, then spawn the next block.
        if (Time.timeScale > 0)
        {
            yield return new WaitForSeconds(0.5f);
            SpawnBlock();
        }
    }

    void UpdateCameraTarget()
    {
        if (cameraTarget == null || vCam == null) return;

        // Use topBlock's position for mid-stack camera target if current block is dropping
        Vector3 followPos = (topBlock != null) ? topBlock.transform.position : Vector3.zero;

        // If a new block is moving, center the camera between the new block and the stack top
        if (currentBlock != null && topBlock != null)
        {
            Vector3 mid = (topBlock.transform.position + currentBlock.transform.position) / 2f;
            followPos = new Vector3(0, mid.y, 0);
        }
        else if (topBlock != null)
        {
            // If we are waiting for a block to land, follow the top block of the stack.
            followPos = new Vector3(0, topBlock.transform.position.y, 0);
        }

        cameraTarget.position = Vector3.Lerp(
            cameraTarget.position,
            followPos,
            Time.deltaTime * 6f // Increased speed for smoother follow
        );

        vCam.m_Lens.OrthographicSize = cameraOrthographicSize;
    }
}