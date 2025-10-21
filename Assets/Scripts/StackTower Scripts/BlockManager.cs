using System.Collections;
using UnityEngine;
using Cinemachine;

public class BlockManager : MonoBehaviour
{
    public GameObject blockPrefab;           // world-space prefab (SpriteRenderer, Rigidbody2D, BoxCollider2D)
    public Transform spawnPoint;             // world-space spawn point
    public Transform cameraTarget;           // empty world-space object; vCam.Follow should be this
    public CinemachineVirtualCamera virtualCamera;

    [Header("Spawn / Camera")]
    public float spawnOffsetY = 2f;          // vertical offset above top block for next spawn
    public float cameraLerpSpeed = 3f;

    private GameObject topBlock;
    private GameObject currentBlock;
    private float highestY = float.MinValue;

    void Start()
    {
        if (cameraTarget == null) Debug.LogWarning("CameraTarget not assigned!");
        if (virtualCamera != null && cameraTarget != null) virtualCamera.Follow = cameraTarget;
        SpawnBlock(autoDrop: true);
    }

    void Update()
    {
        // If there's no active moving block, spawn a new one
        if (currentBlock == null)
        {
            SpawnBlock(autoDrop: false);
        }

        // Optionally update cameraTarget every frame to the midpoint
        UpdateCameraTargetMidpoint();
    }
    void LateUpdate()
    {
        if (cameraTarget == null) return;

        if (topBlock != null && currentBlock != null)
        {
            float midY = (topBlock.transform.position.y + currentBlock.transform.position.y) / 2f;
            Vector3 targetPos = cameraTarget.position;
            targetPos.y = Mathf.Lerp(cameraTarget.position.y, midY, Time.deltaTime * 3f);
            cameraTarget.position = targetPos;
        }
    }

    private void SpawnBlock(bool autoDrop)
    {
        if (blockPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Block Prefab or Spawn Point is not assigned!");
            return;
        }

        // Instantiate new block
        GameObject newBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log("Spawned new block at: " + spawnPoint.position);

        // Update Cinemachine to follow the new block
        if (virtualCamera != null)
        {
            virtualCamera.Follow = newBlock.transform;
        }

        // Initialize block
        Block blockScript = newBlock.GetComponent<Block>();
        if (blockScript != null)
        {
            blockScript.Initialize(this, autoDrop);
        }
        else
        {
            Debug.LogError("Block prefab is missing the 'Block' script!");
        }
    }

    // Called by Block when it lands
    public void OnBlockLanded(GameObject landed)
    {
        // Set topBlock
        topBlock = landed;

        // Update highestY
        if (landed.transform.position.y > highestY) highestY = landed.transform.position.y;

        // Move spawnPoint up so next spawn uses it (optional)
        if (spawnPoint != null)
        {
            spawnPoint.position = new Vector3(spawnPoint.position.x, highestY + spawnOffsetY, spawnPoint.position.z);
        }

        // Smoothly move cameraTarget up to the new midpoint (handled by UpdateCameraTargetMidpoint)
        // Immediately set topBlock but don't change cameraTarget.x
        // currentBlock will be set to null by the Block script (so Update will spawn next)
    }

    private void UpdateCameraTargetMidpoint()
    {
        if (cameraTarget == null) return;

        Vector3 desiredPos = cameraTarget.position;

        if (topBlock != null && currentBlock != null)
        {
            Vector3 mid = (topBlock.transform.position + currentBlock.transform.position) * 0.5f;
            desiredPos = new Vector3(cameraTarget.position.x, mid.y, cameraTarget.position.z);
        }
        else if (topBlock != null)
        {
            desiredPos = new Vector3(cameraTarget.position.x, topBlock.transform.position.y, cameraTarget.position.z);
        }
        else if (currentBlock != null)
        {
            desiredPos = new Vector3(cameraTarget.position.x, currentBlock.transform.position.y, cameraTarget.position.z);
        }

        // Smoothly lerp the cameraTarget's Y only
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, desiredPos, Time.deltaTime * cameraLerpSpeed);
    }

    // Helper so Block script can set manager.currentBlock to null when it starts falling (if needed)
    public void NotifyBlockBecameActive(GameObject block)
    {
        if (block == currentBlock) return;
        currentBlock = block;
    }

    // Utility: if you want Block to tell manager the block is no longer active (after Drop)
    public void NotifyBlockDropped()
    {
        currentBlock = null;
    }
}
