using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// Script auxiliar que gestiona la apertura de la mochila cuando se presiona M
/// y coordina con el TrashClassificationController para activar el modo clasificación
/// </summary>
public class TrashClassificationHelper : MonoBehaviour
{
    private TrashClassificationController trashController;
    private Inventory inventory;
    private ShowMochila showMochila;

    private void Start()
    {
        trashController = FindObjectOfType<TrashClassificationController>();
        inventory = FindObjectOfType<Inventory>();
        showMochila = FindObjectOfType<ShowMochila>();

        if (trashController == null)
            Debug.LogError("[TrashClassificationHelper] TrashClassificationController no encontrado");
        if (inventory == null)
            Debug.LogError("[TrashClassificationHelper] Inventory no encontrado");
        if (showMochila == null)
            Debug.LogError("[TrashClassificationHelper] ShowMochila no encontrado");
    }

    private void Update()
    {
        // Detectar cuando se presiona M para abrir la mochila
        if (Input.GetKeyDown(KeyCode.M))
        {
            OnMochilaOpened();
        }
    }

    /// <summary>
    /// Se ejecuta cuando se abre la mochila (presionar M)
    /// Intenta activar el modo clasificación si se cumplen todas las condiciones
    /// </summary>
    private void OnMochilaOpened()
    {
        if (trashController == null || inventory == null)
            return;

        // Verificar si el jugador está cerca de los tachos
        if (!trashController.IsNearTrashZone)
        {
            Debug.Log("[TrashClassificationHelper] Jugador no está cerca de los tachos");
            return;
        }

        // Verificar si todos los desechos fueron recogidos
        bool allTrashCollected = VerifyAllTrashCollected();
        if (!allTrashCollected)
        {
            Debug.Log("[TrashClassificationHelper] No todos los desechos han sido recogidos");
            return;
        }

        // Intenta activar el modo clasificación
        if (trashController.TryStartTrashClassification(true))
        {
            Debug.Log("[TrashClassificationHelper] ✓ Modo clasificación activado automáticamente");
        }
    }

    /// <summary>
    /// Verifica si todos los desechos (6 items) han sido recogidos (busca en inventario + accesorios)
    /// </summary>
    private bool VerifyAllTrashCollected()
    {
        if (inventory == null)
        {
            Debug.LogError("[TrashClassificationHelper] Inventory es null");
            return false;
        }

        // Obtener Database desde Inventory
        var databaseField = inventory.GetType().GetField("database", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Database database = null;
        if (databaseField != null)
        {
            database = databaseField.GetValue(inventory) as Database;
        }

        if (database == null)
        {
            database = Resources.Load<Database>("Database") ?? 
                      Resources.LoadAll<Database>("")[0];
        }

        if (database == null)
        {
            Debug.LogError("[TrashClassificationHelper] Database no encontrada");
            return false;
        }

        Debug.Log("\n[TrashClassificationHelper] ========== DEBUG DESECHOS ==========");
        
        int trashCount = 0;
        
        // BUSCAR EN INVENTARIO PRINCIPAL
        Debug.Log("[TrashClassificationHelper] Buscando en INVENTARIO PRINCIPAL:");
        var slotList = inventory.GetSlotInfoList();
        
        foreach (SlotInfo slotInfo in slotList)
        {
            if (!slotInfo.isEmpty)
            {
                Item item = database.FindItemInDatabase(slotInfo.itemId);
                if (item != null && item.itemType == Item.ItemType.BASURA)
                {
                    Debug.Log($"  ✓ {item.name} x{slotInfo.amount}");
                    trashCount += slotInfo.amount;
                }
            }
        }
        
        // BUSCAR EN ACCESORIOS
        Debug.Log("[TrashClassificationHelper] Buscando en ACCESORIOS:");
        var accesoriosList = inventory.GetAccesorioSlotInfoList();
        
        foreach (SlotInfo slotInfo in accesoriosList)
        {
            if (!slotInfo.isEmpty)
            {
                Item item = database.FindItemInDatabase(slotInfo.itemId);
                if (item != null && item.itemType == Item.ItemType.BASURA)
                {
                    Debug.Log($"  ✓ {item.name} x{slotInfo.amount}");
                    trashCount += slotInfo.amount;
                }
            }
        }

        Debug.Log($"\n[TrashClassificationHelper] ═══════════════════════════════════");
        Debug.Log($"[TrashClassificationHelper] ✓ TOTAL DE DESECHOS: {trashCount}/6");
        Debug.Log($"[TrashClassificationHelper] ═══════════════════════════════════\n");
        
        return trashCount >= 6;
    }
}
