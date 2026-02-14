using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GetBucket : MonoBehaviour, IInteractable
{
    public GameObject panelbalde;
    public Text time;
    public Text recordatorio;
    public GameObject objeto;

    public GameObject jugador;
    public GameObject holding;
    public GameObject agua; // **NUEVO**: Referencia al objeto de agua para ocultarlo

    float timeElapsed = 0;
    float lerpDuration = 1f;
    Vector3 startValue;
    Vector3 endValue;
    bool movimiento = false;

    private MouseController mouseController;
    private Collider cameraBlocker;
    private GameObject puntero;

    public GameObject pendienteGO;
    
    // Flag para evitar interacción múltiple
    private bool isInteracting = false;
    private bool alreadyPicked = false;

    private void Start()
    {
        mouseController = ConstantObjects.instance.mouseController;
        cameraBlocker = ConstantObjects.instance.cameraBlocker;
        puntero = GameObject.Find("Crosshair/Image");
    }

    void Update()
    {
        if (movimiento)
        {
            if (timeElapsed < lerpDuration)
            {
                mouseController.enabled = false;
                cameraBlocker.enabled = true;
                MenuPausa.instance.Pausar();
                this.transform.position = Vector3.Lerp(startValue, endValue, timeElapsed / lerpDuration);
                timeElapsed += Time.deltaTime;
            }
            if (timeElapsed >= lerpDuration)
            {
                movimiento = false;
                this.transform.SetParent(holding.transform);
                mouseController.enabled = true;
                cameraBlocker.enabled = false;
                MenuPausa.instance.Reanudar();
                
                // **OCULTAR VISUALMENTE LA CUBETA Y EL AGUA**
                OcultarCubeta();
                OcultarAgua();
                //StartCoroutine(Picking());
            }
        }
    }

    /// <summary>
    /// Oculta visualmente la cubeta desactivando todos los MeshRenderer y SkinnedMeshRenderer
    /// pero mantiene el GameObject activo para que la lógica siga funcionando
    /// </summary>
    private void OcultarCubeta()
    {
        // Desactivar todos los MeshRenderer en este objeto y sus hijos
        MeshRenderer[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.enabled = false;
        }

        // Desactivar todos los SkinnedMeshRenderer en este objeto y sus hijos
        SkinnedMeshRenderer[] skinnedRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in skinnedRenderers)
        {
            smr.enabled = false;
        }

        Debug.Log("[GetBucket] Cubeta ocultada visualmente (renderers desactivados)");
    }
    
    /// <summary>
    /// Oculta el agua dentro de la cubeta
    /// </summary>
    private void OcultarAgua()
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
            
            // También ocultar particle systems si existen
            ParticleSystem[] particleSystems = agua.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Stop();
                var emission = ps.emission;
                emission.enabled = false;
            }

            Debug.Log("[GetBucket] Agua ocultada visualmente");
        }
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (isInteracting || alreadyPicked) return;
        
        Debug.Log("[GetBucket] OnInteract llamado");
        
        if (time.text != "0" && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
        {
            isInteracting = true;
            
            // Reproducir animación de pickup
            if (InteractionDetector.instance != null)
            {
                InteractionDetector.instance.PlayPickupAnimation();
            }
            
            // Recoger el balde
            RecogerBalde();
            
            Invoke("ResetInteracting", 1f);
        }
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        if (alreadyPicked) return;
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.agarrar(); // Mostrar icono de agarrar cuando mira al balde
            }
        }
        
        Debug.Log("[GetBucket] OnLookAt - Mostrando feedback visual");
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
    
    private void RecogerBalde()
    {
        LanguageManager lm = LanguageManager.Instancia;
        string txt3 = lm.ObtenerTexto("recordatorios.help_fogata_3");
        string txt4 = lm.ObtenerTexto("recordatorios.help_fogata_4");

        startValue = this.transform.position;
        endValue = jugador.transform.position - 0.25f*(jugador.transform.position-this.transform.position) + new Vector3(0,-2.5f,0);
        movimiento = true;
        panelbalde.SetActive(true);
        recordatorio.text = txt3;
        
        // Destruir el collider para que no se pueda volver a recoger
        BoxCollider boxCollider = this.gameObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Destroy(boxCollider);
        }
        
        pendienteGO.GetComponent<DialogueTrigger>().dialogue.sentences[0] = txt4;
        alreadyPicked = true;
        
        Debug.Log("[GetBucket] Balde recogido");
    }

    // ============== COMPATIBILIDAD CON MOUSE (LEGACY) ==============
    
    private void OnMouseDown()
    {
        // Solo usar si no hay InteractionDetector activo o como fallback
        if (InteractionDetector.instance == null && !alreadyPicked)
        {
            if (time.text != "0" && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
            {
                RecogerBalde();
            }
        }
        
        // Siempre activar el panel si se hace clic (comportamiento legacy)
        if (!alreadyPicked)
        {
            panelbalde.SetActive(true);
        }
    }
}
