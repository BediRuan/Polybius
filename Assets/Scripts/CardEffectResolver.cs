using System.Collections;
using UnityEngine;

public class CardEffectResolver : MonoBehaviour
{
    public HandManager handManager;   // 需要在 Inspector 连到 HandManager

    public bool Resolve(CardInstance card)
    {
        if (card == null || card.template == null)
            return false;

        // --- 回合检查 ---
        if (TurnManager.Instance != null && !TurnManager.Instance.CanPlayerAct())
            return false;

        // --- 能量检查 ---
        if (EnergySystem.Instance != null)
        {
            if (!EnergySystem.Instance.TryPay(card.cost))
                return false;
        }

        // --- 伤害 ---
        int times = Mathf.Max(1, card.hitCount);
        float interval = card.template.hitInterval;   // hitInterval 现在还是模板上的设定

        if (card.damage > 0 && EnemyHealth.Instance != null)
        {
            if (times <= 1 || interval <= 0f)
            {
                for (int i = 0; i < times; i++)
                    EnemyHealth.Instance.TakeDamage(card.damage);
            }
            else
            {
                StartCoroutine(DoMultiHitDamage(card.damage, times, interval));
            }
        }

        // --- 治疗 ---
        if (card.healAmount > 0)
            PlayerHealth.Instance?.Heal(card.healAmount);

        // --- Token 模式 ---
        if (card.template.triggersTokenSelection)
            TokenManager.Instance?.BeginTokenSelection();

        // =========================
        //      弃牌 + 抽牌
        // =========================

        // ? 关键：用实例上的数值，这些可以被 Token 修改
        int discardCount = Mathf.Max(0, card.discardCount);
        int drawCount = Mathf.Max(0, card.drawCount);

        Debug.Log($"[CardEffectResolver] 本次弃 {discardCount} 抽 {drawCount}");

        if (discardCount > 0)
        {
            // 进入选择弃牌模式，由 SelectAndDiscardController 处理
            if (SelectAndDiscardController.Instance != null)
            {
                SelectAndDiscardController.Instance.BeginSelectAndDiscard(
                    discardCount,
                    () =>
                    {
                        // 选择完成后再抽牌
                        if (drawCount > 0)
                            handManager.DrawMultiple(drawCount);
                    });
            }
            else
            {
                Debug.LogWarning("SelectAndDiscardController.Instance 为空，无法进行选择弃牌！");
            }

            return true;
        }
        else
        {
            // 不需要弃牌 → 直接抽牌
            if (drawCount > 0)
                handManager.DrawMultiple(drawCount);
        }

        return true;
    }

    private IEnumerator DoMultiHitDamage(int damage, int times, float interval)
    {
        for (int i = 0; i < times; i++)
        {
            EnemyHealth.Instance.TakeDamage(damage);
            if (i < times - 1)
                yield return new WaitForSeconds(interval);
        }
    }
}
