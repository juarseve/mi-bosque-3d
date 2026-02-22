using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityStandardAssets.Characters.FirstPerson;


public class VideoControlLive : MonoBehaviour
{
    public VideoPlayer VPlayer;

    public FirstPersonController fpsController;

    Transform transf;

     // Start is called before the first frame update
    void Start()
    {
        transf = this.gameObject.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            VPlayer.Play();

            fpsController.SetMode(FirstPersonController.PlayerMode.Cinematic);
            fpsController.ForceLook(-3f, 90f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            VPlayer.Stop();

            fpsController.SetMode(FirstPersonController.PlayerMode.Normal);
        }
    }
}
