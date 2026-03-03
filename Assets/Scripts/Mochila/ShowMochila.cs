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
        
        // ============================================================
        // FIX: Validar que GameManager, playerData y LanguageManager existan
        // ============================================================
        if (GameManager.instance == null)
        {
            Debug.LogError("[ShowMochila] ❌ GameManager.instance es NULL en GetPreguntaObjects. Reintentando...");
            StartCoroutine(RetryLoadQuestionsLater(objectList));
            return;
        }
        
        if (GameManager.instance.playerData == null)
        {
            Debug.LogError("[ShowMochila] ❌ GameManager.instance.playerData es NULL en GetPreguntaObjects. Reintentando...");
            StartCoroutine(RetryLoadQuestionsLater(objectList));
            return;
        }
        
        if (LanguageManager.Instancia == null)
        {
            Debug.LogError("[ShowMochila] ❌ LanguageManager.Instancia es NULL en GetPreguntaObjects. Reintentando...");
            StartCoroutine(RetryLoadQuestionsLater(objectList));
            return;
        }
        
        // ============================================================
        // FIX: Validar que los GameObjects del canvas estén asignados
        // ============================================================
        if (ContentPregunta == null)
        {
            Debug.LogError("[ShowMochila] ❌❌❌ ContentPregunta es NULL! No se pueden crear las preguntas en la UI.");
            Debug.LogError("[ShowMochila] Verifica en el Inspector que 'Content Pregunta' esté asignado en el script ShowMochila.");
            return;
        }
        
        if (Pregunta == null)
        {
            Debug.LogError("[ShowMochila] ❌❌❌ Pregunta (prefab) es NULL! No se pueden instanciar preguntas.");
            Debug.LogError("[ShowMochila] Verifica en el Inspector que 'Pregunta' (prefab) esté asignado en el script ShowMochila.");
            return;
        }
        
        Debug.Log($"[ShowMochila] 🔍 ContentPregunta: {ContentPregunta.name}, activo: {ContentPregunta.activeSelf}");
        Debug.Log($"[ShowMochila] 🔍 ContentPregunta parent: {ContentPregunta.transform.parent?.name}");
        
        Dictionary<int, bool> preguntasPlayer = GameManager.instance.playerData.getPreguntasDict();
        
        // Validar que el diccionario no sea null
        if (preguntasPlayer == null)
        {
            Debug.LogWarning("[ShowMochila] ⚠️ getPreguntasDict() devolvió NULL. Inicializando diccionario vacío.");
            preguntasPlayer = new Dictionary<int, bool>();
        }
        
        Debug.Log($"[ShowMochila] ✓ Preguntas del jugador cargadas: {preguntasPlayer.Count} preguntas");
        
        string nodesb = "???";
        try
        {
            nodesb = LanguageManager.Instancia.ObtenerTexto("menu_inventario.no_desbloq");
            Debug.Log($"[ShowMochila] ✓ Texto de LanguageManager obtenido: '{nodesb}'");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ShowMochila] ⚠️ No se pudo obtener texto de LanguageManager: " + e.Message);
        }

        Debug.Log($"[ShowMochila] 📝 Procesando {objectList.preguntas.Count} preguntas para mostrar en la UI...");

        int preguntasCreadas = 0;
        foreach (PreguntaObject question in objectList.preguntas)
        {
            GameObject pregunta = Instantiate(Pregunta, new Vector3(0, 0, 0), Quaternion.identity);
            pregunta.name = "Pregunta_" + question.ChallengeID;

            // USAR EL MÉTODO ORIGINAL PERO CON MEJORAS
            pregunta.transform.SetParent(ContentPregunta.transform, false);
            RectTransform rt = pregunta.GetComponent<RectTransform>();

            // Mantener la configuración original del prefab pero ajustar posición
            rt.localScale = new Vector3(1, 1, 1);
            rt.localRotation = Quaternion.identity;
            
            // USAR localPosition como en el código original (esto es clave!)
            rt.localPosition = new Vector3(0, cont, 0);
            cont = cont - 40;

            // Ajustar el fondo como en el código original
            RectTransform rtFondo = pregunta.transform.GetChild(0).GetComponent<RectTransform>();
            rtFondo.localPosition = new Vector3(138, rtFondo.localPosition.y, rtFondo.localPosition.z);

            UnityEngine.UI.Button button = pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(delegate { MostrarInfoPreguntas(question.ChallengeID); });

            string textoMostrar = nodesb;
            bool esDesbloqueada = preguntasPlayer.ContainsKey(question.ChallengeID);
            
            if (esDesbloqueada)
            {
                if (preguntasPlayer[question.ChallengeID])
                {
                    textoMostrar = "<color=green>" + question.question + "</color>";
                }
                else
                {
                    textoMostrar = "<color=red>" + question.question + "</color>";
                }
                questionsDict.Add(question.ChallengeID, (question, pregunta, "free"));
            }
            else
            {
                questionsDict.Add(question.ChallengeID, (question, pregunta, "block"));
            }
            
            pregunta.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = textoMostrar;
            preguntasCreadas++;
            
            // Log detallado cada 10 preguntas
            if (preguntasCreadas % 10 == 0 || preguntasCreadas == 1)
            {
                Debug.Log($"[ShowMochila] ✓ Pregunta {preguntasCreadas}/{objectList.preguntas.Count} creada en Y={cont + 40}");
            }
        }

        RectTransform contectRT = ContentPregunta.GetComponent<RectTransform>();
        float newHeight = cont * -1 + 20;
        contectRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        
        Debug.Log($"[ShowMochila] 📏 Tamaño del ContentPregunta ajustado a: {newHeight}");
        Debug.Log($"[ShowMochila] ✅ {preguntasCreadas} preguntas procesadas correctamente!");
        Debug.Log($"[ShowMochila] 📊 Resumen: {questionsDict.Count} preguntas en diccionario, {preguntasPlayer.Count} desbloqueadas por el jugador");
    }
    
    /// <summary>
    /// Reintenta cargar las preguntas después de un delay, cuando GameManager y LanguageManager estén inicializados
    /// </summary>
    private IEnumerator RetryLoadQuestionsLater(PreguntaObjectList objectList)
    {
        Debug.Log("[ShowMochila] 🔄 Esperando 0.5 segundos para reintentar cargar preguntas...");
        yield return new WaitForSeconds(0.5f);
        
        // Reintentar obtener las preguntas
        Debug.Log("[ShowMochila] 🔄 Reintentando cargar preguntas ahora...");
        GetPreguntaObjects(objectList);
    }

    public void OnPreguntasScreen()
    {
        Debug.Log("[ShowMochila] 📖 Abriendo pantalla de preguntas...");
        
        //-----------COLOCA LA SECCIÓN DE PREGUNTAS CORRECTAMENTE-----------
        // NOTA: Esta rotación causa que las preguntas se vean torcidas
        // Solo se ejecuta una vez, pero es problemática
        if (rotaUnaVez == 0) {
            // ============================================================
            // FIX: Comentar o ajustar esta rotación si causa problemas visuales
            // ============================================================
            // Si las preguntas se ven torcidas, comenta la siguiente línea:
            // ContenedorPreg.transform.Rotate(0, 127, 0);
            
            // O mejor aún, asegúrate de que la rotación sea correcta:
            if (ContenedorPreg != null)
            {
                Debug.Log($"[ShowMochila] 🔄 Rotación actual del ContenedorPreg: {ContenedorPreg.transform.localEulerAngles}");
                
                // Si quieres que las preguntas estén de frente, usa esto:
                ContenedorPreg.transform.localRotation = Quaternion.identity; // Sin rotación
                
                // O si necesitas una rotación específica, ajústala aquí:
                // ContenedorPreg.transform.localEulerAngles = new Vector3(0, 180, 0);
                
                Debug.Log($"[ShowMochila] ✓ Nueva rotación del ContenedorPreg: {ContenedorPreg.transform.localEulerAngles}");
            }
            else
            {
                Debug.LogWarning("[ShowMochila] ⚠️ ContenedorPreg es NULL, no se puede ajustar rotación");
            }
            
            rotaUnaVez++;
        }
        //------------------------------------------------------------------

        if (preguntasScreen != null)
        {
            preguntasScreen.SetActive(true);
            Debug.Log("[ShowMochila] ✓ preguntasScreen activado");
        }
        else
        {
            Debug.LogError("[ShowMochila] ❌ preguntasScreen es NULL");
        }
        
        if (infoScreen != null)
        {
            infoScreen.SetActive(false);
            Debug.Log("[ShowMochila] ✓ infoScreen desactivado");
        }
        
        Debug.Log("[ShowMochila] ✅ Pantalla de preguntas abierta correctamente");
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
