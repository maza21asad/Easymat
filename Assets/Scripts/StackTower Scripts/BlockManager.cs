using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BlockManager : MonoBehaviour
{
    [Header("References")]
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public float spawnHeightOffset = 3f;
    public CameraTarget cameraTarget;

    [Header("Score Settings")]
    public int score = 0;
    public TMP_Text scoreText;

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
    private bool isTimerRunning = false;
    public TMP_Text timerText;

    [Header("Wind Settings")]
    public bool windEnabled = false;
    public int windStartAfter = 10;

    [Header("Wind UI")]
    public TMP_Text windForceText;

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Wind Particles")]
    public ParticleSystem windParticles;

    public CinemachineVirtualCamera virtualCamera;
    public float moveSpeed = 1f;
    public float moveRange = 2f;

    private Vector3 initialHolderPos;
    private float moveTimer = 0f;

    [Header("Final Score UI")]
    public TMP_Text finalScoreText;

    [Header("Initial Block Timer")]
    public float firstBlockLimit = 30f;
    private float firstBlockTimer = 0f;
    private bool isFirstBlockTimerRunning = true;
    private int landedBlockCount = 0;

    [Header("Sound Settings")]
    public AudioSource fallSound;
    public AudioSource gameOverSound;
    public AudioSource windSound;

    [Header("Wind Force Settings")]
    public float leftForce = -2.5f;
    public float rightForce = 2.5f;

    [Header("Perfect Placement Settings")]
    public float snapThreshold = 0.4f;
    public float failThreshold = 1.2f;

    // ------------------ FLOOD SETTINGS ------------------
    [Header("Flood Settings")]
    public GameObject floodObject;      // Your flood sprite
    public Animator floodAnimator;      // Flood animator
    // ----------------------------------------------------

    private void Start()
    {
        initialHolderPos = holder.position;

        timer = gameTime;
        isTimerRunning = false;

        if (floodObject != null)
            floodObject.SetActive(false);

        SpawnBlock(autoDrop: true);
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (scoreText != null)
            scoreText.text = "" + score;
    }

    private void Update()
    {
        // ---------------------- MAIN GAME TIMER ----------------------
        if (isTimerRunning)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
                timerText.text = Mathf.Ceil(timer).ToString();

            if (timer <= 0)
            {
                isTimerRunning = false;
                timerText.text = "Time: 0";

                // SHOW FLOOD WHEN TIMER ENDS
                if (floodObject != null)
                    floodObject.SetActive(true);

                if (floodAnimator != null)
                    floodAnimator.Play("flood");

                // Call EndGame to handle UI, score, and cleanup
                EndGame();

                canSpawn = false;
            }
        }

        // ---------------------- FIRST 30 SEC / 9 BLOCK TIMER ----------------------
        if (isFirstBlockTimerRunning)
        {
            firstBlockTimer += Time.deltaTime;

            if (firstBlockTimer >= firstBlockLimit)
            {
                /* * FIX: This is what caused the game to end early.
                * If you want the game to end here, keep this, but since the goal is to end at main timer, 
                * this logic is removed/commented out.
                */
                // if (landedBlockCount < 9)
                // {
                //     EndGame();
                // }

                isFirstBlockTimerRunning = false;
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

        moveTimer += Time.deltaTime * moveSpeed;
        float offsetX = Mathf.Sin(moveTimer) * moveRange;

        float desiredY = topBlock.position.y + (spawnHeightOffset - 0.5f);
        float targetY = holder.position.y;

        if (desiredY > holder.position.y)
            targetY = Mathf.Lerp(holder.position.y, desiredY, Time.deltaTime * 5f);

        holder.position = new Vector3(initialHolderPos.x + offsetX, targetY, holder.position.z);
    }

    public void OnBlockLanded(Transform landedBlock)
    {
        if (fallSound != null)
            fallSound.Play();

        landedBlockCount++;

        if (!isTimerRunning && landedBlockCount >= 1)
            isTimerRunning = true;

        if (isFirstBlockTimerRunning && landedBlockCount >= 9)
            isFirstBlockTimerRunning = false;

        if (topBlock != null)
        {
            float xDiff = landedBlock.position.x - topBlock.position.x;

            if (Mathf.Abs(xDiff) > failThreshold)
            {
                Rigidbody rb = landedBlock.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
                EndGame();
                return;
            }

            if (Mathf.Abs(xDiff) <= snapThreshold)
            {
                landedBlock.position = new Vector3(
                    topBlock.position.x,
                    landedBlock.position.y,
                    landedBlock.position.z
                );
            }
        }

        AddScore(10);
        topBlock = landedBlock;

        StartCoroutine(MoveHolderUp(holderStepUp, 0.3f));

        if (!windEnabled && blockCount >= windStartAfter)
            windEnabled = true;

        if (windSound != null && !windSound.isPlaying)
            windSound.Play();

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
        newBlock.transform.SetParent(holder);

        Block blockScript = newBlock.GetComponent<Block>();
        if (blockScript != null)
            blockScript.Initialize(this, autoDrop);

        if (cameraTarget != null)
            cameraTarget.SetTopBlock(newBlock.transform);

        if (windEnabled && blockScript != null)
        {
            RotateWindParticles(blockScript.WindForce);

            float displayForce = 0f;
            if (blockScript.WindForce > 0) displayForce = rightForce;
            else if (blockScript.WindForce < 0) displayForce = leftForce;

            ShowWindForceText(displayForce);
        }
    }

    private IEnumerator MoveHolderUp(float step, float delay)
    {
        yield return new WaitForSeconds(delay);
        holder.position += new Vector3(0, step, 0);
    }

    private void RotateWindParticles(float force)
    {
        if (windParticles == null) return;

        if (force > 0)
            windParticles.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (force < 0)
            windParticles.transform.rotation = Quaternion.Euler(0, 0, 180);
        else
            windParticles.Stop();

        if (!windParticles.isPlaying && force != 0)
            windParticles.Play();
    }

    void Camofsetadd()
    {
        if (virtualCamera != null)
            virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset.y += 0.5f;
    }

    public void EndGame()
    {
        if (gameOverSound != null)
            gameOverSound.Play();

        // ALSO PLAY FLOOD WHEN GAME ENDS BY FALLING (or time ran out)
        if (floodObject != null)
            floodObject.SetActive(true);

        if (floodAnimator != null)
            floodAnimator.Play("flood");

        if (gamePanel != null) gamePanel.SetActive(false);
        if (newPanel != null) newPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "" + score;

        canSpawn = false;
        isTimerRunning = false;

        if (holder != null)
            holder.gameObject.SetActive(false);

        Block[] allBlocks = FindObjectsOfType<Block>();
        foreach (var block in allBlocks)
            Destroy(block.gameObject);
    }

    public void ShowWindForceText(float force)
    {
        if (!windEnabled || windForceText == null) return;

        string forceText = "0 Force";
        if (force > 0) forceText = "Right Force";
        else if (force < 0) forceText = "Left Force";

        windForceText.text = forceText;

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

    public void OpenSettings()
    {
        canSpawn = false;
        StopAllCoroutines();
        isTimerRunning = false;

        Block[] allBlocks = FindObjectsOfType<Block>();
        foreach (var block in allBlocks)
            block.gameObject.SetActive(false);

        if (holder != null)
            holder.gameObject.SetActive(false);

        if (gamePanel != null)
            gamePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (gamePanel != null)
            gamePanel.SetActive(true);

        if (holder != null)
            holder.gameObject.SetActive(true);

        isTimerRunning = true;
        canSpawn = true;
    }
}