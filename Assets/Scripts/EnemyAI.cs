using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

    public float gravity;
    public Vector2 velocity;
    public bool isWalkingLeft = true;

    // Raycasting
    const float groundRayLength = 0.1f;
    private BoxCollider2D box;
    float boxColliderheight;
    float boxColliderWidth;

    public LayerMask floorMask;
    public LayerMask wallMask;

    private bool grounded = false;

    private bool shouldDie = false;
    private float deathTimer = 0;

    public float timeBeforeDestroy = 1.0f;

    protected enum EnemyState {

        walking,
        falling,
        dead,
        idle
    }

    protected EnemyState state = EnemyState.falling;

    public int damage = 1;

    protected bool hasBeenVisible = false;
    // Sprite
    private SpriteRenderer sprite;
    protected Animator animator;

    protected virtual void Awake()
    {
        
    }

    void Start() {

        box = GetComponent<BoxCollider2D>();
        boxColliderheight = box.size.y * 0.5f; // make it half the size
        boxColliderWidth = box.size.x * 0.5f;
        enabled = false;
        // Set sprite
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        Fall();
    }

    protected virtual void Update()
    {
        UpdateEnemy();
    }
    protected virtual void UpdateEnemy()
    {
        UpdateEnemyPosition();
        CheckCrushed();
    }

    public void Crush () {

        state = EnemyState.dead;
        animator.SetBool("walk", false);
        GetComponent<Collider2D>().enabled = false;
        shouldDie = true;
    }

    void CheckCrushed () {

        if (shouldDie) {

            if (deathTimer <= timeBeforeDestroy) {

                deathTimer += Time.deltaTime;

            } else {

                shouldDie = false;

                Destroy (this.gameObject);
            }
        }
    }

    protected virtual void UpdateEnemyPosition()
    {
        if (state == EnemyState.dead) {
            animator.SetBool("walk", false);
            return;
        }

        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        if (state == EnemyState.falling)
        {
            pos.y += velocity.y * Time.deltaTime;
            velocity.y -= gravity * Time.deltaTime;
        }

        if (state == EnemyState.walking)
        {
            if (isWalkingLeft)
            {
                pos.x -= velocity.x * Time.deltaTime;
                sprite.flipX = false;
            }
            else
            {
                pos.x += velocity.x * Time.deltaTime;
                sprite.flipX = true;
            }
            animator.SetBool("walk", true);
        }

        if (velocity.y <= 0)
            pos = CheckGround(pos);

        CheckWalls(pos, scale.x);

        transform.localPosition = pos;
        transform.localScale = scale;
    }

    Vector3 CheckGround (Vector3 pos) {

        Vector2 originLeft = new Vector2(pos.x - boxColliderWidth, pos.y - boxColliderheight);
        Vector2 originMiddle = new Vector2(pos.x, pos.y - boxColliderheight);
        Vector2 originRight = new Vector2(pos.x + boxColliderWidth, pos.y - boxColliderheight);

        RaycastHit2D hitLeft   = Physics2D.Raycast(originLeft, Vector2.down, groundRayLength, floorMask);
        RaycastHit2D hitMiddle = Physics2D.Raycast(originMiddle, Vector2.down, groundRayLength, floorMask);
        RaycastHit2D hitRight  = Physics2D.Raycast(originRight, Vector2.down, groundRayLength, floorMask);

        RaycastHit2D hit = hitLeft.collider ? hitLeft :
                        hitMiddle.collider ? hitMiddle :
                        hitRight;

        if (hit.collider && velocity.y <= 0)
        {
            grounded = true;
            velocity.y = 0;
            state = EnemyState.walking; 
            const float groundSkin = 0.01f;
            pos.y = hit.point.y + boxColliderheight + groundSkin;
        }
        else
        {
            grounded = false;
            if (state != EnemyState.falling)
                Fall();
        }
        return pos;
    }

    // TODO modify this to use the better raycast setup
    void CheckWalls (Vector3 pos, float direction)
    {
        /*
        Vector2 originTop = new Vector2 (pos.x + direction * 0.4f, pos.y + .5f - 0.2f);
        Vector2 originMiddle = new Vector2 (pos.x + direction * 0.4f, pos.y);
        Vector2 originBottom = new Vector2 (pos.x + direction * 0.4f, pos.y - .5f + 0.2f);

        RaycastHit2D wallTop = Physics2D.Raycast (originTop, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallMiddle = Physics2D.Raycast (originMiddle, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallBottom = Physics2D.Raycast (originBottom, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);

        if (wallTop.collider != null || wallMiddle.collider != null || wallBottom.collider != null) {

            RaycastHit2D hitRay = wallTop;

            if (wallTop) {
                hitRay = wallTop;
            } else if (wallMiddle) {
                hitRay = wallMiddle;
            } else if (wallBottom) {
                hitRay = wallBottom;
            }
            isWalkingLeft = !isWalkingLeft;
        }*/
        if (isWalkingLeft)
        {
            direction = -1;
        }
        else
        {
            direction = 1;
        }
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
            if (!hit.collider.CompareTag("Player"))
            {
                isWalkingLeft = !isWalkingLeft;
            }
        }
    }
    
    void OnBecameVisible () {
        enabled = true;
        hasBeenVisible = true;
    }

    void Fall() {
        velocity.y = 0;
        state = EnemyState.falling;
        grounded = false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
        {   
            // Handled by the health script
            //DamagePlayer(col.collider.GetComponent<PlayerHealth>());
        }
    }
    void DamagePlayer(PlayerHealth player)
    {
        player.TakeDamage(damage);
    }

    IEnumerator PlayerDeadWait() {
        yield return new WaitForSeconds(2);
        //Application.LoadLevel("GameOver");
    }
}
