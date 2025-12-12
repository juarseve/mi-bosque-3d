using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System;

public class MenuPausa : MonoBehaviour
{
    public float countdown;
    private Collider cameraBlocker;
    public static bool IsPaused = false;
    public static bool IsPausedByOtherCanvas = false;
    public static bool IsCronometroPressed = false;

    public GameObject MenuPausaUI, panelEstrellas, Panel;
    public SceneChanger sceneChanger;
    private FirstPersonController fpscontroller;
    public static MenuPausa instance;
    public MouseController mouseController;
    [SerializeField] private AudioMixerSnapshot PausedSnapshot;
    [SerializeField] private AudioMixerSnapshot UnpausedSnapshot;
    [SerializeField] private float fadeTime=1.0f;
    [SerializeField] public  ManejadorModoJuego manejadorJuego;
    [SerializeField] public  GameObject pistaexplicacion;

    public GameObject actionLogger;

    private NotificarLogros NL;

    private void Awake()
    {
        instance = this;
        cameraBlocker = GameObject.FindGameObjectWithTag("Blocker").GetComponent<Collider>();

    }
    private void Start()
    {
        fpscontroller = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        mouseController = GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>();
        sceneChanger.gameObject.SetActive(true);
        NL = GameObject.Find("NotifLogros").GetComponent<NotificarLogros>();

        actionLogger = GameObject.Find("ActionLogger");
    }

    void Update()
    {
        if (countdown > 0)
        {
            countdown = countdown - Time.deltaTime;
        }
        else if (countdown < 0)
        {
            countdown = 0;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                pistaexplicacion.SetActive(false);
                PauseGame();
                NL.cerrar();
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!(IsCronometroPressed))
            {
                IsCronometroPressed = true;
                contextoContrarreloj();
               
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                IsCronometroPressed = false;
                ModoNormal();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

            }
        }
        if (manejadorJuego.PerdioContrarreloj)
        {
            //ModoNormal();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            fpscontroller.enabled = false;
            mouseController.enabled = false;
            IsCronometroPressed = false;
            //manejadorJuego.PerdioContrarreloj = false;
        }



    }

    public void Pausar()
    {
        fpscontroller.enabled = false;
        mouseController.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("1P "+IsPausedByOtherCanvas);
        IsPausedByOtherCanvas=true;
        Debug.Log("2P "+IsPausedByOtherCanvas);
        PausedSnapshot.TransitionTo(fadeTime);
        cameraBlocker.enabled=true;
        countdown = 5;

    }
    
    public void Reanudar()
    {
        fpscontroller.enabled = true;
        Debug.Log("1R "+IsPausedByOtherCanvas);
        IsPausedByOtherCanvas=false;
        Debug.Log("2R "+IsPausedByOtherCanvas);
        UnpausedSnapshot.TransitionTo(fadeTime);
        cameraBlocker.enabled=false;
        
        // Bloquear y ocultar el cursor al volver al juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ResumeGame()
    {
        fpscontroller.enabled = true;

        if(!IsPausedByOtherCanvas){
            mouseController.enabled=true;
            UnpausedSnapshot.TransitionTo(fadeTime);
            cameraBlocker.enabled=false;
        }
        Time.timeScale = 1f;
        IsPaused = false;
        MenuPausaUI.SetActive(false);
        Panel.SetActive(true);
        try
        {
            actionLogger.GetComponent<ActionLogger>().actionLogger.jugando = true;
        }
        catch (System.Exception e)
        {
            Debug.Log("error en "+e.Message);
        }
        
    }

    public void PauseGame()
    {
        if(!IsPausedByOtherCanvas)
        {
            PausedSnapshot.TransitionTo(fadeTime);
            fpscontroller.enabled = false;
            mouseController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            MenuPausaUI.SetActive(true);
            Panel.SetActive(false);
            IsPaused = true;
            Time.timeScale = 0f;
            cameraBlocker.enabled = true;
            try
            {
                actionLogger.GetComponent<ActionLogger>().actionLogger.jugando = false;
                actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Pause Game", actionLogger.GetComponent<ActionLogger>().actionLogger.locacion);
            }
            catch (Exception e)
            {
                Debug.Log("no se capto AC: " + e.Message);
            }
        }
        
        
    }

    public void MenuMapa()
    {
        actionLogger.GetComponent<ActionLogger>().actionLogger.locacion = "Menu Partida";
        actionLogger.GetComponent<ActionLogger>().actionLogger.jugando = true;
        actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Change Scene", SceneManager.GetActiveScene().name+"-Mapa");
        UnpausedSnapshot.TransitionTo(fadeTime);
        IsPaused = false;
        Time.timeScale = 1f;
        //GameObject.Find("Audio").GetComponent<SoundManager>().PauseAudio();
        sceneChanger.FadeToLevel(GameManager.instance.currentStation);
    }

    public void Lobby()
    {
        UnpausedSnapshot.TransitionTo(fadeTime);
        IsPaused = false;
        Time.timeScale = 1f;
        //GameObject.Find("Audio").GetComponent<SoundManager>().PauseAudio();
        GameManager.instance.scene = 2;
        sceneChanger.FadeToLevel(0);

    }

    public void MenuInicial(string name)
    {
        UnpausedSnapshot.TransitionTo(fadeTime);
        IsPaused = false;
        Time.timeScale = 1f;
        GameManager.instance.scene=0;
        //GameObject.Find("Audio").GetComponent<SoundManager>().PauseAudio();
        GameManager.ZenMode = false;
        SceneManager.LoadScene(name);
    }

    public void contextoContrarreloj()
    {
        ResumeGame();
        manejadorJuego.activarExplicacion();
        fpscontroller.enabled = false;
        mouseController.enabled = false;
    }
    public void ModoContrarreloj()
    {
        manejadorJuego.activarTemporizador();
        fpscontroller.enabled = true;
        mouseController.enabled = true;
    }
    public void ModoNormal()
    {
        manejadorJuego.desactivarCronometro();
        ResumeGame();
        manejadorJuego.activarDespedida();
        fpscontroller.enabled = false;
        mouseController.enabled = false;

    }
    public void reiniciarModoNormal()
    {
        manejadorJuego.desactivarDespedida();
        fpscontroller.enabled = true;
        mouseController.enabled = true;
       
    }

}
