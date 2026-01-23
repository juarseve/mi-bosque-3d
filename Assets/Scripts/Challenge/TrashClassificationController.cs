using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class TrashClassificationController : MonoBehaviour
{
    [Header("Referencias FPS")]
    public FirstPersonController firstPersonController;
    public Transform cameraPivot; // Transform que controla la posición de la cámara
    public Transform playerBody; // Body del jugador para posicionamiento
    
    [Header("Configuración de cámara")]
    public Vector3 posicionCameraRelativoTachos = new Vector3(5f, 3f, -8f); // Posición de la cámara relativa a los tachos
    public Vector3 cameraFixedRotation = new Vector3(20f, -30f, 0f); // Rotación: mirando hacia los tachos
    public float cameraMoveSpeed = 2f; // Velocidad de animación de la cámara
    
    [Header("Configuración de posicionamiento")]
    public Vector3 posicionJugadorRelativoTachos = new Vector3(-3f, 0f, 0f); // Posición del jugador cerca del letrero
    public GameObject trashBinsParent; // Parent de los tachos de basura
    
    [Header("Referencias internas")]
    private TrashZoneTrigger trashZoneTrigger; // Para resetear estado de zona
    
    [Header("Estado")]
    private bool isClassifying = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Vector3 originalPlayerPosition;
    private bool originalCanMove;
    private bool originalCanRotate;
    private bool originalCanJump = true; // Guardar estado original del salto
    
    // Control de zona cercana
    private bool isNearTrashZone = false;

    private void Start()
    {
        // Intentar encontrar el FirstPersonController
        if (firstPersonController == null)
        {
            firstPersonController = FindObjectOfType<FirstPersonController>();
            
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
            }
        }
        
        // Intentar encontrar el body del jugador
        if (playerBody == null && firstPersonController != null)
        {
            playerBody = firstPersonController.transform;
        }
        
        // Intentar encontrar los tachos
        if (trashBinsParent == null)
        {
            Debug.Log("[TrashClassificationController] Buscando tachos automáticamente...");
            
            // Opción 1: Buscar por tag
            GameObject[] tachos = GameObject.FindGameObjectsWithTag("TrashBins");
            if (tachos.Length > 0)
            {
                trashBinsParent = tachos[0].transform.parent?.gameObject ?? tachos[0];
                Debug.Log($"[TrashClassificationController] ✓ Tachos encontrados por tag: {trashBinsParent.name}");
            }
            else
            {
                // Opción 2: Buscar por nombre
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.Contains("botes") ||
                        obj.name.Contains("Tacho") ||
                        obj.name.Contains("EstacionBasura") || 
                        obj.name.Contains("TrashZone") || 
                        obj.name.Contains("Basura") ||
                        obj.name.Contains("Trash") ||
                        obj.name.Contains("trash"))
                    {
                        trashBinsParent = obj;
                        Debug.Log($"[TrashClassificationController] ✓ Tachos encontrados por nombre: {trashBinsParent.name}");
                        break;
                    }
                }
            }
            
            if (trashBinsParent == null)
            {
                Debug.LogError("[TrashClassificationController] ✗ NO se encontraron los tachos automáticamente");
                Debug.LogError("[TrashClassificationController] SOLUCIÓN: Asigna manualmente 'Trash Bins Parent' en el Inspector");
            }
        }
        else
        {
            Debug.Log($"[TrashClassificationController] ✓ Trash Bins Parent ya asignado: {trashBinsParent.name}");
        }
        
        // Cargar referencia a TrashZoneTrigger
        if (trashZoneTrigger == null)
        {
            trashZoneTrigger = FindObjectOfType<TrashZoneTrigger>();
            if (trashZoneTrigger != null)
            {
                Debug.Log("[TrashClassificationController] ✓ TrashZoneTrigger encontrado");
            }
        }
    }

    /// <summary>
    /// Establece que el jugador está cerca de los tachos
    /// </summary>
    public void SetNearTrashZone(bool isNear)
    {
        isNearTrashZone = isNear;
        Debug.Log($"[TrashClassificationController] SetNearTrashZone: {isNear}");
    }


    /// <summary>
    /// Intenta activar el modo de clasificación si se cumplen todas las condiciones
    /// Condiciones: todos los desechos recogidos + jugador cerca de tachos + mochila abierta
    /// </summary>
    public bool TryStartTrashClassification(bool allTrashCollected)
    {
        Debug.Log("\n========== TryStartTrashClassification DEBUG ==========");
        Debug.Log($"  - isClassifying: {isClassifying}");
        Debug.Log($"  - firstPersonController: {(firstPersonController != null ? "✓" : "✗")}");
        Debug.Log($"  - allTrashCollected: {allTrashCollected}");
        Debug.Log($"  - isNearTrashZone: {isNearTrashZone}");
        Debug.Log("====================================================\n");
        
        if (isClassifying)
        {
            Debug.LogWarning("[TrashClassificationController] Ya está en modo clasificación");
            return false;
        }

        if (firstPersonController == null)
        {
            Debug.LogError("[TrashClassificationController] FirstPersonController no está asignado");
            return false;
        }

        // Verificar condiciones
        // DESHABILITADO TEMPORALMENTE PARA TESTING
        /*
        if (!allTrashCollected)
        {
            Debug.LogWarning("[TrashClassificationController] ✗ No se puede activar - No todos los desechos han sido recogidos");
            return false;
        }
        */

        if (!isNearTrashZone)
        {
            Debug.LogWarning("[TrashClassificationController] ✗ No se puede activar - El jugador no está cerca de los tachos");
            Debug.LogWarning("[TrashClassificationController] isNearTrashZone = false - SetNearTrashZone(true) NO fue llamado");
            return false;
        }

        // Todas las condiciones se cumplen
        StartTrashClassification();
        return true;
    }

    /// <summary>
    /// Activa el modo de clasificación de desechos
    /// </summary>
    public void StartTrashClassification()
    {
        Debug.Log("\n========== INICIANDO MODO CLASIFICACIÓN ==========");
        
        if (isClassifying)
        {
            Debug.LogWarning("[TrashClassificationController] Ya está en modo clasificación");
            return;
        }

        if (firstPersonController == null)
        {
            Debug.LogError("[TrashClassificationController] ✗ FirstPersonController no está asignado");
            return;
        }

        isClassifying = true;

        // Guardar estado original
        originalCanMove = firstPersonController.canMove;
        originalCanRotate = firstPersonController.canRotate;
        originalPlayerPosition = firstPersonController.transform.position;
        
        // Deshabilitar movimiento y salto (SPACE)
        firstPersonController.canMove = false;
        firstPersonController.canRotate = false;
        
        // Intentar deshabilitar salto en RigidbodyFirstPersonController si existe
        var rigidbodyFPS = firstPersonController.GetComponent<RigidbodyFirstPersonController>();
        if (rigidbodyFPS != null)
        {
            originalCanJump = rigidbodyFPS.enabled;
            rigidbodyFPS.enabled = false;
            Debug.Log("[TrashClassificationController] ✓ RigidbodyFirstPersonController deshabilitado");
        }
        
        Debug.Log("[TrashClassificationController] ✓ Movimiento y salto DESHABILITADOS");
        
        // IMPORTANTE: Mantener isNearTrashZone=true mientras se clasifica
        // para que el drag&drop siga funcionando aunque el jugador se aleje
        isNearTrashZone = true;
        Debug.Log("[TrashClassificationController] ✓ isNearTrashZone BLOQUEADO EN TRUE mientras clasifica");
        
        if (cameraPivot != null)
        {
            originalCameraPosition = cameraPivot.localPosition;
            originalCameraRotation = cameraPivot.localRotation;
            Debug.Log("[TrashClassificationController] ✓ Guardada posición de cámara");
        }
        else
        {
            Debug.LogError("[TrashClassificationController] ✗ cameraPivot es null");
        }

        // BLOQUEAR MOVIMIENTO completamente
        firstPersonController.canMove = false;
        firstPersonController.canRotate = false;
        Debug.Log("[TrashClassificationController] ✓ Movimiento bloqueado");

        // Teleportar al jugador a la izquierda de los tachos
        if (trashBinsParent != null)
        {
            Vector3 targetPos = trashBinsParent.transform.position + posicionJugadorRelativoTachos;
            firstPersonController.transform.position = targetPos;
            Debug.Log($"[TrashClassificationController] ✓ Jugador teleportado a: {targetPos}");
            Debug.Log($"  - Posición tachos: {trashBinsParent.transform.position}");
            Debug.Log($"  - Offset: {posicionJugadorRelativoTachos}");
        }
        else
        {
            Debug.LogWarning("[TrashClassificationController] ⚠️ trashBinsParent es null - no se teleporta el jugador");
            Debug.LogWarning("[TrashClassificationController] ⚠️ Buscar manualmente el objeto de tachos y asignarlo en el Inspector");
        }

        // Configurar cámara en posición fija mirando hacia atrás-derecha
        if (cameraPivot != null && trashBinsParent != null)
        {
            // Animar la cámara en lugar de teleportarla instantáneamente
            Vector3 targetCameraPos = trashBinsParent.transform.position + posicionCameraRelativoTachos;
            Quaternion targetCameraRot = Quaternion.Euler(cameraFixedRotation);
            
            // Iniciar corrutina para animar la cámara
            StartCoroutine(AnimateCameraToPosition(targetCameraPos, targetCameraRot));
            
            Debug.Log($"[TrashClassificationController] ✓ Animación de cámara iniciada");
            Debug.Log($"  - Posición tachos: {trashBinsParent.transform.position}");
            Debug.Log($"  - Offset cámara: {posicionCameraRelativoTachos}");
            Debug.Log($"  - Posición objetivo: {targetCameraPos}");
            Debug.Log($"  - Rotación (Euler): {cameraFixedRotation}");
        }
        else
        {
            if (cameraPivot == null)
                Debug.LogError("[TrashClassificationController] ✗ cameraPivot es null - No se puede mover la cámara");
            if (trashBinsParent == null)
                Debug.LogError("[TrashClassificationController] ✗ trashBinsParent es null - No se puede posicionar cámara relativa");
        }

        Debug.Log("[TrashClassificationController] ✓✓✓ Modo clasificación ACTIVADO ✓✓✓");
        Debug.Log("====================================================\n");
    }

    /// <summary>
    /// Desactiva el modo de clasificación y restaura el estado original
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

        // Restaurar movimiento y salto
        firstPersonController.canMove = originalCanMove;
        firstPersonController.canRotate = originalCanRotate;
        
        // Restaurar RigidbodyFirstPersonController si fue deshabilitado
        var rigidbodyFPS = firstPersonController.GetComponent<RigidbodyFirstPersonController>();
        if (rigidbodyFPS != null)
        {
            rigidbodyFPS.enabled = originalCanJump;
            Debug.Log("[TrashClassificationController] ✓ RigidbodyFirstPersonController restaurado");
        }
        
        Debug.Log("[TrashClassificationController] ✓ Movimiento y salto RESTAURADOS");
        
        // Resetear isNearTrashZone para volver al comportamiento normal
        isNearTrashZone = false;
        Debug.Log("[TrashClassificationController] ✓ isNearTrashZone desbloqueado (vuelve a normal)");
        
        // Restaurar posición del jugador
        firstPersonController.transform.position = originalPlayerPosition;
        
        // Restaurar cámara
        if (cameraPivot != null)
        {
            cameraPivot.localPosition = originalCameraPosition;
            cameraPivot.localRotation = originalCameraRotation;
            Debug.Log($"[TrashClassificationController] Cámara restaurada");
        }

        // Resetear estado de la zona de trigger
        if (trashZoneTrigger != null)
        {
            trashZoneTrigger.ResetZoneState();
        }

        Debug.Log("[TrashClassificationController] ✓ Modo clasificación DESACTIVADO");
    }

    /// <summary>
    /// Retorna el estado actual de clasificación
    /// </summary>
    public bool IsClassifying => isClassifying;

    /// <summary>
    /// Retorna si el jugador está cerca de la zona de tachos
    /// </summary>
    public bool IsNearTrashZone => isNearTrashZone;

    /// <summary>
    /// Resetea forzadamente el estado si queda atascado
    /// </summary>
    public void ForceResetState()
    {
        Debug.Log("[TrashClassificationController] ⚠️ FORZANDO RESETEO DE ESTADO");
        if (firstPersonController != null)
        {
            firstPersonController.canMove = true;
            firstPersonController.canRotate = true;
        }
        isClassifying = false;
        isNearTrashZone = false;
        Debug.Log("[TrashClassificationController] ✓ Estado reseteado forzadamente");
    }

    /// <summary>
    /// Anima la cámara suavemente desde su posición actual a la posición y rotación objetivo
    /// </summary>
    private IEnumerator AnimateCameraToPosition(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (cameraPivot == null)
            yield break;

        Vector3 startPosition = cameraPivot.position;
        Quaternion startRotation = cameraPivot.rotation;
        float elapsedTime = 0f;
        float animationDuration = 1f / cameraMoveSpeed; // Duración inversamente proporcional a la velocidad

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            
            // Usar Lerp y Slerp para animación suave
            cameraPivot.position = Vector3.Lerp(startPosition, targetPosition, t);
            cameraPivot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            
            yield return null;
        }

        // Asegurar que llega exactamente al objetivo
        cameraPivot.position = targetPosition;
        cameraPivot.rotation = targetRotation;
        
        Debug.Log("[TrashClassificationController] ✓ Animación de cámara completada");
    }
}