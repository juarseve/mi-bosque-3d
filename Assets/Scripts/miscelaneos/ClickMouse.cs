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
    public bool isDynamicSeedOnly = false; // Flag para árboles dinámicos que solo dan semillas sin marcar misiones
    public GameObject logroSist;
    public GameObject fpscontroller;
    public GameObject canvasJoy = null;
    public GameObject actionLogger;

    private GameObject puntero;

    private bool tempResult;
    private NotificarLogros NL;

    public string specieName;
    
    // Flag para evitar doble interacci�n
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
    
    // ============== IMPLEMENTACI�N DE IInteractable ==============
    
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
            // Resetear flag despu�s de un peque�o delay
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
    
    // ============== L�GICA DE INTERACCI�N ==============
    
    private void HandleInteraction()
    {
        tempResult = false;
        Debug.Log("ClickMouse: Interacción iniciada con " + specieName + " | isKnown=" + isKnown + ", isDynamicSeedOnly=" + isDynamicSeedOnly);
        
        try
        {
            // Si es un árbol dinámico solo para semillas (como Bototillo dinámico),
            // abrimos la galería pero NO marcamos misiones
            if (!isDynamicSeedOnly && !isKnown)
            {
                // MARCAR COMO CONOCIDO INMEDIATAMENTE para evitar doble procesamiento en clicks rápidos
                isKnown = true;
                Debug.Log("[ClickMouse] Marcando isKnown=true para " + specieName);
                
                if (isAnimal)
                {
                    if (!string.IsNullOrEmpty(specieName))
                    {
                        Debug.Log("ClickMouse: Procesando animal " + specieName);
                        Debug.Log("[DEBUG] logroSist is null? " + (logroSist == null));
                        if (logroSist != null)
                        {
                            LogrosGlobales logros = logroSist.GetComponent<LogrosGlobales>();
                            Debug.Log("[DEBUG] logros is null? " + (logros == null));
                            if (logros != null)
                            {
                                Debug.Log("[DEBUG] logros.misiones is null? " + (logros.misiones == null));
                                Debug.Log("[DEBUG] logros.misiones.Count = " + (logros.misiones != null ? logros.misiones.Count.ToString() : "null"));
                                if (logros.misiones != null && logros.misiones.Count > 6 && logros.misiones[6] != null && logros.misiones[6].requisitos != null && logros.misiones[6].requisitos.Contains(specieName))
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
                                Debug.Log("[ClickMouse] Animal: Llamando ProgresarMision(0, '" + specieName + "')");
                                logros.ProgresarMision(0, specieName);
                                Debug.Log("[ClickMouse] Animal: Llamando ProgresarMision(6, '" + specieName + "')");
                                logros.ProgresarMision(6, specieName);
                            }
                        }
                    }
                }
                else if (isPlant)
                {
                    if (!string.IsNullOrEmpty(specieName))
                    {
                        Debug.Log("ClickMouse: Procesando planta " + specieName);
                        Debug.Log("[DEBUG] logroSist is null? " + (logroSist == null));
                        if (logroSist != null)
                        {
                            LogrosGlobales logros = logroSist.GetComponent<LogrosGlobales>();
                            Debug.Log("[DEBUG] logros is null? " + (logros == null));
                            if (logros != null)
                            {
                                Debug.Log("[DEBUG] logros.misiones is null? " + (logros.misiones == null));
                                Debug.Log("[DEBUG] logros.misiones.Count = " + (logros.misiones != null ? logros.misiones.Count.ToString() : "null"));
                                if (logros.misiones != null && logros.misiones.Count > 7 && logros.misiones[7] != null && logros.misiones[7].requisitos != null && logros.misiones[7].requisitos.Contains(specieName))
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
                                Debug.Log("[ClickMouse] Planta: Llamando ProgresarMision(0, '" + specieName + "')");
                                logros.ProgresarMision(0, specieName);
                                Debug.Log("[ClickMouse] Planta: Llamando ProgresarMision(7, '" + specieName + "')");
                                logros.ProgresarMision(7, specieName);
                            }
                        }
                    }
                }
            }
            else if (isDynamicSeedOnly)
            {
                Debug.Log("ClickMouse: Árbol dinámico solo para semillas - abriendo galería sin marcar misiones");
            }
            else if (isKnown)
            {
                Debug.Log("ClickMouse: Especie ya conocida (" + specieName + ") - saltando procesamiento de misiones");
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
    
    // ============== GALER�A ==============
    
    public void ShowGallery()
    {
        Debug.Log("ClickMouse: Mostrando galer�a para " + specieName);
        
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
                // Asignar referencia directa a Panel3 para que Limpiar() pueda reactivarlo
                GaleryScript.panel3Ref = Panel3;
                // Asignar referencias a las cajas de objetivos (si están asignadas en esta especie)
                // Nota: Solo las especies objetivo (Ardilla, Iguana, Pechiche) tendrán CuadroChallengeDos asignado
                if (CuadroChallengeDos != null)
                {
                    // Determinar a qué especie pertenece esta caja y asignarla al Galery
                    // Esto permite que Limpiar() sepa qué cajas reactivar
                    AsignarCajasAGalery();
                }
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
                Debug.LogWarning("ClickMouse: Error obteniendo estaci�n del p�jaro: " + e.Message);
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
            Debug.LogWarning("ClickMouse: Error obteniendo estaci�n: " + e.Message);
        }
        
        if (BookPages.instance != null)
        {
            BookPages.instance.registrarEspecie(specieName, estacionId);
        }
        
        Debug.Log("ClickMouse: Especie registrada: " + specieName + " en estaci�n " + estacionId);
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
    
    /// <summary>
    /// Busca y asigna las referencias de las 3 cajas objetivo al script Galery
    /// para que pueda reactivarlas después de cerrar la galería
    /// </summary>
    private void AsignarCajasAGalery()
    {
        // Buscar las cajas de objetivos en la escena
        GameObject ardillaCaja = GameObject.Find("ArdillaCaja");
        GameObject iguanaCaja = GameObject.Find("IguanaCaja");
        GameObject pechicheCaja = GameObject.Find("PechicheCaja");
        
        if (GaleryScript != null)
        {
            if (ardillaCaja != null) GaleryScript.ardillaCajaRef = ardillaCaja;
            if (iguanaCaja != null) GaleryScript.iguanaCajaRef = iguanaCaja;
            if (pechicheCaja != null) GaleryScript.pechicheCajaRef = pechicheCaja;
            
            Debug.Log("[ClickMouse] Referencias de cajas asignadas a Galery");
        }
    }
}
