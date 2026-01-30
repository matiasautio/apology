using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 3;
    private int currentHp;
    public float invincibilityTime = 1.5f; // 無敵時間
    private bool isInvincible = false;

    [Header("UI Settings")]
    public GameObject[] hpIcons;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        Debug.Log("a");
        currentHp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateHPUI(); // UIを更新
    }

#if UNITY_EDITOR
    // Editor 時に設定ミスを早期発見する簡易チェック
    private void OnValidate()
    {
        var col = GetComponent<Collider2D>();
        var rbComp = GetComponent<Rigidbody2D>();
        if (col == null)
            Debug.LogWarning("[PlayerHealth] Collider2D が見つかりません。Player に Collider2D を追加してください。");
        else if (col.isTrigger)
            Debug.LogWarning("[PlayerHealth] Player の Collider2D が 'Is Trigger' になっています。非Triggerで衝突イベントを受け取りたい場合はオフにしてください。");

        if (rbComp == null)
            Debug.LogWarning("[PlayerHealth] Rigidbody2D が見つかりません。少なくとも Player か Enemy のどちらかに Rigidbody2D が必要です。");
    }
#endif

    // 敵やアイテムに触れた時の判定（非Trigger — Collider.isTrigger はオフ） 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;

        GameObject otherGO = (collision.collider != null) ? collision.collider.gameObject : collision.gameObject;
        string otherTag = (otherGO != null) ? otherGO.tag : "(no-go)";
        string otherName = (otherGO != null) ? otherGO.name : "(no-go)";
        string otherLayer = (otherGO != null) ? LayerMask.LayerToName(otherGO.layer) : "(no-go)";

        Debug.Log($"[PlayerHealth] Collision with '{otherName}' (tag:{otherTag}, layer:{otherLayer}), colliderIsTrigger:{(collision.collider != null ? collision.collider.isTrigger.ToString() : "n/a")}, contacts:{collision.contactCount}, otherRb:{(collision.rigidbody!=null?collision.rigidbody.bodyType.ToString():"none")}");

        if (isInvincible)
        {
            Debug.Log("[PlayerHealth] Ignored collision: currently invincible");
            return;
        }

        // タグが子オブジェクトについているケースや、タグが root にあるケースに対応
        bool collidedWithEnemy = false;
        if (otherGO != null && otherGO.CompareTag("Enemy")) collidedWithEnemy = true;
        else if (collision.collider != null && collision.collider.transform != null && collision.collider.transform.root != null && collision.collider.transform.root.CompareTag("Enemy")) collidedWithEnemy = true;

        if (collidedWithEnemy)
        {
            TakeDamage(1);

            // --- オプション: ノックバックを入れる場合は以下のコメントを外してください ---
            // if (rb != null && collision.contactCount > 0)
            // {
            //     Vector2 hitPoint = collision.GetContact(0).point;
            //     Vector2 dir = ((Vector2)transform.position - hitPoint).normalized;
            //     rb.AddForce(dir * 200f);
            // }
        }
        else
        {
            Debug.Log($"[PlayerHealth] Collision ignored — not tagged 'Enemy' (actual tag: {otherTag})");
        }
    }

    void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log("痛い！ 残りHP: " + currentHp);

        UpdateHPUI();

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    // 無敵時間と点滅の演出
    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        if (spriteRenderer == null) yield break;

        float blinkInterval = 0.1f;
        float elapsed = 0f;
        while (elapsed < invincibilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    // アイテム（煎餅）で回復した時もこれを呼ぶ
    public void Heal(int amount)
    {
        currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
        UpdateHPUI();
    }

    // UIの表示・非表示を切り替えるメソッド
    void UpdateHPUI()
    {
        for (int i = 0; i < hpIcons.Length; i++)
        {
            // 現在のHPより小さいインデックスの画像だけを表示する
            if (i < currentHp)
            {
                hpIcons[i].SetActive(true);
            }
            else
            {
                hpIcons[i].SetActive(false);
            }
        }
    }

    void Die()
    {
        Debug.Log("ゲームオーバー");
    }
}