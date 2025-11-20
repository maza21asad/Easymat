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

    [Header("Holder Settings")]
    public float holderStepUp = 0.5f;

    [Header("Holder")]
    public Transform holder;

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
    public bool windEnabled = false;
    public int windStartAfter = 10;

    [Header("Wind UI")]
    public TMP_Text windForceText;

    public CinemachineVirtualCamera virtualCamera;

    public float moveSpeed = 1f;
    public float moveRange = 2f;

    private Vector3 initialHolderPos;
    private float moveTimer = 0f;

    private void Start()
    {
        initialHolderPos = holder.position;

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

                if (gamePanel != null) gamePanel.SetActive(false);
                if (newPanel != null) newPanel.SetActive(true);

                canSpawn = false;
            }
        }
    }

    private IEnumerator MoveHolderUp(float step, float delay)
    {
        yield return new WaitForSeconds(delay);

        holder.position += new Vector3(0, step, 0);
    }

    private void LateUpdate()
    {
        UpdateHolderMovement();
    }

    private void UpdateHolderMovement()
    {
        if (holder == null || topBlock == null) return;

        // Left-right motion
        moveTimer += Time.deltaTime * moveSpeed;
        float offsetX = Mathf.Sin(moveTimer) * moveRange;

        // Smooth vertical follow
        float desiredY = topBlock.position.y + (spawnHeightOffset - 0.5f);
        float targetY = holder.position.y;

        if (desiredY > holder.position.y)
            targetY = Mathf.Lerp(holder.position.y, desiredY, Time.deltaTime * 5f);

        holder.position = new Vector3(initialHolderPos.x + offsetX, targetY, holder.position.z);
    }

    public void OnBlockLanded(Transform landedBlock)
    {
        topBlock = landedBlock;

        StartCoroutine(MoveHolderUp(holderStepUp, 0.3f));

        if (!windEnabled && blockCount >= windStartAfter)
            windEnabled = true;

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

        float yOffset = -0.7f;
        spawnPoint.position = new Vector3(holder.position.x, holder.position.y + yOffset, holder.position.z);

        GameObject newBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);

        // ?? FIX: Parent to holder so no shaking
        newBlock.transform.SetParent(holder);

        Block blockScript = newBlock.GetComponent<Block>();
        if (blockScript != null)
            blockScript.Initialize(this, autoDrop);

        if (cameraTarget != null)
            cameraTarget.SetTopBlock(newBlock.transform);

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
        if (gamePanel != null) gamePanel.SetActive(false);
        if (newPanel != null) newPanel.SetActive(true);

        canSpawn = false;
        isTimerRunning = false;
    }

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
