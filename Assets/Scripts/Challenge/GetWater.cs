using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetWater : MonoBehaviour, IInteractable
{
    public GameObject panelwater;
    public GameObject panelbalde;
    public Text time;
    public Text recordatorio;
    public GameObject agua;
    public GameObject pendienteGO;
    
    private GameObject puntero;
    
    // Flag para evitar interacción múltiple
    private bool isInteracting = false;
    private bool alreadyCollected = false;

    private void Start()
    {
        puntero = GameObject.Find("Crosshair/Image");
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (isInteracting || alreadyCollected) return;
        
        Debug.Log("[GetWater] OnInteract llamado");
        
        if (!(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
        {
            isInteracting = true;
            
            // Reproducir animación de interacción
            if (InteractionDetector.instance != null)
            {
                InteractionDetector.instance.PlayInteractionAnimation();
            }
            
            // Recoger agua
            RecogerAgua();
            
            Invoke("ResetInteracting", 1f);
        }
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        if (alreadyCollected) return;
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.puntero(); // Mostrar icono de interacción cuando mira al agua
            }
        }
        
        Debug.Log("[GetWater] OnLookAt - Mostrando feedback visual");
    }
    
    public void OnLookAway()
    {
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.mira(); // Restaurar puntero normal
            }
        }
    }
    
    // ============== LÓGICA DE RECOLECCIÓN ==============
    
    /// <summary>
    /// Lógica principal para recoger agua
    /// </summary>
    private void RecogerAgua()
    {
        LanguageManager lm = LanguageManager.Instancia;
        string txt0 = lm.ObtenerTexto("recordatorios.help_fogata_0");
        string txt1 = lm.ObtenerTexto("recordatorios.help_fogata_1");
        string txt2 = lm.ObtenerTexto("recordatorios.help_fogata_2");

        if (time.text != "0" && (panelbalde.activeSelf == true))
        {
            // **IMPORTANTE**: Activar el GameObject del agua pero ocultarlo visualmente
            agua.SetActive(true);
            OcultarAguaVisualmente();
            
            panelbalde.SetActive(false);
            recordatorio.text = txt0;
            panelwater.SetActive(true);
            pendienteGO.GetComponent<DialogueTrigger>().dialogue.sentences[0] = txt1;
            
            alreadyCollected = true;
            Debug.Log("[GetWater] Agua recogida - GameObject activo pero invisible");
        }
        else if (time.text != "0" && (panelbalde.activeSelf == false) && (panelwater.activeSelf == false))
        {
            recordatorio.text = txt2;
        }
    }
    
    /// <summary>
    /// Oculta el agua visualmente pero mantiene el GameObject activo
    /// </summary>
    private void OcultarAguaVisualmente()
    {
        if (agua != null)
        {
            // Desactivar todos los MeshRenderer del agua y sus hijos
            MeshRenderer[] meshRenderers = agua.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in meshRenderers)
            {
                mr.enabled = false;
            }

            // Desactivar todos los SkinnedMeshRenderer del agua y sus hijos
            SkinnedMeshRenderer[] skinnedRenderers = agua.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smr in skinnedRenderers)
            {
                smr.enabled = false;
            }
            
            // Ocultar particle systems si existen (burbujas, splash, etc.)
            ParticleSystem[] particleSystems = agua.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Stop();
                var emission = ps.emission;
                emission.enabled = false;
            }
            
            // Desactivar cualquier Renderer adicional
            Renderer[] allRenderers = agua.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in allRenderers)
            {
                r.enabled = false;
            }

            Debug.Log("[GetWater] Agua ocultada visualmente (renderers desactivados)");
        }
        else
        {
            Debug.LogWarning("[GetWater] No se ha asignado el GameObject 'agua' en el Inspector");
        }
    }
    
    // ============== MÉTODO PÚBLICO PARA COMPATIBILIDAD ==============
    
    /// <summary>
    /// Método público para llamar desde otros scripts (mantiene compatibilidad)
    /// </summary>
    public void recogerAgua()
    {
        RecogerAgua();
    }

    // ============== COMPATIBILIDAD CON MOUSE (LEGACY) ==============
    
    private void OnMouseDown()
    {
        // Solo usar si no hay InteractionDetector activo o como fallback
        if (InteractionDetector.instance == null && !alreadyCollected)
        {
            if (!(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
            {
                RecogerAgua();
            }
        }
    }
    
    // ============== MOUSE HOVER (LEGACY) ==============
    
    private void OnMouseEnter()
    {
        if (!alreadyCollected)
        {
            OnLookAt();
        }
    }
    
    private void OnMouseExit()
    {
        OnLookAway();
    }
}