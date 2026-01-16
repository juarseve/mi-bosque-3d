using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Nest : MonoBehaviour, IInteractable
{
    public static bool home = false;
    public GameObject feedback;
    public Text message;
    private bool active = true;
    public GameObject recordatorio;
    
    // Referencia al puntero para feedback visual
    private GameObject puntero;
    
    // Flag para evitar doble interacción
    private bool isInteracting = false;
    
    void Start()
    {
        puntero = GameObject.Find("Crosshair/Image");
        // Resetear el estado al iniciar
        home = false;
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (isInteracting) return;
        
        Debug.Log("[Nest] OnInteract llamado. active=" + active + ", Squirrel.caught=" + Squirrel.caught + ", Squirrel.activate=" + Squirrel.activate);
        
        // Solo permitir interacción si:
        // 1. La madriguera está activa (no se ha completado)
        // 2. El conejo está capturado (Squirrel.caught == true)
        // 3. El juego no está pausado
        if (active && Squirrel.caught && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
        {
            isInteracting = true;
            Debug.Log("[Nest] ¡Completando misión del conejo!");
            
            // Reproducir animación de soltar (pickup inverso)
            if (InteractionDetector.instance != null)
            {
                InteractionDetector.instance.PlayPickupAnimation();
            }
            
            // Completar la misión
            CompletarMision();
            
            Invoke("ResetInteracting", 1f);
        }
        else
        {
            // Dar feedback de por qué no se puede interactuar
            if (!active)
            {
                Debug.Log("[Nest] La misión ya fue completada");
            }
            else if (!Squirrel.caught)
            {
                Debug.Log("[Nest] Necesitas capturar al conejo primero (Squirrel.caught = false)");
            }
        }
    }
    
    /// <summary>
    /// Completa la misión del conejo
    /// </summary>
    private void CompletarMision()
    {
        // Marcar como completado
        home = true;
        Squirrel.activate = false;
        Squirrel.caught = false; // El conejo ya no está "capturado" porque está en casa
        active = false;
        
        // Mostrar feedback
        StartCoroutine(ShowFeedback());
        
        // Ocultar recordatorio
        if (recordatorio != null)
        {
            recordatorio.SetActive(false);
        }
        
        // Desactivar el collider bloqueador (tag "Rabbit")
        DesactivarBloqueador();
        
        Debug.Log("[Nest] Misión completada. home=" + home);
    }
    
    /// <summary>
    /// Desactiva el collider que bloquea el paso
    /// </summary>
    private void DesactivarBloqueador()
    {
        GameObject[] rabbitObjects = GameObject.FindGameObjectsWithTag("Rabbit");
        
        foreach (GameObject rabbitObj in rabbitObjects)
        {
            // Desactivar todos los colliders
            Collider[] colliders = rabbitObj.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
                Debug.Log("[Nest] Collider desactivado en " + rabbitObj.name + ": " + col.GetType().Name);
            }
            
            // También buscar en hijos
            Collider[] childColliders = rabbitObj.GetComponentsInChildren<Collider>();
            foreach (Collider col in childColliders)
            {
                col.enabled = false;
            }
        }
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        // Solo mostrar puntero si el conejo está capturado y la misión no está completa
        if (!Squirrel.caught || !active)
        {
            return;
        }
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.agarrar(); // Mostrar mano para indicar que puede soltar el conejo
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

    // Mantener OnMouseDown para compatibilidad con sistemas antiguos
    private void OnMouseDown()
    {
        if (InteractionDetector.instance == null)
        {
            if (active && Squirrel.caught && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
            {
                CompletarMision();
            }
        }
    }

    IEnumerator ShowFeedback()
    {
        if (message != null)
        {
            message.text = LanguageManager.Instancia.ObtenerTexto("recordatorios.feedback");
        }
        if (feedback != null)
        {
            feedback.SetActive(true);
            yield return new WaitForSeconds(3.0f);
            feedback.SetActive(false);
        }
    }
}
