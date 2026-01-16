using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detector de proximidad para objetos interactuables.
/// Usa Physics.OverlapSphere para detectar objetos cercanos (más confiable que triggers).
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Radio de detección de objetos interactuables")]
    public float detectionRadius = 25f;
    
    [Tooltip("Ángulo máximo desde la dirección de la cámara para considerar un objeto 'enfrentado'")]
    [Range(0f, 180f)]
    public float maxAngle = 90f;
    
    [Tooltip("Referencia a la cámara del jugador")]
    public Camera playerCamera;
    
    [Tooltip("LayerMask para filtrar objetos interactuables")]
    public LayerMask interactableLayer;
    
    [Header("Animación de Interacción")]
    [Tooltip("Animator del personaje para reproducir animación al interactuar")]
    public Animator playerAnimator;
    
    [Tooltip("Nombre del trigger de animación de interacción normal (ej. presionar botón)")]
    public string interactionAnimTrigger = "Interact";
    
    [Tooltip("Nombre del trigger de animación de recoger/pickup (ej. recoger conejo)")]
    public string pickupAnimTrigger = "Pickup";
    
    [Tooltip("AnimationClip de interacción normal (opcional - asignar 'buttong pushing')")]
    public AnimationClip interactionAnimation;
    
    [Tooltip("AnimationClip de pickup (opcional - asignar animación de recoger del suelo)")]
    public AnimationClip pickupAnimation;
    
    [Header("Debug")]
    [Tooltip("Mostrar información de debug en consola")]
    public bool showDebugLogs = true;
    
    [Tooltip("Mostrar Gizmos siempre (no solo cuando está seleccionado)")]
    public bool alwaysShowGizmos = true;
    
    // Lista de objetos interactuables detectados este frame
    private List<GameObject> detectedObjects = new List<GameObject>();
    
    // Objeto actualmente enfocado
    private IInteractable currentFocused;
    private GameObject currentFocusedObject;
    
    // Referencia al puntero para feedback visual
    private GameObject puntero;
    private Puntero punteroScript;
    
    // Singleton para acceso fácil
    public static InteractionDetector instance;
    
    // Info de debug
    private int lastDetectedCount = 0;
    private string lastFocusedName = "Ninguno";
    
    // Flag para controlar qué animación reproducir
    // true = ya se reprodujo una animación personalizada, no reproducir la default
    private bool animationHandled = false;

    void Awake()
    {
        instance = this;
        Debug.Log("[InteractionDetector] Awake - Inicializando en: " + gameObject.name);
    }
    
    void Start()
    {
        // Buscar la cámara si no está asignada
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Debug.Log("[InteractionDetector] Cámara encontrada: " + playerCamera.name);
            }
            else
            {
                Debug.LogError("[InteractionDetector] ERROR: No se encontró cámara principal!");
            }
        }
        
        // Buscar el Animator del personaje si no está asignado
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
            if (playerAnimator == null)
            {
                playerAnimator = GetComponentInChildren<Animator>();
            }
            if (playerAnimator == null)
            {
                // Buscar en el padre (FPSController)
                Transform parent = transform.parent;
                while (parent != null && playerAnimator == null)
                {
                    playerAnimator = parent.GetComponentInChildren<Animator>();
                    parent = parent.parent;
                }
            }
            
            if (playerAnimator != null)
            {
                Debug.Log("[InteractionDetector] Animator encontrado: " + playerAnimator.gameObject.name);
            }
            else
            {
                Debug.LogWarning("[InteractionDetector] No se encontró Animator del personaje");
            }
        }
        
        // Buscar el puntero
        puntero = GameObject.Find("Crosshair/Image");
        if (puntero != null)
        {
            punteroScript = puntero.GetComponent<Puntero>();
            Debug.Log("[InteractionDetector] Puntero encontrado");
        }
        else
        {
            Debug.LogWarning("[InteractionDetector] No se encontró el puntero (Crosshair/Image)");
        }
        
        Debug.Log("[InteractionDetector] Iniciado correctamente. Radio: " + detectionRadius + ", Ángulo: " + maxAngle);
        Debug.Log("[InteractionDetector] Layer mask: " + interactableLayer.value);
    }
    
    void Update()
    {
        // Verificar si el juego no está pausado
        if (MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas)
        {
            return;
        }
        
        // Detectar objetos interactuables cercanos usando OverlapSphere
        DetectNearbyInteractables();
        
        // Encontrar el mejor objeto interactuable
        UpdateFocusedInteractable();
        
        // Detectar input de interacción
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (showDebugLogs) Debug.Log("[InteractionDetector] Tecla E presionada");
            TryInteract();
        }
        
        // Mantener compatibilidad con clic del mouse
        if (Input.GetButtonDown("Fire1"))
        {
            if (showDebugLogs) Debug.Log("[InteractionDetector] Clic izquierdo detectado");
            TryInteract();
        }
    }
    
    /// <summary>
    /// Detecta objetos interactuables cercanos usando Physics.OverlapSphere
    /// </summary>
    void DetectNearbyInteractables()
    {
        detectedObjects.Clear();
        
        // Usar OverlapSphere para detectar todos los colliders en el radio
        // Si layerMask es 0 o -1, buscar en todas las layers
        Collider[] colliders;
        if (interactableLayer.value == 0)
        {
            colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        }
        else
        {
            colliders = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);
        }
        
        foreach (Collider col in colliders)
        {
            // Verificar si tiene IInteractable
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                detectedObjects.Add(col.gameObject);
            }
        }
        
        // Debug log si cambió la cantidad
        if (showDebugLogs && detectedObjects.Count != lastDetectedCount)
        {
            Debug.Log("[InteractionDetector] Objetos detectados: " + detectedObjects.Count);
            foreach (var obj in detectedObjects)
            {
                Debug.Log("  - " + obj.name);
            }
            lastDetectedCount = detectedObjects.Count;
        }
    }
    
    /// <summary>
    /// Actualiza cuál es el objeto interactuable enfocado actualmente
    /// </summary>
    void UpdateFocusedInteractable()
    {
        if (detectedObjects.Count == 0)
        {
            SetFocused(null, null);
            return;
        }
        
        if (playerCamera == null)
        {
            Debug.LogError("[InteractionDetector] No hay cámara asignada!");
            return;
        }
        
        IInteractable bestInteractable = null;
        GameObject bestObject = null;
        float bestScore = float.MinValue;
        
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraPosition = playerCamera.transform.position;
        
        foreach (GameObject obj in detectedObjects)
        {
            if (obj == null) continue;
            
            Vector3 directionToObject = (obj.transform.position - cameraPosition).normalized;
            
            // Calcular el ángulo entre la dirección de la cámara y la dirección al objeto
            float angle = Vector3.Angle(cameraForward, directionToObject);
            
            // Solo considerar objetos dentro del ángulo máximo
            if (angle <= maxAngle)
            {
                // Calcular distancia desde el detector (personaje)
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                
                // Calcular score: priorizar objetos más alineados con la cámara y más cercanos
                float angleScore = 1f - (angle / maxAngle);
                float distanceScore = 1f - (distance / detectionRadius);
                
                // Combinar scores (dar más peso al ángulo)
                float totalScore = (angleScore * 0.7f) + (distanceScore * 0.3f);
                
                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestInteractable = obj.GetComponent<IInteractable>();
                    bestObject = obj;
                }
            }
        }
        
        SetFocused(bestInteractable, bestObject);
    }
    
    /// <summary>
    /// Establece el objeto enfocado actual y maneja los callbacks visuales
    /// </summary>
    void SetFocused(IInteractable newFocused, GameObject newFocusedObject)
    {
        // Si es el mismo objeto, no hacer nada
        if (currentFocusedObject == newFocusedObject) return;
        
        string newName = newFocusedObject != null ? newFocusedObject.name : "Ninguno";
        
        if (showDebugLogs && newName != lastFocusedName)
        {
            Debug.Log("[InteractionDetector] Cambio de foco: " + lastFocusedName + " -> " + newName);
            lastFocusedName = newName;
        }
        
        // Notificar al anterior que dejamos de mirarlo
        if (currentFocused != null)
        {
            try
            {
                currentFocused.OnLookAway();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[InteractionDetector] Error en OnLookAway: " + e.Message);
            }
        }
        
        // Actualizar referencias
        currentFocused = newFocused;
        currentFocusedObject = newFocusedObject;
        
        // Notificar al nuevo que lo estamos mirando
        if (currentFocused != null)
        {
            try
            {
                currentFocused.OnLookAt();
                if (showDebugLogs) Debug.Log("[InteractionDetector] Llamando OnLookAt en: " + currentFocusedObject.name);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[InteractionDetector] Error en OnLookAt: " + e.Message);
            }
        }
        else
        {
            // Si no hay objeto enfocado, restaurar el puntero
            RestaurarPuntero();
        }
    }
    
    /// <summary>
    /// Restaura el puntero al estado normal
    /// </summary>
    void RestaurarPuntero()
    {
        if (punteroScript != null)
        {
            punteroScript.mira();
        }
        else if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.mira();
            }
        }
    }
    
    /// <summary>
    /// Reproduce la animación de interacción normal (presionar botón, etc.)
    /// Solo se reproduce si no se ha manejado otra animación
    /// </summary>
    public void PlayInteractionAnimation()
    {
        // Marcar que se manejó la animación
        animationHandled = true;
        
        if (playerAnimator == null) return;
        
        if (!string.IsNullOrEmpty(interactionAnimTrigger))
        {
            try
            {
                playerAnimator.SetTrigger(interactionAnimTrigger);
                Debug.Log("[InteractionDetector] Reproduciendo animación: " + interactionAnimTrigger);
            }
            catch (System.Exception e) { Debug.LogWarning("[InteractionDetector] Error: " + e.Message); }
        }
        
        if (interactionAnimation != null)
        {
            try
            {
                playerAnimator.Play(interactionAnimation.name);
            }
            catch (System.Exception e) { Debug.LogWarning("[InteractionDetector] Error: " + e.Message); }
        }
    }
    
    /// <summary>
    /// Reproduce la animación de pickup (recoger algo del suelo)
    /// Marca que la animación fue manejada para evitar reproducir la animación por defecto
    /// </summary>
    public void PlayPickupAnimation()
    {
        // Marcar que ya se manejó la animación (evita que se reproduzca la default después)
        animationHandled = true;
        
        if (playerAnimator == null)
        {
            Debug.LogWarning("[InteractionDetector] No hay Animator para pickup");
            return;
        }
        
        if (!string.IsNullOrEmpty(pickupAnimTrigger))
        {
            try
            {
                playerAnimator.SetTrigger(pickupAnimTrigger);
                Debug.Log("[InteractionDetector] Reproduciendo animación pickup: " + pickupAnimTrigger);
            }
            catch (System.Exception e) { Debug.LogWarning("[InteractionDetector] Error: " + e.Message); }
        }
        
        if (pickupAnimation != null)
        {
            try
            {
                playerAnimator.Play(pickupAnimation.name);
            }
            catch (System.Exception e) { Debug.LogWarning("[InteractionDetector] Error: " + e.Message); }
        }
    }
    
    /// <summary>
    /// Intenta interactuar con el objeto enfocado
    /// </summary>
    public void TryInteract()
    {
        // Resetear flag de animación al inicio de cada interacción
        animationHandled = false;
        
        if (currentFocused != null && currentFocusedObject != null)
        {
            Debug.Log("[InteractionDetector] ¡INTERACTUANDO con " + currentFocusedObject.name + "!");
            
            try
            {
                // Llamar al objeto para que maneje su lógica
                // Si el objeto necesita una animación específica, llamará a PlayPickupAnimation() o PlayInteractionAnimation()
                currentFocused.OnInteract();
                
                // Solo reproducir animación por defecto si el objeto NO manejó ninguna animación
                if (!animationHandled)
                {
                    Debug.Log("[InteractionDetector] Objeto no manejó animación, reproduciendo default");
                    PlayInteractionAnimation();
                }
                else
                {
                    Debug.Log("[InteractionDetector] Objeto manejó su propia animación");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[InteractionDetector] Error al interactuar: " + e.Message);
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log("[InteractionDetector] No hay objeto enfocado para interactuar");
            }
        }
    }
    
    /// <summary>
    /// Obtiene el objeto actualmente enfocado (para uso externo)
    /// </summary>
    public GameObject GetFocusedObject()
    {
        return currentFocusedObject;
    }
    
    /// <summary>
    /// Verifica si hay algún objeto interactuable en rango
    /// </summary>
    public bool HasInteractableInRange()
    {
        return detectedObjects.Count > 0;
    }
    
    /// <summary>
    /// Dibuja gizmos siempre para debug
    /// </summary>
    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos) return;
        DrawDebugGizmos();
    }
    
    /// <summary>
    /// Dibuja gizmos cuando está seleccionado
    /// </summary>
    void OnDrawGizmosSelected()
    {
        DrawDebugGizmos();
    }
    
    /// <summary>
    /// Dibuja los gizmos de debug
    /// </summary>
    void DrawDebugGizmos()
    {
        // Dibujar el radio de detección
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Dibujar esfera sólida más pequeña para ver el centro
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        // Dibujar el cono de visión si hay cámara
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 forward = cam.transform.forward * detectionRadius;
            
            // Línea central
            Gizmos.DrawRay(transform.position, forward);
            
            // Líneas del cono (horizontal)
            Quaternion leftRot = Quaternion.AngleAxis(-maxAngle, Vector3.up);
            Quaternion rightRot = Quaternion.AngleAxis(maxAngle, Vector3.up);
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawRay(transform.position, leftRot * forward);
            Gizmos.DrawRay(transform.position, rightRot * forward);
            
            // Líneas del cono (vertical)
            Quaternion upRot = Quaternion.AngleAxis(-maxAngle, cam.transform.right);
            Quaternion downRot = Quaternion.AngleAxis(maxAngle, cam.transform.right);
            Gizmos.DrawRay(transform.position, upRot * forward);
            Gizmos.DrawRay(transform.position, downRot * forward);
        }
        
        // Dibujar líneas a objetos detectados
        if (Application.isPlaying)
        {
            // Líneas rojas a objetos en rango pero no enfocados
            Gizmos.color = Color.red;
            foreach (var obj in detectedObjects)
            {
                if (obj != null && obj != currentFocusedObject)
                {
                    Gizmos.DrawLine(transform.position, obj.transform.position);
                    Gizmos.DrawWireSphere(obj.transform.position, 0.3f);
                }
            }
            
            // Línea verde al objeto enfocado
            if (currentFocusedObject != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentFocusedObject.transform.position);
                Gizmos.DrawWireSphere(currentFocusedObject.transform.position, 0.5f);
            }
        }
    }
}
