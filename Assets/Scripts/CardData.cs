using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Card", order = 0)]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string cardName;
    public Sprite artwork;

    [Header("Stats")]
    public int cost;

    // 新增：
    public int damage;      // 单次伤害
    public int hitCount;    // 攻击次数
    public int healAmount;  // ✅ 回复生命量（不是回复卡就填 0）
    [TextArea(2, 5)]
    public string description;

    // 💡 新增：这张卡对应的 prefab（可以是每张卡单独做的变体）
    [Header("Prefab")]
    public CardUI prefab;
}
