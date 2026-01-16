using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;


public class TargetManager : MonoBehaviour
{
    // Singleton instance
    public static TargetManager instance;

    int numTargetRed;
    int numTargetGreen;

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

    public NPCController npc;

    public GameObject RedTargets;

    public GameObject GreenTargets;

    bool canNext = true;

    bool islooked;

    bool isDone;

    public bool salto = false;
    public bool bandera = false;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        panel.SetActive(false);

        for (int i = 0; i < 13; i++)
        {
            message[i] = LanguageManager.Instancia.ObtenerTexto("tutorial.dialogo_" + i);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(texting());
        RedTargets.SetActive(false);
        GreenTargets.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.Debug.Log("Update index: " + index);
        ImageShow();
        sendBoolNPC();
        Debug.Log("Index 3: " + index);
        pauseTexting();
        activeTexting();
        activePlayerMovement();
        tasks();
        finishedTutorial();
        activeButton();

    }

    void activeButton()
    {
        if (text.text == message[index] && index != 12)
        {
            //button.SetActive(true);
        }
    }

    IEnumerator texting()
    {
        UnityEngine.Debug.Log("Hace algo texting.");
        string son = LanguageManager.Instancia.ObtenerTexto("tutorial.son");
        // leemos cada letra del mensaje para su visibilidad con una espera de 0.02 segundos
        foreach (char letter in message[index].ToCharArray())
        {
            text.text += letter;
            yield return new WaitForSeconds(0.01f);
        }

        if (index == 1) 
            keyboardImage.SetActive(true);
        if (index == 3) 
            keyboardImage.SetActive(false);
        if (index == 5)
            keyboardRun.SetActive(true);
        if (index == 6)
            keyboardRun.SetActive(false);
        if (index == 9)
            keyboardJump.SetActive(true);
        if (index == 10)
            keyboardJump.SetActive(false);
        if (index == 12)
        {
            UnityEngine.Debug.Log("Tutorial completed, player can move again.");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false; 
            panel.SetActive(false);
        }
            

    }

    // set the next message in the screen
    public void nextMessage()
    {
        Debug.Log("Entra a nextMessage");
        Debug.Log("Index: " + index);

        if (canNext)
        {
            panel.SetActive(true);
            if (index < message.Length - 1)
            {
                index++;
                text.text = "";
                StartCoroutine(texting());
            }

        }
        else
        {
            UnityEngine.Debug.Log("No entra al can");
            text.text = "";
        }

    }


    void pauseTexting()
    {
        Debug.Log("Index 2: " + index);
        // si los index son los indicados entramos al if
        if (index == 2 || index == 6)
        {
            // si el texto en pantalla es igual al mensaje pausamos el texto
            Debug.Log("Texto: " + text.text);
            Debug.Log("Mensaje: " + message[index]);
            if (text.text == message[index])
            {

                canNext = false;
                button.SetActive(false);
            }

        }

        if (index == 9)
        {
            canNext = false;
            button.SetActive(false);
        }

    }

    void activeTexting()
    {
        if (index == 3 && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)))
        {
            Debug.Log("Player is moving.");
            canNext = true;  // Allow the next step after moving
                             // button.SetActive(true);
        }

        if (!player.GetComponent<FirstPersonController>().m_CharacterController.isGrounded && index >= 8)
        {
            salto = true;
        }
        if (salto && player.GetComponent<FirstPersonController>().m_CharacterController.isGrounded)
        {
            bandera = true;
        }

        if (text.text == "")
        {
            canNext = true;
            //button.SetActive(true);
            //panel.SetActive(true);
            //text.text = "Continuemos";
            nextMessage();
        }



    }


    void activePlayerMovement()
    {
        UnityEngine.Debug.Log("Phase movement");
        if (button.activeSelf == false)
        {
            UnityEngine.Debug.Log("Phase movement");
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE
            Cursor.lockState = CursorLockMode.Locked;
            player.GetComponent<FirstPersonController>().enabled = true;

#elif UNITY_ANDROID || UNITY_IOS
            joystick.SetActive(true);

#endif
        }

        if (panel.activeSelf == true)
        {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            player.GetComponent<FirstPersonController>().enabled = false;

#elif UNITY_ANDROID || UNITY_IOS
            joystick.SetActive(false);
            
#endif


        }
    }

    void ImageShow()
    {
        if (index == 1)
        {
            keyboardImage.gameObject.SetActive(true);
        }

        if (index == 3)
        {
            keyboardImage.gameObject.SetActive(false);
        }
        if (index == 5)
        {
            keyboardRun.gameObject.SetActive(true);
        }

        if (index == 6)
        {
            keyboardRun.gameObject.SetActive(false);
        }
        if (index == 9)
        {
            keyboardJump.gameObject.SetActive(true);
        }

        if (index == 10)
        {
            keyboardJump.gameObject.SetActive(false);
        }

    }

    //set boolean to the npc in the scene using method
    void sendBoolNPC()
    {
        if (index == 3)
        {
            npc.stateWalking();
        }

        if (index == 6)
        {
            npc.stateRunning();
        }

        if (index == 9)
        {
            npc.stateJumping();
        }

        if (index == 10)
        {
            npc.stateIdle();
        }
    }


    void tasks()
    {
        numTargetRed = GameObject.FindGameObjectsWithTag("TargetRed").Length;

        numTargetGreen = GameObject.FindGameObjectsWithTag("TargetGreen").Length;


        if (index == 12)
        {
            isDone = true;
        }

    }

    void finishedTutorial()
    {
        //if (isDone)
        player.GetComponent<FirstPersonController>().enabled = false;
        Debug.Log("Tutorial completed, player can move again.");
        
    }

    public void StartTutorial()
    {
        panel.SetActive(true);
        isDone = false;
        index = 0;
        RedTargets.SetActive(false);
        GreenTargets.SetActive(false);
        text.text = ""; 
        StartCoroutine(texting());
        Debug.Log("Tutorial started!");
    }


}
