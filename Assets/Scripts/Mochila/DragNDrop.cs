using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragNDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ===== REFERENCIAS INVENTARIO =====
    public Inventory inventory;
    public Transform inventoryPanel;
    public Slot mySlot;
    public Slot destinationSlot;
    private Image myImage;
    public Database database;
    private GameObject[] farmeables;
    Ray ray;
    RaycastHit hit;
    public static int basura = 0;
    public static int plantado = 0;

    public GameObject panel;
    private Image mochila;
    public static bool isAccesory = false;
    public GameObject LogroSist;
    public GameObject texto;
    
    // ===== REFERENCIAS CLASIFICACIÓN =====
    private TrashClassificationController trashClassificationController;
    private static bool classificationStarted = false;
    
    // ===== VARIABLES PARA DRAG VISUAL =====
    private GameObject draggedImageObject;
    private Canvas mainCanvas;
    
    // Offset manual para ajustar desfase del ghost
    public float ghostOffsetX = 0f;
    public float ghostOffsetY = 0f;

    private void Start()
    {
        // Obtener referencias básicas
        inventory = FindObjectOfType<Inventory>();
        inventoryPanel = transform.parent.parent;
        myImage = this.GetComponent<Image>();
        farmeables = GameObject.FindGameObjectsWithTag("Farm");
        panel = GameObject.Find("FBTrash");
        
        GameObject mocchilaObj = GameObject.FindGameObjectWithTag("Mochila");
        if (mocchilaObj != null)
        {
            mochila = mocchilaObj.GetComponent<Image>();
        }
        
        LogroSist = GameObject.Find("SistemaLogros");
        trashClassificationController = FindObjectOfType<TrashClassificationController>();
        texto = GameObject.Find("BasuraCajaTexto/Text");
        
        // Obtener Canvas - buscar el Canvas raíz de esta jerarquía
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.isRootCanvas)
            {
                mainCanvas = c;
                break;
            }
        }
        
        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInParent<Canvas>();
        }
        
        Debug.Log($"[DragNDrop] Canvas encontrado: {(mainCanvas != null ? mainCanvas.gameObject.name : "NULL")}");
        Debug.Log($"[DragNDrop] TrashClassificationController: {(trashClassificationController != null ? "FOUND" : "NULL")}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mySlot = transform.parent.GetComponent<Slot>();
        
        if (myImage == null)
        {
            Debug.LogError("[DragNDrop] myImage es null");
            return;
        }
        
        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInParent<Canvas>();
        }
        
        if (mainCanvas == null)
        {
            Debug.LogError("[DragNDrop] mainCanvas es null - abortando drag");
            return;
        }

        Debug.Log($"[DragNDrop] OnBeginDrag iniciado - Canvas: {mainCanvas.name}");
        Debug.Log($"[DragNDrop] Sprite a arrastrar: {myImage.sprite.name}");

        // Crear ghost image
        draggedImageObject = new GameObject("DraggedItem_Ghost");
        draggedImageObject.layer = LayerMask.NameToLayer("Default");
        
        // Agregar componente Image
        Image ghostImage = draggedImageObject.AddComponent<Image>();
        
        // ASEGURAR que el sprite esté asignado
        if (myImage.sprite != null)
        {
            ghostImage.sprite = myImage.sprite;
            Debug.Log($"[DragNDrop] Sprite asignado al ghost: {ghostImage.sprite.name}");
        }
        else
        {
            Debug.LogError("[DragNDrop] ✗ myImage.sprite es NULL");
        }
        
        // Color TOTALMENTE VISIBLE - blanco opaco
        ghostImage.color = new Color(1f, 1f, 1f, 1f);
        ghostImage.raycastTarget = false;
        
        // Configurar RectTransform
        RectTransform ghostRT = draggedImageObject.GetComponent<RectTransform>();
        
        // Usar tamaño absoluto positivo (80x80 píxeles)
        ghostRT.sizeDelta = new Vector2(80f, 80f);
        Debug.Log($"[DragNDrop] Tamaño del ghost fijado: {ghostRT.sizeDelta}");
        
        // Parente en el canvas con worldPositionStays=false para que use coordenadas screen-space
        ghostRT.SetParent(mainCanvas.transform, false);
        ghostRT.SetAsLastSibling();
        
        // Configurar pivot en el centro para que el cursor esté en el centro del ghost
        ghostRT.pivot = new Vector2(0.5f, 0.5f);
        
        // Posición inicial - MÉTODO SIMPLIFICADO para Screen Space - Overlay
        RectTransform canvasRT = mainCanvas.GetComponent<RectTransform>();
        
        Debug.Log($"[DragNDrop] Canvas RenderMode: {mainCanvas.renderMode}");
        Debug.Log($"[DragNDrop] Canvas Size: {canvasRT.sizeDelta}");
        Debug.Log($"[DragNDrop] Mouse Position (eventData.position): {eventData.position}");
        
        // Para Screen Space - Overlay, convertir de screen space a canvas local space
        Vector2 localPos;
        if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Método directo para Overlay: restar el centro del canvas
            localPos = eventData.position - canvasRT.sizeDelta / 2f;
            Debug.Log($"[DragNDrop] Usando método OVERLAY - localPos: {localPos}");
        }
        else
        {
            // Para otros modos, usar el método de Unity
            Camera cam = mainCanvas.renderMode == RenderMode.ScreenSpaceCamera ? mainCanvas.worldCamera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, cam, out localPos);
            Debug.Log($"[DragNDrop] Usando RectTransformUtility - localPos: {localPos}");
        }
        
        ghostRT.anchoredPosition = localPos;
        
        Debug.Log($"[DragNDrop] ✓ Ghost posicionado:");
        Debug.Log($"  - anchoredPosition: {ghostRT.anchoredPosition}");
        Debug.Log($"  - position (world): {ghostRT.position}");
        Debug.Log($"  - localPosition: {ghostRT.localPosition}");
        
        // Escalar para que sea más visible
        ghostRT.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        
        // DEBUGGING: hacer el fondo visible si no tiene sprite
        if (ghostImage.sprite == null)
        {
            ghostImage.color = new Color(1f, 0f, 0f, 1f); // Rojo si no tiene sprite
            Debug.LogWarning("[DragNDrop] ⚠️ No hay sprite, usando color rojo para debug");
        }
        
        // Ocultar original parcialmente
        Color c = myImage.color;
        c.a = 0.3f;
        myImage.color = c;
        myImage.raycastTarget = false;
        
        // Dimming mochila
        if (mochila != null)
        {
            c = mochila.color;
            c.a = 0.5f;
            mochila.color = c;
        }
        
        Debug.Log($"[DragNDrop] ✓ Ghost creado - Posición: {ghostRT.position}, Size: {ghostRT.sizeDelta}, Scale: {ghostRT.localScale}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedImageObject != null)
        {
            RectTransform ghostRT = draggedImageObject.GetComponent<RectTransform>();
            if (ghostRT != null && mainCanvas != null)
            {
                RectTransform canvasRT = mainCanvas.GetComponent<RectTransform>();
                Vector2 localPos;
                
                if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Método directo para Overlay
                    localPos = eventData.position - canvasRT.sizeDelta / 2f;
                }
                else
                {
                    // Para otros modos
                    Camera cam = mainCanvas.renderMode == RenderMode.ScreenSpaceCamera ? mainCanvas.worldCamera : null;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, cam, out localPos);
                }
                
                ghostRT.anchoredPosition = localPos;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[DragNDrop] OnEndDrag llamado");
        
        // Restaurar visual
        Color c = myImage.color;
        c.a = 1f;
        myImage.color = c;
        myImage.raycastTarget = true;
        
        if (mochila != null)
        {
            c = mochila.color;
            c.a = 1f;
            mochila.color = c;
        }
        
        // Destruir ghost
        if (draggedImageObject != null)
        {
            Debug.Log($"[DragNDrop] ✓ Destruyendo ghost image");
            Destroy(draggedImageObject);
        }

        // Procesar el drop
        Item item = database.FindItemInDatabase(mySlot.slotInfo.itemId);
        
        if (item == null) return;

        if (item.itemType == Item.ItemType.SEMILLAS)
        {
            HandleSeedsLogic(item);
        }
        else if (item.itemType == Item.ItemType.BASURA)
        {
            HandleTrashLogic(item);
        }
        else
        {
            // Lógica de inventory swap normal
            HandleInventorySwap();
        }
    }

    private void HandleSeedsLogic(Item seed)
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit)) return;

        CheckAndPlantSeed(seed, hit.collider.name);
    }

    private void CheckAndPlantSeed(Item seed, string colliderName)
    {
        // Lógica simplificada - Puedes expandir con los demás tipos de semillas
        if (seed.id == 3 && colliderName == "Teca_Semilla")
        {
            PlantSeed("Teca_planta", seed, "Semilla2");
        }
        else if (seed.id == 4 && colliderName == "Ceibo_Semilla")
        {
            PlantSeed("Ceibo_planta", seed, "Semilla1");
        }
        else if (seed.id == 8 && colliderName == "Bototillo_Semilla")
        {
            PlantSeed("Bototillo_planta", seed, "Semilla2");
        }
        else if (seed.id == 9 && colliderName == "Judea_Semilla")
        {
            PlantSeed("Judea_planta", seed, "Semilla1");
        }
        else if (seed.id == 10 && colliderName == "Guayacan_Semilla")
        {
            PlantSeed("Guayacan_planta", seed, "Semilla2");
        }
        else if (seed.id == 11 && colliderName == "Jacaranda_Semilla")
        {
            PlantSeed("Jacaranda_planta", seed, "Semilla1");
        }
    }

    private void PlantSeed(string plantName, Item seed, string missionKey)
    {
        foreach (GameObject g in farmeables)
        {
            if (g.name == plantName)
            {
                plantado++;
                inventory.RemoveItem(seed.id, mySlot.slotInfo, false);
                inventory.TimeFarmM(g);
                try
                {
                    LogroSist.GetComponent<LogrosGlobales>().ProgresarMision(5, missionKey);
                    LogroSist.GetComponent<LogrosGlobales>().ProgresarLogro(5);
                }
                catch { }
                break;
            }
        }
    }

    private void HandleTrashLogic(Item trash)
    {
        // Activar modo clasificación SOLO si no está ya activo
        if (!classificationStarted && trashClassificationController != null && !trashClassificationController.IsClassifying)
        {
            // Verificar si todos los desechos fueron recogidos
            bool allTrashCollected = VerifyAllTrashCollected();
            
            // Intentar activar el modo clasificación
            if (trashClassificationController.TryStartTrashClassification(allTrashCollected))
            {
                classificationStarted = true;
                Debug.Log("[DragNDrop] Modo clasificación activado");
            }
            else
            {
                // No se cumplen las condiciones
                string errorMsg = LanguageManager.Instancia.ObtenerTexto("recordatorios.no_puede_clasificar");
                if (string.IsNullOrEmpty(errorMsg))
                {
                    errorMsg = "No puedes clasificar basura aún. Debes estar cerca de los tachos de basura.";
                }
                inventory.ShowMessageM(errorMsg);
                Debug.LogWarning("[DragNDrop] No se pueden cumplir condiciones para clasificación");
                return;
            }
        }

        // Raycast al tacho - IGNORAR el TrashZoneCollider (trigger)
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        
        RaycastHit correctHit = default;
        bool foundBin = false;
        
        // Buscar el primer collider que NO sea el trigger de la zona de basura
        foreach (RaycastHit h in hits)
        {
            Debug.Log($"[DragNDrop] Raycast detectó: {h.collider.name} (es trigger: {h.collider.isTrigger})");
            
            // Ignorar triggers y el TrashZoneCollider específicamente
            if (h.collider.isTrigger || h.collider.name == "TrashZoneCollider")
            {
                continue;
            }
            
            // Si encontramos un collider que no es trigger, usarlo
            if (!foundBin)
            {
                correctHit = h;
                foundBin = true;
                Debug.Log($"[DragNDrop] ✓ Tacho detectado: {h.collider.name}");
                break;
            }
        }
        
        if (!foundBin)
        {
            Debug.Log("[DragNDrop] ✗ No se detectó ningún tacho (solo se encontró el trigger)");
            return;
        }

        Debug.Log($"[DragNDrop] Validando contra tacho: {correctHit.collider.name}");

        // Verificar basura correcta - mapeo basado en nombres de items y tachos
        bool isCorrect = false;
        string messageKey = "";
        string trashName = trash.name.ToLower();
        string binName = correctHit.collider.name.ToLower();

        Debug.Log($"[DragNDrop] Item: '{trash.name}' ({trash.id}), Tacho: '{correctHit.collider.name}'");
        Debug.Log($"[DragNDrop] Validando: trashName='{trashName}', binName='{binName}'");

        // Papel y Cartón
        if ((trashName.Contains("papel") || trashName.Contains("carton") || trash.id == 5) && 
            (binName.Contains("papel") || binName.Contains("carton")))
        {
            isCorrect = true;
            messageKey = "basura_papel";
            Debug.Log("[DragNDrop] ✓ PAPEL/CARTÓN validado correctamente");
        }
        // Vidrio
        else if ((trashName.Contains("vidrio") || trashName.Contains("botella") || trash.id == 6) && 
                 binName.Contains("vidrio"))
        {
            isCorrect = true;
            messageKey = "basura_vidrio";
            Debug.Log("[DragNDrop] ✓ VIDRIO validado correctamente");
        }
        // Plástico
        else if ((trashName.Contains("plastico") || trashName.Contains("plastic") || trash.id == 7) && 
                 binName.Contains("plastico"))
        {
            isCorrect = true;
            messageKey = "basura_plastico";
            Debug.Log("[DragNDrop] ✓ PLÁSTICO validado correctamente");
        }
        else
        {
            Debug.Log($"[DragNDrop] ✗ No coincide: '{trash.name}' -> '{correctHit.collider.name}'");
        }

        if (isCorrect)
        {
            HandleCorrectTrash(messageKey);
        }
        else
        {
            string badText = LanguageManager.Instancia.ObtenerTexto("recordatorios.basura_mal");
            inventory.ShowMessageM(badText);
            Debug.Log($"[DragNDrop] Basura colocada incorrectamente");
        }
    }

    private void HandleCorrectTrash(string messageKey)
    {
        inventory.RemoveItem(mySlot.slotInfo.itemId, mySlot.slotInfo, true);
        basura += 1;
        
        string text = LanguageManager.Instancia.ObtenerTexto($"recordatorios.{messageKey}");
        inventory.ShowMessageM(text);
        
        string counterText = LanguageManager.Instancia.ObtenerTexto("recordatorios.basura_faltante");
        if (texto != null)
        {
            texto.GetComponent<Text>().text = counterText + (6 - basura);
        }

        if (basura == 6)
        {
            if (texto != null && texto.transform.parent != null)
            {
                GameObject.Destroy(texto.transform.parent.gameObject);
            }
            
            if (trashClassificationController != null)
            {
                trashClassificationController.EndTrashClassification();
                classificationStarted = false;
                Debug.Log("[DragNDrop] ¡Clasificación completada!");
            }
        }
    }

    private void HandleInventorySwap()
    {
        Color color = mochila.color;
        color.a = 1f;
        mochila.color = color;
        
        if (destinationSlot != null)
        {
            if (destinationSlot.slotInfo.id != mySlot.slotInfo.id)
            {
                inventory.SwapSlotsWrapper(mySlot.slotInfo.id, destinationSlot.slotInfo.id, this.transform, destinationSlot.itemImage.transform, isAccesory);
                destinationSlot.itemImage.transform.localPosition = Vector3.zero;
            }
            else
            {
                inventory.SwapSlotsWrapper(mySlot.slotInfo.id, mySlot.slotInfo.id, this.transform, mySlot.itemImage.transform, isAccesory);
            }
        }
        else
        {
            inventory.SwapSlotsWrapper(mySlot.slotInfo.id, mySlot.slotInfo.id, this.transform, mySlot.itemImage.transform, isAccesory);
        }

        destinationSlot = null;
    }

    /// <summary>
    /// Verifica si todos los desechos (6 items de basura) han sido recogidos del inventario + accesorios
    /// </summary>
    private bool VerifyAllTrashCollected()
    {
        int trashCount = 0;
        
        // Contar en INVENTARIO PRINCIPAL
        foreach (SlotInfo slotInfo in inventory.GetSlotInfoList())
        {
            if (!slotInfo.isEmpty && database != null)
            {
                Item item = database.FindItemInDatabase(slotInfo.itemId);
                if (item != null && item.itemType == Item.ItemType.BASURA)
                {
                    trashCount += slotInfo.amount;
                }
            }
        }
        
        // Contar en ACCESORIOS
        foreach (SlotInfo slotInfo in inventory.GetAccesorioSlotInfoList())
        {
            if (!slotInfo.isEmpty && database != null)
            {
                Item item = database.FindItemInDatabase(slotInfo.itemId);
                if (item != null && item.itemType == Item.ItemType.BASURA)
                {
                    trashCount += slotInfo.amount;
                }
            }
        }

        Debug.Log($"[DragNDrop] Total de desechos en inventario + accesorios: {trashCount}");
        return trashCount >= 6;
    }
}
