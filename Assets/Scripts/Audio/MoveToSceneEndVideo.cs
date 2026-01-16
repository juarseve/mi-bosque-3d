using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;


public class MoveToSceneEndVideo : MonoBehaviour
{

    public VideoPlayer video;
    public string sceneName;
    // Start is called before the first frame update

    void Start() { video.loopPointReached += EndReached; }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.playbackSpeed = vp.playbackSpeed / 10.0F;
        SceneManager.LoadScene("Lobby");
    }
}
