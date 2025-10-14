using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance;

    [Header("Prefabs")]
    public GameObject blockPrefab;      // the normal falling block prefab
    public GameObject movingBlockPrefab; // the prefab that will float left-right

    public Transform spawnPoint;

    [Header("Spawn settings")]
    public float firstSpawnDelay = 0.1f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Immediately spawn the first falling block
        Invoke(nameof(SpawnFirstBlock), firstSpawnDelay);

        // Subscribe to block grounded event
        BlockController.OnBlockGrounded += HandleBlockGrounded;
    }

    void OnDestroy()
    {
        BlockController.OnBlockGrounded -= HandleBlockGrounded;
    }

    void SpawnFirstBlock()
    {
        SpawnFallingBlock();
    }

    void HandleBlockGrounded(BlockController bc)
    {
        // When any block grounds, spawn a moving block above it
        // Option A: spawn at spawnPoint
        SpawnMovingBlock();
    }

    public void SpawnFallingBlock()
    {
        if (blockPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Spawner missing prefab or spawnPoint");
            return;
        }

        GameObject go = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        // Ensure it's dynamic so it falls
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Dynamic;
        // Ensure script present
        if (go.GetComponent<BlockController>() == null) go.AddComponent<BlockController>();
    }

    public void SpawnMovingBlock()
    {
        if (movingBlockPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Spawner missing moving prefab or spawnPoint");
            return;
        }

        GameObject go = Instantiate(movingBlockPrefab, spawnPoint.position, Quaternion.identity);

        // Ensure it has MovingBlockController
        if (go.GetComponent<MovingBlockController>() == null) go.AddComponent<MovingBlockController>();

        // Ensure it has Rigidbody2D and Collider2D
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        // Start as kinematic — script will set rb.bodyType = Kinematic in Awake
        rb.bodyType = RigidbodyType2D.Kinematic;
        // freeze rotation
        rb.freezeRotation = true;

        // Also add BlockController so this block can notify when it lands after dropping
        if (go.GetComponent<BlockController>() == null) go.AddComponent<BlockController>();
    }
}
