using UnityEngine;
public class EnemyBird : EnemyAI
{
    [SerializeField] GameObject rockPrefab;
    [SerializeField] float dropInterval = 3f;
    [SerializeField] float dropSpeed = 5f;
    float dropTimer;
    [SerializeField] float flySpeed = 2f;
    [SerializeField] float verticalAmplitude = 1f;
    [SerializeField] float verticalFrequency = 2f;
    float startY;

    protected override void Awake()
    {
        base.Awake();
        startY = transform.position.y;
    }

    protected override void UpdateEnemy()
    {
        base.UpdateEnemy();

        dropTimer += Time.deltaTime;

        if (dropTimer >= dropInterval)
        {
            dropTimer = 0f;
            DropRock();
        }
        // Remove bird if they fly away to the left
        Vector3 v = Camera.main.WorldToViewportPoint(transform.position);
        if (hasBeenVisible && v.x < -1f)// || v.x > 1.1f))
            Destroy(gameObject);
    }
    protected override void UpdateEnemyPosition()
    {
        if (state == EnemyState.dead) {
            animator.SetBool("walk", false);
            return;
        }

        Vector3 pos = transform.localPosition;

        // Horizontal patrol
        pos.x += (isWalkingLeft ? -1 : 1) * flySpeed * Time.deltaTime;

        // Vertical sine wave
        pos.y = startY + Mathf.Sin(Time.time * verticalFrequency) * verticalAmplitude;

        transform.localPosition = pos;
        animator.SetBool("walk", true);
    }

    void DropRock()
    {
        GameObject rock = Instantiate(
            rockPrefab,
            transform.position,
            Quaternion.identity
        );
        rock.GetComponent<Rock>().Init(this);
        rock.GetComponent<Rock>()
            .Launch(Vector2.down, dropSpeed);
    }
}
