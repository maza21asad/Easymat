using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

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

    [Header("Wind UI")]
    public TMP_Text windForceText;     // Drag your TMP_Text here

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

        initialHolderPos = holder != null ? holder.position : Vector3.zero;

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

                if (gamePanel != null)
                    gamePanel.SetActive(false);

                if (newPanel != null)
                    newPanel.SetActive(true);

                canSpawn = false;
                Debug.Log("Time's up! Game over triggered.");
            }
        }
    }

    private void LateUpdate()
    {
        UpdateHolderMovement();
    }

    private void UpdateHolderMovement()
    {
        if (holder == null || topBlock == null) return;

        // Horizontal: continuous sine movement
        moveTimer += Time.deltaTime * moveSpeed;
        float offsetX = Mathf.Sin(moveTimer) * moveRange;

        // Vertical: smoothly follow tower, only upwards
        float desiredY = topBlock.position.y + (spawnHeightOffset - 0.5f);
        float targetY = holder.position.y;
        if (desiredY > holder.position.y)
        {
            targetY = Mathf.Lerp(holder.position.y, desiredY, Time.deltaTime * 5f);
        }

        // Apply new position to holder (visual)
        Vector3 newPos = new Vector3(
            initialHolderPos.x + offsetX,
            targetY,
            holder.position.z
        );
        holder.position = newPos;

        // --- Smooth follow for currently held block ---
        if (topBlock != null && topBlock.CompareTag("wBlock"))
        {
            Vector3 blockTargetPos = new Vector3(newPos.x, newPos.y - 0.7f, newPos.z);
            topBlock.position = Vector3.Lerp(topBlock.position, blockTargetPos, Time.deltaTime * 8f);
        }
    }

    public void OnBlockLanded(Transform landedBlock)
    {
        topBlock = landedBlock;

        // Enable wind after enough blocks
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
            float yOffset = -0.7f;
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
            blockScript.Initialize(this, autoDrop);

        if (cameraTarget != null)
            cameraTarget.SetTopBlock(newBlock.transform);

        // Show wind text for this block **before dropping**
        if (windEnabled && blockScript != null)
        {
            float displayForce = blockScript.WindForce;
            displayForce = displayForce > 0 ? 2.5f : (displayForce < 0 ? -2.5f : 0f);
            ShowWindForceText(displayForce);
        }
    }

    void Camofsetadd()
    {
        if (virtualCamera != null)
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

    // --- DOTween Wind Force Text ---
    public void ShowWindForceText(float force)
    {
        if (!windEnabled || windForceText == null) return;

        windForceText.text = force.ToString("F1") + " Force";

        windForceText.gameObject.SetActive(true);
        windForceText.alpha = 0f;
        windForceText.transform.localScale = Vector3.zero;

        Sequence s = DOTween.Sequence();
        s.AppendInterval(0.7f)
         .Append(windForceText.DOFade(1f, 0.25f))
         .Join(windForceText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
         .AppendInterval(0.6f)
         .Append(windForceText.DOFade(0f, 0.5f))
         .Join(windForceText.transform.DOScale(0.8f, 0.5f))
         .OnComplete(() => windForceText.gameObject.SetActive(false));
    }
}
