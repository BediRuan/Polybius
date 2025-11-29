using UnityEngine;

public class CardEffectResolver : MonoBehaviour
{
    public void Resolve(CardInstance card)
    {
        if (card == null || card.template == null)
            return;

        // ========= 回合检查（可选，看你是否想移动进来） =========
        if (TurnManager.Instance != null && !TurnManager.Instance.CanPlayerAct())
        {
            Debug.Log("现在不是玩家回合，不能打出卡牌效果！");
            return;
        }

        // ========= 能量检查 =========
        if (EnergySystem.Instance != null)
        {
            bool paid = EnergySystem.Instance.TryPay(card.cost);
            if (!paid)
            {
                Debug.Log($"能量不足，无法打出：{card.template.cardName}");
                return;
            }
        }

        // ========= 伤害效果 =========
        if (card.damage > 0 && EnemyHealth.Instance != null)
        {
            int times = Mathf.Max(1, card.hitCount);   // hitCount=0 时视为打一次
            for (int i = 0; i < times; i++)
            {
                EnemyHealth.Instance.TakeDamage(card.damage);
            }
        }

        // ========= 治疗效果 =========
        if (card.healAmount > 0 && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.Heal(card.healAmount);
        }

        // TODO：以后可以在这里继续加：
        // - 抽牌：HandManager.Instance.DrawMultiple(card.drawCount);
        // - 获得护甲：PlayerArmor.Instance.AddArmor(card.blockAmount);
        // - 给敌人上 debuff：EnemyDebuffSystem.Instance.AddDebuff(...);

        Debug.Log(
            $"[Effect] 解析卡牌：{card.template.cardName} " +
            $"cost={card.cost}, damage={card.damage}x{card.hitCount}, heal={card.healAmount}"
        );
    }
}
