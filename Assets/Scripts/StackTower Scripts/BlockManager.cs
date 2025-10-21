using UnityEngine;
using Cinemachine;

public class BlockManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public CinemachineVirtualCamera virtualCamera;

    private GameObject currentBlock;

    void Start()
    {
        // Spawn first block that will automatically fall
        SpawnBlock(autoDrop: true);
    }

    public void OnBlockLanded(GameObject landedBlock)
    {
        // 1. CRITICAL FIX: Stop the camera from following the landed block.
        // This is important to prevent the camera from shaking or fighting
        // between the old and new block positions.
        if (virtualCamera != null && virtualCamera.Follow == landedBlock.transform)
        {
            virtualCamera.Follow = null;
        }

        // 2. Move spawn point above the landed block
        // Adjust the Y-offset to be based on the landed block's top edge
        // A value of '2f' seems too high based on the video (blocks look about 1 unit tall).
        // Let's use 1.0f as an estimate for 1 unit above the block's center, adjust in Inspector.
        // A better approach would be to calculate it based on the block's size.
        // Assuming block has a sprite/box collider of Y size ~1.5 units, let's use a conservative 1.6f.
        Vector3 newPos = landedBlock.transform.position + new Vector3(0, 1.6f, 0);
        spawnPoint.position = newPos;

        // 3. Spawn next moving block
        SpawnBlock(autoDrop: false);
    }

    private void SpawnBlock(bool autoDrop)
    {
        if (blockPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Block Prefab or Spawn Point not assigned!");
            return;
        }

        GameObject newBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        currentBlock = newBlock;

        // Set camera follow target
        if (virtualCamera != null)
            virtualCamera.Follow = newBlock.transform; // Camera now follows the new block

        // Initialize block
        Block blockScript = newBlock.GetComponent<Block>();
        if (blockScript != null)
            blockScript.Initialize(this, autoDrop);
        else
            Debug.LogError("Block prefab missing 'Block' script!");
    }
}