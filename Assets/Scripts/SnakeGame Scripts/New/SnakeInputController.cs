using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeInputController : MonoBehaviour
{
    public Vector2Int CurrentDirection { get; private set; } = Vector2Int.up;

    private Vector2 touchStart;

    private void Update()
    {
        HandleKeyboard();
        HandleSwipe();
    }

    private void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            SetDirection(Vector2Int.up);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            SetDirection(Vector2Int.down);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SetDirection(Vector2Int.left);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SetDirection(Vector2Int.right);
    }

    private void HandleSwipe()
    {
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
            touchStart = t.position;

        if (t.phase == TouchPhase.Ended)
        {
            Vector2 delta = t.position - touchStart;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x > 50) SetDirection(Vector2Int.right);
                else if (delta.x < -50) SetDirection(Vector2Int.left);
            }
            else
            {
                if (delta.y > 50) SetDirection(Vector2Int.up);
                else if (delta.y < -50) SetDirection(Vector2Int.down);
            }
        }
    }

    private void SetDirection(Vector2Int direction)
    {
        // Prevent reversing
        if (direction == -CurrentDirection) return;
        CurrentDirection = direction;
    }
}
