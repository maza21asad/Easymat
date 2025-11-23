using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid
{
    // Normal apple (Red apple)
    private Vector2Int redApplePosition;
    private GameObject redAppleObject;
    //private Vector2Int foodGridPosition;
    //private GameObject foodGameObject;

    private int redAppleEatCount = 0;

    // Golden apple
    private Vector2Int goldenApplePosition;
    private GameObject goldenAppleObject;
    private float goldenAppleTimer;
    private float goldenAppleDuration = 5f; // 5 seconds
    private bool goldenAppleActive = false;

    private int width;
    private int height;
    private Snake snake;

    // ==================================================== added new ============================================================
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

        //SpawnFood();
        SpawnRedApple();
        //SpawnGoldenAppleRandomly();
    }

    //private void SpawnFood()
    //{
    //    do
    //    {
    //        foodGridPosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
    //    }
    //    //while (snake.GetGridPosition() == foodGridPosition);
    //    while (snake.GetFullSnakeGridPositionList().IndexOf(foodGridPosition) != -1);

    //    foodGameObject = new GameObject("Food", typeof(SpriteRenderer));
    //    foodGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.redAppleSprite;
    //    foodGameObject.transform.position = new Vector3(foodGridPosition.x, foodGridPosition.y);
    //}
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

    // ==================================================== added new ============================================================
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
    //private void SpawnGoldenAppleRandomly()
    //{
    //    // Decide if golden apple should spawn (e.g., 25% chance)
    //    if (Random.value > 0.75f)
    //        return;

    //    do
    //    {
    //        goldenApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
    //    }
    //    while (snake.GetFullSnakeGridPositionList().Contains(goldenApplePosition));

    //    goldenAppleObject = new GameObject("GoldenApple", typeof(SpriteRenderer));
    //    goldenAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.goldenAppleSprite;

    //    goldenAppleObject.transform.position = new Vector3(goldenApplePosition.x, goldenApplePosition.y, 0);

    //    goldenAppleActive = true;
    //    goldenAppleTimer = goldenAppleDuration;
    //}


    //public bool TrySnakeEatFood(Vector2Int snakeGridPosition)
    //{
    //    if (snakeGridPosition == foodGridPosition)
    //    {
    //        Object.Destroy(foodGameObject);
    //        SpawnFood();
    //        GameHandler.AddScore();
    //        Debug.Log("Snake ate food");
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
    public bool TrySnakeEatFood(Vector2Int snakeGridPosition)
    {
        // RED APPLE (Grow + 10 points)
        if (snakeGridPosition == redApplePosition)
        {
            Object.Destroy(redAppleObject);
            SpawnRedApple();
            GameHandler.AddScore(10);

            redAppleEatCount++;

            // Every 5 red apples → spawn golden apple
            if (redAppleEatCount % 5 == 0 && !goldenAppleActive)
            {
                SpawnGoldenApple();
            }

            return true; // This tells Snake.cs to grow
        }

        // GOLDEN APPLE (No growth + 50 points)
        if (goldenAppleActive && snakeGridPosition == goldenApplePosition)
        {
            Object.Destroy(goldenAppleObject);
            goldenAppleActive = false;

            GameHandler.AddScore(50);

            return false; // IMPORTANT: tells Snake.cs NOT to grow
        }

        return false;
    }
    //public bool TrySnakeEatFood(Vector2Int snakeGridPosition)
    //{
    //    // Normal Red Apple
    //    if (snakeGridPosition == redApplePosition)
    //    {
    //        Object.Destroy(redAppleObject);
    //        SpawnRedApple();
    //        GameHandler.AddScore(10);  // Red apple = 10 points
    //        return true;
    //    }

    //    // Golden Apple
    //    if (goldenAppleActive && snakeGridPosition == goldenApplePosition)
    //    {
    //        Object.Destroy(goldenAppleObject);
    //        goldenAppleActive = false;
    //        GameHandler.AddScore(50);  // Golden apple = 50 points
    //        return true;
    //    }

    //    return false;
    //}


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
