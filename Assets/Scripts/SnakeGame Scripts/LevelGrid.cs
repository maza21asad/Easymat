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

            // Every 30 red apples → Diamond apple 10% chance
            if (redAppleEatCount % 30 == 0 && !diamondAppleActive)
            {
                float chance = Random.value; // 0.0 to 1.0
                if (chance <= 0.10f) // 10% chance
                {
                    SpawnDiamondApple();
                }
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
