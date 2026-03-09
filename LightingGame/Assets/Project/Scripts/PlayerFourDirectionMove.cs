using UnityEngine;

public class PlayerFourDirectionMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Sprite frontSprite;   // S
    public Sprite backSprite;    // W
    public Sprite leftSprite;    // A
    public Sprite rightSprite;   // D

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(moveX, moveY).normalized;

        if (moveX < 0)
        {
            sr.sprite = leftSprite;
        }
        else if (moveX > 0)
        {
            sr.sprite = rightSprite;
        }
        else if (moveY > 0)
        {
            sr.sprite = backSprite;
        }
        else if (moveY < 0)
        {
            sr.sprite = frontSprite;
        }
    }

    void FixedUpdate()
    {
        rb.velocity = moveInput * moveSpeed;
    }
}