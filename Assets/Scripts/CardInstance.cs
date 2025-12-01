using UnityEngine;

/// <summary>
/// 战斗中的一张“具体卡牌”
/// 由 CardData 生成的运行时副本，数字可以被修改（Token / Buff）
/// </summary>
[System.Serializable]
public class CardInstance
{
    public CardData template; // 指向这张牌的模板 ScriptableObject

    // 可变数值（从模板拷贝一份出来）
    public int cost;
    public int damage;
    public int hitCount;
    public int healAmount;   // 回复量

    // ?? 新增：弃牌数 & 抽牌数（可被 Token 改）
    public int discardCount;
    public int drawCount;

    public CardInstance(CardData template)
    {
        this.template = template;

        cost = template.cost;
        damage = template.damage;
        hitCount = template.hitCount;
        healAmount = template.healAmount;   // 从模板拷贝

        // ?? 新增：从模板拷贝弃牌 / 抽牌数
        discardCount = template.discardCount;
        drawCount = template.drawCount;
    }
}
