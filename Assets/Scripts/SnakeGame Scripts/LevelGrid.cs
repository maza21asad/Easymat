using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private RectTransform goldenAppleCircleRect; // store rect for easy positioning

    private Image goldenAppleCircleUI;

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
        //if (goldenAppleActive)
        //{
        //    goldenAppleTimer -= Time.deltaTime;

        //    if (goldenAppleCircleUI != null)
        //    {
        //        //goldenAppleCircleUI.fillAmount = goldenAppleTimer / goldenAppleDuration;
        //        // Update fill
        //        goldenAppleCircleUI.fillAmount = Mathf.Clamp01(goldenAppleTimer / goldenAppleDuration);

        //        // Update UI position (follows apple)
        //        //Vector3 worldPos = goldenAppleObject.transform.position;
        //        ////Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        //        //Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        //        //goldenAppleCircleUI.rectTransform.position = screenPos;
        //    }

        //    if (goldenAppleTimer <= 0f)
        //    {
        //        Object.Destroy(goldenAppleObject);
        //        goldenAppleActive = false;

        //        if (goldenAppleCircleUI != null)
        //            GameObject.Destroy(goldenAppleCircleUI.gameObject);
        //    }
        //}

        //================================

        //if (goldenAppleActive)
        //{
        //    goldenAppleTimer -= Time.deltaTime;

        //    // Update fill
        //    if (goldenAppleCircleUI != null)
        //    {
        //        goldenAppleCircleUI.fillAmount = Mathf.Clamp01(goldenAppleTimer / goldenAppleDuration);
        //    }

        //    // Position the UI above the apple (robust conversion to canvas space)
        //    if (goldenAppleCircleUI != null && goldenAppleObject != null)
        //    {
        //        Canvas canvas = GameHandler.instance.MainCanvas;
        //        if (canvas == null)
        //        {
        //            Debug.LogWarning("MainCanvas is null on GameHandler. Assign the Canvas in the inspector.");
        //        }
        //        else if (Camera.main == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        //        {
        //            Debug.LogWarning("Camera.main is null and Canvas is not ScreenSpace-Overlay. Assign a camera or set Canvas to Overlay.");
        //        }
        //        else
        //        {
        //            // world -> screen
        //            Vector3 worldPos = goldenAppleObject.transform.position;
        //            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);

        //            // convert screen point to local point in canvas
        //            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        //            Vector2 localPoint;
        //            bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //                canvasRect, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main, out localPoint);

        //            if (ok)
        //            {
        //                RectTransform uiRT = goldenAppleCircleUI.rectTransform;

        //                // Ensure pivot/anchor so anchoredPosition works predictably
        //                uiRT.pivot = new Vector2(0.5f, 0.5f);
        //                uiRT.anchorMin = uiRT.anchorMax = new Vector2(0.5f, 0.5f);

        //                // place UI slightly above the apple (offset in local canvas space)
        //                float yOffsetPixels = 10f; // adjust visually
        //                uiRT.anchoredPosition = localPoint + new Vector2(0f, yOffsetPixels);
        //            }
        //            else
        //            {
        //                // fallback: try direct screen-to-world assignment (less reliable)
        //                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        //                goldenAppleCircleUI.rectTransform.position = screenPos;
        //            }
        //        }
        //    }

        //    // Timer finished
        //    if (goldenAppleTimer <= 0f)
        //    {
        //        if (goldenAppleObject != null) Object.Destroy(goldenAppleObject);
        //        goldenAppleActive = false;

        //        if (goldenAppleCircleUI != null)
        //            GameObject.Destroy(goldenAppleCircleUI.gameObject);

        //        goldenAppleCircleUI = null;
        //    }
        //}

        if (goldenAppleActive)
        {
            goldenAppleTimer -= Time.deltaTime;

            if (goldenAppleCircleUI != null)
                goldenAppleCircleUI.fillAmount = goldenAppleTimer / goldenAppleDuration;

            if (goldenAppleTimer <= 0f)
            {
                Object.Destroy(goldenAppleObject);
                goldenAppleActive = false;

                if (goldenAppleCircleUI != null)
                    GameObject.Destroy(goldenAppleCircleUI.gameObject);
            }
        }



        //==========================================

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

        //goldenAppleCircleUI = CreateGoldenAppleCircleUI();
        //goldenAppleCircleUI.fillAmount = 1f;

        goldenAppleCircleUI = CreateWorldTimer(GameAssets.Instance.circleSprite, new Color(0f, 1f, 0f, 1f), 1.2f, goldenAppleObject.transform);
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
            if (redAppleEatCount % 2 == 0 && !goldenAppleActive)
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
            if (redAppleEatCount % 12 == 0 && !metalAppleActive)
            {
                SpawnMetalApple();
            }

            return true; // grow
        }

        if (goldenAppleActive && snakeGridPosition == goldenApplePosition)
        {
            Object.Destroy(goldenAppleObject);
            goldenAppleActive = false;

            if (goldenAppleCircleUI != null)
            {
                GameObject.Destroy(goldenAppleCircleUI.gameObject);
                goldenAppleCircleUI = null;
                goldenAppleCircleRect = null;
            }

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

    private Image CreateWorldTimer(Sprite circleSprite, Color color, float size, Transform followTarget)
    {
        // Create parent canvas (World Space)
        GameObject root = new GameObject("WorldTimerUI");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = -1; // BEHIND THE APPLE

        // Create image
        GameObject imgObj = new GameObject("TimerCircle");
        imgObj.transform.SetParent(root.transform, false);

        Image img = imgObj.AddComponent<Image>();
        img.sprite = circleSprite;
        img.color = color;

        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillClockwise = false;
        img.fillAmount = 1f;

        // Size the circle (world units)
        RectTransform rt = img.rectTransform;
        rt.sizeDelta = new Vector2(size, size);

        // Parent to the apple so it follows automatically
        root.transform.SetParent(followTarget, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localScale = Vector3.one;

        return img;
    }
    //private Image CreateGoldenAppleCircleUI()
    //{
    //    // Create UI under main canvas
    //    GameObject uiObj = new GameObject("GoldenAppleTimerCircle");
    //    uiObj.transform.SetParent(GameHandler.instance.MainCanvas.transform, false);

    //    Image img = uiObj.AddComponent<Image>();
    //    img.sprite = GameAssets.Instance.circleSprite;
    //    img.color = new Color(1f, 0.92f, 0.2f, 0.95f);

    //    img.type = Image.Type.Filled;
    //    img.fillMethod = Image.FillMethod.Radial360;
    //    img.fillOrigin = (int)Image.Origin360.Top;
    //    img.fillClockwise = false;
    //    img.fillAmount = 1f;

    //    RectTransform rt = img.rectTransform;
    //    rt.sizeDelta = new Vector2(60, 60);

    //    return img;
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
