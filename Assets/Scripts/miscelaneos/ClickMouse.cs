using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System;


public class ClickMouse : MonoBehaviour, IInteractable
{
    private Collider cameraBlocker;

    public GameObject Panel;
    public GameObject Galeria;
    private Galery GaleryScript;
    public GameObject Panel3;
    private MouseController mouseController;
    public static bool IsGalery = false;
    public GameObject CuadroChallengeDos;
    public bool isAnimal;
    public bool isPlant;
    public bool isKnown;
    public GameObject logroSist;
    public GameObject fpscontroller;
    public GameObject canvasJoy = null;
    public GameObject actionLogger;

    private GameObject puntero;

    private bool tempResult;
    private NotificarLogros NL;

    public string specieName;
    
    // Flag para evitar doble interacción
    private bool isInteracting = false;

    private void Awake()
    {
        //mouseController = GameObject.FindGameObjectWithTag("Player").GetComponent<MouseController>();
        //cameraBlocker = GameObject.FindGameObjectWithTag("Blocker").GetComponent<Collider>();
        
        //GaleryScript = Galeria.GetComponent<Galery>();
    }
    
    void Start()
    {
        puntero = GameObject.Find("Crosshair/Image");
        actionLogger = GameObject.Find("ActionLogger");
        tempResult = false;
        
        // Obtener referencias de forma segura
        if (ConstantObjects.instance != null)
        {
            cameraBlocker = ConstantObjects.instance.cameraBlocker;
            mouseController = ConstantObjects.instance.mouseController;
        }
        
        if (Panel != null)
        {
            Panel.SetActive(false);
        }
        
        isKnown = false;
        
        GameObject notifLogros = GameObject.Find("NotifLogros");
        if (notifLogros != null)
        {
            NL = notifLogros.GetComponent<NotificarLogros>();
        }
    }
    
    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (isInteracting) return;
        isInteracting = true;
        
        try
        {
            HandleInteraction();
        }
        finally
        {
            // Resetear flag después de un pequeño delay
            Invoke("ResetInteracting", 0.5f);
        }
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.puntero();
            }
        }
    }
    
    public void OnLookAway()
    {
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.mira();
            }
        }
    }
    
    // ============== LÓGICA DE INTERACCIÓN ==============
    
    private void HandleInteraction()
    {
        tempResult = false;
        Debug.Log("ClickMouse: Interacción iniciada con " + specieName);
        
        try
        {
            if (!isKnown)
            {
                if (isAnimal)
                {
                    if (!string.IsNullOrEmpty(specieName))
                    {
                        Debug.Log("ClickMouse: Procesando animal " + specieName);
                        if (logroSist != null)
                        {
                            LogrosGlobales logros = logroSist.GetComponent<LogrosGlobales>();
                            if (logros != null && logros.misiones[6].requisitos.Contains(specieName))
                            {
                                tempResult = logros.ProgresarLogro(6);
                                if (fpscontroller != null)
                                {
                                    Player player = fpscontroller.GetComponent<Player>();
                                    if (player != null)
                                    {
                                        player.gainEXP(1);
                                    }
                                }
                            }
                            logros.ProgresarMision(0, specieName);
                            logros.ProgresarMision(6, specieName);
                        }
                    }
                }
                else if (isPlant)
                {
                    if (!string.IsNullOrEmpty(specieName))
                    {
                        Debug.Log("ClickMouse: Procesando planta " + specieName);
                        if (logroSist != null)
                        {
                            LogrosGlobales logros = logroSist.GetComponent<LogrosGlobales>();
                            if (logros != null && logros.misiones[7].requisitos.Contains(specieName))
                            {
                                if (fpscontroller != null)
                                {
                                    Player player = fpscontroller.GetComponent<Player>();
                                    if (player != null)
                                    {
                                        player.gainEXP(1);
                                    }
                                }
                                tempResult = logros.ProgresarLogro(7);
                            }
                            logros.ProgresarMision(0, specieName);
                            logros.ProgresarMision(7, specieName);
                        }
                    }
                }
                isKnown = true;
            }

            if (actionLogger != null)
            {
                ActionLogger logger = actionLogger.GetComponent<ActionLogger>();
                if (logger != null && logger.actionLogger != null)
                {
                    logger.actionLogger.agregarAccion("Interact especie", specieName);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("ClickMouse: Error en HandleInteraction: " + e.Message);
        }
        
        if (!(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas || tempResult))
        {
            ShowGallery();
            if (cameraBlocker != null)
            {
                cameraBlocker.enabled = true;
            }
        }
    }
    
    // ============== COMPATIBILIDAD CON MOUSE (LEGACY) ==============
    
    private void OnMouseEnter()
    {
        OnLookAt();
    }
    
    private void OnMouseExit()
    {
        OnLookAway();
    }
    
    private void OnMouseDown()
    {
        // Solo usar si no hay InteractionDetector activo
        if (InteractionDetector.instance == null)
        {
            HandleInteraction();
        }
    }
    
    // ============== GALERÍA ==============
    
    public void ShowGallery()
    {
        Debug.Log("ClickMouse: Mostrando galería para " + specieName);
        
#if UNITY_ANDROID || UNITY_IOS
        if (canvasJoy != null)
        {
            canvasJoy.SetActive(false);
        }
#endif
        
        // Actualizar referencias si es necesario
        if (ConstantObjects.instance != null)
        {
            cameraBlocker = ConstantObjects.instance.cameraBlocker;
            mouseController = ConstantObjects.instance.mouseController;
        }
        
        MenuPausa.instance.Pausar();
        
        if (mouseController != null)
        {
            mouseController.enabled = false;
        }
        
        if (Panel != null)
        {
            Panel.SetActive(true);
        }
        
        if (Galeria != null)
        {
            Galeria.SetActive(true);
            
            if (GaleryScript == null)
            {
                GaleryScript = Galeria.GetComponent<Galery>();
            }
            
            if (GaleryScript != null)
            {
                GaleryScript.name = specieName;
                GaleryScript.visible = true;
            }
        }
        
        if (Panel3 != null)
        {
            Panel3.SetActive(false);
        }
        
        // Registrar especie en el libro
        registrarEspecieId();
        
        CerrarCuadroChallengeDos();
        IsGalery = true;
        
        if (NL != null)
        {
            NL.cerrar();
        }
    }

    private void registrarEspecieId()
    {
        Debug.Log("ClickMouse: Registrando especie " + specieName);
        
        if (gameObject.tag == "Bird")
        {
            string estacionPajaro = "1";
            try
            {
                Transform parent = gameObject.transform.parent?.parent?.parent?.parent;
                if (parent != null)
                {
                    Estacion estacion = parent.GetComponent<Estacion>();
                    if (estacion != null)
                    {
                        estacionPajaro = estacion.ID.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("ClickMouse: Error obteniendo estación del pájaro: " + e.Message);
            }
            
            if (BookPages.instance != null)
            {
                BookPages.instance.registrarEspecie(specieName, estacionPajaro);
            }
            return;
        }
        
        // Para otras especies
        string estacionId = "1";
        try
        {
            Transform parent = gameObject.transform.parent?.parent?.parent;
            if (parent != null)
            {
                Estacion estacion = parent.GetComponent<Estacion>();
                if (estacion != null)
                {
                    estacionId = estacion.ID.ToString();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("ClickMouse: Error obteniendo estación: " + e.Message);
        }
        
        if (BookPages.instance != null)
        {
            BookPages.instance.registrarEspecie(specieName, estacionId);
        }
        
        Debug.Log("ClickMouse: Especie registrada: " + specieName + " en estación " + estacionId);
    }


    public void Continuar()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (canvasJoy != null)
        {
            canvasJoy.SetActive(true);
        }
#endif
        
        if (ConstantObjects.instance != null)
        {
            cameraBlocker = ConstantObjects.instance.cameraBlocker;
            mouseController = ConstantObjects.instance.mouseController;
        }
        
        Time.timeScale = 1f;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            FirstPersonController fps = player.GetComponent<FirstPersonController>();
            if (fps != null)
            {
                fps.enabled = true;
            }
        }
        
        if (Galeria != null)
        {
            Galery galery = Galeria.GetComponent<Galery>();
            if (galery != null)
            {
                galery.visible = false;
            }
        }
        
        MenuPausa.instance.Reanudar();
        
        if (mouseController != null)
        {
            mouseController.enabled = true;
        }
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (Panel != null)
        {
            Panel.SetActive(false);
        }
        
        if (Panel3 != null)
        {
            Panel3.SetActive(true);
        }
        
        if (Galeria != null)
        {
            Galeria.SetActive(false);
        }
        
        IsGalery = false;
        
        if (cameraBlocker != null)
        {
            cameraBlocker.enabled = false;
        }
    }
    
    public void CerrarCuadroChallengeDos()
    {
        if (CuadroChallengeDos != null)
        {
            CuadroChallengeDos.SetActive(false);
        }
    }
}
