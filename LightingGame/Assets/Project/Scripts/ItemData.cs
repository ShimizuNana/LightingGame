using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemType itemType;
}

public enum ItemType
{
    Generic,
    Glass,
    Key,
    Quest
}