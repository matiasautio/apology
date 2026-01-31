using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMove : MonoBehaviour
{
    public string sceneName;
    public void LoadSceneByName()
    {
        SceneManager.LoadScene(sceneName);
    }
}
