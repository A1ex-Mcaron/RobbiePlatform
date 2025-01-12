using UnityEngine;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;

    [Header("移动参数")]
    public float speed = 8f;
    public float crouchSpeedDivisor = 3f;

    [Header("跳跃参数")]
    public float jumpForce = 6.3f;
    public float jumpHoldForce = 1.9f;
    public float jumpHoldDuration = 0.1f;
    public float crouchJumpBoost = 2.5f;

    float jumpTime;

    [Header("状态")]
    public bool isCrouch;
    public bool isOnGround;
    public bool isJump;
    public bool isHeadBlocked;

    [Header("环境监测")]
    public float footOffset = 0.4f;
    public float headCleearance = 0.5f;
    public float groundDistance = 0.2f;

    public LayerMask groundLayer;

    float xVelocity;

    //按键设置
    bool jumpPressed;
    bool jumpHeld;
    bool crouchHeld;


    //碰撞体尺寸
    Vector2 colliderStandSize;
    Vector2 colliderStandOffset;
    Vector2 colliderCrouchSize;
    Vector2 colliderCrouchOffset;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        colliderStandSize = coll.size;
        colliderStandOffset = coll.offset;
        colliderCrouchSize = new Vector2(coll.size.x, coll.size.y / 2f);
        colliderCrouchOffset = new Vector2(coll.offset.x, coll.offset.y - 0.45f);
    }


    void Update()
    {
        if (!jumpPressed)
        {
            jumpPressed = Input.GetButtonDown("Jump");
        }

        jumpHeld = Input.GetButton("Jump");
        crouchHeld = Input.GetButton("Crouch");
    }

    private void FixedUpdate()
    {
        PhysicsCheck();
        GroundMovement();
        MidAirmovement();
    }

    void PhysicsCheck()
    {
        //Vector2 pos = transform.position;
        //Vector2 offset = new Vector2(-footOffset, 0f);

        //RaycastHit2D leftCheck = Physics2D.Raycast(pos + offset, Vector2.down, groundDistance, groundLayer);
        //Debug.DrawRay(pos + offset, Vector2.down, Color.red, 0.2f);

        RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset, 0f), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, 0f), Vector2.down, groundDistance, groundLayer);


        if (leftCheck || rightCheck)
            isOnGround = true;
        else isOnGround = false;

        RaycastHit2D headCheck = Raycast(new Vector2(0f, coll.size.y), Vector2.up, headCleearance, groundLayer);

        if (headCheck)
            isHeadBlocked = true;
        else isHeadBlocked = false;

    }

    void GroundMovement()
    {
        if (crouchHeld && !isCrouch && isOnGround)
            Crouch();
        else if (!crouchHeld && isCrouch && !isHeadBlocked)
            StandUp();
        else if (!isOnGround && isCrouch)
            StandUp();


        xVelocity = Input.GetAxis("Horizontal");//-1f 1f

        if (isCrouch)
            xVelocity /= crouchSpeedDivisor;

        rb.linearVelocity = new Vector2(xVelocity * speed, rb.linearVelocity.y);

        FilpDirction();
    }

    void MidAirmovement()
    {
        if(jumpPressed && isOnGround && !isJump)
        {
            if(isCrouch && isOnGround)
            {
                //StandUp();
                rb.AddForce(new Vector2(0f, crouchJumpBoost), ForceMode2D.Impulse);
            }

            isOnGround = false;
            isJump = true;

            jumpTime = Time.time + jumpHoldDuration;

            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        else if (isJump)
        {
            if (jumpHeld)
            {
                rb.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);
                jumpPressed = false;
            }
            if (jumpTime < Time.time)
                isJump = false;
        }
    }

    void FilpDirction()
    {
        if (xVelocity < 0)
            transform.localScale = new Vector2(-1, 1);
        if (xVelocity > 0)
            transform.localScale = new Vector2(1, 1);
    }

    void Crouch()
    {
        isCrouch = true;
        coll.size = colliderCrouchSize;
        coll.offset = colliderCrouchOffset;
    }

    void StandUp()
    {
        isCrouch = false;
        coll.size = colliderStandSize;
        coll.offset = colliderStandOffset;
    }


    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDiraction, float length, LayerMask layer)
    {
        Vector2 pos = transform.position;

        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDiraction, length, layer);

        Color color = hit ? Color.red : Color.green;

        Debug.DrawRay(pos + offset, rayDiraction * length, color);

        return hit;
    }
}
