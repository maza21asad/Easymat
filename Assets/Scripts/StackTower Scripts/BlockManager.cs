using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public class BlockManager : MonoBehaviour
{
    [Header("References")]
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public float spawnHeightOffset = 3f;
    public CameraTarget cameraTarget;

    [Header("Holder")]
    public Transform holder;  // Holder for moving blocks

    private int blockCount = 0;
    private bool canSpawn = true;
    private Transform topBlock;

    [Header("UI Panels")]
    public GameObject newPanel;    // Game Over Panel
    public GameObject gamePanel;   // Game Panel

    [Header("Timer Settings")]
    public float gameTime = 60f;
    private float timer;
    public TMP_Text timerText;
    private bool isTimerRunning = false;

    [Header("Wind Settings")]
    public bool windEnabled = false;   // Wind is OFF at start
    public int windStartAfter = 10;    // Wind starts after 10 blocks

    public CinemachineVirtualCamera virtualCamera;



    public float moveSpeed = 1f;      // Speed of left-right movement
    public float moveRange = 2f;      // Horizontal movement range

    private Vector3 initialHolderPos; // starting position for left-right movement
    private float moveTimer = 0f;


    private void Start()
    {
        if (blockPrefab == null || spawnPoint == null)
        {
            Debug.LogError("BlockManager: Missing blockPrefab or spawnPoint reference!");
            return;
        }

        SpawnBlock(autoDrop: true); // First block will move like others

        timer = gameTime;
        isTimerRunning = true;


    }

    private void Update()
    {
        if (isTimerRunning)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
                timerText.text = Mathf.Ceil(timer).ToString();

            if (timer <= 0)
            {
                /*isTimerRunning = false;
                timerText.text = "Time: 0";
                //EndGame();*/

                isTimerRunning = false;
                timerText.text = "Time: 0";

                // Time over ? show game over panel
                if (gamePanel != null)
                    gamePanel.SetActive(false);

                if (newPanel != null)
                    newPanel.SetActive(true);

                canSpawn = false; // stop spawning blocks
                Debug.Log("? Time's up! Game over triggered.");
            }
        }
    }


    private void LateUpdate()
    {
        UpdateHolderMovement();
    }

    /*private void UpdateHolderHeight()
    {
        if (holder != null && topBlock != null)
        {
            // Distance above top block
            float offsetY = spawnHeightOffset - 0.5f;

            // Smooth movement to avoid sudden jump
            Vector3 targetPos = new Vector3(holder.position.x, topBlock.position.y + offsetY, holder.position.z);
            holder.position = Vector3.Lerp(holder.position, targetPos, Time.deltaTime);

            Debug.Log($"Holder Current Y: {holder.position.y:F2}, Target Y: {targetPos.y:F2}, Top Block Y: {topBlock.position.y:F2}");
        }
    }*/

    private void UpdateHolderMovement()
    {
        if (holder == null || topBlock == null) return;

        // Horizontal: continuous sine
        moveTimer += Time.deltaTime * moveSpeed;
        float offsetX = Mathf.Sin(moveTimer) * moveRange;

        // Vertical: smoothly follow tower, only upwards
        float desiredY = topBlock.position.y + (spawnHeightOffset - 0.5f);
        float targetY = holder.position.y;
        if (desiredY > holder.position.y)
        {
            targetY = Mathf.Lerp(holder.position.y, desiredY, Time.deltaTime * 5f); // Increase factor for faster smoothness
        }

        // Apply position to holder (visual)
        Vector3 newPos = new Vector3(
            initialHolderPos.x + offsetX,
            targetY,
            holder.position.z
        );
        holder.position = newPos;

        // --- Smooth follow for currently held block ---
        if (topBlock != null && topBlock.CompareTag("wBlock"))
        {
            Vector3 blockTargetPos = new Vector3(newPos.x, newPos.y - 0.7f, newPos.z); // adjust offset
            topBlock.position = Vector3.Lerp(topBlock.position, blockTargetPos, Time.deltaTime * 8f); // higher factor for snappier follow
        }
    }




    public void OnBlockLanded(Transform landedBlock)
    {
        //landedBlock.tag = "Block";  // Reset to normal block tag
        topBlock = landedBlock;


        // When wind should start
        if (!windEnabled && blockCount >= windStartAfter)
        {
            windEnabled = true;
            Debug.Log($"Wind system activated after {blockCount} blocks!");
        }

        if (canSpawn)
            StartCoroutine(SpawnNextBlock());
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
        Camofsetadd();

        // Holder logic
        if (holder != null)
        {
            float yOffset = -0.7f; // move 0.5 units down
            spawnPoint.position = new Vector3(holder.position.x, holder.position.y + yOffset, holder.position.z);
        }
        else
        {
            float middleX = 0f;
            float spawnY = spawnHeightOffset;
            float spawnZ = 0f;

            if (topBlock != null)
            {
                spawnY = topBlock.position.y + spawnHeightOffset;
                spawnZ = topBlock.position.z;
            }

            spawnPoint.position = new Vector3(middleX, spawnY, spawnZ);
        }

        GameObject newBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        newBlock.tag = "Block";

        Block blockScript = newBlock.GetComponent<Block>();
        if (blockScript != null)
            blockScript.Initialize(this, autoDrop); // Pass manager reference

        if (cameraTarget != null)
            cameraTarget.SetTopBlock(newBlock.transform);
    }

    void Camofsetadd()
    {
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset.y += 0.4f;
    }

    public int GetBlockCount()
    {
        return blockCount;
    }

    public void EndGame()
    {
        Debug.Log("Game Over! A block hit the ground.");

        if (gamePanel != null)
            gamePanel.SetActive(false);

        if (newPanel != null)
            newPanel.SetActive(true);

        canSpawn = false;
        isTimerRunning = false;
    }
}