using UnityEngine;
using System;

public class BlockController : MonoBehaviour
{
    public static event Action<BlockController> OnBlockGrounded;

    [HideInInspector] public Rigidbody2D rb;

    bool grounded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("No Rigidbody2D on " + name);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // If we collide with ground or another block, mark as grounded
        if (grounded) return;

        if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Blocks"))
        {
            grounded = true;
            // tag this as a block so future blocks detect it
            gameObject.tag = "Blocks";

            // notify spawner (if any listeners)
            OnBlockGrounded?.Invoke(this);
        }
    }
}
