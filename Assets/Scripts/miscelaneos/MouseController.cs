using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
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