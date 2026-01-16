using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class TrashZoneTrigger : MonoBehaviour
{
    private TrashClassificationController trashController;
    private FirstPersonController fpsController;
    private bool hasEnteredZone = false;

    private void Start()
    {
        // Obtener referencias
        trashController = FindObjectOfType<TrashClassificationController>();
        fpsController = FindObjectOfType<FirstPersonController>();

        // Verificar que sea un trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            Debug.Log($"[TrashZoneTrigger] Collider configurado como trigger en: {gameObject.name}");
        }

        if (trashController == null)
        {
            Debug.LogError("[TrashZoneTrigger] TrashClassificationController no encontrado");
        }
        if (fpsController == null)
        {
            Debug.LogError("[TrashZoneTrigger] FirstPersonController no encontrado");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasEnteredZone)
            return;

        hasEnteredZone = true;

        Debug.Log("[TrashZoneTrigger] ¡Jugador entró a la zona de clasificación!");

        // Activar modo clasificación automáticamente
        if (trashController != null)
        {
            trashController.StartTrashClassification();
            Debug.Log("[TrashZoneTrigger] Modo clasificación ACTIVADO automáticamente");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        hasEnteredZone = false;

        Debug.Log("[TrashZoneTrigger] ¡Jugador salió de la zona de clasificación!");

        // Desactivar modo clasificación
        if (trashController != null)
        {
            trashController.EndTrashClassification();
            Debug.Log("[TrashZoneTrigger] Modo clasificación DESACTIVADO automáticamente");
        }
    }
}
