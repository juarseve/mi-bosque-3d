using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private Database database; //Referencia a la base de datos
    [SerializeField]
    private GameObject slotPrefab; //Referencia al prefab del slot
    [SerializeField]
    private Transform InventoryPanel; //Referencia al panel de semillas
    [SerializeField]
    private Transform accesoriosInventory; //Referencia al panel de accesorios
    [SerializeField]
    private List<SlotInfo> slotInfoList; // Lista con la informacion de todos los slots (inventario)
    [SerializeField]
    private List<SlotInfo> slotAccesorioList;
    [SerializeField]
    private int capacity; //capacidad de mi inventario

    private string jsonString; // Texto en formato json que usaremos para guardar el inventario.
    public Text message;
    public GameObject feedback;
    public AudioScript audioScript;
    /*private void Start()
    {
        slotInfoList = new List<SlotInfo>();
        if (PlayerPrefs.HasKey("inventory"))
        {
            LoadSavedInventory();
        }
        else
        {
            LoadEmptyInventory();
        }
    }*/


    public void SaveInventory()
    {
        InventoryWrapper inventoryWrapper = new InventoryWrapper();
        inventoryWrapper.slotInfoList = this.slotInfoList;
        //jsonString = JsonUtility.ToJson(inventoryWrapper);
        Player.instance.playerData.inventoryWrapper = inventoryWrapper;
        //Para accesorios
        InventoryWrapper accesoryWrapper = new InventoryWrapper();
        accesoryWrapper.slotInfoList = this.slotAccesorioList;
        Player.instance.playerData.accesoryWrapper = accesoryWrapper;
    }

    public void LoadEmptyInventory()
    {
        //Para semillas
        for (int i = 0; i < capacity; i++)
        {
            GameObject slot = Instantiate<GameObject>(slotPrefab, InventoryPanel);
            Slot newSlot = slot.GetComponent<Slot>();
            newSlot.message=this.message;
            newSlot.feedback=this.feedback;
            newSlot.SetUp(i);
            newSlot.database = database;
            SlotInfo newSlotInfo = newSlot.slotInfo;
            slotInfoList.Add(newSlotInfo);
        }
        //Para accesorios
        for (int i = 0; i < capacity; i++)
        {
            GameObject slot = Instantiate<GameObject>(slotPrefab, accesoriosInventory);
            Slot newSlot = slot.GetComponent<Slot>();
            newSlot.message = this.message;
            newSlot.feedback = this.feedback;
            newSlot.SetUp(i);
            newSlot.database = database;
            SlotInfo newSlotInfo = newSlot.slotInfo;
            slotAccesorioList.Add(newSlotInfo);
        }
    }


    public void LoadSavedInventory(InventoryWrapper inventoryWrapper, InventoryWrapper accesoryWrapper)
    {
        //jsonString = PlayerPrefs.GetString("inventory");
        Debug.Log(jsonString);
        //InventoryWrapper inventoryWrapper = JsonUtility.FromJson<InventoryWrapper>(jsonString);
        this.slotInfoList = inventoryWrapper.slotInfoList;
        for (int i = 0; i < capacity; i++)
        {
            GameObject slot = Instantiate<GameObject>(slotPrefab, InventoryPanel);
            Slot newSlot = slot.GetComponent<Slot>();
            newSlot.message=this.message;
            newSlot.feedback=this.feedback;
            newSlot.SetUp(i);
            newSlot.database = database;
            newSlot.slotInfo = slotInfoList[i];
            newSlot.UpdateUI();
        }

        this.slotAccesorioList = accesoryWrapper.slotInfoList;
        for (int i = 0; i < capacity; i++)
        {
            GameObject slot = Instantiate<GameObject>(slotPrefab, accesoriosInventory);
            Slot newSlot = slot.GetComponent<Slot>();
            newSlot.message = this.message;
            newSlot.feedback = this.feedback;
            newSlot.SetUp(i);
            newSlot.database = database;
            newSlot.slotInfo = slotAccesorioList[i];
            newSlot.UpdateUI();
        }
    }

    public SlotInfo FindItemInInventory(int itemId, List<SlotInfo> slotInfList)
    {
        foreach (SlotInfo slotInfo in slotInfList)
        {
            if (slotInfo.itemId == itemId && !slotInfo.isEmpty)
            {
                return slotInfo;
            }
        }
        return null;
    }

    private SlotInfo FindSuitableSlot(int itemId, List<SlotInfo> slotInfList)
    {
        foreach (SlotInfo slotInfo in slotInfList)
        {
            if (slotInfo.itemId == itemId && slotInfo.amount < slotInfo.maxAmount && !slotInfo.isEmpty && database.FindItemInDatabase(itemId).isStackable)
            {
                return slotInfo;
            }
        }
        foreach (SlotInfo slotInfo in slotInfList)
        {
            if (slotInfo.isEmpty)
            {
                slotInfo.EmptySlot();
                return slotInfo;
            }
        }
        return null;
    }

    private Slot FindSlot(int id)
    {
        return InventoryPanel.GetChild(id).GetComponent<Slot>();
    }

    private Slot FindSlotAcc(int id)
    {
        return accesoriosInventory.GetChild(id).GetComponent<Slot>();
    }

    public void AddItem(int itemId)
    {
        Debug.LogWarning(itemId);
        Item item = database.FindItemInDatabase(itemId); //busco en la base de datos
        if (item != null)
        {
            SlotInfo slotInfo = FindSuitableSlot(itemId, slotInfoList);
            if (slotInfo != null)
            {
                slotInfo.amount++;
                slotInfo.itemId = itemId;
                slotInfo.isEmpty = false;

                FindSlot(slotInfo.id).UpdateUI();
            }
        }
    }

    public void AddItemAcc(int itemId)
    {
        Debug.LogWarning(itemId);
        Item item = database.FindItemInDatabase(itemId); //busco en la base de datos
        if (item != null)
        {
            SlotInfo slotInfo = FindSuitableSlot(itemId, slotAccesorioList);
            if (slotInfo != null)
            {
                slotInfo.amount++;
                slotInfo.itemId = itemId;
                slotInfo.isEmpty = false;

                FindSlotAcc(slotInfo.id).UpdateUI();
            }
        }
    }

    public void RemoveItem(int itemId)
    {
        SlotInfo slotInfo = FindItemInInventory(itemId, slotInfoList);
        
        if (slotInfo != null)
        {
            if (slotInfo.amount == 1)
            {
                slotInfo.EmptySlot();
            }
            else
            {
                slotInfo.amount--;
            }
            FindSlot(slotInfo.id).UpdateUI();
        }
    }
    public void RemoveItem(int itemId, SlotInfo slotInfo, bool inAccesory)
    {
        if (slotInfo != null)
        {
            if (slotInfo.amount == 1)
            {
                slotInfo.EmptySlot();
            }
            else
            {
                slotInfo.amount--;
            }
            if (inAccesory)
            {
                FindSlotAcc(slotInfo.id).UpdateUI();
            }
            else
            {
                FindSlot(slotInfo.id).UpdateUI();
            }
        }
    }

    public void SwapSlotsWrapper(int id_o, int id_d, Transform image_o, Transform image_d, bool isAccesory)
    {
        if (isAccesory)
        {
            SwapSlots(id_o, id_d, image_o, image_d, slotAccesorioList, accesoriosInventory);
        }
        else
        {
            SwapSlots(id_o, id_d, image_o, image_d, slotInfoList, InventoryPanel);
        }
    }


    public void SwapSlots(int id_o, int id_d, Transform image_o, Transform image_d, List<SlotInfo> slotInfoList, Transform InventoryPanel)
    {
        //intercambio las imagenes
        image_o.SetParent(InventoryPanel.GetChild(id_d));
        image_d.SetParent(InventoryPanel.GetChild(id_o));
        image_o.localPosition = Vector3.zero;
        image_d.localPosition = Vector3.zero;

        if (id_o != id_d)
        {
            SlotInfo origin = slotInfoList[id_o];
            SlotInfo destination = slotInfoList[id_d];

            //intercambio en el inventario
            slotInfoList[id_o] = destination;
            slotInfoList[id_o].id = id_o;
            slotInfoList[id_d] = origin;
            slotInfoList[id_d].id = id_d;

            //intercambio en los slots(items) basado en los cambios en el inventario

            Slot originSlot = InventoryPanel.GetChild(id_o).GetComponent<Slot>();
            originSlot.slotInfo = slotInfoList[id_o];
            Slot destinationSlot = InventoryPanel.GetChild(id_d).GetComponent<Slot>();
            destinationSlot.slotInfo = slotInfoList[id_d];

            originSlot.itemImage = image_d.GetComponent<Image>();
            destinationSlot.itemImage = image_o.GetComponent<Image>();

            originSlot.amountText = originSlot.itemImage.transform.GetChild(0).GetComponent<Text>();
            destinationSlot.amountText = destinationSlot.itemImage.transform.GetChild(0).GetComponent<Text>();


        }

    }



    //funciones para hacer pruebas
    [ContextMenu("Test Add - itemId = 1")]
    public void TestAdd()
    {
        AddItem(1);
    }
    [ContextMenu("Test Add F - itemId = ?")]
    public void TestAddF(int id)
    {
        AddItem(id);
    }

    public void TestAddAcc(int id)
    {
        AddItemAcc(id);
    }
    [ContextMenu("Test Add - itemId = 2")]
    public void TestAdd2()
    {
        AddItem(2);
    }
    [ContextMenu("Test Add - itemId = 3")]
    public void TestAdd3()
    {
        AddItem(3);
    }
    [ContextMenu("Test Remove F - itemId = ?")]
    public void TestRemoveF(int id)
    {
        RemoveItem(id);
    }
    [ContextMenu("Test Remove - itemId = 1")]
    public void TestRemove()
    {
        RemoveItem(1);
    }
    [ContextMenu("Test Remove - itemId = 2")]
    public void TestRemove2()
    {
        RemoveItem(2);
    }
    [ContextMenu("Test Remove - itemId = 3")]
    public void TestRemove3()
    {
        RemoveItem(3);
    }
    [ContextMenu("Test Save")]
    public void TestSave()
    {
        SaveInventory();
    }

    public void TestFiltrarMision()
    {
        foreach (SlotInfo x in this.slotInfoList)
        {
            if (!x.isEmpty)
            {
                Item item = database.FindItemInDatabase(x.itemId);
                if (item.itemType != Item.ItemType.MISION)
                {
                    Slot tmp = FindSlot(x.id);
                    tmp.itemImage.enabled = false;
                    tmp.amountText.gameObject.SetActive(false);
                }
                else
                {
                    FindSlot(x.id).UpdateUI();
                }

            }
        }
    }

    public void TestFiltrarBasura()
    {
        foreach (SlotInfo x in this.slotInfoList)
        {
            if (!x.isEmpty)
            {
                Item item = database.FindItemInDatabase(x.itemId);
                if (item.itemType != Item.ItemType.BASURA)
                {
                    Slot tmp = FindSlot(x.id);
                    tmp.itemImage.enabled = false;
                    tmp.amountText.gameObject.SetActive(false);
                }
                else
                {
                    FindSlot(x.id).UpdateUI();
                }

            }
        }
    }


    [ContextMenu("Test Semillas")]
    public void TestFiltrarSemillas()
    {
        foreach (SlotInfo x in this.slotInfoList)
        {
            if (!x.isEmpty)
            {
                Item item = database.FindItemInDatabase(x.itemId);
                if (item.itemType != Item.ItemType.SEMILLAS)
                {
                    Slot tmp = FindSlot(x.id);
                    tmp.itemImage.enabled = false;
                    tmp.amountText.gameObject.SetActive(false);
                }
                else
                {
                    FindSlot(x.id).UpdateUI();
                }


            }
        }
    }
    [ContextMenu("Test Herramientas")]
    public void TestFiltrarHerramientas()
    {
        foreach (SlotInfo x in this.slotInfoList)
        {
            if (!x.isEmpty)
            {
                Item item = database.FindItemInDatabase(x.itemId);
                if (item.itemType != Item.ItemType.HERRAMIENTAS)
                {
                    Slot tmp = FindSlot(x.id);
                    tmp.itemImage.enabled = false;
                    tmp.amountText.gameObject.SetActive(false);
                }
                else
                {
                    FindSlot(x.id).UpdateUI();
                }

            }
        }
    }
    [ContextMenu("Test Todo")]
    public void TestFiltrarAll()
    {
        foreach (SlotInfo x in this.slotInfoList)
        {
            if (!x.isEmpty)
            {
                FindSlot(x.id).UpdateUI();
            }
        }
    }

    public void TimeFarmM(GameObject g){
        Debug.LogWarning(g.name);
        StartCoroutine(TimeFarm(g));
    }

    public void ShowMessageM(string msj){
        StartCoroutine(ShowMessage(msj));
    }
    private IEnumerator TimeFarm(GameObject g)
    {
        Camera.main.GetComponent<ShowMochila>().Continuar();

        //Farmer farmer = areaFarm.GetComponent<Farmer>();
        //message.text = "Sembrando";
        audioScript.reproducir();
        string text = LanguageManager.Instancia.ObtenerTexto("recordatorios.sembrando");
        message.text = text;
        //GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ShowMochila>().Continuar();
        feedback.SetActive(true);
        //GameObject.FindGameObjectWithTag("Bag").transform.GetChild(0).gameObject.SetActive(false);
        //feedback.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        feedback.SetActive(false);
        //feedback.SetActive(false);
        g.GetComponent<Animator>().SetTrigger("Crece");
    }

    public IEnumerator ShowMessage(string msj)
    {
        message.text = msj;
        feedback.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        feedback.SetActive(false);
    }

    // ============================================================
    // MÉTODO PARA AGREGAR TODAS LAS SEMILLAS - USAR CON CUIDADO
    // ============================================================
    
    [ContextMenu("Agregar Todas las Semillas")]
    public void AgregarTodasLasSemillas()
    {
        // Agregar una semilla de cada tipo disponible en el juego
        AddItem(3);  // Teca
        AddItem(4);  // Ceibo
        AddItem(8);  // Bototillo
        AddItem(9);  // Judea
        AddItem(10); // Guayaca (Guayacán)
        AddItem(11); // Jacaranda
        
        Debug.Log("✅ Se agregaron todas las semillas al inventario");
        
        // Mostrar mensaje de confirmación
        ShowMessageM("Todas las semillas agregadas al inventario");
    }
    
}

[System.Serializable]
public struct InventoryWrapper
{
    public List<SlotInfo> slotInfoList;
}



