using UnityEngine;
using TMPro;

public class TokenNumberOnCard : TokenNumberBase
{
    public enum NumberType
    {
        CardCost,
        CardDamage,
        CardHitCount,
        CardHeal,
        // 🔵 新增：
        CardDiscardCount,
        CardDrawCount
    }

    public NumberType numberType;

    private CardUI cardUI;

    protected override void Awake()
    {
        base.Awake();
        cardUI = GetComponentInParent<CardUI>();
    }

    protected override bool CanUseToken()
    {
        // 必须是 Token 模式 + 有有效的 CardInstance
        return base.CanUseToken() && cardUI != null && cardUI.card != null;
    }

    protected override void ApplyToken(int newValue)
    {
        var card = cardUI.card;
        if (card == null)
            return;

        // 不允许负数
        newValue = Mathf.Max(0, newValue);

        switch (numberType)
        {
            case NumberType.CardCost:
                card.cost = newValue;
                Debug.Log($"Token 修改：{card.template.cardName} 的费用改为 {newValue}");
                break;

            case NumberType.CardDamage:
                card.damage = newValue;
                Debug.Log($"Token 修改：{card.template.cardName} 的伤害改为 {newValue}");
                break;

            case NumberType.CardHitCount:
                card.hitCount = newValue;
                Debug.Log($"Token 修改：{card.template.cardName} 的次数改为 {newValue}");
                break;

            case NumberType.CardHeal:
                card.healAmount = newValue;
                Debug.Log($"Token 修改：{card.template.cardName} 的治疗量改为 {newValue}");
                break;

            // 🔵 新增：弃牌数
            case NumberType.CardDiscardCount:
                card.discardCount = newValue;
                Debug.Log($"Token 修改：{card.template.cardName} 的弃牌数改为 {newValue}");
                break;

            // 🔵 新增：抽牌数
            case NumberType.CardDrawCount:
                card.drawCount = newValue;
                Debug.Log($"Token 修改：{card.template.cardName} 的抽牌数改为 {newValue}");
                break;
        }

        // 改完数值刷新 UI（会更新所有 TMP 数字）
        cardUI.Refresh();
    }
}
