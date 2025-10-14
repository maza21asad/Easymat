using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public Transform ground; // Ground or first base block

    [Header("Game State")]
    private GameObject currentBlock;
    private GameObject lastBlock; // Stores the block that was just placed (base for the current one)
    private bool isSpawningAllowed = true;
    private int score = 0;

    void Start()
    {
        // First base block (Set its tag to 'Block' or 'Ground' and scale to your desired starting width)
        SpawnBaseBlock();
        SpawnNextBlock();
    }

    void Update()
    {
        if (currentBlock != null)
        {
            // Move the current block left & right
            currentBlock.transform.position += Vector3.right * Mathf.Sin(Time.time * 2f) * Time.deltaTime * 5f;
        }
    }

    void SpawnBaseBlock()
    {
        GameObject baseBlock = Instantiate(blockPrefab, ground.position, Quaternion.identity);
        baseBlock.GetComponent<Rigidbody2D>().isKinematic = true;
        baseBlock.transform.localScale = new Vector3(5f, 1f, 1f); // Example starting width
        lastBlock = baseBlock; // Set the first block as the base
    }

    public void SpawnNextBlock()
    {
        if (!isSpawningAllowed) return;

        isSpawningAllowed = false;
        currentBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);

        // Ensure the new block has the same width as the block it's landing on
        if (lastBlock != null)
        {
            currentBlock.transform.localScale = lastBlock.transform.localScale;
        }

        currentBlock.GetComponent<Block>().Initialize(this);
    }

    // Called by Block script when it lands
    public void OnBlockLanded(GameObject landedBlock)
    {
        // 1. Core Block Cutting Logic
        if (lastBlock != null)
        {
            CutBlock(landedBlock, lastBlock);
        }

        // Check if the block was completely missed (GameOver handles logic if true)
        if (!isSpawningAllowed) return;

        // 2. Score and Next Spawn
        score++;
        Debug.Log("Score: " + score); // Replace with UI update

        // Store the newly placed block (the potentially cut one) as the base for the NEXT block
        lastBlock = landedBlock;

        isSpawningAllowed = true;
        SpawnNextBlock();
    }

    void CutBlock(GameObject currentBlockObj, GameObject baseBlockObj)
    {
        float baseWidth = baseBlockObj.transform.localScale.x;
        float baseCenter = baseBlockObj.transform.position.x;
        float currentCenter = currentBlockObj.transform.position.x;

        // The amount of horizontal offset between the centers
        float deltaX = currentCenter - baseCenter;
        float excess = Mathf.Abs(deltaX);

        // Check for a complete miss (Game Over)
        if (excess >= baseWidth)
        {
            // The block missed completely, let it fall off.
            GameOver();
            return;
        }

        // 1. Calculate the New Block Dimensions
        float newWidth = baseWidth - excess; // The width of the perfectly placed part

        // Calculate the new center position for the resized block
        // The center shifts towards the base block's center by half the excess amount
        float direction = Mathf.Sign(deltaX);
        float newXPosition = currentCenter - (excess / 2f * direction);

        // 2. Apply the new size and position to the landed block
        currentBlockObj.transform.localScale = new Vector3(newWidth, currentBlockObj.transform.localScale.y, 1);
        currentBlockObj.transform.position = new Vector3(newXPosition, currentBlockObj.transform.position.y, 0);

        // 3. Update Spawn Point (Move it up for the next block)
        // Assuming block height (localScale.y) is 1.0
        spawnPoint.position += Vector3.up * currentBlockObj.transform.localScale.y;

        // 4. (Optional) Spawn the cut-off scrap piece
        if (excess > 0.01f) // Check if there's actually a piece to cut
        {
            SpawnScrapPiece(currentBlockObj, excess, direction);
        }
    }

    void SpawnScrapPiece(GameObject currentBlockObj, float excessWidth, float direction)
    {
        GameObject scrap = Instantiate(blockPrefab, currentBlockObj.transform.position, Quaternion.identity);

        // Position the scrap piece right next to the new block
        float scrapXPosition = currentBlockObj.transform.position.x + (currentBlockObj.transform.localScale.x / 2f + excessWidth / 2f) * direction;

        scrap.transform.localScale = new Vector3(excessWidth, currentBlockObj.transform.localScale.y, 1);
        scrap.transform.position = new Vector3(scrapXPosition, currentBlockObj.transform.position.y, 0);

        // Give the scrap piece physics to fall away
        Rigidbody2D scrapRb = scrap.GetComponent<Rigidbody2D>();
        scrapRb.isKinematic = false;

        // Give it a little initial force for effect
        scrapRb.velocity = new Vector2(5f * direction, 2f);
        scrapRb.angularVelocity = 50f * direction;

        // Clean up the scrap piece after a few seconds
        Destroy(scrap, 3f);
    }

    public void GameOver()
    {
        // Stop the swing and spawning
        isSpawningAllowed = false;
        currentBlock = null;

        // Stop all game movement and show the final score
        Time.timeScale = 0f;
        Debug.Log("GAME OVER! Final Score: " + score);
        // TODO: Implement a proper UI game over screen here
    }
}