using UnityEngine;

public class EnemyKId : EnemyAI
{

    [SerializeField] GameObject rockPrefab;
    [SerializeField] float throwInterval = 5f;
    [SerializeField] float rockSpeed = 6f;

    float throwTimer;
    // isWalkingLeft bool from parent class will determine this
    int facingDirection = 1; // 1 = right, -1 = left

    protected override void UpdateEnemy()
    {
        base.UpdateEnemy();
        UpdateThrowing();
    }

    void UpdateThrowing()
    {
        throwTimer += Time.deltaTime;

        if (throwTimer >= throwInterval)
        {
            throwTimer = 0f;
            ThrowRock();
        }
    }

    void ThrowRock()
    {
        animator.SetTrigger("throw");
        if (isWalkingLeft)
            facingDirection = -1;
        else
            facingDirection = 1;
        Vector3 spawnPos = transform.position;
        spawnPos.x += facingDirection * 0.6f; // adjust for enemy width

        GameObject rock = Instantiate(rockPrefab, spawnPos, Quaternion.identity);
        rock.GetComponent<Rock>().Init(this);
        rock.GetComponent<Rock>()
            .Launch(Vector2.right * facingDirection, rockSpeed);
    }
}
