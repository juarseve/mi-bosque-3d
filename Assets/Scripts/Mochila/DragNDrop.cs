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
            Debug.LogError("[DragNDrop] mainCanvas es null - abortando drag");
            return;
        }

        Debug.Log($"[DragNDrop] Iniciando drag - Canvas: {mainCanvas.name}");

        // Crear ghost image
        draggedImageObject = new GameObject("DraggedItem_Ghost");
        draggedImageObject.layer = LayerMask.NameToLayer("UI");
        
        // Agregar componente Image ANTES de SetParent
        Image ghostImage = draggedImageObject.AddComponent<Image>();
        ghostImage.sprite = myImage.sprite;
        ghostImage.color = new Color(myImage.color.r, myImage.color.g, myImage.color.b, 0.8f);
        ghostImage.raycastTarget = false;
        
        // Configurar RectTransform ANTES de SetParent
        RectTransform ghostRT = draggedImageObject.GetComponent<RectTransform>();
        RectTransform sourceRT = GetComponent<RectTransform>();
        ghostRT.sizeDelta = sourceRT.sizeDelta;
        
        // SetParent WORLDSPACE para Screen Space - Overlay
        ghostRT.SetParent(mainCanvas.transform, true);
        ghostRT.SetAsLastSibling();
        
        // Establecer posición en screen space
        ghostRT.position = eventData.position;
        
        // Escalar
        ghostRT.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        // Ocultar original
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
        
        Debug.Log($"[DragNDrop] Ghost creado en posición: {ghostRT.position}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedImageObject != null)
        {
            RectTransform ghostRT = draggedImageObject.GetComponent<RectTransform>();
            ghostRT.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restaurar visual
        Color c = myImage.color;
        c.a = 1f;
        myImage.color = c;
        myImage.raycastTarget = true;
        
        c = mochila.color;
        c.a = 1f;
        mochila.color = c;
        
        // Destruir ghost
        if (draggedImageObject != null)
        {
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
        // Activar primera persona
        if (!classificationStarted && trashClassificationController != null)
        {
            trashClassificationController.StartTrashClassification();
            classificationStarted = true;
            Debug.Log("[DragNDrop] Modo clasificación activado");
        }

        // Raycast al tacho
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit))
        {
            Debug.Log("[DragNDrop] Raycast no detectó colisión");
            return;
        }

        Debug.Log($"[DragNDrop] Raycast detectó: {hit.collider.name}");

        // Verificar basura correcta
        bool isCorrect = false;
        string messageKey = "";

        if (trash.id == 5 && hit.collider.name == "BotePapelCarton")
        {
            isCorrect = true;
            messageKey = "basura_papel";
        }
        else if (trash.id == 6 && hit.collider.name == "BoteVidrio")
        {
            isCorrect = true;
            messageKey = "basura_vidrio";
        }
        else if (trash.id == 7 && hit.collider.name == "BotePlastico")
        {
            isCorrect = true;
            messageKey = "basura_plastico";
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
}
