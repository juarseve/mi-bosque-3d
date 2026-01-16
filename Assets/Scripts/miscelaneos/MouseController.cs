using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// MouseController - Configura el sistema de interacción y maneja el cursor.
/// </summary>
public class MouseController : MonoBehaviour
{
    private Rect screenRect;

    public FirstPersonController fpsController;
    public Camera fpsCamera;
    
    [Header("Configuración del Detector de Interacción")]
    [Tooltip("Referencia al InteractionDetector (se crea automáticamente si no existe)")]
    public InteractionDetector interactionDetector;
    
    [Tooltip("El Transform donde se colocará el detector (Boy 2)")]
    public Transform detectorParent;
    
    [Tooltip("Radio de detección de objetos interactuables")]
    public float detectionRadius = 25f;
    
    [Tooltip("Ángulo máximo de visión para interactuar")]
    [Range(0f, 180f)]
    public float maxInteractionAngle = 90f;
    
    [Tooltip("LayerMask para detectar objetos interactuables (dejar en 0 para todas las layers)")]
    public LayerMask layerToHit;
    
    [Header("Animación de Interacción Normal")]
    [Tooltip("AnimationClip de interacción normal (asignar 'buttong pushing')")]
    public AnimationClip interactionAnimation;
    
    [Tooltip("Nombre del trigger en el Animator para la animación de interacción")]
    public string interactionAnimTrigger = "Interact";
    
    [Header("Animación de Pickup (Recoger)")]
    [Tooltip("AnimationClip de pickup (asignar animación de recoger del suelo)")]
    public AnimationClip pickupAnimation;
    
    [Tooltip("Nombre del trigger en el Animator para la animación de pickup")]
    public string pickupAnimTrigger = "Pickup";
    
    [Header("Debug")]
    public bool showDebugInfo = true;

    void Start()
    {
        Debug.Log("[MouseController] Iniciando...");
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        screenRect = new Rect(0, 0, Screen.width, Screen.height);

        SetupInteractionDetector();
    }
    
    void SetupInteractionDetector()
    {
        if (detectorParent == null)
        {
            if (fpsController != null)
            {
                Transform player = fpsController.transform;
                detectorParent = FindChildRecursive(player, "Boy 2");
                
                if (detectorParent == null)
                {
                    foreach (Transform child in player)
                    {
                        if (child.GetComponent<Animator>() != null)
                        {
                            detectorParent = child;
                            break;
                        }
                    }
                    
                    if (detectorParent == null)
                    {
                        detectorParent = player;
                    }
                }
            }
            else
            {
                Debug.LogError("[MouseController] fpsController no asignado!");
                return;
            }
        }
        
        interactionDetector = detectorParent.GetComponent<InteractionDetector>();
        
        if (interactionDetector == null)
        {
            interactionDetector = detectorParent.gameObject.AddComponent<InteractionDetector>();
            Debug.Log("[MouseController] Se creó InteractionDetector en: " + detectorParent.name);
        }
        
        // Configurar el detector
        interactionDetector.detectionRadius = detectionRadius;
        interactionDetector.maxAngle = maxInteractionAngle;
        interactionDetector.playerCamera = fpsCamera;
        interactionDetector.interactableLayer = layerToHit;
        interactionDetector.showDebugLogs = showDebugInfo;
        interactionDetector.alwaysShowGizmos = true;
        
        // Configurar animaciones
        if (interactionAnimation != null)
        {
            interactionDetector.interactionAnimation = interactionAnimation;
        }
        interactionDetector.interactionAnimTrigger = interactionAnimTrigger;
        
        if (pickupAnimation != null)
        {
            interactionDetector.pickupAnimation = pickupAnimation;
        }
        interactionDetector.pickupAnimTrigger = pickupAnimTrigger;
        
        // Buscar y asignar el Animator
        Animator playerAnimator = detectorParent.GetComponent<Animator>();
        if (playerAnimator == null)
        {
            playerAnimator = detectorParent.GetComponentInChildren<Animator>();
        }
        if (playerAnimator != null)
        {
            interactionDetector.playerAnimator = playerAnimator;
        }
        
        Debug.Log("[MouseController] InteractionDetector configurado - Radio: " + detectionRadius);
    }
    
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            Transform found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    void Update()
    {
        if (!(MenuPausa.IsPaused))
        {
            if (screenRect.Contains(Input.mousePosition))
            {
                fpsController.enabled = true;
            }
            else
            {
                fpsController.enabled = false;
            }
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        
        GUIStyle headerStyle = new GUIStyle();
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.yellow;
        
        float y = 10;
        float lineHeight = 20;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), "=== SISTEMA DE INTERACCIÓN ===", headerStyle);
        y += lineHeight + 5;
        
        if (interactionDetector != null)
        {
            GUI.Label(new Rect(10, y, 400, lineHeight), "Radio: " + interactionDetector.detectionRadius, style);
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 400, lineHeight), "En rango: " + (interactionDetector.HasInteractableInRange() ? "SÍ" : "NO"), style);
            y += lineHeight;
            
            GameObject focused = interactionDetector.GetFocusedObject();
            string focusedName = focused != null ? focused.name : "Ninguno";
            GUI.Label(new Rect(10, y, 400, lineHeight), "Enfocado: " + focusedName, style);
            y += lineHeight + 10;
            
            // Estado del desafío del conejo
            GUI.Label(new Rect(10, y, 400, lineHeight), "=== DESAFÍO DEL CONEJO ===", headerStyle);
            y += lineHeight + 5;
            
            GUI.Label(new Rect(10, y, 400, lineHeight), "Desafío activo: " + (Squirrel.activate ? "SÍ" : "NO"), style);
            y += lineHeight;
            
            string caughtColor = Squirrel.caught ? "<color=green>SÍ</color>" : "<color=red>NO</color>";
            GUI.Label(new Rect(10, y, 400, lineHeight), "Conejo capturado: " + (Squirrel.caught ? "SÍ" : "NO"), style);
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 400, lineHeight), "Misión completada: " + (Nest.home ? "SÍ" : "NO"), style);
            y += lineHeight + 10;
            
            GUI.Label(new Rect(10, y, 400, lineHeight), "Presiona E para interactuar", style);
        }
        else
        {
            GUI.Label(new Rect(10, y, 400, lineHeight), "ERROR: InteractionDetector no configurado", style);
        }
    }

    void OnApplicationFocus(bool ApplicationIsBack)
    {
        if (ApplicationIsBack == true)
        {
            bool hasActiveUI = false;
            
            if (MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas)
            {
                hasActiveUI = true;
            }
            
            if (DialogueManager.instance != null && DialogueManager.instance.isDialogueActive)
            {
                hasActiveUI = true;
            }
            
            if (BookPages.instance != null && BookPages.instance.isOpen)
            {
                hasActiveUI = true;
            }
            
            if (!hasActiveUI)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
    
    private bool HasActiveUIChildren(Transform parent)
    {
        if (parent.gameObject.activeSelf && 
            (parent.GetComponent<Button>() != null || 
             parent.GetComponent<Image>() != null || 
             parent.GetComponent<Text>() != null ||
             parent.GetComponent<TMPro.TextMeshProUGUI>() != null))
        {
            return true;
        }
        
        foreach (Transform child in parent)
        {
            if (HasActiveUIChildren(child))
            {
                return true;
            }
        }
        
        return false;
    }
}