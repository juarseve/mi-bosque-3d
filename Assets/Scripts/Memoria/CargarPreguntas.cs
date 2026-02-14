using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using UnityEngine.Events;
using System.Threading;
using UnityStandardAssets.Characters.FirstPerson;

public class CargarPreguntas : MonoBehaviour
{

    //public Animator canvasAnim;
    public GameObject canvasPreguntasImagenes;
    public GameObject canvasFeedback;
    //public GameObject pacoCorrecto, pacoIncorrecto;
    public Text cantidadEstrellas, pregunta, desafio, preguntaImagen;
    public TextMeshProUGUI Title;
    public Text titlePequeño, dialogo;
    private string texto, textoTitle;
    public Button m_opcionAImagenes, m_opcionBImagenes, m_opcionCImagenes, m_opcionDImagenes;
    public GameObject pr1, pr2, pr3, pr4, pr5;
    public Image m_ImagenA, m_ImagenB, m_ImagenC, m_ImagenD, f_Imagen;
    private int value_A, value_B, value_C, value_D;
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
    private int n_pregunta = 0;
    public RawImage imagen;
    public string nombreEvento;
    public AudioClip correct;
    public AudioClip incorrect;
    public GameObject joystick;
    public GameObject val1, val2,val3,val4,val5;
    public Sprite correcto;
   


    //public ShowMochila mochila;


    void Start()
    {
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("BEGIN START");
        GameObject player = GameObject.Find("FPSController");
        FirstPersonController fps = player.GetComponent<FirstPersonController>();
        fps.canRotate = false;
        fps.canMove = true;
        fps.m_WalkSpeed = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        usadas = new List<int>();
        RestClient.Instance.Get(GetPreguntaObjects);
        StartCoroutine(Preguntas());
        n_estacion = Random.Range(1, 7);
        n_pregunta++;
        Debug.Log("pregunta: " + n_pregunta);
        Debug.Log("Cabaña seleccionada: " + EscenaMiniJuego.idCabin);
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("END START");
    }

    public void DestroyScriptInstance()
    {
        // Removes this script instance from the game object
        //Destroy(this.gameObject);
    }

    public void CargarNuevaPregunta() {
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("BEGIN CARGAR NUEVA");
        canvasPreguntasImagenes.transform.Find("Opción A").gameObject.SetActive(true);
        canvasPreguntasImagenes.transform.Find("Opción B").gameObject.SetActive(true);
        canvasPreguntasImagenes.transform.Find("Opción C").gameObject.SetActive(true);
        canvasPreguntasImagenes.transform.Find("Opción D").gameObject.SetActive(true);

        if (n_pregunta < 5)
        {
            
            if (n_pregunta == 1)
            {
                pr1.SetActive(false);
                pr2.SetActive(true);
                
            }
            if (n_pregunta == 2)
            {
                
                pr2.SetActive(false);
                pr3.SetActive(true);
                
            }
            if (n_pregunta == 3)
            {
                pr3.SetActive(false);
                pr4.SetActive(true);
            }
            if (n_pregunta == 4)
            {
               
                pr4.SetActive(false);
                pr5.SetActive(true);
            }
            usadas = new List<int>();
            RestClient.Instance.Get(GetPreguntaObjects);
            StartCoroutine(Preguntas());
            n_estacion = Random.Range(1, 7);
            n_pregunta++;
            m_opcionAImagenes.interactable = true;
            m_opcionBImagenes.interactable = true;
            m_opcionCImagenes.interactable = true;
            m_opcionDImagenes.interactable = true;
            Debug.Log("pregunta: " + n_pregunta);
        }
        else {
            //SceneManager.LoadScene("Lobby");
            
            GameObject player = GameObject.Find("FPSController");
            FirstPersonController fps = player.GetComponent<FirstPersonController>();
            fps.canRotate = true;
            fps.canMove = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            fps.m_WalkSpeed = 8;

            // Pone el EventToTrigger en vacio para que se pueda llamar al evento cuantas veces quiera
            GameObject evento = GameObject.Find("DialogueManager");
            DialogueManager ev = evento.GetComponent<DialogueManager>();
            ev.currentEventToTrigger = "";

            //Activa el collider`para que el jugador pueda acceder de nuevo al minijuego
            GameObject trigger = GameObject.Find("MinigameColliderMamal");
            BoxCollider box = trigger.GetComponent<BoxCollider>();
            box.isTrigger = true;

            GameObject trigger2 = GameObject.Find("MinigameColliderAves");
            BoxCollider box2 = trigger2.GetComponent<BoxCollider>();
            box2.isTrigger = true;

            GameObject trigger3 = GameObject.Find("MinigameColliderPlants");
            BoxCollider box3 = trigger3.GetComponent<BoxCollider>();
            box3.isTrigger = true;

            SceneManager.UnloadSceneAsync("Memoria");
        }
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("END START");
    }

    IEnumerator Preguntas()
    {
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("BEGIN ENUM PREGUNTAS");
        //MenuPausa.instance.Pausar();
        //GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = false;
        //Panel.SetActive(false);
        //mira.SetActive(false);
        Time.timeScale = 1f;

        m_opcionAImagenes.onClick.AddListener(delegate { Wrapper(value_A); });
        m_opcionBImagenes.onClick.AddListener(delegate { Wrapper(value_B); });
        m_opcionCImagenes.onClick.AddListener(delegate { Wrapper(value_C); });
        m_opcionDImagenes.onClick.AddListener(delegate { Wrapper(value_D); });


        //texto = "Hola, veamos si has prestado atencion, a ver si puedes contestar la siguiente pregunta";
        //Title.enabled = true;
        //titlePequeño.enabled = false;
        //dialogo.enabled = false;
        //canvasAnim.SetBool("IsOpen", true);
        //textoTitle = "Desafío \nN°" + (n_estacion - 1) + "\nContesta la siguiente pregunta";
        //canvasDialogo.SetActive(true);
        //StartCoroutine(Dialogo(Title, textoTitle));
        yield return new WaitForSeconds(0.2f);
        //Title.enabled = false;
        //titlePequeño.enabled = true;
        //dialogo.enabled = true;
        //canvasAnim.SetBool("IsOpen", false);
        //fondoCanvasDialogo.SetActive(false);

        canvasPreguntasImagenes.SetActive(true);
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("END  ENUM PREGUNTAS");
    }

    public void Continuar()
    {
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
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
    }

    public void Wrapper(int i)
    {
        Debug.Log("Prueba de N_pregunta " + n_pregunta);
        Debug.Log("Este es mi i" + i);
        if (i == 1)
        {
            RespuestaCorrecta();
            if (n_pregunta == 1)
            {
                Image imgval1 = val1.GetComponent<Image>();
                imgval1.sprite = correcto;
                val1.SetActive(true);
            }
            if (n_pregunta == 2)
            {
                Image imgval2 = val2.GetComponent<Image>();
                imgval2.sprite = correcto;
                val2.SetActive(true);
            }
            if (n_pregunta == 3)
            {
                Image imgval3 = val3.GetComponent<Image>();
                imgval3.sprite = correcto;
                val3.SetActive(true);
            }
            if (n_pregunta == 4)
            {
                Image imgval4 = val4.GetComponent<Image>();
                imgval4.sprite = correcto;
                val4.SetActive(true);
            }
            if (n_pregunta == 5)
            {
                Image imgval5 = val5.GetComponent<Image>();
                imgval5.sprite = correcto;
                val5.SetActive(true);
            }
            AudioSourceSFX.instance.PlaySound(correct);
        }
        else
        {
            RespuestaIncorrecta();
            if (n_pregunta == 1)
            {
                val1.SetActive(true);
            }
            if (n_pregunta == 2)
            {
                val2.SetActive(true);
            }
            if (n_pregunta == 3)
            {
                val3.SetActive(true);
            }
            if (n_pregunta == 4)
            {
                val4.SetActive(true);
            }
            if (n_pregunta == 5)
            {
                val5.SetActive(true);
            }
            AudioSourceSFX.instance.PlaySound(incorrect);
        }
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("END CONTINUAR");
    }

    private void RespuestaIncorrecta()
    {
        //canvasPreguntasImagenes.SetActive(false);
        canvasPreguntasImagenes.transform.Find("Opción A").gameObject.SetActive(false);
        canvasPreguntasImagenes.transform.Find("Opción B").gameObject.SetActive(false);
        canvasPreguntasImagenes.transform.Find("Opción C").gameObject.SetActive(false);
        canvasPreguntasImagenes.transform.Find("Opción D").gameObject.SetActive(false);

        canvasFeedback.transform.Find("Titulo correcto").gameObject.SetActive(false);
        canvasFeedback.transform.Find("Subtitulo correcto").gameObject.SetActive(false);
        canvasFeedback.transform.Find("Titulo incorrecto").gameObject.SetActive(true);
        /*pacoCorrecto.SetActive(false);
        pacoIncorrecto.SetActive(true);*/
        canvasFeedback.transform.Find("Feedback").gameObject.GetComponent<Text>().text = feedback;
        /*canvasFeedback.transform.Find("check").gameObject.SetActive(false);
        canvasFeedback.transform.Find("cross").gameObject.SetActive(true);*/
        f_Imagen.sprite = Resources.Load<Sprite>(q.image);
        
        // Configurar botón de continuar correctamente
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
        Debug.Log("marca");
        canvasFeedback.SetActive(true);
        //fondoCanvasDialogo.SetActive(true);
        //personaje.PersonajeTriste();
        //texto = "¡Vaya! Esta vez no has acertado. La respuesta correcta es " + respuesta + ".\n" + feedback;
        //Dialogue dialogue = new Dialogue();
        //dialogue.sentences = new string[] { texto };
        //dialogue.title = new string[dialogue.sentences.Length];
        //dialogue.sprites = new Sprite[dialogue.sentences.Length];
        //DialogueManager.instance.StartDialogue(dialogue, nombreEvento, this.gameObject, 0, false, true);
        //StartCoroutine(Dialogo(fondoCanvasDialogo, dialogoPersonaje, texto));
        //yield return new WaitForSeconds(13.0f);
        //Continuar();
        //mochila.desbloquearPregunta(q.Id, false);

        m_opcionAImagenes.interactable = false;
        m_opcionBImagenes.interactable = false;
        m_opcionCImagenes.interactable = false;
        m_opcionDImagenes.interactable = false;
    }

    private void CloseFeedbackCanvas()
    {
        canvasFeedback.GetComponent<Animator>().SetBool("show", true);
        MenuPausa.instance.Reanudar();
        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = true;
        EventManager.eventManager.TriggerEvent(nombreEvento);
        EventManager.StopListening(nombreEvento);
    }

    private void RespuestaCorrecta()
    {
        //canvasPreguntasImagenes.SetActive(false);
        canvasPreguntasImagenes.transform.Find("Opción A").gameObject.SetActive(false);
        canvasPreguntasImagenes.transform.Find("Opción B").gameObject.SetActive(false);
        canvasPreguntasImagenes.transform.Find("Opción C").gameObject.SetActive(false);
        canvasPreguntasImagenes.transform.Find("Opción D").gameObject.SetActive(false);

        canvasFeedback.transform.Find("Titulo correcto").gameObject.SetActive(true);
        canvasFeedback.transform.Find("Subtitulo correcto").gameObject.SetActive(true);
        canvasFeedback.transform.Find("Titulo incorrecto").gameObject.SetActive(false);
        /*pacoCorrecto.SetActive(true);
        pacoIncorrecto.SetActive(false);*/
        canvasFeedback.transform.Find("Feedback").gameObject.GetComponent<Text>().text = feedback;
        /*canvasFeedback.transform.Find("check").gameObject.SetActive(true);
        canvasFeedback.transform.Find("cross").gameObject.SetActive(false);*/
        f_Imagen.sprite = Resources.Load<Sprite>(q.image);
        
        // Configurar botón de continuar correctamente
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
        Debug.Log("marca");
        canvasFeedback.SetActive(true);
        GameManager.instance.playerData.earnEXP(1);
        
        //fondoCanvasDialogo.SetActive(true);
        //texto = "¡Felicidades! Respuesta correcta.\n" + feedback;
        //texto = "Felicidades tu respuesta: " + respuesta + " es correcta.\n" + feedback;
        //StartCoroutine(Dialogo(fondoCanvasDialogo, dialogoPersonaje, texto));
        //Dialogue dialogue = new Dialogue();
        //dialogue.sentences = new string[] { texto };
        //dialogue.title = new string[dialogue.sentences.Length];
        //dialogue.sprites = new Sprite[dialogue.sentences.Length];
        //        dialogue.title="";
        //DialogueManager.instance.StartDialogue(dialogue, nombreEvento, this.gameObject, 1, false, true);
        int x = 0;
        int y = 0;
        int.TryParse(cantidadEstrellas.text, out x);
        int.TryParse(desafio.text, out y);
        x += 5;
        y += 1;
        cantidadEstrellas.text = x.ToString();
        desafio.text = y.ToString();
        //Player.instance.PreguntasCorrectas();
        //yield return new WaitForSeconds(13.0f);
        //Continuar();
        //mochila.desbloquearPregunta(q.Id, true);

      
        m_opcionAImagenes.interactable = false;
        m_opcionBImagenes.interactable = false;
        m_opcionCImagenes.interactable = false;
        m_opcionDImagenes.interactable = false;
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
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("BEGIN GET PREGUNTAS OBJECTS");
        questions = new List<PreguntaObject>();
        /*
         * NOTA:
         * LOS JSON ACTUALES NO TRAEN CATEGORIA
         * TODA LA CLASIFICACION SERA COMENTADA
         * OSEA TODO EL SGTE FOR EACH
         * NOTA2: 
         * Yay ya hay categorias, se arreglo el json
         * descomentando el bloque
         */
        foreach (PreguntaObject archivoraiz in objectList.preguntas)
        {
            if (EscenaMiniJuego.idCabin == 0)       // CABAÑA "ANIMALES" (MAMÍFEROS)
            {
                if (archivoraiz.Category.Equals("Mamiferos"))
                    {
                        if (questions == null)
                    {
                        Debug.Log("es null");
                        questions.Add(archivoraiz);
                    }
                    questions.Add(archivoraiz);
                    Debug.Log("Categoría: " + archivoraiz.Category);
                }
            }
            if (EscenaMiniJuego.idCabin == 1)       // CABAÑA AVES
            {
                if (archivoraiz.Category.Equals("Aves"))
                    {
                        if (questions == null)
                    {
                        Debug.Log("es null");
                        questions.Add(archivoraiz);
                    }
                    questions.Add(archivoraiz);
                    Debug.Log("Categoría: " + archivoraiz.Category);
                }
            }
            if (EscenaMiniJuego.idCabin == 2)       // CABAÑA "PLANTAS" (BOSQUE)
            {
                if (archivoraiz.Category.Equals("Bosque"))
                {
                    if (questions == null)
                    {
                        Debug.Log("es null");
                        questions.Add(archivoraiz);
                    }
                    questions.Add(archivoraiz);
                    Debug.Log("Categoría: " + archivoraiz.Category);
                }
            }
        }


        bool repeat = true;
        int cont = 0;
        while (repeat)
        {
            //Debug.Log("COUNT:" + questions.Count);
            int rdn = Random.Range(0, questions.Count);
            Debug.Log("Questions count: " + questions.Count);
            Debug.Log(rdn);
            /*for(int i = 0; i < questions.Count; i++)
            {
                PreguntaObject pqu = questions[i];
                print(pqu.Feedback);
            }*/
            //Debug.Log(questions.Count);
            /*foreach (string t in usadas)
            {
                Debug.Log(t);
            }*/

            q = questions[rdn];     // OBJETO PREGUNTA
            if (!usadas.Contains(q.ChallengeID) || cont == questions.Count)
            {
                if (cont != questions.Count)
                {
                    usadas.Add(q.ChallengeID);
                }

                LoadImage(q.image);
                pregunta.text = q.question;
                /*Debug.Log(q.Gallery);
                preguntaImagen.text = q.question;
                m_opcionAImagenes.GetComponentInChildren<Text>().text = "A. " + q.Options[0];
                m_opcionBImagenes.GetComponentInChildren<Text>().text = "B. " + q.Options[1];
                m_opcionCImagenes.GetComponentInChildren<Text>().text = "C. " + q.Options[2];
                m_opcionDImagenes.GetComponentInChildren<Text>().text = "D. " + q.Options[3];
                if (q.Gallery.Count == 4)
                {
                    m_ImagenA.sprite = Resources.Load<Sprite>("Questions/Images/" + q.Gallery[0]);
                    m_ImagenB.sprite = Resources.Load<Sprite>("Questions/Images/" + q.Gallery[1]);
                    m_ImagenC.sprite = Resources.Load<Sprite>("Questions/Images/" + q.Gallery[2]);
                    m_ImagenD.sprite = Resources.Load<Sprite>("Questions/Images/" + q.Gallery[3]);
                }
                else
                {
                    m_ImagenA.sprite = Resources.Load<Sprite>("Questions/Images/default");
                    m_ImagenB.sprite = Resources.Load<Sprite>("Questions/Images/default");
                    m_ImagenC.sprite = Resources.Load<Sprite>("Questions/Images/default");
                    m_ImagenD.sprite = Resources.Load<Sprite>("Questions/Images/default");
                }*/
                List<string> galeriaImagenes = new List<string>();
                foreach (Option opt in q.options)
                {
                    galeriaImagenes.Add(opt.image);
                }
                Debug.Log(galeriaImagenes);
                preguntaImagen.text = q.question;
                /*m_opcionAImagenes.GetComponentInChildren<Text>().text = "A. " + q.options[0];
                m_opcionBImagenes.GetComponentInChildren<Text>().text = "B. " + q.Options[1];
                m_opcionCImagenes.GetComponentInChildren<Text>().text = "C. " + q.Options[2];
                m_opcionDImagenes.GetComponentInChildren<Text>().text = "D. " + q.Options[3];*/
                m_opcionAImagenes.GetComponentInChildren<Text>().text = "A. " + q.options[0].text;
                m_opcionBImagenes.GetComponentInChildren<Text>().text = "B. " + q.options[1].text;
                m_opcionCImagenes.GetComponentInChildren<Text>().text = "C. " + q.options[2].text;
                m_opcionDImagenes.GetComponentInChildren<Text>().text = "D. " + q.options[3].text;
                //if (q.Gallery.Count == 4)
                if (galeriaImagenes.Count == 4)
                {
                    /*m_ImagenA.sprite = Resources.Load<Sprite>("Questions/Images/" + q.Gallery[0]);
                    m_ImagenB.sprite = Resources.Load<Sprite>("Questions/Images/" + q.Gallery[1]);
                    m_ImagenC.sprite = Resources.Load<Sprite>("Questions/Images/" + q.Gallery[2]);
                    m_ImagenD.sprite = Resources.Load<Sprite>("Questions/Images/" + q.Gallery[3]);*/
                    m_ImagenA.sprite = Resources.Load<Sprite>("Questions/Images/" + galeriaImagenes[0]);
                    m_ImagenB.sprite = Resources.Load<Sprite>("Questions/Images/" + galeriaImagenes[1]);
                    m_ImagenC.sprite = Resources.Load<Sprite>("Questions/Images/" + galeriaImagenes[2]);
                    m_ImagenD.sprite = Resources.Load<Sprite>("Questions/Images/" + galeriaImagenes[3]);
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
        Debug.Log("CARGAR PREGUNTAS SCRIPT");
        Debug.Log("END PREGUNTAS OBJECT");
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
        imagen.texture = Resources.Load<Texture2D>(address);
    }
}