using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Slot : MonoBehaviour, IDropHandler
{
    public Database database;
    public Image itemImage;
    public Text amountText;
    public Text speciesNameText; // Nuevo: para mostrar el nombre de la especie
    public Text message;
    public GameObject feedback;

    public SlotInfo slotInfo;

    public void SetUp(int id)
    {
        slotInfo = new SlotInfo();
        slotInfo.id = id;
        slotInfo.EmptySlot();
    }

    public void UpdateUI()
    {
        
        if (slotInfo.isEmpty)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
            
            // Ocultar nombre de especie si el slot está vacío
            if (speciesNameText != null)
            {
                speciesNameText.gameObject.SetActive(false);
            }
        }
        else
        {
            Item item = database.FindItemInDatabase(slotInfo.itemId);
            itemImage.sprite = item.itemImage;
            itemImage.enabled = true;
            
            // Mostrar nombre de la especie si es una semilla
            if (speciesNameText != null && item.itemType == Item.ItemType.SEMILLAS && !string.IsNullOrEmpty(item.name))
            {
                speciesNameText.text = item.name;
                speciesNameText.gameObject.SetActive(true);
            }
            else if (speciesNameText != null)
            {
                speciesNameText.gameObject.SetActive(false);
            }
            
            if (slotInfo.amount > 1)
            {
                amountText.text = slotInfo.amount.ToString();
                amountText.gameObject.SetActive(true);
            }
            else
                amountText.gameObject.SetActive(false);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        DragNDrop DnD = eventData.pointerDrag.GetComponent<DragNDrop>();
        DnD.destinationSlot = this;
    }


}
[System.Serializable]
public class SlotInfo
{
    public int id;
    public bool isEmpty;
    public int itemId;
    public int amount;
    public int maxAmount = 3;

    public void EmptySlot()
    {
        isEmpty = true;
        amount = 0;
        itemId = -1;
    }
}
