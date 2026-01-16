using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeds : MonoBehaviour, IInteractable
{
    public int id;
    public Animator semillasAnim;
    bool entregada = false;
    public GameObject padreSemillas;
    
    // Referencia al puntero para feedback visual
    private GameObject puntero;
    
    // Referencia al ClickMouse del mismo objeto (para abrir galería)
    private ClickMouse clickMouse;
    
    // Flag para evitar doble interacción
    private bool isInteracting = false;

    private void Start()
    {
        padreSemillas = GameObject.Find("SemillasAnim");
        puntero = GameObject.Find("Crosshair/Image");
        
        // Buscar ClickMouse en el mismo objeto para poder abrir la galería
        clickMouse = GetComponent<ClickMouse>();
        if (clickMouse != null)
        {
            Debug.Log("[Seeds] ClickMouse encontrado en " + gameObject.name);
        }
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (isInteracting) return;
        isInteracting = true;
        
        Debug.Log("[Seeds] OnInteract llamado en " + gameObject.name);
        
        try
        {
            // Primero recoger la semilla
            RecogerSemilla();
            
            // Luego abrir la galería si hay ClickMouse
            if (clickMouse != null)
            {
                Debug.Log("[Seeds] Llamando a ClickMouse.OnInteract para abrir galería");
                clickMouse.OnInteract();
            }
        }
        finally
        {
            // Resetear flag después de un pequeño delay
            Invoke("ResetInteracting", 0.5f);
        }
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        if (entregada) return;
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.puntero();
            }
        }
    }
    
    public void OnLookAway()
    {
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.mira();
            }
        }
    }

    // ============== LÓGICA DE RECOLECCIÓN ==============
    
    public void RecogerSemilla()
    {
        if (entregada)
        {
            Debug.Log("[Seeds] Semilla ya entregada");
            return;
        }
        
        entregada = true;
        Debug.Log("[Seeds] Recogiendo semilla ID " + id);
        
        // Añadir a la mochila
        if (GameManager.instance != null && GameManager.instance.mochila != null)
        {
            GameManager.instance.mochila.TestAddF(id);
            Debug.Log("[Seeds] Semilla añadida a la mochila");
        }
        else
        {
            Debug.LogWarning("[Seeds] No se pudo añadir a la mochila - GameManager o mochila es null");
        }
        
        // Actualizar UI de semillas
        if (padreSemillas != null)
        {
            // Desactivar todos los indicadores
            for (int i = 0; i < padreSemillas.transform.childCount; i++)
            {
                padreSemillas.transform.GetChild(i).gameObject.SetActive(false);
            }

            // Activar el indicador correspondiente
            int childIndex = GetSemillaChildIndex(id);
            if (childIndex >= 0 && childIndex < padreSemillas.transform.childCount)
            {
                padreSemillas.transform.GetChild(childIndex).gameObject.SetActive(true);
                Debug.Log("[Seeds] UI de semilla actualizada");
            }
        }
        
        // Reproducir animación
        if (semillasAnim != null)
        {
            semillasAnim.SetTrigger("NuevaSemilla");
            Debug.Log("[Seeds] Animación de nueva semilla activada");
        }
        
        Debug.Log("[Seeds] Semilla " + id + " recogida exitosamente");
    }
    
    /// <summary>
    /// Obtiene el índice del hijo para mostrar según el ID de la semilla
    /// </summary>
    private int GetSemillaChildIndex(int seedId)
    {
        switch (seedId)
        {
            case 3:  return 1;  // teca
            case 4:  return 0;  // ceibo
            case 8:  return 2;  // bototillo
            case 9:  return 4;  // judea
            case 10: return 5;  // guayacan
            case 11: return 3;  // jacaranda
            default: return -1;
        }
    }

    // ============== COMPATIBILIDAD CON MOUSE (LEGACY) ==============
    
    private void OnMouseDown()
    {
        // Solo usar si no hay InteractionDetector activo
        if (InteractionDetector.instance == null)
        {
            RecogerSemilla();
            
            // También abrir galería con mouse
            if (clickMouse != null)
            {
                clickMouse.OnInteract();
            }
        }
    }
    
    private void OnMouseEnter()
    {
        OnLookAt();
    }
    
    private void OnMouseExit()
    {
        OnLookAway();
    }
}
