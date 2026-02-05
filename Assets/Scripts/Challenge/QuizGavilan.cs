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
/// Reutiliza los canvas existentes: "Canvas preguntas con imagenes" y "Canvas feedback"
/// </summary>
public class QuizGavilan : MonoBehaviour
{
    [Header("Canvas Existentes (Reutilizados de WallTrigger_2)")]
    public GameObject canvasPreguntasImagenes;
    public GameObject canvasFeedback;
    public GameObject controlPanel;
    
    [Header("UI Preguntas")]
    public Text preguntaText;
    public Text preguntaImagenText;
    public Image imagenPregunta;
    public Button opcionA, opcionB, opcionC, opcionD;
    public Image imagenA, imagenB, imagenC, imagenD;
    
    [Header("UI Feedback")]
    public Image feedbackImagen;
    public GameObject pacoCorrecto, pacoIncorrecto;
    
    [Header("UI Contador")]
    public Text contadorText;
    
    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip victoriaSound;
    
    [Header("Configuración")]
    public int preguntasParaGanar = 2;
    public GameObject joystick;
    public GameObject LogroSist;
    public GameObject fpscontroller;
    public GameObject cage;  // Objeto bloqueador que se destruye al completar
    
    [Header("Diálogos Completado/Pendiente")]
    public GameObject dialogoDesafioCompleto;
    public GameObject dialogoDesafioPendiente;
    
    [Header("Referencias Adicionales")]
    public Text cantidadEstrellas;
    public Text desafioText;
    public ShowMochila mochila;
    
    private List<PreguntaObject> preguntas;
    private int preguntaActual = 0;
    private int respuestasCorrectas = 0;
    private PreguntaObject currentQuestion;
    private bool quizCompletado = false;
    private bool quizEnProgreso = false;
    private int levelId = 4;
    
    public static DateTime inicio;
    public static bool quizIniciado = false;
    
    private NotificarLogros NL;
    private GameObject actionLogger;
    private string currentFeedback;
    
    private void Start()
    {
        var notifLogrosObj = GameObject.Find("NotifLogros");
        if (notifLogrosObj != null)
        {
            NL = notifLogrosObj.GetComponent<NotificarLogros>();
        }
        
        actionLogger = GameObject.Find("ActionLogger");
        
        // Buscar LogroSist si no está asignado
        if (LogroSist == null)
        {
            LogroSist = GameObject.Find("SistemaLogros");
            if (LogroSist != null)
            {
                Debug.Log("[QuizGavilan] LogroSist encontrado automáticamente");
            }
            else
            {
                Debug.LogWarning("[QuizGavilan] No se encontró SistemaLogros, búscalo en el Inspector");
            }
        }
        
        // Buscar fpscontroller si no está asignado
        if (fpscontroller == null)
        {
            fpscontroller = GameObject.FindGameObjectWithTag("Player");
            if (fpscontroller != null)
            {
                Debug.Log("[QuizGavilan] fpscontroller encontrado automáticamente");
            }
        }
        
        CargarPreguntas();
    }
    
    private void CargarPreguntas()
    {
        string path = "Questions/PreguntasGavilan";
        
        string idiomaGuardado = PlayerPrefs.GetString("idioma");
        if (idiomaGuardado == "textos_english")
            path = "Questions/QuestionsGavilan";
        else if (idiomaGuardado == "textos_portugues")
            path = "Questions/PerguntasGavilan";
        
        Debug.Log("[QuizGavilan] Intentando cargar preguntas desde: " + path);
        
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile == null)
        {
            Debug.LogWarning("[QuizGavilan] No se encontró el archivo en: " + path + ", intentando con PreguntasGavilan por defecto");
            jsonFile = Resources.Load<TextAsset>("Questions/PreguntasGavilan");
        }
        
        if (jsonFile != null)
        {
            try
            {
                // Intentar detectar y corregir problemas de codificación
                string jsonText = jsonFile.text;
                
                // Verificar si hay caracteres corruptos (? o ?)
                if (jsonText.Contains("?") || jsonText.Contains("?Cu?l") || jsonText.Contains("tr?ficas"))
                {
                    Debug.LogError("[QuizGavilan] ?? PROBLEMA DE CODIFICACIÓN DETECTADO en el JSON");
                    Debug.LogError("[QuizGavilan] El archivo no está siendo leído correctamente por Unity");
                    
                    // Intentar recodificar
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(jsonText);
                    jsonText = System.Text.Encoding.UTF8.GetString(bytes);
                    Debug.Log("[QuizGavilan] Intentando recodificar a UTF-8...");
                }
                
                Debug.Log("[QuizGavilan] Archivo cargado, primeros 300 caracteres: " + jsonText.Substring(0, Mathf.Min(300, jsonText.Length)));
                
                PreguntaObject[] preguntasArray = JsonHelper.GetJsonArray<PreguntaObject>(jsonText);
                preguntas = new List<PreguntaObject>(preguntasArray);
                
                Debug.Log("[QuizGavilan] ? Preguntas cargadas correctamente: " + preguntas.Count);
                
                // Verificar que las preguntas tengan datos válidos
                for (int i = 0; i < preguntas.Count; i++)
                {
                    var p = preguntas[i];
                    Debug.Log($"[QuizGavilan] Pregunta {i + 1}: {p.question}");
                    
                    if (p.options == null || p.options.Length == 0)
                    {
                        Debug.LogError($"[QuizGavilan] ?? La pregunta {i + 1} no tiene opciones!");
                    }
                    else
                    {
                        Debug.Log($"[QuizGavilan] - Tiene {p.options.Length} opciones");
                        for (int j = 0; j < p.options.Length; j++)
                        {
                            Debug.Log($"[QuizGavilan]   Opción {j}: {p.options[j].text}");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[QuizGavilan] ? Error al parsear JSON: " + e.Message);
                Debug.LogError("[QuizGavilan] Stack trace: " + e.StackTrace);
                preguntas = new List<PreguntaObject>();
            }
        }
        else
        {
            Debug.LogError("[QuizGavilan] ? No se pudo cargar el archivo de preguntas desde ninguna ruta");
            preguntas = new List<PreguntaObject>();
        }
    }
    
    public void IniciarQuiz()
    {
        if (quizCompletado || quizEnProgreso || preguntas == null || preguntas.Count == 0)
        {
            Debug.Log("[QuizGavilan] No se puede iniciar quiz");
            return;
        }
        
        Debug.Log("[QuizGavilan] Iniciando Quiz del Gavilán");
        
        inicio = DateTime.Now;
        quizIniciado = true;
        quizEnProgreso = true;
        SendStartRequest();
        
        preguntaActual = 0;
        respuestasCorrectas = 0;
        
#if UNITY_ANDROID || UNITY_IOS
        if (joystick != null) joystick.SetActive(false);
#endif
        
        MenuPausa.instance.Pausar();
        GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>().enabled = false;
        Time.timeScale = 1f;
        
        if (controlPanel != null)
        {
            var animator = controlPanel.GetComponent<Animator>();
            if (animator != null) animator.SetBool("hide", true);
        }
        
        ConfigurarBotones();
        MostrarPregunta();
        
        if (canvasPreguntasImagenes != null) canvasPreguntasImagenes.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (NL != null) NL.cerrar();
    }
    
    private void ConfigurarBotones()
    {
        if (opcionA != null)
        {
            opcionA.onClick.RemoveAllListeners();
            opcionA.onClick.AddListener(() => SeleccionarRespuesta(0));
        }
        if (opcionB != null)
        {
            opcionB.onClick.RemoveAllListeners();
            opcionB.onClick.AddListener(() => SeleccionarRespuesta(1));
        }
        if (opcionC != null)
        {
            opcionC.onClick.RemoveAllListeners();
            opcionC.onClick.AddListener(() => SeleccionarRespuesta(2));
        }
        if (opcionD != null)
        {
            opcionD.onClick.RemoveAllListeners();
            opcionD.onClick.AddListener(() => SeleccionarRespuesta(3));
        }
    }
    
    private void MostrarPregunta()
    {
        if (preguntas == null || preguntas.Count == 0)
        {
            Debug.LogError("[QuizGavilan] ? No hay preguntas disponibles para mostrar!");
            return;
        }
        
        if (preguntaActual >= preguntas.Count)
        {
            Debug.Log("[QuizGavilan] Todas las preguntas respondidas, mostrando resultado");
            MostrarResultado();
            return;
        }
        
        currentQuestion = preguntas[preguntaActual];
        
        if (currentQuestion == null)
        {
            Debug.LogError($"[QuizGavilan] ? La pregunta {preguntaActual} es null!");
            return;
        }
        
        Debug.Log($"[QuizGavilan] Mostrando pregunta {preguntaActual + 1}: {currentQuestion.question}");
        
        if (contadorText != null)
        {
            contadorText.text = "Pregunta " + (preguntaActual + 1) + "/" + preguntas.Count;
            Debug.Log($"[QuizGavilan] Contador actualizado: {contadorText.text}");
        }
        
        if (preguntaText != null)
        {
            preguntaText.text = currentQuestion.question;
            Debug.Log($"[QuizGavilan] preguntaText actualizado: {preguntaText.text}");
            Debug.Log($"[QuizGavilan] preguntaText GameObject: {preguntaText.gameObject.name}, activo: {preguntaText.gameObject.activeSelf}");
        }
        else
            Debug.LogWarning("[QuizGavilan] ?? preguntaText es null");
            
        if (preguntaImagenText != null)
        {
            preguntaImagenText.text = currentQuestion.question;
            Debug.Log($"[QuizGavilan] preguntaImagenText actualizado: {preguntaImagenText.text}");
            Debug.Log($"[QuizGavilan] preguntaImagenText GameObject: {preguntaImagenText.gameObject.name}, activo: {preguntaImagenText.gameObject.activeSelf}");
        }
        else
            Debug.LogWarning("[QuizGavilan] ?? preguntaImagenText es null");
        
        if (imagenPregunta != null && !string.IsNullOrEmpty(currentQuestion.image))
        {
            Sprite sprite = Resources.Load<Sprite>(currentQuestion.image);
            if (sprite == null)
                sprite = Resources.Load<Sprite>("Questions/ImagesGavilan/" + currentQuestion.ChallengeID);
            if (sprite != null)
            {
                imagenPregunta.sprite = sprite;
                imagenPregunta.enabled = true;
                Debug.Log($"[QuizGavilan] Imagen de pregunta cargada: {currentQuestion.image}");
            }
            else
            {
                Debug.LogWarning($"[QuizGavilan] No se encontró imagen para la pregunta: {currentQuestion.image}");
            }
        }
        
        if (currentQuestion.options == null)
        {
            Debug.LogError("[QuizGavilan] ? Las opciones de la pregunta son null!");
            return;
        }
        
        if (currentQuestion.options.Length < 4)
        {
            Debug.LogError($"[QuizGavilan] ? La pregunta solo tiene {currentQuestion.options.Length} opciones, se requieren 4!");
            return;
        }
        
        if (currentQuestion.options.Length >= 4)
        {
            if (opcionA != null)
            {
                var textA = opcionA.GetComponentInChildren<Text>();
                if (textA != null)
                {
                    textA.text = "A. " + currentQuestion.options[0].text;
                    Debug.Log($"[QuizGavilan] Opción A actualizada: {textA.text}");
                }
                else
                    Debug.LogWarning("[QuizGavilan] ?? No se encontró Text en opcionA");
            }
            else
                Debug.LogWarning("[QuizGavilan] ?? opcionA es null");
                
            if (opcionB != null)
            {
                var textB = opcionB.GetComponentInChildren<Text>();
                if (textB != null)
                {
                    textB.text = "B. " + currentQuestion.options[1].text;
                    Debug.Log($"[QuizGavilan] Opción B actualizada: {textB.text}");
                }
            }
            else
                Debug.LogWarning("[QuizGavilan] ?? opcionB es null");
                
            if (opcionC != null)
            {
                var textC = opcionC.GetComponentInChildren<Text>();
                if (textC != null)
                {
                    textC.text = "C. " + currentQuestion.options[2].text;
                    Debug.Log($"[QuizGavilan] Opción C actualizada: {textC.text}");
                }
            }
            else
                Debug.LogWarning("[QuizGavilan] ?? opcionC es null");
                
            if (opcionD != null)
            {
                var textD = opcionD.GetComponentInChildren<Text>();
                if (textD != null)
                {
                    textD.text = "D. " + currentQuestion.options[3].text;
                    Debug.Log($"[QuizGavilan] Opción D actualizada: {textD.text}");
                }
            }
            else
                Debug.LogWarning("[QuizGavilan] ?? opcionD es null");
            
            CargarImagenOpcion(imagenA, currentQuestion.options[0].image);
            CargarImagenOpcion(imagenB, currentQuestion.options[1].image);
            CargarImagenOpcion(imagenC, currentQuestion.options[2].image);
            CargarImagenOpcion(imagenD, currentQuestion.options[3].image);
        }
        
        if (opcionA != null) opcionA.interactable = true;
        if (opcionB != null) opcionB.interactable = true;
        if (opcionC != null) opcionC.interactable = true;
        if (opcionD != null) opcionD.interactable = true;
        
        Debug.Log("[QuizGavilan] ? Pregunta mostrada correctamente");
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
        
        Sprite defaultSprite = Resources.Load<Sprite>("Questions/Images/default.");
        if (defaultSprite != null)
            imagen.sprite = defaultSprite;
    }
    
    private void SeleccionarRespuesta(int indice)
    {
        if (currentQuestion == null || currentQuestion.options == null) return;
        
        if (opcionA != null) opcionA.interactable = false;
        if (opcionB != null) opcionB.interactable = false;
        if (opcionC != null) opcionC.interactable = false;
        if (opcionD != null) opcionD.interactable = false;
        
        bool esCorrecta = currentQuestion.options[indice].correctOption;
        
        if (actionLogger != null)
        {
            var logger = actionLogger.GetComponent<ActionLogger>();
            if (logger != null && logger.actionLogger != null)
                logger.actionLogger.agregarAccion(currentQuestion.question, esCorrecta ? "correcta" : "incorrecta");
        }
        
        currentFeedback = currentQuestion.feedback != null ? currentQuestion.feedback.feedback : "";
        
        if (esCorrecta)
        {
            respuestasCorrectas++;
            if (correctSound != null && AudioSourceSFX.instance != null)
                AudioSourceSFX.instance.PlaySound(correctSound);
        }
        else
        {
            if (incorrectSound != null && AudioSourceSFX.instance != null)
                AudioSourceSFX.instance.PlaySound(incorrectSound);
        }
        
        MostrarFeedback(esCorrecta);
    }
    
    private void MostrarFeedback(bool correcto)
    {
        if (canvasPreguntasImagenes != null)
            canvasPreguntasImagenes.SetActive(false);
        
        if (canvasFeedback != null)
        {
            var tituloCorrecto = canvasFeedback.transform.Find("Titulo correcto");
            if (tituloCorrecto != null) tituloCorrecto.gameObject.SetActive(correcto);
            
            var subtituloCorrecto = canvasFeedback.transform.Find("Subtitulo correcto");
            if (subtituloCorrecto != null) subtituloCorrecto.gameObject.SetActive(correcto);
            
            var tituloIncorrecto = canvasFeedback.transform.Find("Titulo incorrecto");
            if (tituloIncorrecto != null) tituloIncorrecto.gameObject.SetActive(!correcto);
            
            var feedbackTextObj = canvasFeedback.transform.Find("Feedback");
            if (feedbackTextObj != null)
            {
                var feedbackText = feedbackTextObj.GetComponent<Text>();
                if (feedbackText != null) feedbackText.text = currentFeedback;
            }
            
            var checkObj = canvasFeedback.transform.Find("check");
            if (checkObj != null) checkObj.gameObject.SetActive(correcto);
            
            var crossObj = canvasFeedback.transform.Find("cross");
            if (crossObj != null) crossObj.gameObject.SetActive(!correcto);
            
            if (feedbackImagen != null && currentQuestion != null)
            {
                Sprite sprite = Resources.Load<Sprite>("Questions/ImagesGavilan/" + currentQuestion.ChallengeID);
                if (sprite != null) feedbackImagen.sprite = sprite;
            }
            
            var buttonObj = canvasFeedback.transform.Find("Button");
            if (buttonObj != null)
            {
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(ContinuarDespuesFeedback);
                }
            }
            
            canvasFeedback.SetActive(true);
        }
        
        if (pacoCorrecto != null) pacoCorrecto.SetActive(correcto);
        if (pacoIncorrecto != null) pacoIncorrecto.SetActive(!correcto);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void ContinuarDespuesFeedback()
    {
        if (canvasFeedback != null)
            canvasFeedback.SetActive(false);
        
        preguntaActual++;
        
        if (preguntaActual < preguntas.Count)
        {
            ConfigurarBotones();
            MostrarPregunta();
            if (canvasPreguntasImagenes != null)
                canvasPreguntasImagenes.SetActive(true);
        }
        else
        {
            MostrarResultado();
        }
    }
    
    private void MostrarResultado()
    {
        quizEnProgreso = false;
        bool ganoMedalla = respuestasCorrectas >= preguntasParaGanar;
        
        Debug.Log($"[QuizGavilan] ========== RESULTADO DEL QUIZ ==========");
        Debug.Log($"[QuizGavilan] Respuestas correctas: {respuestasCorrectas}/{preguntas.Count}");
        Debug.Log($"[QuizGavilan] Mínimo requerido: {preguntasParaGanar}");
        Debug.Log($"[QuizGavilan] ¿Ganó medalla?: {ganoMedalla}");
        Debug.Log($"[QuizGavilan] ======================================");
        
        if (ganoMedalla)
        {
            Debug.Log("[QuizGavilan] ¡QUIZ APROBADO! Reproduciendo sonido de victoria y completando desafío...");
            if (victoriaSound != null && AudioSourceSFX.instance != null)
                AudioSourceSFX.instance.PlaySound(victoriaSound);
            
            CompletarDesafio();
        }
        else
        {
            Debug.Log("[QuizGavilan] Quiz no aprobado. El jugador puede intentar de nuevo.");
        }
        
        MostrarDialogoResultado(ganoMedalla);
        quizCompletado = ganoMedalla;
        
        Debug.Log($"[QuizGavilan] quizCompletado = {quizCompletado}");
    }
    
    private void MostrarDialogoResultado(bool victoria)
    {
        if (canvasFeedback != null)
        {
            var tituloCorrecto = canvasFeedback.transform.Find("Titulo correcto");
            if (tituloCorrecto != null)
            {
                tituloCorrecto.gameObject.SetActive(victoria);
                var text = tituloCorrecto.GetComponent<Text>();
                if (text != null) text.text = victoria ? "¡FELICIDADES!" : "¡Sigue intentando!";
            }
            
            var subtituloCorrecto = canvasFeedback.transform.Find("Subtitulo correcto");
            if (subtituloCorrecto != null)
            {
                subtituloCorrecto.gameObject.SetActive(victoria);
                var text = subtituloCorrecto.GetComponent<Text>();
                if (text != null) text.text = victoria ? "¡Has ganado la Medalla del Gavilán!" : "";
            }
            
            var tituloIncorrecto = canvasFeedback.transform.Find("Titulo incorrecto");
            if (tituloIncorrecto != null) tituloIncorrecto.gameObject.SetActive(!victoria);
            
            var feedbackTextObj = canvasFeedback.transform.Find("Feedback");
            if (feedbackTextObj != null)
            {
                var feedbackText = feedbackTextObj.GetComponent<Text>();
                if (feedbackText != null)
                {
                    if (victoria)
                        feedbackText.text = "Has respondido " + respuestasCorrectas + " de " + preguntas.Count + " preguntas correctamente.\n\nAhora comprendes la importancia de las cadenas tróficas.\n\nRecuerda: NO debes alimentar o cazar animales silvestres.";
                    else
                        feedbackText.text = "Has respondido " + respuestasCorrectas + " de " + preguntas.Count + " preguntas correctamente.\n\nNecesitas al menos " + preguntasParaGanar + " respuestas correctas.\n\nTe recomendamos ver el video nuevamente.";
                }
            }
            
            var checkObj = canvasFeedback.transform.Find("check");
            if (checkObj != null) checkObj.gameObject.SetActive(victoria);
            
            var crossObj = canvasFeedback.transform.Find("cross");
            if (crossObj != null) crossObj.gameObject.SetActive(!victoria);
            
            var buttonObj = canvasFeedback.transform.Find("Button");
            if (buttonObj != null)
            {
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(CerrarResultado);
                }
            }
            
            if (pacoCorrecto != null) pacoCorrecto.SetActive(victoria);
            if (pacoIncorrecto != null) pacoIncorrecto.SetActive(!victoria);
            
            canvasFeedback.SetActive(true);
        }
    }
    
    private void CompletarDesafio()
    {
        Debug.Log("[QuizGavilan] ¡Desafío completado!");
        
        // ========================================
        // PASO 1: DESBLOQUEAR EL PASO (CRÍTICO - SIEMPRE SE EJECUTA)
        // ========================================
        if (cage != null)
        {
            Debug.Log("[QuizGavilan] Destruyendo cage para desbloquear el paso");
            Destroy(cage);
        }
        else
        {
            Debug.LogWarning("[QuizGavilan] cage no está asignado. El paso no se desbloqueará!");
        }
        
        // Actualizar Player Data (crítico) - CON VERIFICACIÓN
        if (Player.instance != null && Player.instance.playerData != null)
        {
            // Verificar que el array de misiones tenga suficiente tamaño
            if (Player.instance.playerData.misiones != null && Player.instance.playerData.misiones.Length > 2)
            {
                Player.instance.playerData.misiones[2] = true;
                Debug.Log("[QuizGavilan] PlayerData actualizado: misiones[2] = true");
            }
            else
            {
                Debug.LogWarning("[QuizGavilan] El array de misiones no tiene suficientes elementos. Tamaño: " + (Player.instance.playerData.misiones?.Length ?? 0));
            }
            
            // Verificar que el array de logros tenga suficiente tamaño
            if (Player.instance.playerData.logros != null && Player.instance.playerData.logros.Length > 2)
            {
                Player.instance.playerData.logros[2] = DateTime.Now.ToString();
                Debug.Log("[QuizGavilan] PlayerData actualizado: logros[2] = " + Player.instance.playerData.logros[2]);
            }
            else
            {
                Debug.LogWarning("[QuizGavilan] El array de logros no tiene suficientes elementos. Tamaño: " + (Player.instance.playerData.logros?.Length ?? 0));
            }
        }
        else
        {
            Debug.LogWarning("[QuizGavilan] Player.instance o playerData es null");
        }
        
        // Actualizar diálogos
        if (dialogoDesafioPendiente != null) dialogoDesafioPendiente.SetActive(false);
        if (dialogoDesafioCompleto != null) dialogoDesafioCompleto.SetActive(true);
        
        // Dar experiencia si es posible
        if (fpscontroller != null)
        {
            var player = fpscontroller.GetComponent<Player>();
            if (player != null) 
            {
                player.gainEXP(4);
                Debug.Log("[QuizGavilan] EXP otorgado: +4");
            }
        }
        
        // Iniciar siguiente desafío
        ChallengePass5.inicio = DateTime.Now;
        
        // ========================================
        // PASO 2: LOGROS Y MEDALLAS (OPCIONAL - SI EXISTE LogroSist)
        // ========================================
        if (LogroSist != null)
        {
            LogrosGlobales logros = LogroSist.GetComponent<LogrosGlobales>();
            if (logros != null)
            {
                // Verificar que las listas estén inicializadas
                if (logros.logros != null && logros.logros.Count > 2 && logros.misiones != null && logros.misiones.Count > 2)
                {
                    Debug.Log("[QuizGavilan] LogrosGlobales encontrado, registrando logro y misión");
                    
                    try
                    {
                        // Progresar logro y misión
                        logros.ProgresarLogro(2);
                        logros.ProgresarMision(2, "Test Cadenas Tróficas");
                        
                        // Actualizar UI de estrellas/desafíos
                        if (cantidadEstrellas != null && desafioText != null)
                        {
                            int x = 0, y = 0;
                            int.TryParse(cantidadEstrellas.text, out x);
                            int.TryParse(desafioText.text, out y);
                            cantidadEstrellas.text = (x + 10).ToString();
                            desafioText.text = (y + 1).ToString();
                        }
                        
                        // Registrar en servidor
                        Mision mision = logros.misiones[2];
                        
                        if (!GameManager.OfflineMode)
                        {
                            Debug.Log("[QuizGavilan] Registrando misión completada (online)");
                            Peticiones.instance.registerPlayerMission(mision.nombre, Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                            Peticiones.instance.registerFinishMission(Player.instance.playerData, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), levelId);
                            Peticiones.instance.registerPlayerPrize(logros.logros[2].nombre, Player.instance.playerData);
                        }
                        else
                        {
                            Debug.Log("[QuizGavilan] Registrando misión completada (offline)");
                            var acObj = GameObject.Find("ActionLogger");
                            if (acObj != null)
                            {
                                ActionLogger ac = acObj.GetComponent<ActionLogger>();
                                if (ac != null && ac.actionLogger != null)
                                {
                                    ac.actionLogger.online = false;
                                    ac.actionLogger.agregarPeticion("mision", mision.nombre, Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                                    ac.actionLogger.agregarPeticion("finish mision", "" + levelId, Player.instance.playerData.Token, null, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                                    ac.actionLogger.agregarPeticion("prize", logros.logros[2].nombre, Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                                }
                            }
                        }
                        
                        CreateStadistics();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("[QuizGavilan] Error al registrar logros/misiones: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogWarning("[QuizGavilan] LogrosGlobales no está completamente inicializado. Logros: " + (logros.logros?.Count ?? 0) + ", Misiones: " + (logros.misiones?.Count ?? 0));
                }
            }
            else
            {
                Debug.LogWarning("[QuizGavilan] No se encontró componente LogrosGlobales en LogroSist");
            }
        }
        else
        {
            Debug.LogWarning("[QuizGavilan] LogroSist no está asignado. Se omitirá el registro de logros/medallas.");
        }
        
        Debug.Log("[QuizGavilan] Desafío completado exitosamente - PASO DESBLOQUEADO");
    }
    
    private void CerrarResultado()
    {
        Debug.Log("[QuizGavilan] Cerrando resultado del quiz");
        
        if (canvasFeedback != null) canvasFeedback.SetActive(false);
        if (canvasPreguntasImagenes != null) canvasPreguntasImagenes.SetActive(false);
        
#if UNITY_ANDROID || UNITY_IOS
        if (joystick != null) joystick.SetActive(true);
#endif
        
        if (controlPanel != null)
        {
            var animator = controlPanel.GetComponent<Animator>();
            if (animator != null) animator.SetBool("hide", false);
        }
        
        Time.timeScale = 1f;
        
        // Reanudar con verificación de seguridad
        if (MenuPausa.instance != null)
        {
            try
            {
                MenuPausa.instance.Reanudar();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[QuizGavilan] Error al reanudar MenuPausa: " + e.Message);
            }
        }
        
        // Reactivar el mouse controller con verificación
        try
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var mouseController = player.GetComponent<MouseController>();
                if (mouseController != null) 
                {
                    mouseController.enabled = true;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[QuizGavilan] Error al reactivar MouseController: " + e.Message);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("[QuizGavilan] Quiz cerrado correctamente");
    }
    
    public void SendStartRequest()
    {
        try
        {
            if (!GameManager.OfflineMode)
            {
                JObject res = Peticiones.instance.registerStartMission("Bosque-Estación 4", Player.instance.playerData, inicio.ToString("yyyy-MM-dd hh:mm:ss"));
                if (res != null && res["payload"] != null && res["payload"]["GameLevelInstanceId"] != null)
                    levelId = (int)res["payload"]["GameLevelInstanceId"];
            }
            else
            {
                var acObj = GameObject.Find("ActionLogger");
                if (acObj != null)
                {
                    ActionLogger ac = acObj.GetComponent<ActionLogger>();
                    if (ac != null && ac.actionLogger != null)
                    {
                        ac.actionLogger.online = false;
                        ac.actionLogger.agregarPeticion("start mision", "Bosque-Estación 4", Player.instance.playerData.Token, inicio.ToString("yyyy-MM-dd hh:mm:ss"), null);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("[QuizGavilan] Error al registrar inicio: " + e.Message);
        }
    }
    
    public void CreateStadistics()
    {
        try
        {
            if (LogroSist == null) return;
            var logrosGlobales = LogroSist.GetComponent<LogrosGlobales>();
            if (logrosGlobales == null || logrosGlobales.misiones == null || logrosGlobales.misiones.Count <= 2) return;
            
            StadisticsData.Stadistics tmp1 = new StadisticsData.Stadistics("mission_data");
            StadisticsData.DataMission dat1 = new StadisticsData.DataMission(inicio, logrosGlobales.misiones[2].nombre);
            tmp1.data = dat1;
            GameManager.instance.CallEnumerator(JsonConvert.SerializeObject(tmp1, Formatting.Indented));
            GameManager.estas.lista.Add(tmp1);
            
            StadisticsData.Stadistics tmp2 = new StadisticsData.Stadistics("experiencie_data");
            StadisticsData.DataExperiencie dat2 = new StadisticsData.DataExperiencie(4);
            tmp2.data = dat2;
            GameManager.instance.CallEnumerator(JsonConvert.SerializeObject(tmp2, Formatting.Indented));
            GameManager.estas.lista.Add(tmp2);
            
            if (logrosGlobales.logros != null && logrosGlobales.logros.Count > 2)
            {
                StadisticsData.Stadistics tmp3 = new StadisticsData.Stadistics("prize_data");
                StadisticsData.DataPrize dat3 = new StadisticsData.DataPrize(logrosGlobales.logros[2].nombre);
                tmp3.data = dat3;
                GameManager.instance.CallEnumerator(JsonConvert.SerializeObject(tmp3, Formatting.Indented));
                GameManager.estas.lista.Add(tmp3);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[QuizGavilan] Error estadísticas: " + e.Message);
        }
    }
    
    public bool EstaCompletado() { return quizCompletado; }
    
    public void ResetearQuiz()
    {
        preguntaActual = 0;
        respuestasCorrectas = 0;
        quizEnProgreso = false;
    }
}
