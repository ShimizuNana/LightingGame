using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemType itemType;

    [Header("Placement Console")]
    public bool canPlaceOnPlacementConsole = false;

    [Header("Change Console")]
    public bool canPlaceOnChangeConsole = false;

    [Header("Glass Settings")]
    public bool isColoredGlass = false;
    public LightColor glassColor = LightColor.None;

    [Header("Mirror Settings")]
    public bool isMirror = false;

    [Header("Angle Sprites (0,45,90,135,180,225,270,315)")]
    public Sprite[] angleSprites = new Sprite[8];
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