using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public Image displayImage;      // 表示用のUI Image
    public Sprite[] introSprites;  // 表示したい画像リスト
    public float displayTime = 2.0f; // 1枚あたりの表示時間
    public string nextSceneName = "TitleScene"; // 次のシーン名

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        foreach (Sprite s in introSprites)
        {
            displayImage.sprite = s;
            
            // 【演出】ここでフェードインを入れるとさらにオシャレ
            yield return StartCoroutine(Fade(0, 1, 0.5f)); 

            yield return new WaitForSeconds(displayTime);

            // 【演出】フェードアウト
            yield return StartCoroutine(Fade(1, 0, 0.5f));
        }

        // すべて終わったらタイトルへ
        SceneManager.LoadScene(nextSceneName);
    }

    // 簡単なフェード処理
    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0;
        Color c = displayImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            displayImage.color = c;
            yield return null;
        }
    }

    // スキップ機能（クリックしたらタイトルへ）
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}