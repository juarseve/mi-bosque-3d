using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class ShowMochila : MonoBehaviour
{
    public GameObject infoWindow;
    public GameObject mochila;
    public GameObject showBook;
    public GameObject mochilaGo;
    public GameObject mochilaIcon;
    public GameObject tablonIcon;
    public GameObject helpIcon;
    public GameObject lupaIcon;
    public GameObject salidaMochila;
    private Collider cameraBlocker;
    public GameObject accesoryPanel;
    public GameObject seedsPanel;
    public GameObject perfilScreen;
    public GameObject medallasScreen;
    public GameObject preguntaScreen;
    public GameObject misionesScreen;
    public GameObject infoScreen;
    
    // Referencias para el toggle visual de la mochila con imágenes
    public Image seedsButtonImage;
    public Image accesoriesButtonImage;
    public Sprite seedsButtonActive;      // Imagen cuando Semillas está activo
    public Sprite seedsButtonInactive;    // Imagen cuando Semillas está inactivo
    public Sprite accesoriesButtonActive;   // Imagen cuando Accesorios está activo
    public Sprite accesoriesButtonInactive; // Imagen cuando Accesorios está inactivo

    public static bool IsBackPack = false;
    public static bool isInfo = false;
    private FirstPersonController firstPersonController;
    public Profile profile;

    public GameObject cerrarMochila;
    public GameObject CanvasPlayerGUI;
    public LogrosButton logrosButton;

    private List<PreguntaObject> questions;
    private Dictionary<int, (PreguntaObject, GameObject, string)> questionsDict = new Dictionary<int, (PreguntaObject, GameObject, string)>();

    public GameObject Pregunta;
    public GameObject ContentPregunta;
    public GameObject preguntasScreen;
    public GameObject SeccionPreguntas;
    public GameObject SeccionPreguntaInfo;
    public GameObject PreguntaInfoTitulo;
    public GameObject PreguntaStacion;
    public GameObject PreguntaRespuesta;
    public GameObject ContenedorPreg;

    public static int rotaUnaVez = 0;

    private NotificarLogros NL;

    private void Awake()
    {
        firstPersonController = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        cameraBlocker = GameObject.FindGameObjectWithTag("Blocker").GetComponent<Collider>();

        RestClient.Instance.Get(GetPreguntaObjects);

    }

    private void Start()
    {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE 
        InitializeIcons();
#endif
#if UNITY_ANDROID || UNITY_IOS
        if(GameObject.Find("Control Mochila") != null) {
            GameObject.Find("Control Mochila").SetActive(false);
        }
#endif
        infoWindow.SetActive(false);
        mochila.SetActive(false);
        salidaMochila.SetActive(false);
#if UNITY_ANDROID || UNITY_IOS
        mochilaIcon.SetActive(false);
#endif
        Player.instance.playerData.mochilaDesbloqueada = true;
        NL = GameObject.Find("NotifLogros").GetComponent<NotificarLogros>();
        
        // Inicializar las imágenes de los botones de la mochila
        // Por defecto, Semillas está activo
        InitializeMochilaButtons();
    }
    
    /// <summary>
    /// Inicializa las imágenes de los botones de Semillas y Accesorios
    /// </summary>
    private void InitializeMochilaButtons()
    {
        if (seedsButtonImage != null && seedsButtonActive != null)
        {
            // Semillas por defecto activo
            seedsButtonImage.sprite = seedsButtonActive;
        }
        
        if (accesoriesButtonImage != null && accesoriesButtonInactive != null)
        {
            // Accesorios inactivo por defecto
            accesoriesButtonImage.sprite = accesoriesButtonInactive;
        }
    }

    // Asegura que los íconos del HUD estén visibles (llamable desde GameManager tras cargar una partida)
    public void InitializeIcons()
    {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE 
        if (cerrarMochila != null) cerrarMochila.SetActive(true);
        if (mochilaIcon != null) mochilaIcon.SetActive(true);
        if (tablonIcon != null) tablonIcon.SetActive(true);
        if (helpIcon != null) helpIcon.SetActive(true);
        if (lupaIcon != null) lupaIcon.SetActive(true);
#endif
    }

    //public void ShowBackPack()
    public void ShowWindow(GameObject window)
    {
        if (!(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
        {
            MenuPausa.instance.Pausar();
            window.SetActive(true);
            if (window.Equals(mochila))
            {
#if UNITY_ANDROID || UNITY_IOS
            CanvasPlayerGUI.SetActive(false);
            GameObject.Find("FPSController").GetComponent<JoystickController>().enabled = false;
#endif
                IsBackPack = true;
                //salidaMochila.SetActive(true);
#if UNITY_ANDROID || UNITY_IOS
            salidaMochila.SetActive(false);
#endif
                GameObject.Find("Control Mochila").GetComponent<MochilaCtrl>().Desnotificar();
            }
            else
            {
                isInfo = true;
                infoScreen.SetActive(true);
            }
            //Time.timeScale = 0f;
            cameraBlocker.enabled = false;
            NL.cerrar();
        }
    }
    public void SwitchShowWindow(String objetivo)
    {
               if (objetivo =="mochila")
       {
                Debug.Log("yendo a mochila");
                mochila.SetActive(true);
                IsBackPack = true;
            //salidaMochila.SetActive(true);
            StartCoroutine(DelayAction(objetivo));
        }
       else if (objetivo=="info")
       {
                Debug.Log("yendo a libro");
                isInfo = true;                
                infoWindow.SetActive(true);
            StartCoroutine(DelayAction(objetivo));
            
       }
        Debug.Log("listo");
    }
    IEnumerator DelayAction(String objetivo)
    {
        yield return new WaitForSeconds(1);
        if (objetivo == "mochila")
        {

            IsBackPack = true;

        }
        else if (objetivo == "info")
        {
            isInfo = true;
        }
        Debug.Log("listo2");
    }

    public void OnProfileScreen()
    {
        perfilScreen.SetActive(true);
        profile.UpdateProfile();
        infoScreen.SetActive(false);
    }

    public void OnUndoProfile()
    {
        infoScreen.SetActive(true);
        perfilScreen.SetActive(false);
    }


    public void Continuar()
    {
#if UNITY_ANDROID || UNITY_IOS
        CanvasPlayerGUI.SetActive(true);
        GameObject.Find("FPSController").GetComponent<JoystickController>().enabled = true;
        GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>().handle.anchoredPosition = Vector2.zero;
        GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>().input = Vector2.zero;
#endif
        perfilScreen.SetActive(false);
        infoWindow.SetActive(false); // En caso de cerrar de inmediato sin mostrar primero la mochila
        Time.timeScale = 1f;
        mochila.SetActive(false);
        salidaMochila.SetActive(false);
        misionesScreen.SetActive(false);
        preguntaScreen.SetActive(false);
        medallasScreen.SetActive(false);
        mochilaGo.SetActive(true);
        MenuPausa.instance.Reanudar();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        IsBackPack = false;
        isInfo = false;
    }
    public void SwitchContinuar()
    {
#if UNITY_ANDROID || UNITY_IOS
        CanvasPlayerGUI.SetActive(true);
        GameObject.Find("FPSController").GetComponent<JoystickController>().enabled = true;
        GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>().handle.anchoredPosition = Vector2.zero;
        GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>().input = Vector2.zero;
#endif
        perfilScreen.SetActive(false);
        infoWindow.SetActive(false); // En caso de cerrar de inmediato sin mostrar primero la mochila
        mochila.SetActive(false);
        salidaMochila.SetActive(false);
        misionesScreen.SetActive(false);
        preguntaScreen.SetActive(false);
        medallasScreen.SetActive(false);
        mochilaGo.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;       
        IsBackPack = false;
        isInfo = false;
    }

    public void OnAccesoryPanelShow()
    {
        seedsPanel.SetActive(false);
        accesoryPanel.SetActive(true);
        DragNDrop.isAccesory = true;
        
        // Actualizar imágenes de los botones
        UpdateMochilaButtonImages(false); // false = Accesorios activo
    }

    public void OnSeedsPanelShow()
    {
        seedsPanel.SetActive(true);
        accesoryPanel.SetActive(false);
        DragNDrop.isAccesory = false;
        
        // Actualizar imágenes de los botones
        UpdateMochilaButtonImages(true); // true = Semillas activo
    }
    
    /// <summary>
    /// Actualiza las imágenes de los botones de Semillas y Accesorios
    /// </summary>
    /// <param name="isSeedsActive">true si Semillas debe estar activo, false si Accesorios</param>
    private void UpdateMochilaButtonImages(bool isSeedsActive)
    {
        if (seedsButtonImage != null)
        {
            seedsButtonImage.sprite = isSeedsActive ? seedsButtonActive : seedsButtonInactive;
        }
        
        if (accesoriesButtonImage != null)
        {
            accesoriesButtonImage.sprite = isSeedsActive ? accesoriesButtonInactive : accesoriesButtonActive;
        }
    }
    public void mochilaInt( bool abrir)
    {
        if(abrir)
        {
            ShowWindow(mochila);
        }
        else
        {
            Continuar();
        }
    }
    void Update()
    {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE
        if (Input.GetKeyUp(KeyCode.M))
        {
            if (IsBackPack)
            {
                Continuar();
            }
            else if(isInfo)
            {
                Continuar();
                ShowWindow(mochila);
            }
            else
            {
                ShowWindow(mochila);
            }
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            if (isInfo)
            {
                Continuar();
            }
            else if (IsBackPack)
            {
                Continuar();
                ShowWindow(infoWindow);
            }
            else
            {
                ShowWindow(infoWindow);
            }
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (isInfo || IsBackPack)
                Continuar();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (isInfo || IsBackPack)
            {
                SwitchContinuar();
                showBook.GetComponent<ShowBook>().SwitchdisplayBook();
            }
        }
#endif
#if UNITY_ANDROID || UNITY_IOS
        if (logrosButton.Pressed)
        {
            logrosButton.setPress();
            CanvasPlayerGUI.SetActive(false);
            if (IsBackPack || isInfo)
            {
                Continuar();
            }
            else
            {
                ShowWindow(infoWindow);
            }
        }
#endif


    }


    void GetPreguntaObjects(PreguntaObjectList objectList)
    {
        int cont = 0;
        //Dictionary<int, bool> preguntasPlayer = profile.player.playerData.getPreguntasDict();
        Dictionary<int, bool> preguntasPlayer = GameManager.instance.playerData.getPreguntasDict();

        string nodesb = LanguageManager.Instancia.ObtenerTexto("menu_inventario.no_desbloq");

        foreach (PreguntaObject question in objectList.preguntas)
        {
            GameObject pregunta = Instantiate(Pregunta, new Vector3(0, 0, 0), Quaternion.identity);

            pregunta.transform.parent = ContentPregunta.transform;
            RectTransform rt = pregunta.GetComponent<RectTransform>();

            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, rt.rect.width);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, rt.rect.height);
            rt.localScale = new Vector3(1, 1, 1);
            rt.localPosition = new Vector3(0, cont, 0);
            cont = cont - 40;

            RectTransform rtFondo = pregunta.transform.GetChild(0).GetComponent<RectTransform>();
            rtFondo.localPosition = new Vector3(138, rtFondo.localPosition.y, rtFondo.localPosition.z);

            //if (preguntasPlayer.ContainsKey(question.Id))
            //{
            //    if (preguntasPlayer[question.Id])
            //    {
            //        pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "<color=green>" + question.Text + "</color>";
            //    }
            //    else
            //    {
            //        pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "<color=red>" + question.Text + "</color>";
            //    }
            //}
            //else
            //{
            //    pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "???????????";
            //}

            //pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = question.Text;



            UnityEngine.UI.Button button = pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(delegate { MostrarInfoPreguntas(question.ChallengeID); });

            if (preguntasPlayer.ContainsKey(question.ChallengeID))
            {
                if (preguntasPlayer[question.ChallengeID])
                {
                    pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "<color=green>" + question.question + "</color>";
                }
                else
                {
                    pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "<color=red>" + question.question + "</color>";
                }
                questionsDict.Add(question.ChallengeID, (question, pregunta, "free"));
            }
            else
            {
                //pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "- Pregunta no desbloqueada -";
                pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = nodesb;
                questionsDict.Add(question.ChallengeID, (question, pregunta, "block"));
            }



        }

        RectTransform contectRT = ContentPregunta.GetComponent<RectTransform>();
        Debug.Log(cont * -1 + 20);
        contectRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cont * -1 + 20);

    }

    public void OnPreguntasScreen()
    {
        //-----------COLOCA LA SECCIÓN DE PREGUNTAS CORRECTAMENTE-----------
        if (rotaUnaVez == 0) {
            ContenedorPreg.transform.Rotate(0, 127, 0);
            rotaUnaVez++;
        }
        //------------------------------------------------------------------


        preguntasScreen.SetActive(true);
        infoScreen.SetActive(false);
    }


    public void MostrarInfoPreguntas(int id)
    {
        string resp = LanguageManager.Instancia.ObtenerTexto("menu_inventario.respuesta");
        string est = LanguageManager.Instancia.ObtenerTexto("menu_inventario.estaciones");

        (PreguntaObject, GameObject, string) tuplaQueestion = questionsDict[id];
        if (tuplaQueestion.Item3 != "block")
        {

            PreguntaObject question = tuplaQueestion.Item1;

            PreguntaInfoTitulo.GetComponent<UnityEngine.UI.Text>().text = question.question;

            Option correcta=new Option();
            foreach (Option opt in question.options)
            {
                if (opt.correctOption)
                {
                    correcta = opt;
                }
            }
            PreguntaRespuesta.GetComponent<UnityEngine.UI.Text>().text = resp + correcta.text + "\n\n" + question.feedback.feedback;
            //PreguntaRespuesta.GetComponent<UnityEngine.UI.Text>().text = "Respuesta: " + question.Options[question.Answer] + "\n\n" + question.feedback.feedback;
            PreguntaStacion.GetComponent<UnityEngine.UI.Text>().text = est;

            List<int> estacionesDePreg = new List<int>();
            foreach (GameLevelsChallenges gl in question.gameLevelsChallenges)
            {
                estacionesDePreg.Add(gl.GameLevelId - 1);
            }

            //foreach (int estacion in question.Stations)
            foreach (int estacion in estacionesDePreg)
            {
                PreguntaStacion.GetComponent<UnityEngine.UI.Text>().text += " " + estacion;

            }
            SeccionPreguntas.SetActive(false);
            SeccionPreguntaInfo.SetActive(true);
        }




    }

    public void CerrarPreguntaInfo()
    {
        SeccionPreguntaInfo.SetActive(false);
        SeccionPreguntas.SetActive(true);
    }


    public void desbloquearPregunta(int id, bool correcta)
    {
        (PreguntaObject, GameObject, string) tuplaQueestion = questionsDict[id];
        PreguntaObject question = tuplaQueestion.Item1;
        GameObject pregunta = tuplaQueestion.Item2;
        if (correcta)
        {
            //pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "<color=green>" + question.Text + "</color>";
            pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "<color=green>" + question.question + "</color>";
            questionsDict.Remove(id);
            questionsDict.Add(id, (question, pregunta, "free"));
            GameManager.instance.playerData.addPregunta(id, correcta);
        }
        else if (tuplaQueestion.Item3.Equals("block"))
        {
            pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "<color=red>" + question.question + "</color>";
            questionsDict.Remove(id);
            questionsDict.Add(id, (question, pregunta, "free"));
            GameManager.instance.playerData.addPregunta(id, correcta);
        }
        else {
            pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "<color=red>" + question.question + "</color>";
            questionsDict.Remove(id);
            questionsDict.Add(id, (question, pregunta, "free"));
            GameManager.instance.playerData.addPregunta(id, correcta);
        }


        //profile.player.playerData.addPregunta(id, correcta);
    }

}
