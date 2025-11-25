using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid
{
    // Normal apple (Red apple)
    private Vector2Int redApplePosition;
    private GameObject redAppleObject;

    private int redAppleEatCount = 0;

    // Golden apple
    private Vector2Int goldenApplePosition;
    private GameObject goldenAppleObject;
    private float goldenAppleTimer;
    private float goldenAppleDuration = 5f; // 5 seconds
    private bool goldenAppleActive = false;

    // Diamond apple
    private Vector2Int diamondApplePosition;
    private GameObject diamondAppleObject;
    private bool diamondAppleActive = false;

    // Metal apple
    private Vector2Int metalApplePosition;
    private GameObject metalAppleObject;
    private bool metalAppleActive = false;
    private float metalAppleDuration = 10f;
    private float metalAppleTimer = 0f;
    private bool metalAppleWaiting = false; // triggered every 15 apples

    private int width;
    private int height;
    private Snake snake;

    public void Update()
    {
        if (goldenAppleActive)
        {
            goldenAppleTimer -= Time.deltaTime;

            if (goldenAppleTimer <= 0f)
            {
                Object.Destroy(goldenAppleObject);
                goldenAppleActive = false;
            }
        }

        // METAL APPLE countdown
        if (metalAppleActive)
        {
            metalAppleTimer -= Time.deltaTime;

            GameHandler.ShowMetalWarning("Eat the metal apple! Time left: " + metalAppleTimer.ToString("F1"));

            if (metalAppleTimer <= 0f)
            {
                metalAppleActive = false;
                Object.Destroy(metalAppleObject);

                GameHandler.HideMetalWarning();

                // === SNAKE GAME OVER (same as collision) ===
                snake.SendMessage("ForceGameOver");
            }
        }
    }


    public LevelGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public void Setup(Snake snake)
    {
        this.snake = snake;

        SpawnRedApple();
    }

    private void SpawnRedApple()
    {
        do
        {
            redApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(redApplePosition));

        redAppleObject = new GameObject("RedApple", typeof(SpriteRenderer));
        redAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.redAppleSprite;

        redAppleObject.transform.position = new Vector3(redApplePosition.x, redApplePosition.y, 0);
    }

    private void SpawnGoldenApple()
    {
        do
        {
            goldenApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(goldenApplePosition));

        goldenAppleObject = new GameObject("GoldenApple", typeof(SpriteRenderer));
        goldenAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.goldenAppleSprite;

        goldenAppleObject.transform.position = new Vector3(goldenApplePosition.x, goldenApplePosition.y, 0);

        goldenAppleActive = true;
        goldenAppleTimer = goldenAppleDuration;
    }

    private void SpawnDiamondApple()
    {
        do
        {
            diamondApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(diamondApplePosition));

        diamondAppleObject = new GameObject("DiamondApple", typeof(SpriteRenderer));
        diamondAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.diamondAppleSprite;

        diamondAppleObject.transform.position = new Vector3(diamondApplePosition.x, diamondApplePosition.y, 0);

        diamondAppleActive = true;
    }

    private void SpawnMetalApple()
    {
        do
        {
            metalApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(metalApplePosition));

        metalAppleObject = new GameObject("MetalApple", typeof(SpriteRenderer));
        metalAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.metalAppleSprite;

        metalAppleObject.transform.position = new Vector3(metalApplePosition.x, metalApplePosition.y, 0);

        metalAppleActive = true;
        metalAppleTimer = metalAppleDuration;

        GameHandler.ShowMetalWarning("Eat the Metal Apple in 7 seconds!");
    }


    public bool TrySnakeEatFood(Vector2Int snakeGridPosition)
    {
        // RED APPLE = Grow + 10 points
        if (snakeGridPosition == redApplePosition)
        {
            Object.Destroy(redAppleObject);
            SpawnRedApple();
            GameHandler.AddScore(10);

            redAppleEatCount++;

            // Every 5 red apples → Golden apple appears
            if (redAppleEatCount % 5 == 0 && !goldenAppleActive)
            {
                SpawnGoldenApple();
            }

            // Every 10 red apples → Diamond apple 50% chance
            if (redAppleEatCount % 10 == 0 && !diamondAppleActive)
            {
                float chance = Random.value; // 0.0 to 1.0
                if (chance <= 0.50f) // 50% chance
                {
                    SpawnDiamondApple();
                }
            }

            // Every 15 red apples → Metal apple MUST appear
            if (redAppleEatCount % 15 == 0 && !metalAppleActive)
            {
                SpawnMetalApple();
            }

            return true; // grow
        }

        // GOLDEN APPLE = 50 points, NO grow
        if (goldenAppleActive && snakeGridPosition == goldenApplePosition)
        {
            Object.Destroy(goldenAppleObject);
            goldenAppleActive = false;

            GameHandler.AddScore(50);

            return false; // no grow
        }

        // DIAMOND APPLE = 500 points, NO grow
        if (diamondAppleActive && snakeGridPosition == diamondApplePosition)
        {
            Object.Destroy(diamondAppleObject);
            diamondAppleActive = false;

            GameHandler.AddScore(500);

            return false; // no grow
        }

        if (metalAppleActive && snakeGridPosition == metalApplePosition)
        {
            Object.Destroy(metalAppleObject);
            metalAppleActive = false;

            GameHandler.HideMetalWarning();

            // Metal apple gives no score, no growth
            return false;
        }

        return false;
    }

    public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0)
        {
            gridPosition.x = width - 1;
        }
        if (gridPosition.x > width - 1)
        {
            gridPosition.x = 0;
        }
        if (gridPosition.y < 0)
        {
            gridPosition.y = height - 1;
        }
        if (gridPosition.y > height - 1)
        {
            gridPosition.y = 0;
        }
        return gridPosition;
    }
}
