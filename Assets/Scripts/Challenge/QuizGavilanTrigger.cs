using UnityEngine;

/// <summary>
/// Trigger que inicia el Quiz del Gavilán cuando el jugador presiona E
/// Colocar en un objeto con Collider (IsTrigger = true) cerca del gavilán
/// </summary>
public class QuizGavilanTrigger : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    public QuizGavilan quizGavilan;
    
    [Header("Feedback Visual")]
    public GameObject indicadorInteraccion; // Opcional: icono de "Presiona E"
    
    private GameObject puntero;
    private bool jugadorDentro = false;
    private bool yaCompletado = false;
    
    private void Start()
    {
        puntero = GameObject.Find("Crosshair/Image");
        
        if (indicadorInteraccion != null)
        {
            indicadorInteraccion.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Verificar si el quiz ya fue completado
        if (quizGavilan != null && quizGavilan.EstaCompletado())
        {
            yaCompletado = true;
            if (indicadorInteraccion != null)
            {
                indicadorInteraccion.SetActive(false);
            }
        }
    }
    
    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (yaCompletado) return;
        
        if (quizGavilan != null && !MenuPausa.IsPaused && !MenuPausa.IsPausedByOtherCanvas)
        {
            Debug.Log("[QuizGavilanTrigger] Iniciando quiz del Gavilán");
            quizGavilan.IniciarQuiz();
            
            if (indicadorInteraccion != null)
            {
                indicadorInteraccion.SetActive(false);
            }
        }
    }
    
    public void OnLookAt()
    {
        if (yaCompletado) return;
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.agarrar(); // Mostrar icono de interacción
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
    
    // ============== TRIGGER EVENTS ==============
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !yaCompletado)
        {
            jugadorDentro = true;
            
            if (indicadorInteraccion != null)
            {
                indicadorInteraccion.SetActive(true);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            
            if (indicadorInteraccion != null)
            {
                indicadorInteraccion.SetActive(false);
            }
        }
    }
    
    // ============== COMPATIBILIDAD LEGACY ==============
    
    private void OnMouseDown()
    {
        // Solo usar si no hay InteractionDetector activo
        if (InteractionDetector.instance == null && jugadorDentro)
        {
            OnInteract();
        }
    }
}
