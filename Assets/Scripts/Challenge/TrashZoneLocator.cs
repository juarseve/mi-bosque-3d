using UnityEngine;

public class TrashZoneLocator : MonoBehaviour
{
    public void Update()
    {
        // Presionar "V" para encontrar GameObjects con "VFX" o "Trash" en el nombre
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("[VFX LOCATOR] Buscando zonas de basura...");
            
            // Buscar por nombre
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("VFX") || obj.name.Contains("vfx") || 
                    obj.name.Contains("Trash") || obj.name.Contains("trash") ||
                    obj.name.Contains("Basura") || obj.name.Contains("basura") ||
                    obj.name.Contains("Zone") || obj.name.Contains("zone"))
                {
                    Debug.Log($"[VFX LOCATOR] Encontrado: {obj.name}");
                    Debug.Log($"  - Posición: {obj.transform.position}");
                    Debug.Log($"  - Parent: {(obj.transform.parent != null ? obj.transform.parent.name : "ROOT")}");
                }
            }
            
            // También buscar todos los ParticleSystem
            ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
            Debug.Log($"[VFX LOCATOR] Total ParticleSystems encontrados: {particles.Length}");
            foreach (ParticleSystem ps in particles)
            {
                Debug.Log($"  - {ps.gameObject.name} en {ps.transform.position}");
            }
        }
    }
}
