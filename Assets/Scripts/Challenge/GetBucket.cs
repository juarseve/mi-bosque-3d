using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetBucket : MonoBehaviour, IInteractable
{
    [Header("Referencias UI")]
    public GameObject panelbalde;
    public Text time;
    public Text recordatorio;
    public GameObject objeto;

    [Header("Referencias de Movimiento")]
    public GameObject jugador;
    public GameObject holding;
    public GameObject agua;

    [Header("Referencias de Diálogo")]
    public GameObject pendienteGO;
    
    [Header("Configuración de Collider")]
    [Tooltip("El collider debe estar habilitado y NO ser trigger")]
    public bool verificarColliderEnStart = true;
    
    [Header("Debug")]
    public bool mostrarLogsDetallados = true;

    // Movimiento
    float timeElapsed = 0;
    float lerpDuration = 1f;
    Vector3 startValue;
    Vector3 endValue;
    bool movimiento = false;

    private MouseController mouseController;
    private Collider cameraBlocker;
    private GameObject puntero;
    
    // Flags de control
    private bool isInteracting = false;
    private bool alreadyPicked = false;

    private void Start()
    {
        // Buscar referencias
        mouseController = ConstantObjects.instance.mouseController;
        cameraBlocker = ConstantObjects.instance.cameraBlocker;
        puntero = GameObject.Find("Crosshair/Image");
        
        // NUEVO: Verificación exhaustiva del Collider
        if (verificarColliderEnStart)
        {
            VerificarConfiguracionCollider();
        }
        
        // NUEVO: Verificar IInteractable
        IInteractable interactable = GetComponent<IInteractable>();
        if (interactable == null)
        {
            Debug.LogError("[GetBucket] ❌ ERROR CRÍTICO: Este GameObject NO tiene IInteractable!");
        }
        else
        {
            if (mostrarLogsDetallados)
            {
                Debug.Log("[GetBucket] ✅ IInteractable está correctamente implementado");
            }
        }
        
        if (mostrarLogsDetallados)
        {
            Debug.Log($"[GetBucket] ✅ Inicializado en GameObject: '{gameObject.name}'");
            Debug.Log($"[GetBucket] Layer: {LayerMask.LayerToName(gameObject.layer)} ({gameObject.layer})");
            Debug.Log($"[GetBucket] Tag: {gameObject.tag}");
        }
    }
    
    /// <summary>
    /// Verifica que el Collider esté correctamente configurado
    /// </summary>
    private void VerificarConfiguracionCollider()
    {
        Collider[] colliders = GetComponents<Collider>();
        
        if (colliders.Length == 0)
        {
            Debug.LogError($"[GetBucket] ❌ ERROR: '{gameObject.name}' NO tiene ningún Collider!");
            Debug.LogError("[GetBucket] Solución: Agregar un BoxCollider, SphereCollider o MeshCollider");
            return;
        }
        
        if (mostrarLogsDetallados)
        {
            Debug.Log($"[GetBucket] ℹ️ Encontrados {colliders.Length} collider(s)");
        }
        
        foreach (Collider col in colliders)
        {
            if (col == null) continue;
            
            if (mostrarLogsDetallados)
            {
                Debug.Log($"[GetBucket] 🔍 Collider tipo: {col.GetType().Name}");
                Debug.Log($"[GetBucket]    - Enabled: {col.enabled}");
                Debug.Log($"[GetBucket]    - IsTrigger: {col.isTrigger}");
                Debug.Log($"[GetBucket]    - Bounds: {col.bounds}");
            }
            
            // Verificaciones
            if (!col.enabled)
            {
                Debug.LogWarning($"[GetBucket] ⚠️ ADVERTENCIA: El Collider '{col.GetType().Name}' está DESACTIVADO!");
                Debug.LogWarning("[GetBucket] Solución: Marcar 'Enabled' en el Inspector");
            }
            
            if (col.isTrigger)
            {
                Debug.LogWarning($"[GetBucket] ⚠️ ADVERTENCIA: El Collider '{col.GetType().Name}' está marcado como TRIGGER!");
                Debug.LogWarning("[GetBucket] Para InteractionDetector, el collider NO debe ser trigger");
                Debug.LogWarning("[GetBucket] Solución: DESMARCAR 'Is Trigger' en el Inspector");
            }
            
            // Verificar MeshCollider
            MeshCollider meshCol = col as MeshCollider;
            if (meshCol != null)
            {
                if (!meshCol.convex && meshCol.isTrigger)
                {
                    Debug.LogWarning("[GetBucket] ⚠️ MeshCollider no convex no puede ser trigger");
                }
                
                if (mostrarLogsDetallados)
                {
                    Debug.Log($"[GetBucket]    - Convex: {meshCol.convex}");
                    Debug.Log($"[GetBucket]    - SharedMesh: {(meshCol.sharedMesh != null ? meshCol.sharedMesh.name : "NULL")}");
                }
            }
        }
        
        // Verificar Layer
        VerificarLayer();
    }
    
    /// <summary>
    /// Verifica que el Layer sea compatible con InteractionDetector
    /// </summary>
    private void VerificarLayer()
    {
        if (InteractionDetector.instance != null)
        {
            LayerMask interactableLayer = InteractionDetector.instance.interactableLayer;
            int objectLayer = gameObject.layer;
            
            // Verificar si el layer del objeto está incluido en el LayerMask
            bool layerIncluido = (interactableLayer.value & (1 << objectLayer)) != 0;
            
            if (mostrarLogsDetallados)
            {
                Debug.Log($"[GetBucket] 🔍 Verificando compatibilidad de Layer:");
                Debug.Log($"[GetBucket]    - Layer del objeto: {LayerMask.LayerToName(objectLayer)} ({objectLayer})");
                Debug.Log($"[GetBucket]    - InteractableLayer mask: {interactableLayer.value}");
                Debug.Log($"[GetBucket]    - ¿Layer incluido?: {layerIncluido}");
            }
            
            if (!layerIncluido && interactableLayer.value != 0)
            {
                Debug.LogWarning("[GetBucket] ⚠️ PROBLEMA DETECTADO:");
                Debug.LogWarning($"[GetBucket] El Layer '{LayerMask.LayerToName(objectLayer)}' NO está incluido en InteractionDetector.interactableLayer");
                Debug.LogWarning("[GetBucket] Soluciones:");
                Debug.LogWarning("[GetBucket] 1. Cambiar el Layer del balde a uno incluido en InteractionDetector");
                Debug.LogWarning("[GetBucket] 2. O agregar el Layer actual al InteractableLayer del InteractionDetector");
            }
            else if (interactableLayer.value == 0)
            {
                if (mostrarLogsDetallados)
                {
                    Debug.Log("[GetBucket] ℹ️ InteractableLayer está en 0 (acepta todas las layers)");
                }
            }
            else
            {
                if (mostrarLogsDetallados)
                {
                    Debug.Log("[GetBucket] ✅ Layer compatible con InteractionDetector");
                }
            }
        }
        else
        {
            Debug.LogWarning("[GetBucket] ⚠️ No se encontró InteractionDetector.instance en la escena");
        }
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
                
                OcultarCubeta();
                OcultarAgua();
            }
        }
    }

    private void OcultarCubeta()
    {
        MeshRenderer[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.enabled = false;
        }

        SkinnedMeshRenderer[] skinnedRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in skinnedRenderers)
        {
            smr.enabled = false;
        }

        if (mostrarLogsDetallados)
        {
            Debug.Log("[GetBucket] 🙈 Cubeta ocultada visualmente");
        }
    }
    
    private void OcultarAgua()
    {
        if (agua == null) return;
        
        MeshRenderer[] meshRenderers = agua.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.enabled = false;
        }

        SkinnedMeshRenderer[] skinnedRenderers = agua.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in skinnedRenderers)
        {
            smr.enabled = false;
        }
        
        ParticleSystem[] particleSystems = agua.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop();
            var emission = ps.emission;
            emission.enabled = false;
        }

        if (mostrarLogsDetallados)
        {
            Debug.Log("[GetBucket] 💧 Agua ocultada visualmente");
        }
    }

    // ============== IMPLEMENTACIÓN DE IInteractable ==============
    
    public void OnInteract()
    {
        if (mostrarLogsDetallados)
        {
            Debug.Log($"[GetBucket] 🎯 OnInteract llamado en '{gameObject.name}'");
            Debug.Log($"[GetBucket]    - isInteracting: {isInteracting}");
            Debug.Log($"[GetBucket]    - alreadyPicked: {alreadyPicked}");
            string timeText = (time != null ? time.text : "NULL");
            Debug.Log($"[GetBucket]    - time.text: {timeText}");
            Debug.Log($"[GetBucket]    - Juego pausado: {MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas}");
        }
        
        if (isInteracting || alreadyPicked)
        {
            if (mostrarLogsDetallados)
            {
                Debug.Log("[GetBucket] ⏭️ Ignorando interacción (ya en progreso o ya recogido)");
            }
            return;
        }
        
        if (time != null && time.text == "0")
        {
            if (mostrarLogsDetallados)
            {
                Debug.Log("[GetBucket] ⏱️ El tiempo se acabó, no se puede recoger el balde");
            }
            return;
        }
        
        if (MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas)
        {
            if (mostrarLogsDetallados)
            {
                Debug.Log("[GetBucket] ⏸️ Juego pausado, no se puede interactuar");
            }
            return;
        }
        
        isInteracting = true;
        Debug.Log("[GetBucket] ✅ ¡RECOGIENDO BALDE!");
        
        // Reproducir animación de pickup
        if (InteractionDetector.instance != null)
        {
            InteractionDetector.instance.PlayPickupAnimation();
        }
        
        // Recoger el balde
        RecogerBalde();
        
        Invoke("ResetInteracting", 1f);
    }
    
    private void ResetInteracting()
    {
        isInteracting = false;
    }
    
    public void OnLookAt()
    {
        if (alreadyPicked) return;
        
        if (mostrarLogsDetallados)
        {
            Debug.Log($"[GetBucket] 👁️ OnLookAt llamado en '{gameObject.name}'");
        }
        
        if (puntero != null)
        {
            Puntero p = puntero.GetComponent<Puntero>();
            if (p != null)
            {
                p.agarrar();
            }
        }
    }
    
    public void OnLookAway()
    {
        if (mostrarLogsDetallados)
        {
            Debug.Log($"[GetBucket] 👁️‍🗨️ OnLookAway llamado en '{gameObject.name}'");
        }
        
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
    
    private void RecogerBalde()
    {
        LanguageManager lm = LanguageManager.Instancia;
        string txt3 = lm != null ? lm.ObtenerTexto("recordatorios.help_fogata_3") : "Busca agua, ¡escucha a tu alrededor!";
        string txt4 = lm != null ? lm.ObtenerTexto("recordatorios.help_fogata_4") : "Aún nos falta conseguir agua!";

        startValue = this.transform.position;
        endValue = jugador.transform.position - 0.25f*(jugador.transform.position-this.transform.position) + new Vector3(0,-2.5f,0);
        movimiento = true;
        
        if (panelbalde != null)
        {
            panelbalde.SetActive(true);
        }
        
        if (recordatorio != null)
        {
            recordatorio.text = txt3;
        }
        
        // Destruir el collider para que no se pueda volver a recoger
        BoxCollider boxCollider = this.gameObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Destroy(boxCollider);
            if (mostrarLogsDetallados)
            {
                Debug.Log("[GetBucket] 🗑️ BoxCollider destruido");
            }
        }
        
        // Actualizar diálogo pendiente
        if (pendienteGO != null)
        {
            DialogueTrigger trigger = pendienteGO.GetComponent<DialogueTrigger>();
            if (trigger != null && trigger.dialogue != null && trigger.dialogue.sentences != null && trigger.dialogue.sentences.Length > 0)
            {
                trigger.dialogue.sentences[0] = txt4;
            }
        }
        
        alreadyPicked = true;
        
        Debug.Log("[GetBucket] ✅ Balde recogido exitosamente");
    }

    // ============== COMPATIBILIDAD CON MOUSE (LEGACY) ==============
    
    private void OnMouseDown()
    {
        if (InteractionDetector.instance == null && !alreadyPicked)
        {
            if (time != null && time.text != "0" && !(MenuPausa.IsPaused || MenuPausa.IsPausedByOtherCanvas))
            {
                Debug.Log("[GetBucket] 🖱️ OnMouseDown (modo legacy sin InteractionDetector)");
                RecogerBalde();
            }
        }
        
        if (!alreadyPicked && panelbalde != null)
        {
            panelbalde.SetActive(true);
        }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Dibuja gizmos para visualizar el collider en el editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = alreadyPicked ? Color.gray : Color.cyan;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
    
    /// <summary>
    /// Botón de contexto para forzar verificación en el editor
    /// </summary>
    [ContextMenu("Verificar Configuración del Balde")]
    private void VerificarManual()
    {
        Debug.Log("========================================");
        Debug.Log("[GetBucket] 🔍 VERIFICACIÓN MANUAL");
        Debug.Log("========================================");
        VerificarConfiguracionCollider();
        Debug.Log("========================================");
    }
    #endif
}
