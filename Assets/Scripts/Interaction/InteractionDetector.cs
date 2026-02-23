using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detector de interacción basado en Raycast desde la cámara.
/// Usa Physics.Raycast para detectar el objeto que está directamente en el centro de la pantalla.
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    [Header("Configuración de Raycast")]
    [Tooltip("Distancia máxima del raycast para detectar objetos")]
    public float maxRaycastDistance = 25f;

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

    [Tooltip("Color del rayo cuando NO hay objeto interactuable")]
    public Color rayColorNoHit = Color.red;

    [Tooltip("Color del rayo cuando HAY objeto interactuable")]
    public Color rayColorHit = Color.green;

    // Objeto actualmente enfocado
    private IInteractable currentFocused;
    private GameObject currentFocusedObject;

    // Referencia al puntero para feedback visual
    private GameObject puntero;
    private Puntero punteroScript;

    // Singleton para acceso fácil
    public static InteractionDetector instance;

    // Info de debug
    private string lastFocusedName = "Ninguno";
    private RaycastHit lastHit;
    private bool lastHadHit = false;

    // Flag para controlar qué animación reproducir
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

        Debug.Log("[InteractionDetector] Iniciado correctamente - Modo: RAYCAST desde cámara");
        Debug.Log("[InteractionDetector] Distancia máxima: " + maxRaycastDistance);
        Debug.Log("[InteractionDetector] Layer mask: " + interactableLayer.value);
    }

    void Update()
    {
        // Verificar si el juego no está pausado
        if (MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas)
        {
            return;
        }

        // Detectar objeto interactuable con Raycast
        UpdateRaycastInteractable();

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
    /// Detecta objetos interactuables usando Raycast desde el centro de la pantalla
    /// </summary>
    void UpdateRaycastInteractable()
    {
        if (playerCamera == null)
        {
            Debug.LogError("[InteractionDetector] No hay cámara asignada!");
            return;
        }

        // Crear un rayo desde el centro de la pantalla
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        bool hasHit;

        // Hacer raycast con o sin layer mask
        if (interactableLayer.value == 0)
        {
            // Sin filtro de layer (todas las layers)
            hasHit = Physics.Raycast(ray, out hit, maxRaycastDistance);
        }
        else
        {
            // Con filtro de layer
            hasHit = Physics.Raycast(ray, out hit, maxRaycastDistance, interactableLayer);
        }

        // Guardar info para debug gizmos
        lastHadHit = hasHit;
        if (hasHit)
        {
            lastHit = hit;
        }

        IInteractable newFocused = null;
        GameObject newFocusedObject = null;

        if (hasHit)
        {
            // Verificar si tiene IInteractable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                newFocused = interactable;
                newFocusedObject = hit.collider.gameObject;

                if (showDebugLogs && newFocusedObject != currentFocusedObject)
                {
                    Debug.Log("[InteractionDetector] Raycast detectó objeto interactuable: " + newFocusedObject.name + " a " + hit.distance.ToString("F2") + " unidades");
                }
            }
            else
            {
                // Hit con objeto no interactuable
                if (showDebugLogs && currentFocusedObject != null)
                {
                    Debug.Log("[InteractionDetector] Raycast hit objeto SIN IInteractable: " + hit.collider.name);
                }
            }
        }

        // Actualizar el objeto enfocado
        SetFocused(newFocused, newFocusedObject);
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
    /// Verifica si hay algún objeto interactuable en el raycast
    /// </summary>
    public bool HasInteractableInRange()
    {
        return currentFocusedObject != null;
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
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        // Crear un rayo desde el centro de la pantalla
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Application.isPlaying && lastHadHit)
        {
            // Dibujar rayo en verde hasta el objeto hit
            Gizmos.color = rayColorHit;
            Gizmos.DrawLine(ray.origin, lastHit.point);

            // Dibujar esfera en el punto de impacto
            Gizmos.DrawWireSphere(lastHit.point, 0.2f);

            // Si es interactuable, dibujar esfera más grande
            if (currentFocusedObject != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentFocusedObject.transform.position, 0.5f);
                Gizmos.DrawLine(lastHit.point, currentFocusedObject.transform.position);
            }

            // Dibujar línea punteada hasta el final de la distancia máxima
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(lastHit.point, ray.origin + ray.direction * maxRaycastDistance);
        }
        else
        {
            // Dibujar rayo en rojo (no hay hit o no está en play mode)
            Gizmos.color = rayColorNoHit;
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * maxRaycastDistance);
        }

        // Dibujar esfera en el origen del rayo
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(ray.origin, 0.1f);

        // Dibujar indicador de distancia máxima
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(ray.origin + ray.direction * maxRaycastDistance, 0.3f);
    }
}
