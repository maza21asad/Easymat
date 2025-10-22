using UnityEngine;
using System.Collections;

public class BlockManager : MonoBehaviour
{
    [Header("References")]
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public float spawnHeightOffset = 3f;   // Distance above top block for spawning
    public CameraTarget cameraTarget;

    private int blockCount = 0;
    private bool canSpawn = true;
    private Transform topBlock;

    private void Start()
    {
        if (blockPrefab == null || spawnPoint == null)
        {
            Debug.LogError("BlockManager: Missing blockPrefab or spawnPoint reference!");
            return;
        }

        SpawnBlock(autoDrop: true);
    }

    public void OnBlockLanded(Transform landedBlock)
    {
        topBlock = landedBlock;
        if (canSpawn) StartCoroutine(SpawnNextBlock());
    }

    private IEnumerator SpawnNextBlock()
    {
        canSpawn = false;
        yield return new WaitForSeconds(0.6f);
        SpawnBlock(autoDrop: false);
        canSpawn = true;
    }

    private void SpawnBlock(bool autoDrop)
    {
        blockCount++;

        // Move spawn point above the current top block
        if (topBlock != null)
        {
            spawnPoint.position = new Vector3(topBlock.position.x, topBlock.position.y + spawnHeightOffset, topBlock.position.z);
        }

        GameObject newBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        newBlock.tag = "Block";

        Block blockScript = newBlock.GetComponent<Block>();
        if (blockScript != null) blockScript.Initialize(this, autoDrop);

        // Update camera to follow the new block
        if (cameraTarget != null)
        {
            cameraTarget.SetTopBlock(newBlock.transform);
        }

        if (autoDrop)
            Debug.Log($"[BlockManager] Spawned foundation block #{blockCount} (auto-drop).");
        else
            Debug.Log($"[BlockManager] Spawned moving block #{blockCount} at {spawnPoint.position} — waiting for player to drop.");
    }
}
