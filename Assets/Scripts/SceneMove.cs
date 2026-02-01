using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneMove : MonoBehaviour
{
    public string sceneName;
    public AudioClip SE;

    [Tooltip("If true, wait for SE to finish before changing scene.")]
    public bool waitForSE = true;

    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    // Hook this to the UI Button's OnClick() in the Inspector
    public void OnButtonPressed_PlaySEAndLoadScene()
    {
        if (SE != null)
            _audioSource.PlayOneShot(SE);

        if (waitForSE && SE != null)
            StartCoroutine(LoadAfterDelay(SE.length));
        else
            SceneManager.LoadScene(sceneName);
    }

    IEnumerator LoadAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(sceneName);
    }

    // Optional: keep keyboard behaviour but fixed (fires once per keydown)
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (SE != null)
                _audioSource.PlayOneShot(SE);

            if (waitForSE && SE != null)
                StartCoroutine(LoadAfterDelay(SE.length));
            else
                SceneManager.LoadScene(sceneName);
        }
    }
}
