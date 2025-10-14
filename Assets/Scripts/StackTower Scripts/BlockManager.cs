using UnityEngine;

public class BlockManager : MonoBehaviour
{
    // *** CRITICAL: Drag your Block Prefab (from the Project folder) into this slot in the Inspector. ***
    public GameObject blockPrefab;

    // *** CRITICAL: Drag the Transform of an empty GameObject (SpawnPoint) into this slot. ***
    public Transform spawnPoint;

    void Start()
    {
        // This is the correct logic: it ensures the very first block is spawned 
        // and falls automatically when the game starts.
        SpawnBlock(autoDrop: true);
    }

    public void OnBlockLanded(GameObject landedBlock)
    {
        // 1. Calculate the position for the NEXT block (2 units above the landed block)
        Vector3 newPos = landedBlock.transform.position + new Vector3(0, 2f, 0);

        // 2. Update the spawnPoint position FIRST, so the next instantiated block uses it.
        spawnPoint.position = newPos;

        // 3. Spawn the next block. It will be set to move horizontally and wait for the player click.
        SpawnBlock(autoDrop: false);

        // Note: The Destroy(landedBlock) call is now handled by the Block.cs script you provided.
    }

    private void SpawnBlock(bool autoDrop)
    {
        // The block is instantiated at the current position of the spawnPoint Transform.
        GameObject newBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);

        Block blockScript = newBlock.GetComponent<Block>();
        if (blockScript != null)
        {
            // Initialize the block with a reference to this manager and set its drop behavior.
            blockScript.Initialize(this, autoDrop);
        }
        else
        {
            Debug.LogError("Block prefab is missing the 'Block' script!");
        }
    }
}
