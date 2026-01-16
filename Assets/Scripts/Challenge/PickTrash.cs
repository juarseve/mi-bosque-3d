using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickTrash : MonoBehaviour, IInteractable
{
    private bool activate = true;
    public int id;
    public GameObject feedback;
    public Text msj;

    public GameObject jugador;

    float timeElapsed = 0;
    float lerpDuration = 1f;
    Vector3 startValue;
    Vector3 endValue;
    bool movimiento = false;

    private GameObject puntero;
    
    // Flag para evitar doble interacción
    private bool isInteracting = false;

    private void Start()
    {
        puntero = GameObject.Find("Crosshair/Image");
        
        // Buscar el jugador si no está asignado
        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player");
        }
    }

    void Update()
    {
        if (movimiento)
        {
            if (timeElapsed < lerpDuration)
            {
                this.transform.position = Vector3.Lerp(startValue, endValue, timeElapsed / lerpDuration);
                timeElapsed += Time.deltaTime;
            }
            if (timeElapsed >= lerpDuration)
            {
                movimiento = false;
                StartCoroutine(Picking());
            }
        }
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (isInteracting || !activate) return;
        
        if (!(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
        {
            isInteracting = true;
            Debug.Log("[PickTrash] Recogiendo basura ID: " + id);
            
            // Reproducir animación de pickup
            if (InteractionDetector.instance != null)
            {
                InteractionDetector.instance.PlayPickupAnimation();
            }
            
            // Iniciar la recogida
            RecogerBasura();
            
            Invoke("ResetInteracting", 1f);
        }
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        if (!activate) return;
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.agarrar(); // Usar el puntero de agarrar para la basura
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
    
    /// <summary>
    /// Inicia el proceso de recoger la basura
    /// </summary>
    private void RecogerBasura()
    {
        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player");
        }
        
        startValue = this.transform.position;
        endValue = jugador.transform.position + new Vector3(0, -5, 0);
        
        // Destruir el collider para que no se pueda volver a interactuar
        BoxCollider boxCollider = this.gameObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Destroy(boxCollider);
        }
        
        movimiento = true;
        activate = false;
    }

    // ============== COMPATIBILIDAD CON MOUSE (LEGACY) ==============
    
    private void OnMouseEnter()
    {
        OnLookAt();
    }
    
    private void OnMouseExit()
    {
        OnLookAway();
    }

    private void OnMouseDown()
    {
        // Solo usar si no hay InteractionDetector activo
        if (InteractionDetector.instance == null)
        {
            if (activate && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
            {
                RecogerBasura();
            }
        }
    }

    // ============== CORRUTINA DE PICKING ==============

    IEnumerator Picking()
    {
        msj.text = LanguageManager.Instancia.ObtenerTexto("recordatorios.recoge_basura");
        feedback.SetActive(true);
        
        if (GameManager.instance != null && GameManager.instance.mochila != null)
        {
            GameManager.instance.mochila.TestAddAcc(id);
        }
        
        yield return new WaitForSeconds(1f);
        
        feedback.SetActive(false);
        this.gameObject.SetActive(false);
        
        // Notificar a la mochila
        GameObject controlMochila = GameObject.Find("Control Mochila");
        if (controlMochila != null)
        {
            MochilaCtrl mochilaCtrl = controlMochila.GetComponent<MochilaCtrl>();
            if (mochilaCtrl != null)
            {
                mochilaCtrl.Notificar("basura");
            }
        }
        
        Debug.Log("[PickTrash] Basura recogida exitosamente");
    }
}
