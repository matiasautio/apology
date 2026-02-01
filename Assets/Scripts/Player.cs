using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] GameObject niwakaMask;
    [SerializeField] float maskInvincibilityDuration = 5f;

    // Using the Input system
    private PlayerInputActions inputActions;
    bool walk;
    bool walk_left;
    bool walk_right;
    bool run;
    bool jump;
    private Vector2 moveInput;

    // Raycasting
    const float groundRayLength = 0.1f;
    // Player height for raycasting
    // Get the collider box size, we are gonan use it to set the ray origin
    private BoxCollider2D box;
    float boxColliderheight;
    float boxColliderWidth;

    // Items
    int niwakas = 0;
    int niwakaSenbeis = 0;

    // Mental state
    [SerializeField] float mentalStateInterval = 10f;

    float mentalStateTimer = 0f;
    public event System.Action<float> OnMentalStateChanged;

    [SerializeField] private float mentalState = 3f;

    public float MentalState => mentalState;

    // Health
    PlayerHealth playerHealth;

    //SE
    //public AudioSource SE;
    public AudioSource jumpSE;
    public AudioSource groundSE;
    public AudioClip audioClip;
    bool groundCheck;

    // Timer
    float footInterval = 0;
    public float footIntervalMax = 1;

    // dedicated source used to loop/stop the ground SFX so jumpSE remains free for one-shots
    AudioSource groundSESource;

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
        jumpSE = GetComponent<AudioSource>();

        // create a dedicated audio source for ground SFX (loop-capable)
        groundSESource = gameObject.AddComponent<AudioSource>();
        groundSESource.playOnAwake = false;
        groundSESource.loop = true;
        groundSESource.spatialBlend = 0f; // 2D sound
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
        // Get the player health
        playerHealth = GetComponent<PlayerHealth>();
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

    void Start()
    {
        Fall();
    }

    void Update()
    {
        CheckPlayerInput();
        UpdatePlayerPosition();
        UpdateAnimationStates();
        UpdateMentalStateTimer();
    }

    public void Dead ()
    {   
        playerState = PlayerState.dead;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
                niwakaMask.GetComponent<SpriteRenderer>().flipX = true;
                niwakaMask.transform.localPosition = new Vector2(-0.1f, 0.488f);
            }
            else if (walk_right) {
                velocity.x = walkVelocity;
                sprite.flipX = false;
                niwakaMask.GetComponent<SpriteRenderer>().flipX = false;
                niwakaMask.transform.localPosition = new Vector2(0.087f, 0.488f);
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
            jumpSE.PlayOneShot(jumpSE.clip);
            playerState = PlayerState.jumping;
        }

        // Movement on the y axis
        if (playerState == PlayerState.jumping || playerState == PlayerState.bouncing)
        {
            pos.y += velocity.y * Time.deltaTime;
            velocity.y -= gravity * Time.deltaTime;
            // Clamp downward velocity
            velocity.y = Mathf.Max(velocity.y, -20f);
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
        Debug.Log(moveInput);

        footInterval += Time.deltaTime;
        if (footInterval >= footIntervalMax && moveInput.x != 0 && grounded) 
        {
            //groundSE.Stop();
            groundSE.PlayOneShot(audioClip);
            footInterval = 0;
        }
        // else 
        // {
        //     if(!groundSE.isPlaying)
        //     {
        //         groundSE.PlayOneShot(audioClip);
        //     }
        // }
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
            // Add "skin" to fix webgl clipping problem
            const float groundSkin = 0.01f;
            pos.y = hit.point.y + boxColliderheight + groundSkin;

            if (hit.collider.CompareTag("Enemy"))
            {
                //bounce = true;
                //hit.collider.GetComponent<EnemyAI>().Crush();
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
        //playerHealth.BecomeInvincible();
        StartCoroutine(StartNiwaka());
        Debug.Log("Niwaka added, now player has " + niwakas);
    }

    private IEnumerator StartNiwaka()
    {
        niwakaMask.SetActive(true);
        playerHealth.isInvincible = true;
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        yield return new WaitForSeconds(maskInvincibilityDuration);
        niwakaMask.SetActive(false);
        playerHealth.isInvincible = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public void AddNiwakaSenbei()
    {
        niwakaSenbeis += 1;
        playerHealth.Heal(1);
        Debug.Log("Niwaka Senbei added, now player has " + niwakaSenbeis);
    }

    // Health
    public void TakeDamage()
    {   
        //if (isInvulnerable || playerState == PlayerState.dead)
            //return;
        ResetMentalStateTimer();
        SetMentalState(mentalState - 1f);
    }

    // Mental state
    void UpdateMentalStateTimer()
    {
        mentalStateTimer += Time.deltaTime;

        if (mentalStateTimer >= mentalStateInterval)
        {
            mentalStateTimer -= mentalStateInterval; // allows catch-up if frame is long
            SetMentalState(mentalState + 1f);
        }
    }

    void ResetMentalStateTimer()
    {
        mentalStateTimer = 0f;
    }
    public void SetMentalState(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, 10f);

        if (Mathf.Approximately(clamped, mentalState))
            return;

        mentalState = clamped;
        OnMentalStateChanged?.Invoke(mentalState);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        // only care about ground collisions here
        if (!other.transform.tag.Contains("Gound"))
        {
            groundCheck = false;
            if (groundSE != null && groundSE.isPlaying) groundSE.Stop();
            if (groundSESource != null && groundSESource.isPlaying) groundSESource.Stop();
            return;
        }

        if (other.transform.tag.Contains("Gound"))
        {        
            groundCheck = true;
        }

        // consider the player 'moving' if input says so or horizontal velocity is above a small threshold
        bool isMoving = walk || Mathf.Abs(velocity.x) > 0.01f;

        // prefer user-assigned AudioSource (groundSE). fall back to the internal source if needed.
        if (groundSE != null && groundSE.clip != null)
        {
            groundSE.loop = true;
            if (isMoving)
            {
                if (!groundSE.isPlaying) groundSE.Play();
            }
            else
            {
                if (groundSE.isPlaying) groundSE.Stop();
            }
        }
        else if (groundSESource != null && (groundSESource.clip != null || (groundSE != null && groundSE.clip != null)))
        {
            // ensure internal source has a clip if possible
            if (groundSESource.clip == null && groundSE != null) groundSESource.clip = groundSE.clip;
            groundSESource.loop = true;
            if (isMoving)
            {
                if (!groundSESource.isPlaying) groundSESource.Play();
            }
            else
            {
                if (groundSESource.isPlaying) groundSESource.Stop();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!other.collider.CompareTag("Ground")) return;
        groundCheck = false;
        if (groundSE != null && groundSE.isPlaying) groundSE.Stop();
        if (groundSESource != null && groundSESource.isPlaying) groundSESource.Stop();
    }
    
}
