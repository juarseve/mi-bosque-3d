using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using UnityEngine.Events;
using System.Threading;

public class WallTrigger_2 : MonoBehaviour
{

    public Animator canvasAnim;
    public GameObject canvasPreguntasImagenes;
    public GameObject canvasFeedback;
    public GameObject pacoCorrecto, pacoIncorrecto;
    public GameObject controlPanel;
    public Text cantidadEstrellas, pregunta, desafio, preguntaImagen;
    public TextMeshProUGUI Title;
    public Text titlePequeño, dialogo;
    private string texto, textoTitle;
    public Button m_opcionAImagenes, m_opcionBImagenes, m_opcionCImagenes, m_opcionDImagenes;
    public Image m_ImagenA, m_ImagenB, m_ImagenC, m_ImagenD, f_Imagen;
    private int value_A, value_B, value_C, value_D;
    private string opt1, opt2, opt3, opt4;
    private string respuesta;
    private string feedback;
    private string url_preguntas = SystemVariables.url_puerto + "/api/bpv/question";
    private string url_api = SystemVariables.url_puerto + "/resources/bpv/images/species/";
    private string url_info = SystemVariables.url_puerto + "/api/bpv/specie/";
    private List<PreguntaObject> questions;
    static List<int> usadas;
    private PreguntaObject q;
    private SpecieObject tmp = null;
    public int n_estacion;
    public Image imagen;
    public string nombreEvento;
    public AudioClip correct;
    public AudioClip incorrect;
    public GameObject joystick;

    public ShowMochila mochila;

    public GameObject actionLogger;

    private NotificarLogros NL;

    void Start()
    {
        usadas = new List<int>();
        
        var notifLogrosObj = GameObject.Find("NotifLogros");
        if (notifLogrosObj != null)
        {
            NL = notifLogrosObj.GetComponent<NotificarLogros>();
        }
        
        actionLogger = GameObject.Find("ActionLogger");
        if (actionLogger == null)
        {
            Debug.LogWarning("WallTrigger_2: No se encontró el GameObject 'ActionLogger'");
        }
    }
    
    // Maneja el cursor cuando la aplicación recupera el foco (Alt+Tab)
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Si hay una interfaz de preguntas o feedback activa, restaurar el cursor
            bool hasActiveUI = false;
            
            if (canvasPreguntasImagenes != null && canvasPreguntasImagenes.activeInHierarchy)
            {
                hasActiveUI = true;
            }
            
            if (canvasFeedback != null && canvasFeedback.activeInHierarchy)
            {
                hasActiveUI = true;
            }
            
            // También verificar si el juego está pausado
            if (MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas)
            {
                hasActiveUI = true;
            }
            
            if (hasActiveUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Debug.Log("WallTrigger_2: Cursor restaurado porque hay interfaz activa");
            }
            else
            {
                // Si no hay UI activa, bloquear el cursor para modo 3ª persona
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Debug.Log("WallTrigger_2: Cursor bloqueado porque no hay interfaz activa");
            }
        }
    }

    public void DestroyScriptInstance()
    {
        // Removes this script instance from the game object
        //Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider obj)
    {
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("BEGIN COLISION");
#if UNITY_ANDROID || UNITY_IOS
        joystick.SetActive(false);
#endif
        if (obj.gameObject.tag == "Player")
        {
            //StartCoroutine(RestClient.Instance.Get(url_preguntas, GetPreguntaObjects));
            RestClient.Instance.Get(GetPreguntaObjects);
            StartCoroutine(Preguntas());
            controlPanel.GetComponent<Animator>().SetBool("hide", true);

        }
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("END COLISION");
    }

    IEnumerator Preguntas()
    {
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("BEGIN ENIM PREGUNTAS");
        MenuPausa.instance.Pausar();
        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = false;
        //Panel.SetActive(false);
        //mira.SetActive(false);
        Time.timeScale = 1f;

        m_opcionAImagenes.onClick.AddListener(delegate { Wrapper(value_A,opt1); });
        m_opcionBImagenes.onClick.AddListener(delegate { Wrapper(value_B, opt2); });
        m_opcionCImagenes.onClick.AddListener(delegate { Wrapper(value_C, opt3); });
        m_opcionDImagenes.onClick.AddListener(delegate { Wrapper(value_D, opt4); });



        yield return new WaitForSeconds(0.2f);
;

        canvasPreguntasImagenes.SetActive(true);
        
        // Asegurar que el cursor esté visible y desbloqueado para la interfaz
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("END ENUM PREG");
        NL.cerrar();
    }

    public void Continuar()
    {
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("BEGIN CONTINUAR");
#if UNITY_ANDROID || UNITY_IOS
        joystick.SetActive(true);
#endif
        Time.timeScale = 1f;
        //fondoCanvasDialogo.SetActive(false);
        MenuPausa.instance.Reanudar();
        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = true;
        //Panel.SetActive(true);
        //mira.SetActive(true);
        Title.enabled = true;
        DestroyScriptInstance();
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("END CONTINUAR");
    }

    public void Wrapper(int i,string opt)
    {
        if (i == 1)
        {
            RespuestaCorrecta(opt);
            AudioSourceSFX.instance.PlaySound(correct);
        }
        else
        {
            RespuestaIncorrecta(opt);
            AudioSourceSFX.instance.PlaySound(incorrect);
        }
    }

    private void RespuestaIncorrecta(string opt)
    {
        if (q == null)
        {
            Debug.LogError("WallTrigger_2: La pregunta (q) es null en RespuestaIncorrecta");
            return;
        }
        
        //aqui
        if (actionLogger != null && actionLogger.GetComponent<ActionLogger>() != null && 
            actionLogger.GetComponent<ActionLogger>().actionLogger != null && pregunta != null)
        {
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion(pregunta.text, "incorrecta");
        }
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("BEGIN RESPUESTA INC");
        
        if (canvasPreguntasImagenes != null)
        {
            canvasPreguntasImagenes.SetActive(false);
        }
        
        // Asegurar que el cursor esté visible cuando se muestra el feedback
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Asegurar que el cursor esté visible cuando se muestra el feedback
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (canvasFeedback != null)
        {
            var tituloCorrecto = canvasFeedback.transform.Find("Titulo correcto");
            if (tituloCorrecto != null) tituloCorrecto.gameObject.SetActive(false);
            
            var subtituloCorrecto = canvasFeedback.transform.Find("Subtitulo correcto");
            if (subtituloCorrecto != null) subtituloCorrecto.gameObject.SetActive(false);
            
            var tituloIncorrecto = canvasFeedback.transform.Find("Titulo incorrecto");
            if (tituloIncorrecto != null) tituloIncorrecto.gameObject.SetActive(true);
            
            var feedbackText = canvasFeedback.transform.Find("Feedback")?.gameObject?.GetComponent<Text>();
            if (feedbackText != null)
            {
                feedbackText.text = feedback;
            }
            
            var checkObj = canvasFeedback.transform.Find("check");
            if (checkObj != null) checkObj.gameObject.SetActive(false);
            
            var crossObj = canvasFeedback.transform.Find("cross");
            if (crossObj != null) crossObj.gameObject.SetActive(true);
            
            if (f_Imagen != null && q != null)
            {
                f_Imagen.sprite = Resources.Load<Sprite>("Questions/Images3/" + q.ChallengeID);
            }
            
            var buttonObj = canvasFeedback.transform.Find("Button");
            if (buttonObj != null)
            {
                var button = buttonObj.gameObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(CloseFeedbackCanvas);
                }
            }
            
            canvasFeedback.transform.localPosition.Set(33.28f, -0.8f, 0);
            canvasFeedback.SetActive(true);
        }
        
        if (pacoCorrecto != null) pacoCorrecto.SetActive(false);
        if (pacoIncorrecto != null) pacoIncorrecto.SetActive(true);

        if (mochila != null && q != null)
        {
            mochila.desbloquearPregunta(q.ChallengeID, false);
        }
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("END RESP INC");
    }

    private void CloseFeedbackCanvas()
    {
        if (canvasFeedback != null)
        {
            var animator = canvasFeedback.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("show", true);
            }
        }
        
        // Asegurar que el cursor esté bloqueado y oculto cuando se cierra el feedback
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (MenuPausa.instance != null)
        {
            MenuPausa.instance.Reanudar();
        }
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var mouseController = player.GetComponent<MouseController>();
            if (mouseController != null)
            {
                mouseController.enabled = true;
            }
        }
        
        if (EventManager.eventManager != null)
        {
            EventManager.eventManager.TriggerEvent(nombreEvento);
            EventManager.StopListening(nombreEvento);
        }
        
        if (controlPanel != null)
        {
            var animator = controlPanel.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("hide", false);
            }
        }
    }

    private void RespuestaCorrecta(string opt)
    {
        if (q == null)
        {
            Debug.LogError("WallTrigger_2: La pregunta (q) es null en RespuestaCorrecta");
            return;
        }
        
        Debug.Log("mandar a server :" + q.codename +"-" + q.image + "-" + "Bosque-Estación " + n_estacion + "-"+ opt + "-" + q.question);
        
        if (!GameManager.OfflineMode && Peticiones.instance != null && Player.instance != null && Player.instance.playerData != null)
        {
            Peticiones.instance.registerPregunta(Player.instance.playerData, System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), opt, "Bosque-Estación " + n_estacion, ""+q.codename);
        }
            Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("BEGIN RESP CORRECTA");
        if (canvasPreguntasImagenes != null)
        {
            canvasPreguntasImagenes.SetActive(false);
        }
        
        // Asegurar que el cursor esté visible cuando se muestra el feedback
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (actionLogger != null && actionLogger.GetComponent<ActionLogger>() != null && 
            actionLogger.GetComponent<ActionLogger>().actionLogger != null && pregunta != null)
        {
            actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion(pregunta.text, "correcta");
        }

        if (canvasFeedback != null)
        {
            var tituloCorrecto = canvasFeedback.transform.Find("Titulo correcto");
            if (tituloCorrecto != null) tituloCorrecto.gameObject.SetActive(true);
            
            var subtituloCorrecto = canvasFeedback.transform.Find("Subtitulo correcto");
            if (subtituloCorrecto != null) subtituloCorrecto.gameObject.SetActive(true);
            
            var tituloIncorrecto = canvasFeedback.transform.Find("Titulo incorrecto");
            if (tituloIncorrecto != null) tituloIncorrecto.gameObject.SetActive(false);
            
            var feedbackText = canvasFeedback.transform.Find("Feedback")?.gameObject?.GetComponent<Text>();
            if (feedbackText != null)
            {
                feedbackText.text = feedback;
            }
            
            var checkObj = canvasFeedback.transform.Find("check");
            if (checkObj != null) checkObj.gameObject.SetActive(true);
            
            var crossObj = canvasFeedback.transform.Find("cross");
            if (crossObj != null) crossObj.gameObject.SetActive(false);
            
            if (f_Imagen != null && q != null)
            {
                f_Imagen.sprite = Resources.Load<Sprite>("Questions/Images3/" + q.ChallengeID);
            }
            
            var buttonObj = canvasFeedback.transform.Find("Button");
            if (buttonObj != null)
            {
                var button = buttonObj.gameObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(CloseFeedbackCanvas);
                    button.interactable = true; // Asegurar que el botón esté habilitado
                }
            }
            
            canvasFeedback.transform.localPosition.Set(33.28f, -0.8f, 0);
            canvasFeedback.SetActive(true);
        }
        
        if (pacoCorrecto != null) pacoCorrecto.SetActive(true);
        if (pacoIncorrecto != null) pacoIncorrecto.SetActive(false);

        // Actualizar estrellas y desafíos
        if (cantidadEstrellas != null && desafio != null)
        {
            int x = 0;
            int y = 0;
            int.TryParse(cantidadEstrellas.text, out x);
            int.TryParse(desafio.text, out y);
            x += 5;
            y += 1;
            cantidadEstrellas.text = x.ToString();
            desafio.text = y.ToString();
        }
        
        if (Player.instance != null)
        {
            Player.instance.PreguntasCorrectas();
        }
        
        if (mochila != null && q != null)
        {
            mochila.desbloquearPregunta(q.ChallengeID, true);
        }

        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("END RESP CORRECT");
    }

    IEnumerator Dialogo(TextMeshProUGUI dialogoPersonaje, string texto)
    {
        int longitud = texto.Length;
        int contador = 0;
        dialogoPersonaje.text = string.Empty;
        while (contador < longitud)
        {
            dialogoPersonaje.text = dialogoPersonaje.text + texto[contador];
            contador += 1;
            yield return null;
        }
    }

    void GetPreguntaObjects(PreguntaObjectList objectList)
    {
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("BEGIN GET PREGUNTAS OBJECT");
        Debug.Log("preguntas cargadas");
        Debug.Log(objectList.preguntas.Count);
        questions = new List<PreguntaObject>();
        foreach (PreguntaObject archivoraiz in objectList.preguntas)
        {
            List<int> estacionesDePreg = new List<int>();
            foreach (GameLevelsChallenges gl in archivoraiz.gameLevelsChallenges)
            {
                estacionesDePreg.Add(gl.GameLevelId-1);
            }
            //if (IsInEstacion(archivoraiz.Stations))
            if (IsInEstacion(estacionesDePreg))
            {
                if (questions == null)
                {
                    Debug.Log("es null");
                    questions.Add(archivoraiz);
                }
                questions.Add(archivoraiz);
            }

        }


        bool repeat = true;
        int cont = 0;
        while (repeat)
        {
            //Debug.Log("COUNT:" + questions.Count);
            int rdn = Random.Range(0, questions.Count);
            Debug.Log("question");
            Debug.Log(questions.Count);


            q = questions[rdn];
            Debug.Log("la pregunta es "+q.ToString());
            Debug.Log(q.question);
            Debug.Log(q.feedback);
            //Debug.Log(q.question);
            if (!usadas.Contains(q.ChallengeID) || cont==questions.Count)
            {
                if (cont!=questions.Count)
                {
                    usadas.Add(q.ChallengeID);
                }

                //LoadImage(q.image);
                //LoadImage(""+q.ChallengeID);

                imagen.sprite = Resources.Load<Sprite>("Questions/Images3/" + q.ChallengeID);
                Debug.Log("*********** NOMBRE DE IM,AGEN****************");
                Debug.Log(imagen.sprite.name);
                pregunta.text = q.question;

                List<string> galeriaImagenes = new List<string>();
                foreach (Option opt in q.options)
                {
                    galeriaImagenes.Add(""+opt.ChallengeOptionId);
                }


                //Debug.Log(q.Gallery);
                Debug.Log(galeriaImagenes);
                preguntaImagen.text = q.question;

                m_opcionAImagenes.GetComponentInChildren<Text>().text = "A. " + q.options[0].text;
                m_opcionBImagenes.GetComponentInChildren<Text>().text = "B. " + q.options[1].text;
                m_opcionCImagenes.GetComponentInChildren<Text>().text = "C. " + q.options[2].text;
                m_opcionDImagenes.GetComponentInChildren<Text>().text = "D. " + q.options[3].text;
                opt1 = q.options[0].text;
                opt2 = q.options[1].text;
                opt3 = q.options[2].text;
                opt4 = q.options[3].text;

                if (galeriaImagenes.Count == 4)
                {

                    m_ImagenA.sprite = Resources.Load<Sprite>("Questions/Images2/" + galeriaImagenes[0]);
                    m_ImagenB.sprite = Resources.Load<Sprite>("Questions/Images2/" + galeriaImagenes[1]);
                    m_ImagenC.sprite = Resources.Load<Sprite>("Questions/Images2/" + galeriaImagenes[2]);
                    m_ImagenD.sprite = Resources.Load<Sprite>("Questions/Images2/" + galeriaImagenes[3]);
                }
                else
                {
                    m_ImagenA.sprite = Resources.Load<Sprite>("Questions/Images/default");
                    m_ImagenB.sprite = Resources.Load<Sprite>("Questions/Images/default");
                    m_ImagenC.sprite = Resources.Load<Sprite>("Questions/Images/default");
                    m_ImagenD.sprite = Resources.Load<Sprite>("Questions/Images/default");
                }

                //m_opcionA.GetComponentInChildren<Text>().text = "A. " + q.Options[0];
                //m_opcionB.GetComponentInChildren<Text>().text = "B. " + q.Options[1];
                //m_opcionC.GetComponentInChildren<Text>().text = "C. " + q.Options[2];
                //m_opcionD.GetComponentInChildren<Text>().text = "D. " + q.Options[3];
                value_A = 0;
                value_B = 0;
                value_C = 0;
                value_D = 0;
                //for (int i = 0; i < 4; i++)
                {
                    if (q.options[0].correctOption)
                    {
                        value_A = 1;
                        respuesta = q.options[0].text;
                    }
                    else if (q.options[1].correctOption)
                    {
                        value_B = 1;
                        respuesta = q.options[1].text;
                    }
                    else if (q.options[2].correctOption)
                    {
                        value_C = 1;
                        respuesta = q.options[2].text;
                    }
                    else
                    {
                        value_D = 1;
                        respuesta = q.options[3].text;
                    }
                }

                //respuesta = q.Options[q.Answer];
                repeat = false;
                feedback = q.feedback.feedback;
            }
            else
            {
                repeat = true;
            }
            cont = cont + 1;
        }
        Debug.Log("WALL TRIGGER 2 SCRIPT");
        Debug.Log("END GET PREGUNTAS OBJECT");
    }

    bool IsInEstacion(List<int> estaciones)
    {
        foreach (int sp in estaciones)
        {
            if (sp == n_estacion)
            {
                return true;
            }
        }
        return false;
    }


    public void LoadImage(string address)
    {
        Debug.Log("Questions/Images3/" + address);
       
        imagen.sprite = Resources.Load<Sprite>("Questions/Images3/" + address);
    }
}