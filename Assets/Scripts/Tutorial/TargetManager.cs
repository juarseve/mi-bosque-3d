using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;


public class TargetManager : MonoBehaviour
{
    // Singleton instance
    public static TargetManager instance;

    int currentTarget;

    public string[] message;
    public Text text;
    int index;

    public FirstPersonController player;

    public GameObject joystick;

    public GameObject button;

    public GameObject sceneButton;

    public GameObject panel;

    public GameObject keyboardImage;
    public GameObject keyboardRun;
    public GameObject keyboardJump;

    bool canNext = true;

    bool islooked;

    bool isDone;

    public bool salto = false;
    public bool bandera = false;
    TutorialStep currentStep = TutorialStep.None;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if (player == null)
        {
            player = FindObjectOfType<FirstPersonController>();
        }


        panel.SetActive(false);

        for (int i = 0; i < 13; i++)
        {
            message[i] = LanguageManager.Instancia.ObtenerTexto("tutorial.dialogo_" + i);
        }

    }

    void Start()
    {
    }

    void Update()
    {
    }

    void SetPlayerControl(bool enabled)
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
                player.enabled = enabled;
                Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !enabled;
        #elif UNITY_ANDROID || UNITY_IOS
            joystick.SetActive(enabled);
        #endif
    }


    public void StartTutorial()
    {
        Debug.Log("Tutorial started!");
        panel.SetActive(true);
        SetPlayerControl(false);
        currentStep = TutorialStep.Move;
        ShowMoveStep();
    }

    private void ShowMoveStep()
    {
        Debug.Log("ShowMoveStep called");

        if (player == null) Debug.LogError("player is NULL");
        if (panel == null) Debug.LogError("panel is NULL");
        if (keyboardImage == null) Debug.LogError("keyboardImage is NULL");
        if (text == null) Debug.LogError("text is NULL");

        SetPlayerControl(true);
        panel.SetActive(true);
        keyboardImage.SetActive(true);
        text.text = "Use WASD to move";

        StartCoroutine(WaitForMovement());
    }


    IEnumerator WaitForMovement()
    {
        while (
            !Input.GetKey(KeyCode.W) &&
            !Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) &&
            !Input.GetKey(KeyCode.D)
        )
        {
            yield return null;
        }

        keyboardImage.SetActive(false);
        ShowRunStep();
    }

    void ShowRunStep()
    {
        currentStep = TutorialStep.Run;
        text.text = "Hold Shift to run";
        keyboardRun.SetActive(true);
        StartCoroutine(WaitForRun());
    }

    IEnumerator WaitForRun()
    {
        while (true)
        {
            bool isMoving =
                Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.D);

            bool isRunning =
                Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (isMoving && isRunning)
                break;

            yield return null;
        }

        keyboardRun.SetActive(false);
        ShowJumpStep();
    }

    void ShowJumpStep()
    {
        currentStep = TutorialStep.Jump;
        text.text = "Press Space to jump";
        keyboardJump.SetActive(true);
        StartCoroutine(WaitForJump());
    }

    IEnumerator WaitForJump()
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        // Wait until the player leaves the ground
        while (cc.isGrounded)
        {
            yield return null;
        }

        // Wait until the player lands again
        while (!cc.isGrounded)
        {
            yield return null;
        }

        keyboardJump.SetActive(false);
        EndTutorial();
    }

    void EndTutorial()
    {
        panel.SetActive(false);
        keyboardJump.SetActive(false);
        SetPlayerControl(true);
        currentStep = TutorialStep.Completed;
    }

    public enum TutorialStep
    {
        None,
        Move,
        Run,
        Jump,
        Completed
    }

}