using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class MaxJumpsMissionListener : MonoBehaviour
{
    [SerializeField] private int maxJumps = 3;
    [SerializeField] private LogrosGlobales logros;

    private int jumpCount;
    private bool missionFailed;

    void OnEnable()
    {
        Debug.Log("Listener ENABLED");
        FirstPersonController.OnPlayerJump += OnJump;
    }

    void OnDisable()
    {
        Debug.Log("Listener DISABLED");
        FirstPersonController.OnPlayerJump -= OnJump;
    }

    private void OnJump()
    {
        if (missionFailed) return;

        Debug.Log("Listener RECEIVED jump");
        jumpCount++;
        if (jumpCount == 1) {
            logros.ProgresarLogro(20);
        }
        if (jumpCount > maxJumps)
        {
            missionFailed = true;
            FailMission();
        }
    }

    private void FailMission()
    {
        Debug.Log("Mission failed: too many jumps");

        // Mission 8 = "Saltar poco"
        logros.ProgresarLogro(8);
        logros.ProgresarMision(8, "");
        // or trigger UI / retry logic here
    }

    void Awake()
    {
        Debug.Log("Listener AWAKE");
    }

}