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
    
    // Flag para evitar recolección múltiple
    private bool alreadyCollected = false;
    
    // Verificar que sea trigger
    private void Start()
    {
        // Verificar que el collider sea trigger
        Collider collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning("[GetWater] ⚠️ El Collider NO está marcado como Trigger. Activándolo automáticamente.");
            collider.isTrigger = true;
        }
        
        if (mostrarLogs)
        {
            Debug.Log("[GetWater] ✅ Inicializado. Esperando que el jugador entre al trigger con el balde.");
        }
    }
    
    /// <summary>
    /// Se llama automáticamente cuando el jugador entra al trigger del agua
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Verificar que sea el jugador
        if (!other.CompareTag("Player"))
        {
            return;
        }
        
        // Verificar que no se haya recolectado ya
        if (alreadyCollected)
        {
            if (mostrarLogs)
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
        
        // Verificar que el jugador tenga el balde
        if (panelbalde == null || !panelbalde.activeSelf)
        {
            if (mostrarLogs)
            {
                Debug.Log("[GetWater] ❌ El jugador NO tiene el balde. No se puede recoger agua.");
            }
            
            // Mostrar mensaje de que necesita el balde
            if (recordatorio != null)
            {
                LanguageManager lm = LanguageManager.Instancia;
                string txt = lm != null ? lm.ObtenerTexto("recordatorios.help_fogata_4") : "Aún nos falta conseguir agua! Hay un lago cerca del pozo, sigue buscando!";
                recordatorio.text = txt;
            }
            
            return;
        }
        
        // ✅ TODAS LAS CONDICIONES CUMPLIDAS - RECOGER AGUA AUTOMÁTICAMENTE
        RecogerAgua();
    }
    
    /// <summary>
    /// Lógica principal para recoger agua automáticamente
    /// </summary>
    private void RecogerAgua()
    {
        if (mostrarLogs)
        {
            Debug.Log("[GetWater] 💧 ¡Recogiendo agua del lago!");
        }
        
        // Obtener textos traducidos
        LanguageManager lm = LanguageManager.Instancia;
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
    #endif
}