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

    // 数值相关
    public int damage;      // 单次伤害
    public int hitCount;    // 攻击次数
    public int healAmount;  // 回复生命量（不是治疗卡就填 0）
    [Header("Draw / Discard Settings")]
    

    [TextArea(2, 5)]
    public string description;

    [Header("Prefab")]
    public CardUI prefab;

    // ========== 下面是新加/扩展的部分 ==========

    [Header("Special")]
    [Tooltip("打出后是否进入 Token 选择模式")]
    public bool triggersTokenSelection = false;

    [Header("Multi-Hit Settings")]
    [Tooltip("多段攻击时，每一击之间的间隔（秒）。0 表示没有间隔，一次性打完。")]
    public float hitInterval = 0f;

    [Header("Draw / Discard Settings")]
    [Tooltip("打出这张牌时，玩家需要选择弃掉的手牌数量")]
    public int discardCount = 0;

    [Tooltip("在弃牌后，额外抽的牌数")]
    public int drawCount = 0;

  
}
