using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetWater : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelwater;
    public GameObject panelbalde;
    public Text time;
    public Text recordatorio;
    public GameObject agua;
    public GameObject pendienteGO;
    
    [Header("Debug")]
    public bool mostrarLogs = true;
    public bool mostrarLogsDetallados = false;
    
    // Flag para evitar recolección múltiple
    private bool alreadyCollected = false;
    
    private GameObject jugador;
    private bool diagnosticoRealizado = false;
    
    // Verificar que sea trigger
    private void Start()
    {
        // Buscar jugador
        jugador = GameObject.FindGameObjectWithTag("Player");
        
        if (jugador == null)
        {
            Debug.LogError("[GetWater] ❌ NO SE ENCONTRÓ EL JUGADOR (Tag 'Player')");
        }
        else if (mostrarLogs)
        {
            Debug.Log("[GetWater] ✅ Jugador encontrado: " + jugador.name);
        }
        
        // Verificar configuración del collider del agua
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError("[GetWater] ❌ ERROR CRÍTICO: Este GameObject NO tiene Collider!");
            Debug.LogError("[GetWater] Solución: Agregar un BoxCollider, SphereCollider o MeshCollider");
            return;
        }
        
        if (!collider.isTrigger)
        {
            Debug.LogWarning("[GetWater] ⚠️ El Collider NO está marcado como Trigger. Activándolo automáticamente.");
            collider.isTrigger = true;
        }
        
        if (mostrarLogs)
        {
            Debug.Log("[GetWater] ====================================");
            Debug.Log("[GetWater] CONFIGURACIÓN DEL COLLIDER DEL AGUA:");
            Debug.Log("[GetWater] - GameObject: " + gameObject.name);
            Debug.Log("[GetWater] - Layer: " + LayerMask.LayerToName(gameObject.layer) + " (" + gameObject.layer + ")");
            Debug.Log("[GetWater] - Tipo Collider: " + collider.GetType().Name);
            Debug.Log("[GetWater] - IsTrigger: " + collider.isTrigger);
            Debug.Log("[GetWater] - Enabled: " + collider.enabled);
            Debug.Log("[GetWater] - Bounds Size: " + collider.bounds.size);
            Debug.Log("[GetWater] - Bounds Center: " + collider.bounds.center);
            Debug.Log("[GetWater] ====================================");
            Debug.Log("[GetWater] ✅ Inicializado. Esperando que el jugador entre al trigger con el balde.");
        }
    }
    
    private void Update()
    {
        // Realizar diagnóstico una vez que el jugador esté disponible
        if (!diagnosticoRealizado && jugador != null && mostrarLogsDetallados)
        {
            RealizarDiagnosticoCompleto();
            diagnosticoRealizado = true;
        }
    }
    
    /// <summary>
    /// Diagnóstico exhaustivo del jugador y del sistema de triggers
    /// </summary>
    private void RealizarDiagnosticoCompleto()
    {
        Debug.Log("[GetWater] ========================================");
        Debug.Log("[GetWater] 🔍 DIAGNÓSTICO COMPLETO DEL SISTEMA");
        Debug.Log("[GetWater] ========================================");
        
        // 1. Información del jugador
        Debug.Log("[GetWater] 📊 INFORMACIÓN DEL JUGADOR:");
        Debug.Log("[GetWater]    - GameObject: " + jugador.name);
        Debug.Log("[GetWater]    - Tag: " + jugador.tag);
        Debug.Log("[GetWater]    - Layer: " + LayerMask.LayerToName(jugador.layer) + " (" + jugador.layer + ")");
        Debug.Log("[GetWater]    - Posición: " + jugador.transform.position);
        
        // 2. Rigidbody del jugador
        Rigidbody jugadorRB = jugador.GetComponent<Rigidbody>();
        if (jugadorRB != null)
        {
            Debug.Log("[GetWater] ✅ Jugador TIENE Rigidbody:");
            Debug.Log("[GetWater]    - IsKinematic: " + jugadorRB.isKinematic);
            Debug.Log("[GetWater]    - UseGravity: " + jugadorRB.useGravity);
            Debug.Log("[GetWater]    - Mass: " + jugadorRB.mass);
        }
        else
        {
            Debug.LogWarning("[GetWater] ⚠️ Jugador NO tiene Rigidbody");
        }
        
        // 3. Colliders del jugador
        Collider[] jugadorColliders = jugador.GetComponents<Collider>();
        if (jugadorColliders.Length > 0)
        {
            Debug.Log("[GetWater] ✅ Jugador tiene " + jugadorColliders.Length + " collider(s):");
            for (int i = 0; i < jugadorColliders.Length; i++)
            {
                Collider col = jugadorColliders[i];
                Debug.Log($"[GetWater]    [{i}] {col.GetType().Name}:");
                Debug.Log($"[GetWater]        - Enabled: {col.enabled}");
                Debug.Log($"[GetWater]        - IsTrigger: {col.isTrigger}");
                Debug.Log($"[GetWater]        - Bounds: {col.bounds}");
            }
        }
        else
        {
            Debug.LogError("[GetWater] ❌ Jugador NO tiene ningún Collider!");
            Debug.LogError("[GetWater] SOLUCIÓN: Agregar un CharacterController, CapsuleCollider o BoxCollider al jugador");
        }
        
        // 4. CharacterController (alternativa a Collider)
        CharacterController cc = jugador.GetComponent<CharacterController>();
        if (cc != null)
        {
            Debug.Log("[GetWater] ℹ️ Jugador tiene CharacterController:");
            Debug.Log("[GetWater]    - Enabled: " + cc.enabled);
            Debug.Log("[GetWater]    - Radius: " + cc.radius);
            Debug.Log("[GetWater]    - Height: " + cc.height);
            Debug.Log("[GetWater]    - Center: " + cc.center);
        }
        
        // 5. Verificar Layer Collision Matrix
        int waterLayer = gameObject.layer;
        int playerLayer = jugador.layer;
        bool canCollide = !Physics.GetIgnoreLayerCollision(waterLayer, playerLayer);
        
        Debug.Log("[GetWater] 🔍 VERIFICACIÓN DE COLISIÓN DE CAPAS:");
        Debug.Log($"[GetWater]    - Water Layer: {LayerMask.LayerToName(waterLayer)} ({waterLayer})");
        Debug.Log($"[GetWater]    - Player Layer: {LayerMask.LayerToName(playerLayer)} ({playerLayer})");
        Debug.Log($"[GetWater]    - ¿Pueden colisionar?: {canCollide}");
        
        if (!canCollide)
        {
            Debug.LogError("[GetWater] ❌ LAS CAPAS NO PUEDEN COLISIONAR!");
            Debug.LogError("[GetWater] SOLUCIÓN: Ir a Edit > Project Settings > Physics > Layer Collision Matrix");
            Debug.LogError($"[GetWater] Y marcar la intersección entre '{LayerMask.LayerToName(waterLayer)}' y '{LayerMask.LayerToName(playerLayer)}'");
        }
        
        // 6. Distancia entre jugador y agua
        float distancia = Vector3.Distance(jugador.transform.position, transform.position);
        Debug.Log($"[GetWater] 📏 Distancia jugador-agua: {distancia:F2} metros");
        
        Collider waterCollider = GetComponent<Collider>();
        if (waterCollider != null)
        {
            bool jugadorDentro = waterCollider.bounds.Contains(jugador.transform.position);
            Debug.Log($"[GetWater] 📍 ¿Jugador dentro del trigger?: {jugadorDentro}");
        }
        
        Debug.Log("[GetWater] ========================================");
    }
    
    /// <summary>
    /// Se llama automáticamente cuando el jugador entra al trigger del agua
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (mostrarLogsDetallados)
        {
            Debug.Log($"[GetWater] 🎯 OnTriggerEnter llamado - Objeto: {other.gameObject.name}, Tag: {other.tag}");
        }
        
        // Verificar que sea el jugador
        if (!other.CompareTag("Player"))
        {
            if (mostrarLogsDetallados)
            {
                Debug.Log($"[GetWater] ⏭️ Ignorando objeto (no es Player): {other.gameObject.name}");
            }
            return;
        }
        
        Debug.Log("[GetWater] ✅ ¡JUGADOR DETECTADO EN ONTRIGGERENTER!");
        
        // Verificar condiciones y recoger agua
        VerificarYRecogerAgua();
    }
    
    /// <summary>
    /// ALTERNATIVA: OnTriggerStay se llama cada frame que el jugador está dentro
    /// Útil si OnTriggerEnter no se dispara por problemas de física
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (alreadyCollected) return;
        
        if (mostrarLogsDetallados)
        {
            Debug.Log($"[GetWater] 🔄 OnTriggerStay - Objeto: {other.gameObject.name}");
        }
        
        if (!other.CompareTag("Player")) return;
        
        // Verificar condiciones y recoger agua
        VerificarYRecogerAgua();
    }
    
    /// <summary>
    /// Verifica las condiciones y recoge el agua si todo está correcto
    /// </summary>
    private void VerificarYRecogerAgua()
    {
        // Verificar que no se haya recolectado ya
        if (alreadyCollected)
        {
            if (mostrarLogsDetallados)
            {
                Debug.Log("[GetWater] ℹ️ Agua ya recolectada anteriormente");
            }
            return;
        }
        
        // Verificar que el tiempo no sea "0"
        if (time != null && time.text == "0")
        {
            if (mostrarLogs)
            {
                Debug.Log("[GetWater] ⏱️ El tiempo se acabó, no se puede recoger agua");
            }
            return;
        }
        
        // ✅ INTENTAR RECOGER AGUA (la verificación del balde se hace dentro)
        RecogerAgua();
    }
    
    /// <summary>
    /// Lógica principal para recoger agua automáticamente
    /// SOLO se puede recoger si el jugador tiene el balde previamente
    /// </summary>
    private void RecogerAgua()
    {
        // Obtener LanguageManager una sola vez al inicio
        LanguageManager lm = LanguageManager.Instancia;
        
        // ============== VERIFICACIÓN CRÍTICA: ¿TIENE EL BALDE? ==============
        if (panelbalde == null || !panelbalde.activeSelf)
        {
            if (mostrarLogs)
            {
                Debug.Log("[GetWater] ❌ El jugador NO tiene el balde. No se puede recoger agua.");
            }
            
            // Mostrar mensaje de que necesita el balde
            if (recordatorio != null)
            {
                string txt = lm != null ? lm.ObtenerTexto("recordatorios.help_fogata_4") : "Aún nos falta conseguir agua! Hay un lago cerca del pozo, sigue buscando!";
                recordatorio.text = txt;
            }
            
            return; // ⛔ NO CONTINUAR si no tiene el balde
        }
        
        // ============== SI LLEGÓ AQUÍ, TIENE EL BALDE - PROCEDER ==============
        
        if (mostrarLogs)
        {
            Debug.Log("[GetWater] 💧 ¡Recogiendo agua del lago!");
        }
        
        // Obtener textos traducidos
        string txt0 = lm != null ? lm.ObtenerTexto("recordatorios.help_fogata_0") : "Rápido, corre a la fogata y apágala!";
        string txt1 = lm != null ? lm.ObtenerTexto("recordatorios.help_fogata_1") : "No hay tiempo que perder! Apaga la fogata ahora que tienes el agua!";

        // **IMPORTANTE**: Activar el GameObject del agua pero ocultarlo visualmente
        if (agua != null)
        {
            agua.SetActive(true);
            OcultarAguaVisualmente();
        }
        
        // Ocultar panel del balde (ya no se necesita)
        if (panelbalde != null)
        {
            panelbalde.SetActive(false);
        }
        
        // Actualizar recordatorio
        if (recordatorio != null)
        {
            recordatorio.text = txt0;
        }
        
        // Mostrar panel de agua (indicador de que tiene agua)
        if (panelwater != null)
        {
            panelwater.SetActive(true);
        }
        
        // Actualizar diálogo pendiente
        if (pendienteGO != null)
        {
            DialogueTrigger trigger = pendienteGO.GetComponent<DialogueTrigger>();
            if (trigger != null && trigger.dialogue != null && trigger.dialogue.sentences != null && trigger.dialogue.sentences.Length > 0)
            {
                trigger.dialogue.sentences[0] = txt1;
            }
        }
        
        // Marcar como recolectado
        alreadyCollected = true;
        
        if (mostrarLogs)
        {
            Debug.Log("[GetWater] ✅ Agua recogida exitosamente. GameObject activo pero invisible.");
        }
    }
    
    /// <summary>
    /// Oculta el agua visualmente pero mantiene el GameObject activo
    /// </summary>
    private void OcultarAguaVisualmente()
    {
        if (agua == null)
        {
            if (mostrarLogs)
            {
                Debug.LogWarning("[GetWater] ⚠️ No se ha asignado el GameObject 'agua' en el Inspector");
            }
            return;
        }
        
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

        if (mostrarLogs)
        {
            Debug.Log("[GetWater] 🙈 Agua ocultada visualmente (renderers desactivados)");
        }
    }
    
    // ============== MÉTODOS PÚBLICOS PARA COMPATIBILIDAD ==============
    
    /// <summary>
    /// Método público para llamar desde otros scripts (mantiene compatibilidad)
    /// </summary>
    public void recogerAgua()
    {
        if (!alreadyCollected)
        {
            RecogerAgua();
        }
    }
    
    /// <summary>
    /// Resetea el estado para permitir recoger agua nuevamente
    /// </summary>
    public void ResetearEstado()
    {
        alreadyCollected = false;
        
        if (mostrarLogs)
        {
            Debug.Log("[GetWater] 🔄 Estado reseteado. Se puede recoger agua nuevamente.");
        }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Dibuja el área del trigger en el editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = alreadyCollected ? Color.green : Color.cyan;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan transparente
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
    
    /// <summary>
    /// Menú de contexto para forzar diagnóstico manual en el editor
    /// </summary>
    [ContextMenu("Realizar Diagnóstico Completo")]
    private void ForzarDiagnostico()
    {
        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            RealizarDiagnosticoCompleto();
        }
        else
        {
            Debug.LogError("[GetWater] No se encontró el jugador para hacer diagnóstico");
        }
    }
    #endif
}