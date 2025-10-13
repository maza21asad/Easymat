using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered the trigger is a block
        if (other.gameObject.CompareTag("Block"))
        {
            Debug.Log("Game Over! A block hit the game over zone.");
            
        }
    }
}

