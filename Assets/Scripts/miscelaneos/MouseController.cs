using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class MouseController : MonoBehaviour
{

    private Rect screenRect;

    public FirstPersonController fpsController;
    public float rayLength;
    public Camera fpsCamera;

    // --- NUEVO: El punto desde donde sale el rayo (ej. la cabeza/pivote) ---
    [Header("Ajustes TPS")]
    public Transform interactionOrigin;
    // -----------------------------------------------------------------------

    private ClickMouse click;
    public LayerMask layerToHit;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        screenRect = new Rect(0, 0, Screen.width, Screen.height);

        // Si se te olvida asignar el origen, usará la cámara por defecto para que no de error
        if (interactionOrigin == null)
        {
            interactionOrigin = fpsCamera.transform;
            Debug.LogWarning("MouseController: No has asignado el 'interactionOrigin'. Usando la cámara por defecto.");
        }
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

        if (Input.GetButtonDown("Fire1") && !(MenuPausa.IsPaused))
        {
            ClickObject();
        }
    }

    void OnApplicationFocus(bool ApplicationIsBack)
    {
        if (ApplicationIsBack == true)
        {
            // Verificar si hay interfaces de UI activas que necesitan el cursor
            bool hasActiveUI = false;
            
            // Verificar si el juego está pausado
            if (MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas)
            {
                hasActiveUI = true;
            }
            
            // Verificar si hay diálogos abiertos
            if (DialogueManager.instance != null && DialogueManager.instance.isDialogueActive)
            {
                hasActiveUI = true;
            }
            
            // Verificar si el libro está abierto
            if (BookPages.instance != null && BookPages.instance.isOpen)
            {
                hasActiveUI = true;
            }
            
            // Verificar si hay algún Canvas activo que pueda necesitar el cursor
            // Buscar todos los Canvas en la escena
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas != null && canvas.gameObject.activeInHierarchy && canvas.enabled)
                {
                    // Verificar si el canvas es de tipo ScreenSpace (interfaz de usuario)
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || 
                        canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        // Verificar si el canvas tiene algún objeto hijo activo
                        // Esto indica que hay una interfaz visible
                        if (HasActiveUIChildren(canvas.transform))
                        {
                            hasActiveUI = true;
                            break;
                        }
                    }
                }
            }
            
            // Solo bloquear el cursor si no hay UI activa
            if (!hasActiveUI)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                // Si hay UI activa, asegurar que el cursor esté visible
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
    
    // Método auxiliar para verificar si un transform tiene hijos activos
    private bool HasActiveUIChildren(Transform parent)
    {
        // Verificar si el objeto mismo está activo y tiene componentes de UI
        if (parent.gameObject.activeSelf && 
            (parent.GetComponent<Button>() != null || 
             parent.GetComponent<Image>() != null || 
             parent.GetComponent<Text>() != null ||
             parent.GetComponent<TMPro.TextMeshProUGUI>() != null))
        {
            return true;
        }
        
        // Verificar recursivamente los hijos
        foreach (Transform child in parent)
        {
            if (HasActiveUIChildren(child))
            {
                return true;
            }
        }
        
        return false;
    }

    void ClickObject()
    {
        RaycastHit hit;

        // --- CAMBIO: Usamos interactionOrigin.position en lugar de la cámara ---
        // El rayo sale de la cabeza (origin) pero mira hacia donde mira la cámara (direction)
        if (Physics.Raycast(interactionOrigin.position, fpsCamera.transform.forward, out hit, rayLength, layerToHit))
        {

            // Debug opcional: Dibuja una línea roja en la escena para ver el rayo
            Debug.DrawRay(interactionOrigin.position, fpsCamera.transform.forward * rayLength, Color.red, 1f);

            click = hit.collider.gameObject.GetComponent<ClickMouse>();
            if (click != null)
            {
                click.ShowGallery();
            }
        };
    }
}