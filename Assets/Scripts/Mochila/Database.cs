using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Database")]
public class Database : ScriptableObject
{
    public List<Item> items = new List<Item>();

    public Item FindItemInDatabase(int id)
    {
        foreach (Item item in items)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }

    // Unity callback invoked when the ScriptableObject is edited in the inspector.
    // Se asegura de que no existan IDs repetidos entre los ítems, ya que la lógica
    // del inventario asume que cada elemento usa un identificador único.
    private void OnValidate()
    {
        HashSet<int> seen = new HashSet<int>();
        for (int i = 0; i < items.Count; i++)
        {
            int id = items[i].id;
            if (seen.Contains(id))
            {
                Debug.LogWarning($"[Database] ID duplicado detectado: {id} en item '{items[i].name}'",
                    this);
            }
            else
            {
                seen.Add(id);
            }
        }
    }
}


[System.Serializable]
public class Item
{
    public int id;
    public string name;
    [TextArea(5, 5)]
    public string description;
    public bool isStackable;
    public ItemType itemType;
    public Vector2 scrollPos;
    public Sprite itemImage;
    
    public enum ItemType { SEMILLAS, HERRAMIENTAS, MISION, BASURA }

}

