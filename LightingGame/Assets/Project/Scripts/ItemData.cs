using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemType itemType;

    [Header("Pedestal Settings")]
    public bool canPlaceOnPlacementConsole = false;

    [Header("Glass Settings")]
    public bool isColoredGlass = false;
    public LightColor glassColor = LightColor.None;

    [Header("Mirror Settings")]
    public bool isMirror = false;
}

public enum ItemType
{
    Generic,
    Glass,
    Mirror,
    Key,
    Quest
}

public enum LightColor
{
    None,
    Red,
    Blue,
    Green,
    Yellow
}