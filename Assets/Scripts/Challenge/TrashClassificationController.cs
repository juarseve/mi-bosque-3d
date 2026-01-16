using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class TrashClassificationController : MonoBehaviour
{
    [Header("Referencias FPS")]
    public FirstPersonController firstPersonController;
    public Transform cameraPivot; // Transform que controla la posición de la cámara (para mover hacia atrás)
    
    [Header("Configuración de cámara")]
    public float cameraBackDistance = 5f; // Distancia hacia ADELANTE (positivo Z) para primera persona
    
    [Header("Estado")]
    private bool isClassifying = false;
    private Vector3 originalCameraPosition;
    private bool originalCanMove;
    private bool originalCanRotate;

    private void Start()
    {
        // Intentar encontrar el FirstPersonController
        if (firstPersonController == null)
        {
            // Primero intenta por FindObjectOfType
            firstPersonController = FindObjectOfType<FirstPersonController>();
            
            // Si no lo encuentra, intenta por tag "Player"
            if (firstPersonController == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    firstPersonController = playerObj.GetComponent<FirstPersonController>();
                }
            }
            
            if (firstPersonController != null)
            {
                Debug.Log("[TrashClassificationController] FirstPersonController encontrado: " + firstPersonController.gameObject.name);
            }
            else
            {
                Debug.LogError("[TrashClassificationController] ¡NO se encontró FirstPersonController!");
                return;
            }
        }
        
        // Intentar encontrar el cameraPivot
        if (cameraPivot == null)
        {
            if (firstPersonController != null)
            {
                cameraPivot = firstPersonController.cameraPivot;
                
                if (cameraPivot == null)
                {
                    Debug.LogError("[TrashClassificationController] cameraPivot no está asignado en FirstPersonController");
                }
                else
                {
                    Debug.Log("[TrashClassificationController] cameraPivot encontrado: " + cameraPivot.gameObject.name);
                }
            }
        }
    }

    /// <summary>
    /// Activa el modo de clasificación de desechos en primera persona
    /// </summary>
    public void StartTrashClassification()
    {
        if (isClassifying)
        {
            Debug.LogWarning("[TrashClassificationController] Ya está en modo clasificación");
            return;
        }

        if (firstPersonController == null)
        {
            Debug.LogError("[TrashClassificationController] FirstPersonController no está asignado");
            return;
        }

        isClassifying = true;

        // Guardar estado original
        originalCanMove = firstPersonController.canMove;
        originalCanRotate = firstPersonController.canRotate;
        
        // Guardar posición original de la cámara si existe cameraPivot
        if (cameraPivot != null)
        {
            originalCameraPosition = cameraPivot.localPosition;
            Debug.Log($"[TrashClassificationController] Posición original de cámara guardada: {originalCameraPosition}");
        }

        // NO bloquear movimiento - permitir WASD
        // Solo cambiar la perspectiva de cámara
        firstPersonController.canMove = true;  // Permitir movimiento
        firstPersonController.canRotate = true; // Permitir rotación
        
        // Mover la cámara hacia ADELANTE para simular primera persona
        if (cameraPivot != null)
        {
            Vector3 newCameraPos = cameraPivot.localPosition;
            newCameraPos.z += cameraBackDistance; // Mover hacia ADELANTE (positivo Z)
            cameraPivot.localPosition = newCameraPos;
            Debug.Log($"[TrashClassificationController] Cámara movida hacia ADELANTE. Nueva posición: {newCameraPos}");
        }
        else
        {
            Debug.LogWarning("[TrashClassificationController] cameraPivot es null, no se puede mover la cámara");
        }

        Debug.Log("[TrashClassificationController] ✓ Modo clasificación ACTIVADO");
    }

    /// <summary>
    /// Desactiva el modo de clasificación y restaura el movimiento
    /// </summary>
    public void EndTrashClassification()
    {
        if (!isClassifying)
        {
            Debug.LogWarning("[TrashClassificationController] No está en modo clasificación");
            return;
        }

        if (firstPersonController == null)
        {
            Debug.LogError("[TrashClassificationController] FirstPersonController no está asignado");
            return;
        }

        isClassifying = false;

        // Restaurar movimiento
        firstPersonController.canMove = originalCanMove;
        firstPersonController.canRotate = originalCanRotate;
        
        // Restaurar posición de la cámara
        if (cameraPivot != null)
        {
            cameraPivot.localPosition = originalCameraPosition;
            Debug.Log($"[TrashClassificationController] Cámara restaurada a posición original: {originalCameraPosition}");
        }

        Debug.Log("[TrashClassificationController] ✓ Modo clasificación DESACTIVADO - Volviendo a tercera persona");
    }

    /// <summary>
    /// Retorna el estado actual de clasificación
    /// </summary>
    public bool IsClassifying => isClassifying;
}

