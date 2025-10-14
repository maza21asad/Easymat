using UnityEngine;

public class MovingBlockController : MonoBehaviour
{
    [Header("Movement")]
    public float amplitude = 2f;        // half width of movement from center
    public float speed = 2f;            // movement speed

    Rigidbody2D rb;
    Vector2 startPos;
    bool isFalling = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        // Start as kinematic so physics doesn't move it while we program movement
        rb.bodyType = RigidbodyType2D.Kinematic;
        // Freeze rotation (z)
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (isFalling) return;

        // Move left-right with PingPong
        float x = startPos.x + Mathf.PingPong(Time.time * speed, amplitude * 2) - amplitude;
        transform.position = new Vector3(x, transform.position.y, transform.position.z);

        // On left mouse click: drop the block
        if (Input.GetMouseButtonDown(0))
        {
            Drop();
        }
    }

    void Drop()
    {
        isFalling = true;
        // Switch to Dynamic so gravity affects it
        rb.bodyType = RigidbodyType2D.Dynamic;
        // Ensure gravity scale is set to desired value
        rb.gravityScale = 2f;
        // Allow rotation or keep frozen depending on design; keep frozen to avoid tipping:
        rb.freezeRotation = true;
    }
}
