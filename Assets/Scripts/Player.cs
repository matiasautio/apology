using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public float walkVelocity = 7f;
    public float jumpVelocity;
    public float runMultiplier;
    public float bounceVelocity;
    public float gravity;
    public LayerMask wallMask;
    public LayerMask floorMask;

    private Vector2 velocity;

    // Using the Input system
    private PlayerInputActions inputActions;
    bool walk;
    bool walk_left;
    bool walk_right;
    bool jump;
    private Vector2 moveInput;

    // Raycasting
    const float groundRayLength = 0.05f;
    // Player height for raycasting
    // Get the collider box size, we are gonan use it to set the ray origin
    private BoxCollider2D box;
    float boxColliderheight;
    float boxColliderWidth;


    private void Awake()
    {
        inputActions = new PlayerInputActions();
        // Set the height
        box = GetComponent<BoxCollider2D>();
        boxColliderheight = box.size.y * 0.5f; // make it half the size
        boxColliderWidth = box.size.x * 0.5f;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled  += ctx => moveInput = Vector2.zero;
        // Setting callbacks for jumping
        inputActions.Player.Jump.performed += ctx => jump = true;
        inputActions.Player.Jump.canceled  += ctx => jump = false;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    public enum PlayerState {
        jumping,
        idle,
        walking,
        bouncing,
        running,
        dead
    }

    private PlayerState playerState = PlayerState.idle;
    private bool grounded = false;
    private bool bounce = false;

    void Start() {
        Fall();
    }

    void Update() {
        CheckPlayerInput ();
        UpdatePlayerPosition ();
        //UpdateAnimationStates ();
    }

    public void Dead () {
        playerState = PlayerState.dead;
        GetComponent<Animator>().SetBool("isDead", true);
        GetComponent<Collider2D>().enabled = false;
    }

    void UpdatePlayerPosition()
    {
        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        // --- HORIZONTAL MOVEMENT ---
        float moveSpeed = 0f;

        if (walk)
        {
            if (walk_left)
                moveSpeed = -walkVelocity;
            else if (walk_right)
                moveSpeed = walkVelocity;
            if (inputActions.Player.Sprint.IsPressed())
                moveSpeed *= runMultiplier;
            pos.x += moveSpeed * Time.deltaTime;
            // Direction from movement, not scale
            float direction = Mathf.Sign(moveSpeed);
            pos = CheckWallRays(pos, direction);
        }

        // --- JUMP ---
        if (jump && grounded)
        {
            velocity.y = jumpVelocity;
            grounded = false;
            playerState = PlayerState.jumping;
        }

        // --- VERTICAL MOVEMENT ---
        if (playerState == PlayerState.jumping || playerState == PlayerState.bouncing)
        {
            pos.y += velocity.y * Time.deltaTime;
            velocity.y -= gravity * Time.deltaTime;
        }

        if (bounce && playerState != PlayerState.bouncing)
        {
            playerState = PlayerState.bouncing;
            velocity.y = bounceVelocity;
        }

        if (playerState == PlayerState.dead)
        {
            velocity = Vector2.zero;
        }

        // --- COLLISIONS ---
        if (velocity.y <= 0)
            pos = CheckFloorRays(pos);

        if (velocity.y >= 0)
            pos = CheckCeilingRays(pos);

        transform.localPosition = pos;
        transform.localScale = scale;
    }

    void UpdateAnimationStates () {

        if (grounded && !walk && !bounce) {

            GetComponent<Animator>().SetBool("isJumping", false);
            GetComponent<Animator>().SetBool("isRunning", false);        
        }

        if (grounded && walk) {

            GetComponent<Animator>().SetBool("isJumping", false);
            GetComponent<Animator>().SetBool("isRunning", true);
        }

        if (playerState == PlayerState.jumping) {

            GetComponent<Animator>().SetBool("isJumping", true);
            GetComponent<Animator>().SetBool("isRunning", false);

        }
    }

    void CheckPlayerInput()
    {
        bool input_left  = moveInput.x < 0f;
        bool input_right = moveInput.x > 0f;

        walk = input_left || input_right;
        walk_left = input_left && !input_right;
        walk_right = !input_left && input_right;
    }

    Vector3 CheckWallRays(Vector3 pos, float direction)
    {
        // Only block if moving into wall
        if (Mathf.Abs(velocity.x) < 0.001f)
           return pos;

        if (Mathf.Sign(velocity.x) != Mathf.Sign(direction))
            return pos;

        float rayLength = groundRayLength;
        float halfHeight = boxColliderheight;
        float halfWidth  = boxColliderWidth;

        Vector2 originTop = new Vector2(
            pos.x + direction * halfWidth,
            pos.y + halfHeight - 0.1f
        );

        Vector2 originMiddle = new Vector2(
            pos.x + direction * halfWidth,
            pos.y
        );

        Vector2 originBottom = new Vector2(
            pos.x + direction * halfWidth,
            pos.y - halfHeight + 0.1f
        );

        RaycastHit2D hitTop    = Physics2D.Raycast(originTop,    Vector2.right * direction, rayLength, wallMask);
        RaycastHit2D hitMiddle = Physics2D.Raycast(originMiddle, Vector2.right * direction, rayLength, wallMask);
        RaycastHit2D hitBottom = Physics2D.Raycast(originBottom, Vector2.right * direction, rayLength, wallMask);

        RaycastHit2D hit = hitMiddle.collider ? hitMiddle :
                        hitTop.collider ? hitTop :
                        hitBottom;

        if (hit.collider)
        {
            // Clamp to wall
            if (direction > 0)
                pos.x = hit.point.x - halfWidth;
            else
                pos.x = hit.point.x + halfWidth;

            // Stop pushing into wall
            //if (grounded)
                //moveSpeed = 0;
        }

        return pos;
    }



    Vector3 CheckFloorRays(Vector3 pos)
    {
        // If player is jumping, we don't need to check the ground rays
        if (velocity.y > 0)
        {
            grounded = false;
            return pos;
        }
        Vector2 originLeft = new Vector2(pos.x - 0.3f, pos.y - boxColliderheight);
        Vector2 originMiddle = new Vector2(pos.x, pos.y - boxColliderheight);
        Vector2 originRight = new Vector2(pos.x + 0.3f, pos.y - boxColliderheight);

        RaycastHit2D hitLeft   = Physics2D.Raycast(originLeft, Vector2.down, groundRayLength, floorMask);
        RaycastHit2D hitMiddle = Physics2D.Raycast(originMiddle, Vector2.down, groundRayLength, floorMask);
        RaycastHit2D hitRight  = Physics2D.Raycast(originRight, Vector2.down, groundRayLength, floorMask);
        //Debug.DrawRay(originMiddle, Vector2.down * groundRayLength, Color.red);

        RaycastHit2D hit = hitLeft.collider ? hitLeft :
                        hitMiddle.collider ? hitMiddle :
                        hitRight;

        if (hit.collider && velocity.y <= 0)
        {
            grounded = true;
            velocity.y = 0;
            // Snap to surface using extents, not bounds center
            pos.y = hit.point.y + boxColliderheight;

            if (hit.collider.CompareTag("Enemy"))
            {
                bounce = true;
            }

            if (playerState == PlayerState.jumping)
                playerState = PlayerState.idle;
        }
        else
        {
            grounded = false;

            if (playerState != PlayerState.jumping)
                Fall();
        }

        return pos;
    }


    Vector3 CheckCeilingRays(Vector3 pos)
    {
        float rayLength = groundRayLength;
        float halfHeight = boxColliderheight;
        float halfWidth  = boxColliderWidth;

        Vector2 originLeft = new Vector2(
            pos.x - halfWidth + 0.1f,
            pos.y + halfHeight
        );

        Vector2 originMiddle = new Vector2(
            pos.x,
            pos.y + halfHeight
        );

        Vector2 originRight = new Vector2(
            pos.x + halfWidth - 0.1f,
            pos.y + halfHeight
        );

        RaycastHit2D hitLeft   = Physics2D.Raycast(originLeft,   Vector2.up, rayLength, floorMask);
        RaycastHit2D hitMiddle = Physics2D.Raycast(originMiddle, Vector2.up, rayLength, floorMask);
        RaycastHit2D hitRight  = Physics2D.Raycast(originRight,  Vector2.up, rayLength, floorMask);

        RaycastHit2D hit = hitMiddle.collider ? hitMiddle :
                        hitLeft.collider ? hitLeft :
                        hitRight;

        if (hit.collider)
        {
            // Cancel upward movement
            if (velocity.y > 0)
                velocity.y = 0;

            // Clamp just below ceiling
            pos.y = hit.point.y - halfHeight;

            if (hit.collider.CompareTag("QuestionBlock"))
            {
                hit.collider.GetComponent<QuestionBlock>()
                    .QuestionBlockBounce();
            }

            Fall(); // transition to falling
        }

        return pos;
    }

    

    void Fall (){

        velocity.y = 0;

        playerState = PlayerState.jumping;

        bounce = false;
        grounded = false;

    }
}
