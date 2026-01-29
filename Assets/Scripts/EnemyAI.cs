using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

    public float gravity;
    public Vector2 velocity;
    public bool isWalkingLeft = true;

    // Raycasting
    const float groundRayLength = 0.05f;
    private BoxCollider2D box;
    float boxColliderheight;
    float boxColliderWidth;

    public LayerMask floorMask;
    public LayerMask wallMask;

    private bool grounded = false;

    private bool shouldDie = false;
    private float deathTimer = 0;

    public float timeBeforeDestroy = 1.0f;

    private enum EnemyState {

        walking,
        falling,
        dead,
        idle
    }

    private EnemyState state = EnemyState.falling;

    // Start is called before the first frame update
    void Start() {

        box = GetComponent<BoxCollider2D>();
        boxColliderheight = box.size.y * 0.5f; // make it half the size
        boxColliderWidth = box.size.x * 0.5f;
        enabled = false;
        Fall ();
        
    }

    // Update is called once per frame
    void Update() {
        
        UpdateEnemyPosition ();
        CheckCrushed();
    }

    public void Crush () {

        state = EnemyState.dead;
        //GetComponent<Animator>().SetBool("isCrushed", true);
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

    void UpdateEnemyPosition () {

        if (state != EnemyState.dead) {

            Vector3 pos = transform.localPosition;
            Vector3 scale = transform.localScale;

            if (state == EnemyState.falling) {

                pos.y += velocity.y * Time.deltaTime;

                velocity.y -= gravity * Time.deltaTime;
            }

            if (state == EnemyState.walking) {

                if (isWalkingLeft) {

                    pos.x -= velocity.x * Time.deltaTime;

                    scale.x = - 1;
                } else {

                    pos.x += velocity.x * Time.deltaTime;
                    scale.x = 1;
                }

            }

            if (velocity.y <= 0)
                pos = CheckGround (pos);
            
            CheckWalls (pos, scale.x);

            transform.localPosition = pos;
            transform.localScale = scale;

            if (state == EnemyState.idle) {

                pos.y += velocity.y * Time.deltaTime;
                velocity.x = 0;
                isWalkingLeft = true;
            }
        }
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
            pos.y = hit.point.y + boxColliderheight;

            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<Player>().Dead();
                //GetComponent<Animator>().SetBool("isIdle", true);
                StartCoroutine(PlayerDeadWait());
                state = EnemyState.idle;
            }
        }
        else
        {
            grounded = false;
            if (state != EnemyState.falling)
                Fall();
        }
        return pos;
    }

    void CheckWalls (Vector3 pos, float direction) {

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

            if (hitRay.collider.tag == "Player") {

                hitRay.collider.GetComponent<Player>().Dead();
                GetComponent<Animator>().SetBool("isIdle", true);
                this.gameObject.transform.localPosition = new Vector3 (pos.x, pos.y, -1.8f);
                StartCoroutine(PlayerDeadWait());
                state = EnemyState.idle;
            }

            isWalkingLeft = !isWalkingLeft;
        }
    }
    
    void OnBecameVisible () {
        enabled = true;
    }

    void Fall () {
        velocity.y = 0;
        state = EnemyState.falling;
        grounded = false;
    }

    IEnumerator PlayerDeadWait() {
        yield return new WaitForSeconds(2);
        Application.LoadLevel("GameOver");
    }
}
