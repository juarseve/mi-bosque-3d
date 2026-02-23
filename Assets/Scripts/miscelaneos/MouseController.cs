using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// MouseController - Configura el sistema de interacción basado en Raycast y maneja el cursor.
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

    [Tooltip("Distancia máxima del raycast para detectar objetos")]
    public float maxRaycastDistance = 25f;

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

        // Configurar el detector con el nuevo sistema de Raycast
        interactionDetector.maxRaycastDistance = maxRaycastDistance;
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

        Debug.Log("[MouseController] InteractionDetector configurado - Modo: RAYCAST");
        Debug.Log("[MouseController] Distancia máxima: " + maxRaycastDistance);
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
