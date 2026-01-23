using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class TrashZoneTrigger : MonoBehaviour
{
    private TrashClassificationController trashController;
    private FirstPersonController fpsController;
    private bool hasEnteredZone = false;
    private bool hasShownDialogue = false; // Control para mostrar diálogo solo una vez POR SESIÓN
    private int lastFrameDialogueShown = -1; // Para evitar mostrar múltiples veces en el mismo frame
    
    // Alternativa: detección continua por distancia
    private Collider triggerCollider;
    private Transform playerTransform;
    private float detectionRadius = 10f; // Radio ajustado a la mitad

    private void Start()
    {
        // Verificar si hay múltiples instancias de TrashZoneTrigger
        TrashZoneTrigger[] allInstances = FindObjectsOfType<TrashZoneTrigger>();
        Debug.Log($"[TrashZoneTrigger] NÚMERO DE INSTANCIAS: {allInstances.Length}");
        if (allInstances.Length > 1)
        {
            Debug.LogWarning($"[TrashZoneTrigger] ⚠️ HAY {allInstances.Length} INSTANCIAS - ESTO CAUSA QUE EL DIÁLOGO SE MUESTRE MÚLTIPLES VECES");
            // Destruir todas excepto la primera
            for (int i = 1; i < allInstances.Length; i++)
            {
                Debug.Log($"[TrashZoneTrigger] Destruyendo instancia duplicada en: {allInstances[i].gameObject.name}");
                Destroy(allInstances[i].gameObject);
            }
        }
        
        // Obtener referencias
        trashController = FindObjectOfType<TrashClassificationController>();
        fpsController = FindObjectOfType<FirstPersonController>();
        triggerCollider = GetComponent<Collider>();
        
        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            Debug.Log($"[TrashZoneTrigger] Jugador encontrado: {playerObj.name}");
        }

        // Verificar que sea un trigger
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            Debug.Log($"[TrashZoneTrigger] ✓ Collider configurado como trigger en: {gameObject.name}");
        }
        else
        {
            Debug.LogError("[TrashZoneTrigger] ✗ ERROR: No hay Collider en este GameObject");
        }

        if (trashController == null)
        {
            Debug.LogError("[TrashZoneTrigger] ✗ ERROR: TrashClassificationController no encontrado");
        }
        else
        {
            Debug.Log("[TrashZoneTrigger] ✓ TrashClassificationController encontrado");
        }

        if (fpsController == null)
        {
            Debug.LogError("[TrashZoneTrigger] ✗ ERROR: FirstPersonController no encontrado");
        }
        else
        {
            Debug.Log("[TrashZoneTrigger] ✓ FirstPersonController encontrado");
        }

        // Verificar que el jugador tenga Rigidbody
        if (playerObj != null)
        {
            Rigidbody playerRb = playerObj.GetComponent<Rigidbody>();
            if (playerRb == null)
            {
                Debug.LogError("[TrashZoneTrigger] ✗ El jugador NO tiene Rigidbody - esto puede causar que OnTriggerEnter no funcione");
                Debug.LogWarning("[TrashZoneTrigger] Se usará detección por distancia como alternativa");
            }
            else
            {
                Debug.Log("[TrashZoneTrigger] ✓ Jugador tiene Rigidbody");
            }
        }
    }

    private void Update()
    {
        // ALTERNATIVA: Detección continua por distancia (fallback si OnTrigger no funciona)
        // IMPORTANTE: No hacer distance check si el modo de clasificación ya está activo
        if (playerTransform != null && trashController != null && !trashController.IsClassifying)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            // Log de diagnóstico cada 30 frames
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"[TrashZoneTrigger] Distance check: {distance:F2} / {detectionRadius} (En zona: {hasEnteredZone})");
            }
            
            if (distance < detectionRadius)
            {
                // El jugador está cerca
                if (!hasEnteredZone)
                {
                    hasEnteredZone = true;
                    Debug.Log($"[TrashZoneTrigger] ✓✓✓ ¡Jugador detectado por distancia! (distancia: {distance:F2})");
                    
                    if (trashController != null)
                    {
                        trashController.SetNearTrashZone(true);
                        Debug.Log("[TrashZoneTrigger] ✓ Se reportó proximidad a los tachos");
                        
                        // Intentar mostrar diálogo si no ha sido mostrado aún
                        // Solo mostrar UNA VEZ por frame
                        if (!hasShownDialogue && Time.frameCount != lastFrameDialogueShown)
                        {
                            int trashCount = CountTrashItems();
                            Debug.Log($"[TrashZoneTrigger] Items de basura: {trashCount} / 6");
                            
                            if (trashCount >= 6)
                            {
                                Debug.Log("[TrashZoneTrigger] ✓ Condiciones cumplidas - MOSTRANDO DIÁLOGO");
                                ShowTrashClassificationDialogue();
                                hasShownDialogue = true;
                                lastFrameDialogueShown = Time.frameCount;
                            }
                            else
                            {
                                Debug.Log($"[TrashZoneTrigger] ⚠️ No hay suficientes items ({trashCount}/6)");
                            }
                        }
                        else if (hasShownDialogue)
                        {
                            Debug.Log("[TrashZoneTrigger] ℹ️ Diálogo YA FUE MOSTRADO - no se mostrará de nuevo");
                        }
                    }
                }
            }
            else
            {
                // El jugador se fue
                if (hasEnteredZone)
                {
                    // Si está muy lejos, forzar salida del modo clasificación
                    if (distance > detectionRadius + 15f && trashController != null && trashController.IsClassifying)
                    {
                        Debug.Log($"[TrashZoneTrigger] ⚠️ Jugador muy lejos ({distance:F2}m) - FORZANDO SALIDA DEL MODO CLASIFICACIÓN");
                        trashController.ForceResetState();
                        trashController.EndTrashClassification();
                        hasEnteredZone = false;
                    }
                    // NO resetear hasEnteredZone si está clasificando (permite seguir usando drag&drop)
                    else if (trashController != null && !trashController.IsClassifying)
                    {
                        hasEnteredZone = false;
                        Debug.Log($"[TrashZoneTrigger] ✓✓✓ ¡Jugador se alejó! (distancia: {distance:F2})");
                        trashController.SetNearTrashZone(false);
                    }
                    else if (trashController != null && trashController.IsClassifying)
                    {
                        // Mientras está clasificando, mantener isNearTrashZone = true
                        Debug.Log($"[TrashZoneTrigger] ✓ Jugador se alejó pero está CLASIFICANDO - isNearTrashZone se mantiene TRUE");
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TrashZoneTrigger] OnTriggerEnter: {other.gameObject.name} (Tag: {other.tag})");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[TrashZoneTrigger] ✗ El objeto no tiene tag 'Player', es: '{other.tag}'");
            return;
        }

        if (hasEnteredZone)
        {
            Debug.Log("[TrashZoneTrigger] Ya estaba en la zona");
            return;
        }

        hasEnteredZone = true;

        Debug.Log("[TrashZoneTrigger] ✓✓✓ ¡Jugador entró a la zona de tachos por OnTriggerEnter! ✓✓✓");

        if (trashController != null)
        {
            trashController.SetNearTrashZone(true);
            Debug.Log("[TrashZoneTrigger] ✓ Se reportó proximidad a los tachos por OnTriggerEnter");
            // El diálogo se muestra en Update(), no aquí, para evitar duplicados
        }
    }

    /// <summary>
    /// Cuenta cuántos items de basura tiene el jugador
    /// </summary>
    private int CountTrashItems()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        Debug.Log($"[TrashZoneTrigger] CountTrashItems() - Inventory encontrado: {(inventory != null ? "✓ SÍ" : "✗ NO")}");
        
        if (inventory == null)
        {
            Debug.LogError("[TrashZoneTrigger] ✗ NO se encontró Inventory - retornando 0");
            return 0;
        }
        
        // Obtener Database - intentar múltiples formas
        Database database = null;
        
        // Método 1: FindObjectOfType
        database = FindObjectOfType<Database>();
        if (database != null)
        {
            Debug.Log("[TrashZoneTrigger] ✓ Database obtenido con FindObjectOfType");
        }
        
        // Método 2: Desde el Inventory
        if (database == null)
        {
            var inventoryField = inventory.GetType().GetField("database", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inventoryField != null)
            {
                database = (Database)inventoryField.GetValue(inventory);
                if (database != null)
                {
                    Debug.Log("[TrashZoneTrigger] ✓ Database obtenido desde campo privado del Inventory");
                }
            }
        }
        
        if (database == null)
        {
            Debug.LogError("[TrashZoneTrigger] ✗ NO se encontró Database - retornando 0");
            return 0;
        }
            
        int count = 0;
        var slotList = inventory.GetSlotInfoList();
        var accesorioList = inventory.GetAccesorioSlotInfoList();
        
        Debug.Log($"[TrashZoneTrigger] Slots principales: {slotList.Count}, Accesorios: {accesorioList.Count}");
        
        // Contar SOLO items de tipo BASURA en slots principales
        foreach (var slot in slotList)
        {
            if (!slot.isEmpty && slot.itemId >= 0)
            {
                Item item = database.FindItemInDatabase(slot.itemId);
                if (item != null && item.itemType == Item.ItemType.BASURA)
                {
                    Debug.Log($"[TrashZoneTrigger]   ✓ Item BASURA: ID={slot.itemId}, Cantidad={slot.amount}");
                    count += slot.amount;
                }
                else
                {
                    Debug.Log($"[TrashZoneTrigger]   ✗ Item NO es BASURA: ID={slot.itemId}");
                }
            }
        }
        
        // Contar SOLO items de tipo BASURA en accesorios
        foreach (var slot in accesorioList)
        {
            if (!slot.isEmpty && slot.itemId >= 0)
            {
                Item item = database.FindItemInDatabase(slot.itemId);
                if (item != null && item.itemType == Item.ItemType.BASURA)
                {
                    Debug.Log($"[TrashZoneTrigger]   ✓ Accesorio BASURA: ID={slot.itemId}, Cantidad={slot.amount}");
                    count += slot.amount;
                }
                else
                {
                    Debug.Log($"[TrashZoneTrigger]   ✗ Accesorio NO es BASURA: ID={slot.itemId}");
                }
            }
        }
        
        Debug.Log($"[TrashZoneTrigger] *** TOTAL DE ITEMS: {count} ***");
        return count;
    }

    /// <summary>
    /// Muestra el diálogo de clasificación de basura (igual al sistema existente)
    /// </summary>
    private void ShowTrashClassificationDialogue()
    {
        Debug.Log("[TrashZoneTrigger] *** INTENTANDO MOSTRAR DIÁLOGO ***");
        Debug.Log($"[TrashZoneTrigger] DialogueManager.instance: {(DialogueManager.instance != null ? "✓ Disponible" : "✗ NULL")}");
        
        if (DialogueManager.instance == null)
        {
            Debug.LogError("[TrashZoneTrigger] ✗✗✗ DialogueManager.instance no disponible - NO SE MOSTRARÁ DIÁLOGO");
            return;
        }

        Debug.Log("[TrashZoneTrigger] ✓ Mostrando diálogo de clasificación de basura");
        
        // Crear diálogo con traducciones (mismo formato que el resto del juego)
        Dialogue dialogue = new Dialogue();
        dialogue.title = new string[] { "trash_classification.title" };
        dialogue.sentences = new string[] { "trash_classification.sentence" };
        dialogue.sprites = new Sprite[] { null };
        
        // Mostrar diálogo usando DialogueManager (sistema estándar del juego)
        DialogueManager.instance.StartDialogue(dialogue, "", null, 2);
        Debug.Log("[TrashZoneTrigger] ✓✓✓ Diálogo iniciado correctamente");
    }

    private void OnTriggerStay(Collider other)
    {
        // Este callback se ejecuta cada frame mientras hay colisión
        // Útil para verificar que sigue habiendo contacto
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[TrashZoneTrigger] OnTriggerExit: {other.gameObject.name}");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[TrashZoneTrigger] El objeto que sale no tiene tag 'Player'");
            return;
        }

        hasEnteredZone = false;

        Debug.Log("[TrashZoneTrigger] ✓✓✓ ¡Jugador salió de la zona de tachos por OnTriggerExit! ✓✓✓");

        if (trashController != null)
        {
            trashController.SetNearTrashZone(false);
            
            if (trashController.IsClassifying)
            {
                trashController.EndTrashClassification();
                Debug.Log("[TrashZoneTrigger] ✓ Modo clasificación desactivado por proximidad");
            }
        }
    }

    /// <summary>
    /// Método público para resetear el estado de la zona cuando el modo termina
    /// </summary>
    public void ResetZoneState()
    {
        hasEnteredZone = false;
        hasShownDialogue = false; // IMPORTANTE: También resetear el flag del diálogo
        Debug.Log("[TrashZoneTrigger] ✓ Estado de zona reseteado (incluyendo diálogo)");
    }
}
