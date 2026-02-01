using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndingVideoPlayer : MonoBehaviour
{
    public string sceneName = "IntroScene";
    public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.loopPointReached += EndReached;
    }


    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        SceneManager.LoadScene(sceneName);
    }
}
