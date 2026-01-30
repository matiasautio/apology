using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // These are set in the editor
    public float walkVelocity = 7f;
    public float jumpVelocity = 20f;
    public float runMultiplier = 2f;
    public float bounceVelocity = 10f;
    public float gravity = 65f;
    public LayerMask wallMask;
    public LayerMask floorMask;

    // The velocity of the character is modified by movement input (moving) and raycasts into colliders (stopping)
    private Vector2 velocity;

    // Sprite
    private SpriteRenderer sprite;
    private Animator animator;

    // Using the Input system
    private PlayerInputActions inputActions;
    bool walk;
    bool walk_left;
    bool walk_right;
    bool run;
    bool jump;
    private Vector2 moveInput;

    // Raycasting
    const float groundRayLength = 0.05f;
    // Player height for raycasting
    // Get the collider box size, we are gonan use it to set the ray origin
    private BoxCollider2D box;
    float boxColliderheight;
    float boxColliderWidth;

    // Items
    private int niwakas = 0;
    private int niwakaSenbeis = 0;


    private void Awake()
    {
        inputActions = new PlayerInputActions();
        // Set the height
        box = GetComponent<BoxCollider2D>();
        boxColliderheight = box.size.y * 0.5f; // make it half the size
        boxColliderWidth = box.size.x * 0.5f;
        // Set sprite
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled  += ctx => moveInput = Vector2.zero;
        // Setting callbacks for jumping
        inputActions.Player.Jump.performed += ctx => jump = true;
        inputActions.Player.Jump.performed += ctx => debug_jump();
        inputActions.Player.Jump.canceled  += ctx => jump = false;
    }

    private void debug_jump()
    {
        //Debug.Log("Jump pressed!");
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
        UpdateAnimationStates ();
    }

    public void Dead ()
    {
        // Currently not used! We need to come up with the gameplay logic for this first.
        /*
        playerState = PlayerState.dead;
        animator.SetBool("dead", true);
        GetComponent<Collider2D>().enabled = false;
        */
    }

    void UpdatePlayerPosition()
    {
        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        // Movement on the x axis
        if (walk)
        {
            if (walk_left) {
                velocity.x = -walkVelocity;
                sprite.flipX = true;
            }
            else if (walk_right) {
                velocity.x = walkVelocity;
                sprite.flipX = false;
            }
            if (inputActions.Player.Sprint.IsPressed()) {
                run = true;
                velocity.x *= runMultiplier;
            }
            else
            {
                run = false;
            }
            // Direction from movement
            float direction = Mathf.Sign(velocity.x);
            pos = CheckWallRays(pos, direction);
            pos.x += velocity.x * Time.deltaTime;
        }

        // Jumping
        if (jump && grounded)
        {
            velocity.y = jumpVelocity;
            grounded = false;
            playerState = PlayerState.jumping;
        }

        // Movement on the y axis
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

        // Checking collisions
        if (velocity.y <= 0)
            pos = CheckFloorRays(pos);

        if (velocity.y >= 0)
            pos = CheckCeilingRays(pos);

        transform.localPosition = pos;
        transform.localScale = scale;
    }

    void UpdateAnimationStates()
    {
        bool isIdle = grounded && !walk;
        bool isWalking = grounded && walk;
        bool isRunning = isWalking && run;

        animator.SetBool("idle", isIdle);
        animator.SetBool("walk", isWalking);
        animator.SetBool("run", isRunning);
        
        //animator.SetTrigger("jump", !grounded);
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
            velocity.x = 0;
        }

        return pos;
    }



    Vector3 CheckFloorRays(Vector3 pos)
    {
        // TODO add "coytote time" to exiting a platform, to allow nicer jumping
        // move the rear ray little bit outwards from the player if they are grounded
        Vector2 originLeft = new Vector2(pos.x - boxColliderWidth, pos.y - boxColliderheight);
        Vector2 originMiddle = new Vector2(pos.x, pos.y - boxColliderheight);
        Vector2 originRight = new Vector2(pos.x + boxColliderWidth, pos.y - boxColliderheight);

        RaycastHit2D hitLeft   = Physics2D.Raycast(originLeft, Vector2.down, groundRayLength, floorMask);
        RaycastHit2D hitMiddle = Physics2D.Raycast(originMiddle, Vector2.down, groundRayLength, floorMask);
        RaycastHit2D hitRight  = Physics2D.Raycast(originRight, Vector2.down, groundRayLength, floorMask);
        //Debug.DrawRay(originLeft, Vector2.down * groundRayLength, Color.red);

        RaycastHit2D hit = hitLeft.collider ? hitLeft :
                        hitMiddle.collider ? hitMiddle :
                        hitRight;

        if (hit.collider && velocity.y <= 0)
        {
            if (grounded == false)
                //Debug.Log("Standing on solid ground!");
            grounded = true;
            velocity.y = 0;
            // Snap to surface using extents, not bounds center
            pos.y = hit.point.y + boxColliderheight;

            if (hit.collider.CompareTag("Enemy"))
            {
                bounce = true;
                hit.collider.GetComponent<EnemyAI>().Crush();
            }

            if (playerState == PlayerState.jumping)
                playerState = PlayerState.idle;
        }
        else
        {
            if (grounded == true)
                //Debug.Log("Standing on nothing!");
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

    void Fall(){
        velocity.y = 0;
        playerState = PlayerState.jumping;
        bounce = false;
        grounded = false;
    }

    // Items
    public void AddNiwaka()
    {
        niwakas += 1;
        Debug.Log("Niwaka added, now player has " + niwakas);
    }
    public void AddNiwakaSenbei()
    {
        niwakaSenbeis += 1;
        Debug.Log("Niwaka Senbei added, now player has " + niwakaSenbeis);
    }
}
