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

    private int blockCount = 0;
    private bool canSpawn = true;
    private Transform topBlock;

    [Header("UI Panels")]
    public GameObject newPanel;
    public GameObject gamePanel;

    [Header("Timer Settings")]
    public float gameTime = 60f;
    private float timer;
    public TMP_Text timerText;
    private bool isTimerRunning = false;

    [Header("Wind Settings")]
    public bool windEnabled = false;   // ? Wind is OFF at start
    public int windStartAfter = 10;    // ? Wind starts after 10 blocks

    public CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        if (blockPrefab == null || spawnPoint == null)
        {
            Debug.LogError("BlockManager: Missing blockPrefab or spawnPoint reference!");
            return;
        }

        SpawnBlock(autoDrop: true);

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
                EndGame();
            }
        }
    }

    public void OnBlockLanded(Transform landedBlock)
    {
        topBlock = landedBlock;

        // ? When wind should start
        if (!windEnabled && blockCount >= windStartAfter)
        {
            windEnabled = true;
            Debug.Log($"??? Wind system activated after {blockCount} blocks!");
        }

        // ? When 5 blocks are placed, switch panels (your previous condition)
        if (blockCount >= 5)
        {
            if (newPanel != null && !newPanel.activeSelf)
            {
                newPanel.SetActive(true);
                Debug.Log("?? New panel enabled after 5 blocks!");
            }

            if (gamePanel != null && gamePanel.activeSelf)
            {
                gamePanel.SetActive(false);
                Debug.Log("?? Game panel disabled.");
            }
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

        float middleX = 0f;
        float spawnY = spawnHeightOffset;
        float spawnZ = 0f;

        if (topBlock != null)
        {
            spawnY = topBlock.position.y + spawnHeightOffset;
            spawnZ = topBlock.position.z;
        }

        spawnPoint.position = new Vector3(middleX, spawnY, spawnZ);

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

    private void EndGame()
    {
        Debug.Log("? Time’s up! Game Over.");
        if (gamePanel != null)
            gamePanel.SetActive(false);
        if (newPanel != null)
            newPanel.SetActive(true);
    }
}
