using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// Script de DEBUG para identificar exactamente por qué el trigger no funciona
/// </summary>
public class TrashZoneTriggerDebug : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("========== TRASH ZONE TRIGGER DEBUG ==========");
        
        // 1. Verificar ESTE GameObject
        Debug.Log("\n📌 VERIFICANDO TRASH ZONE TRIGGER:");
        DebugGameObject(gameObject, "TrashZoneTrigger");
        
        // 2. Buscar al jugador
        Debug.Log("\n📌 BUSCANDO AL JUGADOR:");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("✗ NO ENCONTRÉ JUGADOR CON TAG 'Player'");
            Debug.Log("Buscando por nombre...");
            
            // Intentar encontrar por nombre
            player = GameObject.Find("FPSController");
            if (player == null) player = GameObject.Find("Player");
            if (player == null) player = GameObject.Find("FirstPersonCharacter");
            
            if (player != null)
            {
                Debug.Log($"✓ Encontré jugador por nombre: {player.name}");
                Debug.Log($"  Tag actual: '{player.tag}'");
                Debug.LogWarning($"  ⚠️ PROBLEMA: El jugador NO tiene tag 'Player', tiene '{player.tag}'");
            }
            else
            {
                Debug.LogError("✗ NO ENCONTRÉ AL JUGADOR EN ABSOLUTO");
                return;
            }
        }
        else
        {
            Debug.Log($"✓ Encontré jugador con tag 'Player': {player.name}");
        }
        
        // 3. Verificar componentes del jugador
        DebugGameObject(player, "Player/Jugador");
        
        // 4. Verificar distancia
        Debug.Log("\n📌 DISTANCIA:");
        float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);
        Debug.Log($"Distancia entre trigger y jugador: {distance:F2} unidades");
        
        if (distance > 50f)
        {
            Debug.LogWarning("⚠️ El jugador está muy lejos del trigger (>50 unidades)");
        }
        
        Debug.Log("\n========== FIN DEBUG ==========\n");
    }
    
    private void DebugGameObject(GameObject go, string name)
    {
        Debug.Log($"\n🔍 Verificando: {name}");
        Debug.Log($"  Nombre: {go.name}");
        Debug.Log($"  Activo: {go.activeInHierarchy}");
        Debug.Log($"  Tag: '{go.tag}'");
        
        // Collider
        Collider col = go.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"  ✗ NO TIENE COLLIDER");
        }
        else
        {
            Debug.Log($"  ✓ Collider: {col.GetType().Name}");
            Debug.Log($"    - Is Trigger: {col.isTrigger}");
            Debug.Log($"    - Habilitado: {col.enabled}");
        }
        
        // Rigidbody
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"  ✗ NO TIENE RIGIDBODY - ¡ESTE ES EL PROBLEMA!");
            Debug.LogWarning($"  → Agrega un Rigidbody a este GameObject");
            Debug.LogWarning($"  → Para trigger: Body Type=Dynamic, Is Kinematic=true, Use Gravity=false");
        }
        else
        {
            Debug.Log($"  ✓ Rigidbody:");
            Debug.Log($"    - Body Type: {rb.constraints}");
            Debug.Log($"    - Is Kinematic: {rb.isKinematic}");
            Debug.Log($"    - Use Gravity: {rb.useGravity}");
        }
        
        // Script
        if (go.GetComponent<TrashZoneTrigger>() != null)
        {
            Debug.Log($"  ✓ Script TrashZoneTrigger: presente");
        }
        else if (go.GetComponent<FirstPersonController>() != null)
        {
            Debug.Log($"  ✓ Script FirstPersonController: presente");
        }
    }
}
