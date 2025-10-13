using UnityEngine;
using System;

public class BlockCollision : MonoBehaviour
{
    public static event Action<bool> OnBlockLanded; // true = game over

    private bool landed = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (landed) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            landed = true;
            OnBlockLanded?.Invoke(true); // game over
        }
        else if (collision.gameObject.CompareTag("Block"))
        {
            landed = true;
            OnBlockLanded?.Invoke(false); // landed on block successfully
        }
    }
}
