using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance;

    [Header("Snake Sprites")]
    public Sprite snakeHeadSprite;
    public Sprite snakeBodySprite;
    public Sprite snakeBodyCornerSprite;
    public Sprite snakeTailSprite;

    [Header("Apple Sprites")]
    public Sprite redAppleSprite;

    private void Awake()
    {
        Instance = this;
    }
}