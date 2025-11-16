using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnakeHUDController : MonoBehaviour
{
    public Text scoreText;

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
