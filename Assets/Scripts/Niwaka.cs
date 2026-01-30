using UnityEngine;

public class Niwaka : MonoBehaviour
{
    public bool isNiwakaSenbei = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player player = other.GetComponent<Player>();
        if (player == null)
            return;

        if (isNiwakaSenbei)
            player.AddNiwakaSenbei();
        else
            player.AddNiwaka();

        Destroy(gameObject);
    }
}

