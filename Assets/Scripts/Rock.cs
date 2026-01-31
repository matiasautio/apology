using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField] float lifetime = 5f;
    [SerializeField] int damage = 1;

    Rigidbody2D rb;
    private EnemyAI owner;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    public void Init(EnemyAI ownerEnemy)
    {
        owner = ownerEnemy;
    }

    public void Launch(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore the thrower of this rock
        if (collision.collider.GetComponent<EnemyAI>() == owner)
            return;
    
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Destroy when other enemies hit and allow for other enemies to take damage
        EnemyAI enemy = collision.collider.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            //enemy.TakeDamage(1);
            Destroy(gameObject);
            return;
        }

        // Destroy on walls/ground
        if (!collision.collider.isTrigger)
        {
            Destroy(gameObject);
        }
    }

}
