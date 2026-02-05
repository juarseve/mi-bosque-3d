using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Quiz del Gavilán - Test de Sabiduría sobre Cadenas Tróficas
/// El jugador debe responder 3 preguntas y acertar al menos 2 para obtener la insignia
/// </summary>
public class QuizGavilan : MonoBehaviour
{
    [Header("UI Referencias")]
    public GameObject canvasQuiz;
    public GameObject canvasFeedback;
    public GameObject canvasResultado;
    
    [Header("Pregunta UI")]
    public Text preguntaText;
    public TextMeshProUGUI preguntaTMP;
    public Image imagenPregunta;
    public Button opcionA, opcionB, opcionC, opcionD;
    public Image imagenA, imagenB, imagenC, imagenD;
    public Text contadorText; // "Pregunta 1/3"
    
    [Header("Feedback UI")]
    public Text feedbackText;
    public Image feedbackImagen;
    public GameObject checkIcon, crossIcon;
    public GameObject tituloCorrecto, tituloIncorrecto;
    public Button continuarFeedbackBtn;
    
    [Header("Resultado Final UI")]
    public Text resultadoTituloText;
    public Text resultadoDescripcionText;
    public Image medallaImagen;
    public Button continuarResultadoBtn;
    public GameObject medallaContainer;
    
    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip victoriaSound;
    
    [Header("Configuración")]
    public int preguntasParaGanar = 2;
    public GameObject joystick;
    public GameObject LogroSist;
    public GameObject fpscontroller;
    
    [Header("Diálogos Completado/Pendiente")]
    public GameObject dialogoDesafioCompleto;
    public GameObject dialogoDesafioPendiente;
    
    // Variables internas
    private List<PreguntaObject> preguntas;
    private int preguntaActual = 0;
    private int respuestasCorrectas = 0;
    private PreguntaObject currentQuestion;
    private bool quizCompletado = false;
    private int levelId = 4;
    
    public static DateTime inicio;
    public static bool quizIniciado = false;
    
    private NotificarLogros NL;
    private GameObject actionLogger;
    
    private void Start()
    {
        var notifLogrosObj = GameObject.Find("NotifLogros");
        if (notifLogrosObj != null)
        {
            NL = notifLogrosObj.GetComponent<NotificarLogros>();
        }
        
        actionLogger = GameObject.Find("ActionLogger");
        
        // Cargar preguntas del JSON
        CargarPreguntas();
        
        // Ocultar canvas al inicio
        if (canvasQuiz != null) canvasQuiz.SetActive(false);
        if (canvasFeedback != null) canvasFeedback.SetActive(false);
        if (canvasResultado != null) canvasResultado.SetActive(false);
    }
    
    /// <summary>
    /// Carga las preguntas del JSON específico del Gavilán
    /// </summary>
    private void CargarPreguntas()
    {
        string path = "Questions/PreguntasGavilan";
        
        // Verificar idioma para cargar el archivo correcto
        string idiomaGuardado = PlayerPrefs.GetString("idioma");
        if (idiomaGuardado == "textos_english")
            path = "Questions/QuestionsGavilan";
        else if (idiomaGuardado == "textos_portugues")
            path = "Questions/PerguntasGavilan";
        
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile == null)
        {
            // Fallback al español si no existe el archivo de idioma
            jsonFile = Resources.Load<TextAsset>("Questions/PreguntasGavilan");
        }
        
        if (jsonFile != null)
        {
            PreguntaObject[] preguntasArray = JsonHelper.GetJsonArray<PreguntaObject>(jsonFile.text);
            preguntas = new List<PreguntaObject>(preguntasArray);
            Debug.Log("[QuizGavilan] Preguntas cargadas: " + preguntas.Count);
        }
        else
        {
            Debug.LogError("[QuizGavilan] No se pudo cargar el archivo de preguntas");
            preguntas = new List<PreguntaObject>();
        }
    }
    
    /// <summary>
    /// Inicia el quiz cuando el jugador interactúa con el trigger
    /// </summary>
    public void IniciarQuiz()
    {
        if (quizCompletado || preguntas == null || preguntas.Count == 0)
        {
            Debug.LogWarning("[QuizGavilan] Quiz ya completado o sin preguntas");
            return;
        }
        
        Debug.Log("[QuizGavilan] Iniciando Quiz del Gavilán");
        
        // Registrar inicio
        inicio = DateTime.Now;
        quizIniciado = true;
        SendStartRequest();
        
        // Resetear variables
        preguntaActual = 0;
        respuestasCorrectas = 0;
        
        // Ocultar joystick en móvil
#if UNITY_ANDROID || UNITY_IOS
        if (joystick != null) joystick.SetActive(false);
#endif
        
        // Pausar juego
        MenuPausa.instance.Pausar();
        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = false;
        Time.timeScale = 1f;
        
        // Configurar botones
        ConfigurarBotones();
        
        // Mostrar primera pregunta
        MostrarPregunta();
        
        // Activar canvas
        canvasQuiz.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (NL != null) NL.cerrar();
    }
    
    private void ConfigurarBotones()
    {
        opcionA.onClick.RemoveAllListeners();
        opcionB.onClick.RemoveAllListeners();
        opcionC.onClick.RemoveAllListeners();
        opcionD.onClick.RemoveAllListeners();
        
        opcionA.onClick.AddListener(() => SeleccionarRespuesta(0));
        opcionB.onClick.AddListener(() => SeleccionarRespuesta(1));
        opcionC.onClick.AddListener(() => SeleccionarRespuesta(2));
        opcionD.onClick.AddListener(() => SeleccionarRespuesta(3));
        
        if (continuarFeedbackBtn != null)
        {
            continuarFeedbackBtn.onClick.RemoveAllListeners();
            continuarFeedbackBtn.onClick.AddListener(ContinuarDespuesFeedback);
        }
        
        if (continuarResultadoBtn != null)
        {
            continuarResultadoBtn.onClick.RemoveAllListeners();
            continuarResultadoBtn.onClick.AddListener(CerrarResultado);
        }
    }
    
    /// <summary>
    /// Muestra la pregunta actual en la UI
    /// </summary>
    private void MostrarPregunta()
    {
        if (preguntaActual >= preguntas.Count)
        {
            MostrarResultado();
            return;
        }
        
        currentQuestion = preguntas[preguntaActual];
        
        // Actualizar contador
        if (contadorText != null)
        {
            contadorText.text = $"Pregunta {preguntaActual + 1}/{preguntas.Count}";
        }
        
        // Actualizar texto de pregunta
        if (preguntaText != null)
        {
            preguntaText.text = currentQuestion.question;
        }
        if (preguntaTMP != null)
        {
            preguntaTMP.text = currentQuestion.question;
        }
        
        // Cargar imagen de pregunta
        if (imagenPregunta != null && !string.IsNullOrEmpty(currentQuestion.image))
        {
            Sprite sprite = Resources.Load<Sprite>(currentQuestion.image);
            if (sprite != null)
            {
                imagenPregunta.sprite = sprite;
                imagenPregunta.enabled = true;
            }
            else
            {
                imagenPregunta.enabled = false;
            }
        }
        
        // Actualizar opciones
        if (currentQuestion.options != null && currentQuestion.options.Length >= 4)
        {
            opcionA.GetComponentInChildren<Text>().text = "A. " + currentQuestion.options[0].text;
            opcionB.GetComponentInChildren<Text>().text = "B. " + currentQuestion.options[1].text;
            opcionC.GetComponentInChildren<Text>().text = "C. " + currentQuestion.options[2].text;
            opcionD.GetComponentInChildren<Text>().text = "D. " + currentQuestion.options[3].text;
            
            // Cargar imágenes de opciones
            CargarImagenOpcion(imagenA, currentQuestion.options[0].image);
            CargarImagenOpcion(imagenB, currentQuestion.options[1].image);
            CargarImagenOpcion(imagenC, currentQuestion.options[2].image);
            CargarImagenOpcion(imagenD, currentQuestion.options[3].image);
        }
        
        // Habilitar botones
        opcionA.interactable = true;
        opcionB.interactable = true;
        opcionC.interactable = true;
        opcionD.interactable = true;
    }
    
    private void CargarImagenOpcion(Image imagen, string path)
    {
        if (imagen == null) return;
        
        if (!string.IsNullOrEmpty(path))
        {
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                imagen.sprite = sprite;
                imagen.enabled = true;
                return;
            }
        }
        imagen.enabled = false;
    }
    
    /// <summary>
    /// Procesa la respuesta seleccionada por el jugador
    /// </summary>
    private void SeleccionarRespuesta(int indice)
    {
        if (currentQuestion == null || currentQuestion.options == null) return;
        
        // Deshabilitar botones mientras procesa
        opcionA.interactable = false;
        opcionB.interactable = false;
        opcionC.interactable = false;
        opcionD.interactable = false;
        
        bool esCorrecta = currentQuestion.options[indice].correctOption;
        string respuestaTexto = currentQuestion.options[indice].text;
        
        // Log de acción
        if (actionLogger != null)
        {
            var logger = actionLogger.GetComponent<ActionLogger>();
            if (logger != null && logger.actionLogger != null)
            {
                logger.actionLogger.agregarAccion(currentQuestion.question, esCorrecta ? "correcta" : "incorrecta");
            }
        }
        
        if (esCorrecta)
        {
            respuestasCorrectas++;
            if (correctSound != null)
            {
                AudioSourceSFX.instance.PlaySound(correctSound);
            }
            MostrarFeedback(true, currentQuestion.feedback.feedback);
        }
        else
        {
            if (incorrectSound != null)
            {
                AudioSourceSFX.instance.PlaySound(incorrectSound);
            }
            MostrarFeedback(false, currentQuestion.feedback.feedback);
        }
    }
    
    /// <summary>
    /// Muestra el feedback después de cada respuesta
    /// </summary>
    private void MostrarFeedback(bool correcto, string feedback)
    {
        canvasQuiz.SetActive(false);
        canvasFeedback.SetActive(true);
        
        if (tituloCorrecto != null) tituloCorrecto.SetActive(correcto);
        if (tituloIncorrecto != null) tituloIncorrecto.SetActive(!correcto);
        if (checkIcon != null) checkIcon.SetActive(correcto);
        if (crossIcon != null) crossIcon.SetActive(!correcto);
        
        if (feedbackText != null)
        {
            feedbackText.text = feedback;
        }
        
        // Imagen de feedback
        if (feedbackImagen != null && currentQuestion != null)
        {
            string imagePath = "Questions/ImagesGavilan/" + currentQuestion.ChallengeID;
            Sprite sprite = Resources.Load<Sprite>(imagePath);
            if (sprite != null)
            {
                feedbackImagen.sprite = sprite;
                feedbackImagen.enabled = true;
            }
            else
            {
                feedbackImagen.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Continúa a la siguiente pregunta después del feedback
    /// </summary>
    private void ContinuarDespuesFeedback()
    {
        canvasFeedback.SetActive(false);
        preguntaActual++;
        
        if (preguntaActual < preguntas.Count)
        {
            canvasQuiz.SetActive(true);
            MostrarPregunta();
        }
        else
        {
            MostrarResultado();
        }
    }
    
    /// <summary>
    /// Muestra el resultado final del quiz
    /// </summary>
    private void MostrarResultado()
    {
        canvasQuiz.SetActive(false);
        canvasFeedback.SetActive(false);
        canvasResultado.SetActive(true);
        
        bool ganoMedalla = respuestasCorrectas >= preguntasParaGanar;
        
        if (ganoMedalla)
        {
            // ¡Victoria!
            if (victoriaSound != null)
            {
                AudioSourceSFX.instance.PlaySound(victoriaSound);
            }
            
            if (resultadoTituloText != null)
            {
                resultadoTituloText.text = "¡Felicidades!";
            }
            if (resultadoDescripcionText != null)
            {
                resultadoDescripcionText.text = $"Has respondido {respuestasCorrectas} de {preguntas.Count} preguntas correctamente.\n\n¡Has ganado la Medalla del Gavilán!\n\nAhora comprendes la importancia de las cadenas tróficas y cómo proteger el ecosistema.";
            }
            if (medallaContainer != null)
            {
                medallaContainer.SetActive(true);
            }
            
            // Otorgar logro
            CompletarDesafio();
        }
        else
        {
            // No ganó la medalla
            if (resultadoTituloText != null)
            {
                resultadoTituloText.text = "¡Sigue intentando!";
            }
            if (resultadoDescripcionText != null)
            {
                resultadoDescripcionText.text = $"Has respondido {respuestasCorrectas} de {preguntas.Count} preguntas correctamente.\n\nNecesitas al menos {preguntasParaGanar} respuestas correctas para ganar la medalla.\n\nTe recomendamos ver el video nuevamente para aprender más sobre las cadenas tróficas.";
            }
            if (medallaContainer != null)
            {
                medallaContainer.SetActive(false);
            }
        }
        
        quizCompletado = ganoMedalla;
    }
    
    /// <summary>
    /// Completa el desafío y otorga la insignia
    /// </summary>
    private void CompletarDesafio()
    {
        Debug.Log("[QuizGavilan] ¡Desafío completado! Otorgando insignia del Gavilán.");
        
        // Actualizar diálogos
        if (dialogoDesafioPendiente != null) dialogoDesafioPendiente.SetActive(false);
        if (dialogoDesafioCompleto != null) dialogoDesafioCompleto.SetActive(true);
        
        // Dar experiencia
        if (fpscontroller != null)
        {
            fpscontroller.GetComponent<Player>().gainEXP(4);
        }
        
        // Progresar logro (índice 2 es el del Gavilán según el sistema actual)
        if (LogroSist != null)
        {
            LogrosGlobales logros = LogroSist.GetComponent<LogrosGlobales>();
            logros.ProgresarLogro(2);
            logros.ProgresarMision(2, "Test Cadenas Tróficas");
            
            // Registrar en servidor
            Mision mision = logros.misiones[2];
            Player.instance.playerData.misiones[2] = true;
            Player.instance.playerData.logros[2] = DateTime.Now.ToString();
            
            if (!GameManager.OfflineMode)
            {
                Peticiones.instance.registerPlayerMission(mision.nombre, Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                Peticiones.instance.registerFinishMission(Player.instance.playerData, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), this.levelId);
                Peticiones.instance.registerPlayerPrize(logros.logros[2].nombre, Player.instance.playerData);
            }
            else
            {
                ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                ac.actionLogger.online = false;
                ac.actionLogger.agregarPeticion("mision", mision.nombre, Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                ac.actionLogger.agregarPeticion("finish mision", "" + this.levelId, Player.instance.playerData.Token, null, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                ac.actionLogger.agregarPeticion("prize", logros.logros[2].nombre, Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            }
        }
        
        // Crear estadísticas
        CreateStadistics();
        
        // Iniciar siguiente misión
        ChallengePass5.inicio = DateTime.Now;
    }
    
    /// <summary>
    /// Cierra el resultado y vuelve al juego
    /// </summary>
    private void CerrarResultado()
    {
        canvasResultado.SetActive(false);
        
#if UNITY_ANDROID || UNITY_IOS
        if (joystick != null) joystick.SetActive(true);
#endif
        
        Time.timeScale = 1f;
        MenuPausa.instance.Reanudar();
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<MouseController>().enabled = true;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// Envía la petición de inicio de misión al servidor
    /// </summary>
    public void SendStartRequest()
    {
        try
        {
            if (!GameManager.OfflineMode)
            {
                JObject res = Peticiones.instance.registerStartMission("Bosque-Estación 4", Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"));
                if (res != null && res["payload"] != null && res["payload"]["GameLevelInstanceId"] != null)
                {
                    levelId = (int)res["payload"]["GameLevelInstanceId"];
                }
            }
            else
            {
                ActionLogger ac = GameObject.Find("ActionLogger").GetComponent<ActionLogger>();
                ac.actionLogger.online = false;
                ac.actionLogger.agregarPeticion("start mision", "Bosque-Estación 4", Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), null);
            }
        }
        catch (Exception e)
        {
            Debug.Log("[QuizGavilan] Error al registrar inicio de nivel: " + e.Message);
        }
    }
    
    /// <summary>
    /// Crea las estadísticas del desafío completado
    /// </summary>
    public void CreateStadistics()
    {
        try
        {
            StadisticsData.Stadistics tmp1 = new StadisticsData.Stadistics("mission_data");
            string name = LogroSist.GetComponent<LogrosGlobales>().misiones[2].nombre;
            StadisticsData.DataMission dat1 = new StadisticsData.DataMission(inicio, name);
            tmp1.data = dat1;
            string json = JsonConvert.SerializeObject(tmp1, Formatting.Indented);
            GameManager.instance.CallEnumerator(json);
            GameManager.estas.lista.Add(tmp1);
            
            StadisticsData.Stadistics tmp2 = new StadisticsData.Stadistics("experiencie_data");
            StadisticsData.DataExperiencie dat2 = new StadisticsData.DataExperiencie(4);
            tmp2.data = dat2;
            string json2 = JsonConvert.SerializeObject(tmp2, Formatting.Indented);
            GameManager.instance.CallEnumerator(json2);
            GameManager.estas.lista.Add(tmp2);
            
            StadisticsData.Stadistics tmp3 = new StadisticsData.Stadistics("prize_data");
            string prize = LogroSist.GetComponent<LogrosGlobales>().logros[2].nombre;
            StadisticsData.DataPrize dat3 = new StadisticsData.DataPrize(prize);
            tmp3.data = dat3;
            string json3 = JsonConvert.SerializeObject(tmp3, Formatting.Indented);
            GameManager.instance.CallEnumerator(json3);
            GameManager.estas.lista.Add(tmp3);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[QuizGavilan] Error creando estadísticas: " + e.Message);
        }
    }
    
    /// <summary>
    /// Método público para verificar si el quiz ya fue completado
    /// </summary>
    public bool EstaCompletado()
    {
        return quizCompletado;
    }
    
    /// <summary>
    /// Resetea el quiz para permitir reintentos
    /// </summary>
    public void ResetearQuiz()
    {
        preguntaActual = 0;
        respuestasCorrectas = 0;
        quizCompletado = false;
    }
}
