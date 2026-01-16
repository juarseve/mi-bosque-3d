using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class DebugTrashClassification : MonoBehaviour
{
    public void Update()
    {
        // Presionar "T" para activar/desactivar modo clasificación manualmente (para testing)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TrashClassificationController controller = FindObjectOfType<TrashClassificationController>();
            if (controller != null)
            {
                if (controller.IsClassifying)
                {
                    controller.EndTrashClassification();
                    Debug.Log("[DEBUG] Modo clasificación DESACTIVADO");
                }
                else
                {
                    controller.StartTrashClassification();
                    Debug.Log("[DEBUG] Modo clasificación ACTIVADO");
                }
            }
            else
            {
                Debug.LogError("[DEBUG] TrashClassificationController no encontrado");
            }
        }

        // Presionar "J" para ver info de la cámara
        if (Input.GetKeyDown(KeyCode.J))
        {
            FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
            if (fpc != null)
            {
                Debug.Log($"[DEBUG] FirstPersonController: {fpc.gameObject.name}");
                Debug.Log($"[DEBUG] Posición jugador: {fpc.transform.position}");
                Debug.Log($"[DEBUG] canMove: {fpc.canMove}, canRotate: {fpc.canRotate}");
                Debug.Log($"[DEBUG] cameraPivot: {(fpc.cameraPivot != null ? fpc.cameraPivot.name : "NULL")}");
                if (fpc.cameraPivot != null)
                {
                    Debug.Log($"[DEBUG] cameraPivot localPosition: {fpc.cameraPivot.localPosition}");
                    Debug.Log($"[DEBUG] cameraPivot worldPosition: {fpc.cameraPivot.position}");
                }
                
                TrashClassificationController tcc = FindObjectOfType<TrashClassificationController>();
                if (tcc != null)
                {
                    Debug.Log($"[DEBUG] IsClassifying: {tcc.IsClassifying}");
                }
            }
        }

        // Presionar "D" para ver info de drag and drop
        if (Input.GetKeyDown(KeyCode.D))
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            Debug.Log($"[DEBUG] Total de Canvas encontrados: {canvases.Length}");
            foreach (Canvas c in canvases)
            {
                Debug.Log($"  - {c.gameObject.name} (renderMode: {c.renderMode}, isRootCanvas: {c.isRootCanvas})");
            }
        }
    }
}
