using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas-based SnakeController that preserves original grid movement + move-history logic,
/// rotating body segments (straight / corner) and tail. Uses GameGrid.GridToUI(...) for UI positions.
/// </summary>
public class SnakeController : MonoBehaviour
{
    private enum Direction { Left, Right, Up, Down }
    private enum State { Alive, Dead }

    [Header("References (UI)")]
    public GameGrid grid;                         // GameGrid component (for GridToUI & grid bounds)
    public SnakeInputController input;                 // Swipe / keyboard input
    public RectTransform headRect;                // UI Image RectTransform for head
    public RectTransform bodyContainer;           // Parent for body segment instances
    public GameObject bodyPrefab;                 // Prefab with Image + RectTransform
    public RectTransform tailRectPrefab;          // Optional: prefab for tail Image (RectTransform). Or can be created at runtime.
    public RectTransform foodRect;                // Food UI RectTransform (Image)

    [Header("Sprites")]
    public Sprite bodyStraightSprite;
    public Sprite bodyCornerSprite;
    public Sprite tailSprite;

    [Header("Movement")]
    public float snakeMoveSpeed = 2f;    // multiplier for speed
    public float gridMoveTimerMax = 0.2f;

    // internal state
    private State state;
    private Vector2Int gridPosition; // head grid position
    private Direction gridMoveDirection;

    private float gridMoveTimer;
    private List<SnakeMovePosition> snakeMovePositionList;
    private List<SnakeBodyPart> snakeBodyPartList;
    private int snakeBodySize;

    // tail UI instance
    private RectTransform tailRect;
    private Image tailImage;

    [Header("Game Over")]
    public SnakeGameOverUI gameOverUI; // optional

    private void Awake()
    {
        // initialize
        gridPosition = new Vector2Int(grid.gridWidth / 2, grid.gridHeight / 2);
        gridMoveDirection = Direction.Right;
        gridMoveTimer = 0f;
        snakeMovePositionList = new List<SnakeMovePosition>();
        snakeBodyPartList = new List<SnakeBodyPart>();
        snakeBodySize = 0;
        state = State.Alive;

        // create tail UI
        if (tailRectPrefab != null)
        {
            // instantiate tail under same parent as bodyContainer for ordering
            RectTransform created = Instantiate(tailRectPrefab, bodyContainer);
            created.name = "SnakeTail";
            tailRect = created;
            tailImage = tailRect.GetComponent<Image>();
            if (tailImage != null && tailSprite != null)
            {
                tailImage.sprite = tailSprite;
            }
            // draw behind everything by setting sibling index to 0 (or set Canvas order)
            tailRect.SetAsFirstSibling();
        }
        else
        {
            // create a simple tail if no prefab provided
            GameObject g = new GameObject("SnakeTail", typeof(RectTransform), typeof(Image));
            g.transform.SetParent(bodyContainer, false);
            tailRect = g.GetComponent<RectTransform>();
            tailImage = g.GetComponent<Image>();
            if (tailSprite != null) tailImage.sprite = tailSprite;
            tailRect.SetAsFirstSibling();
        }
    }

    private void Start()
    {
        // place visuals correctly
        UpdateVisualsImmediate();
    }

    private void Update()
    {
        if (state == State.Alive)
        {
            HandleInput();        // reads InputController
            HandleGridMovement(); // timed discrete movement
        }
    }

    private void HandleInput()
    {
        // Map InputController.CurrentDirection (Vector2Int) to Direction enum
        Vector2Int dir = input.CurrentDirection;

        Direction newDir = gridMoveDirection;

        if (dir == Vector2Int.up) newDir = Direction.Up;
        else if (dir == Vector2Int.down) newDir = Direction.Down;
        else if (dir == Vector2Int.left) newDir = Direction.Left;
        else if (dir == Vector2Int.right) newDir = Direction.Right;

        // Prevent reversing
        if (!IsOppositeDirection(newDir, gridMoveDirection))
            gridMoveDirection = newDir;
    }

    private bool IsOppositeDirection(Direction a, Direction b)
    {
        return (a == Direction.Up && b == Direction.Down) ||
               (a == Direction.Down && b == Direction.Up) ||
               (a == Direction.Left && b == Direction.Right) ||
               (a == Direction.Right && b == Direction.Left);
    }

    private void HandleGridMovement()
    {
        gridMoveTimer += Time.deltaTime * snakeMoveSpeed;
        if (gridMoveTimer < gridMoveTimerMax) return;
        gridMoveTimer -= gridMoveTimerMax;

        SnakeMovePosition previous = (snakeMovePositionList.Count > 0) ? snakeMovePositionList[0] : null;
        SnakeMovePosition newMove = new SnakeMovePosition(previous, gridPosition, gridMoveDirection);
        snakeMovePositionList.Insert(0, newMove);

        Vector2Int dirVec = DirectionToVector(gridMoveDirection);
        gridPosition += dirVec;

        // clamp to grid bounds (keeps inside panel). If you want wrapping, change here.
        gridPosition.x = Mathf.Clamp(gridPosition.x, 0, grid.gridWidth - 1);
        gridPosition.y = Mathf.Clamp(gridPosition.y, 0, grid.gridHeight - 1);

        // Check food
        Vector2Int foodGridPos = GridIndexFromUI(foodRect);
        if (gridPosition == foodGridPos)
        {
            snakeBodySize++;
            CreateSnakeBody();
            //MoveFoodToRandom();
            //GameManager.Instance.AddScore(1);
        }

        // Trim move history to size
        if (snakeMovePositionList.Count >= snakeBodySize + 1)
        {
            snakeMovePositionList.RemoveAt(snakeMovePositionList.Count - 1);
        }

        // Self collision (head vs body)
        foreach (var b in snakeBodyPartList)
        {
            if (gridPosition == b.GetGridPosition())
            {
                state = State.Dead;
                if (gameOverUI != null) gameOverUI.ShowGameOver();
                return;
            }
        }

        // Update head UI & rotation
        UpdateVisualsImmediate();
    }

    private void UpdateVisualsImmediate()
    {
        // Head
        headRect.anchoredPosition = grid.GridToUI(gridPosition);
        headRect.localEulerAngles = Vector3.forward * AngleFromDirection(gridMoveDirection);

        UpdateSnakeBodyPartsUI();

        // Tail: place and rotate behind last body part or head if none
        if (snakeBodyPartList.Count > 0)
        {
            SnakeBodyPart last = snakeBodyPartList[snakeBodyPartList.Count - 1];
            Vector2Int tailGridPos = last.GetGridPosition();
            Direction lastDir = last.GetDirection();
            Vector2Int offset = DirectionToVectorInverse(lastDir);
            tailGridPos += offset;
            tailRect.anchoredPosition = grid.GridToUI(tailGridPos);
            tailRect.localEulerAngles = Vector3.forward * AngleFromDirection(lastDir);
        }
        else
        {
            Vector2Int tailGridPos = gridPosition + DirectionToVectorInverse(gridMoveDirection);
            tailRect.anchoredPosition = grid.GridToUI(tailGridPos);
            tailRect.localEulerAngles = Vector3.forward * AngleFromDirection(gridMoveDirection);
        }
    }

    private void CreateSnakeBody()
    {
        // Add data and instantiate a UI body segment at the end
        SnakeBodyPart newPart = new SnakeBodyPart(snakeBodyPartList.Count, bodyPrefab, bodyContainer, bodyStraightSprite, bodyCornerSprite, grid);
        snakeBodyPartList.Add(newPart);
    }

    private void UpdateSnakeBodyPartsUI()
    {
        // ensure we have instantiated UI children equal to bodyGrid count
        for (int i = 0; i < snakeBodyPartList.Count; i++)
        {
            if (i >= snakeMovePositionList.Count) break;
            snakeBodyPartList[i].SetSnakeMovePosition(snakeMovePositionList[i]);
        }
    }

    // Convert a UI anchoredPosition of foodRect back to grid index (approx)
    private Vector2Int GridIndexFromUI(RectTransform uiRect)
    {
        Vector2 anchored = uiRect.anchoredPosition;
        // inverse calculation of GridToUI
        // panelOrigin was calculated inside GameGrid.GridToUI logic: uses centered origin.
        float halfCell = grid.cellSize / 2f;
        // compute grid origin (same as in GameGrid)
        Vector2 panelOrigin = -new Vector2((grid.gridWidth * grid.cellSize) / 2f, (grid.gridHeight * grid.cellSize) / 2f);
        float fx = (anchored.x - panelOrigin.x - halfCell) / grid.cellSize;
        float fy = (anchored.y - panelOrigin.y - halfCell) / grid.cellSize;
        return new Vector2Int(Mathf.RoundToInt(fx), Mathf.RoundToInt(fy));
    }

    private Vector2Int DirectionToVector(Direction d)
    {
        switch (d)
        {
            case Direction.Up: return new Vector2Int(0, 1);
            case Direction.Down: return new Vector2Int(0, -1);
            case Direction.Left: return new Vector2Int(-1, 0);
            case Direction.Right: return new Vector2Int(1, 0);
        }
        return Vector2Int.right;
    }

    private Vector2Int DirectionToVectorInverse(Direction d) // offset to put tail behind segment
    {
        switch (d)
        {
            case Direction.Up: return Vector2Int.down;
            case Direction.Down: return Vector2Int.up;
            case Direction.Left: return Vector2Int.right;
            case Direction.Right: return Vector2Int.left;
        }
        return Vector2Int.left;
    }

    private float AngleFromDirection(Direction d)
    {
        switch (d)
        {
            case Direction.Up: return 90f;
            case Direction.Down: return 270f;
            case Direction.Left: return 180f;
            case Direction.Right: return 0f;
        }
        return 0f;
    }

    // === Nested classes to mimic your original structure ===

    // One move record
    private class SnakeMovePosition
    {
        public SnakeMovePosition previous;
        private Vector2Int gridPosition;
        private Direction direction;

        public SnakeMovePosition(SnakeMovePosition previous, Vector2Int pos, Direction dir)
        {
            this.previous = previous;
            this.gridPosition = pos;
            this.direction = dir;
        }

        public Vector2Int GetGridPosition() => gridPosition;
        public Direction GetDirection() => direction;
        public Direction GetPreviousDirection()
        {
            if (previous == null) return Direction.Right;
            return previous.direction;
        }
    }

    // Single UI body part wrapper (keeps track of its UI instance + sprite)
    private class SnakeBodyPart
    {
        private SnakeMovePosition snakeMovePosition;
        private RectTransform rect;
        private Image img;
        private Sprite straight;
        private Sprite corner;
        private GameGrid gridRef;
        private Direction lastKnownDirection = Direction.Right;

        public SnakeBodyPart(int index, GameObject bodyPrefab, RectTransform parent, Sprite straightSprite, Sprite cornerSprite, GameGrid grid)
        {
            straight = straightSprite;
            corner = cornerSprite;
            gridRef = grid;

            GameObject go = Object.Instantiate(bodyPrefab, parent);
            go.name = "SnakeBody_" + index;
            rect = go.GetComponent<RectTransform>();
            img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            if (straight != null) img.sprite = straight;
        }

        public void SetSnakeMovePosition(SnakeMovePosition mp)
        {
            snakeMovePosition = mp;
            Vector2Int gp = mp.GetGridPosition();
            rect.anchoredPosition = gridRef.GridToUI(gp);

            Direction dir = mp.GetDirection();
            Direction prevDir = mp.GetPreviousDirection();

            // Straight vs corner
            if (dir == prevDir)
            {
                img.sprite = straight;
                float angle = 0f;
                if (dir == Direction.Up || dir == Direction.Down) angle = 90f;
                else angle = 0f;
                rect.localEulerAngles = Vector3.forward * angle;
            }
            else
            {
                img.sprite = corner;
                float angle = 0f;

                // replicate your corner mapping
                if (dir == Direction.Up)
                {
                    angle = (prevDir == Direction.Left) ? 90f : 180f; // left->up, right->up
                }
                else if (dir == Direction.Down)
                {
                    angle = (prevDir == Direction.Left) ? 0f : 270f;
                }
                else if (dir == Direction.Left)
                {
                    angle = (prevDir == Direction.Up) ? 270f : 180f;
                }
                else if (dir == Direction.Right)
                {
                    angle = (prevDir == Direction.Up) ? 0f : 90f;
                }

                rect.localEulerAngles = Vector3.forward * angle;
            }

            lastKnownDirection = dir;
        }

        public Vector2Int GetGridPosition()
        {
            return snakeMovePosition != null ? snakeMovePosition.GetGridPosition() : new Vector2Int(-999, -999);
        }

        public Direction GetDirection()
        {
            return snakeMovePosition != null ? snakeMovePosition.GetDirection() : Direction.Right;
        }
    }
}
