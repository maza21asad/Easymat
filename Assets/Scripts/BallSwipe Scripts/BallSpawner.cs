using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform spawnPoint;
    public GameManager gameManager;

    private float spawnDelay = 1f; // base delay
    private float minSpawnDelay = 0.3f; // cap minimum delay
    private float lastSpawnTime;

    [Header("Ball Settings")]
    public float ballScale = 0.5f; // 🔹 Half size by default

    private void Update()
    {
        if (gameManager.currentBall == null && Time.time - lastSpawnTime >= spawnDelay)
        {
            SpawnBall();
        }
    }

    public BallController SpawnBall()
    {
        if (ballPrefab == null || spawnPoint == null)
            return null;

        GameObject go = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);

        // Resize
        go.transform.localScale *= ballScale;

        BallController bc = go.GetComponent<BallController>();
        if (bc == null) bc = go.AddComponent<BallController>();

        // Initialize with random color (takes care of mid balls after 40)
        bc.InitRandom();

        if (gameManager != null)
            gameManager.currentBall = bc;

        lastSpawnTime = Time.time;

        // Optional: keep spawn delay slower
        spawnDelay = 1f;

        return bc;
    }

}
