using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public GameObject ground;
    public GameObject inGamePanel;
    public GameObject settingsPanel;

    private GameObject currentBlock;
    private float moveSpeed = 2f;
    private bool movingRight = true;
    private bool isFalling = false;
    private float xLimit = 2f;
    private bool firstBlockSpawned = false;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        // Just create the first static block only
        SpawnInitialBlock();
    }

    void Update()
    {
        if (currentBlock != null && !isFalling)
        {
            MoveBlock();

            if (Input.GetMouseButtonDown(0))
            {
                DropBlock();
            }
        }

        // Only spawn the first moving block once, when the initial block is ready
        if (!firstBlockSpawned)
        {
            firstBlockSpawned = true;
            Invoke(nameof(SpawnNextBlock), 1f); // delay spawn 1s
        }
    }

    void MoveBlock()
    {
        Vector3 pos = currentBlock.transform.position;

        if (movingRight)
            pos.x += moveSpeed * Time.deltaTime;
        else
            pos.x -= moveSpeed * Time.deltaTime;

        if (pos.x > xLimit) movingRight = false;
        if (pos.x < -xLimit) movingRight = true;

        currentBlock.transform.position = pos;
    }

    void DropBlock()
    {
        isFalling = true;
        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void SpawnInitialBlock()
    {
        GameObject baseBlock = Instantiate(blockPrefab, new Vector3(0, -3f, 0), Quaternion.identity);
        baseBlock.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        baseBlock.tag = "Block";
    }

    void SpawnNextBlock()
    {
        currentBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        currentBlock.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        currentBlock.tag = "Block";
        isFalling = false;
    }

    void OnEnable()
    {
        BlockCollision.OnBlockLanded += HandleBlockLanding;
    }

    void OnDisable()
    {
        BlockCollision.OnBlockLanded -= HandleBlockLanding;
    }

    void HandleBlockLanding(bool onGround)
    {
        if (onGround)
        {
            inGamePanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
        else
        {
            SpawnNextBlock();
        }
    }
}
