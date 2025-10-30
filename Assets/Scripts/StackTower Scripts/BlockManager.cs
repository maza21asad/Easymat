using UnityEngine;
using System.Collections;
using Cinemachine;
using TMPro;

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
                isTimerRunning = false;
                timerText.text = "Time: 0";
                //EndGame();
            }
        }
    }


    private void LateUpdate()
    {
        UpdateHolderHeight();
    }

    private void UpdateHolderHeight()
    {
        if (holder != null && topBlock != null)
        {
            // Distance above top block
            float offsetY = spawnHeightOffset - 0.5f;

            // Smooth movement to avoid sudden jump
            Vector3 targetPos = new Vector3(
                holder.position.x,
                topBlock.position.y + offsetY,
                holder.position.z
            );

            holder.position = Vector3.Lerp(holder.position, targetPos, Time.deltaTime * 2f);
        }
    }


    public void OnBlockLanded(Transform landedBlock)
    {
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
