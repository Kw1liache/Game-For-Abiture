using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 9f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Спрайты девочки")]
    [SerializeField] private Sprite girlRightSprite;
    [SerializeField] private Sprite girlLeftSprite;

    [Header("Спрайты мальчика")]
    [SerializeField] private Sprite boyRightSprite;
    [SerializeField] private Sprite boyLeftSprite;

    [Header("Текущий персонаж")]
    [SerializeField] private int currentCharacter = 0;

    private bool isGrounded = false;
    private float moveInput = 0f;
    private bool jumpPressed = false;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Sprite currentRightSprite;
    private Sprite currentLeftSprite;

    private bool onClimbable = false;
    private bool isClimbing = false;

    private float climbPercentage = 0f;
    private Vector2 climbStart;
    private Vector2 climbEnd;
    private float climbSpeed = 0.5f;

    private float climbInput = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        if (groundCheck == null)
            groundCheck = transform.Find("GroundCheck");

        ChangeCharacter(currentCharacter);
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (onClimbable)
        {
            HandleClimb();
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
            isGrounded = false;
        }
    }

    private void Update()
    {
        if (moveInput > 0)
        {
            sprite.sprite = currentRightSprite;
        }
        else if (moveInput < 0)
        {
            sprite.sprite = currentLeftSprite;
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, checkRadius, groundLayer);
        isGrounded = colliders.Length > 0;
    }

    public void ChangeCharacter(int characterIndex)
    {
        if (characterIndex == 0)
        {
            currentCharacter = 0;
            currentRightSprite = girlRightSprite;
            currentLeftSprite = girlLeftSprite;
        }
        else if (characterIndex == 1)
        {
            currentCharacter = 1;
            currentRightSprite = boyRightSprite;
            currentLeftSprite = boyLeftSprite;
        }

        if (moveInput > 0)
            sprite.sprite = currentRightSprite;
        else if (moveInput < 0)
            sprite.sprite = currentLeftSprite;
        else
            sprite.sprite = currentRightSprite;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>().x;
    }

    public void OnJump(InputValue value)
    {
        if (onClimbable) return;

        if (value.isPressed && isGrounded)
        {
            jumpPressed = true;
        }
    }

    public void OnClimb(InputValue value)
    {
        climbInput = value.Get<Vector2>().y;
    }

    private void HandleClimb()
    {
        if (climbInput != 0)
        {
            isClimbing = true;

            climbPercentage += climbInput * climbSpeed * Time.fixedDeltaTime;
            climbPercentage = Mathf.Clamp01(climbPercentage);

            Vector2 targetPos = Vector2.Lerp(climbStart, climbEnd, climbPercentage);
            rb.MovePosition(targetPos);

            float ladderX = (climbStart.x + climbEnd.x) / 2f;

            if (transform.position.x < ladderX)
                sprite.flipX = false; // смотрит вправо
            else
                sprite.flipX = true;  // смотрит влево
        }
        else
        {
            isClimbing = false;
        }

        if (climbPercentage == 0f || climbPercentage == 1f)
        {
            isClimbing = false;
            onClimbable = false;
        }

        rb.gravityScale = isClimbing ? 0f : 1f;
    }

    public void SetClimbableData(bool onClimbable, Vector2 start, Vector2 end, bool isDown, float speed)
    {
        this.onClimbable = onClimbable;
        this.climbStart = start;
        this.climbEnd = end;
        this.climbSpeed = speed;

        climbPercentage = isDown ? 0f : 1f;
    }

    public void OffClimbable()
    {
        onClimbable = false;
        isClimbing = false;
        rb.gravityScale = 1f;
    }
}