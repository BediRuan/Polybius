using System.Collections;
using UnityEngine;

public class CardEffectResolver : MonoBehaviour
{
    public HandManager handManager;   // 在 Inspector 连到 HandManager

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
        float interval = card.template.hitInterval;   // 间隔设定仍然从模板

        if (card.damage > 0 && EnemyHealth.Instance != null)
        {
            // 先根据力量 buff 修改每击伤害
            int damagePerHit = GetModifiedDamage(card.damage);

            if (times <= 1 || interval <= 0f)
            {
                for (int i = 0; i < times; i++)
                    EnemyHealth.Instance.TakeDamage(damagePerHit);
            }
            else
            {
                StartCoroutine(DoMultiHitDamage(damagePerHit, times, interval));
            }
        }

        // --- 治疗 ---
        if (card.healAmount > 0)
            PlayerHealth.Instance?.Heal(card.healAmount);

        
        // --- 力量 buff（可以被 token 修改） ---
        if (card.powerStacksToAdd > 0 && PlayerBuffManager.Instance != null)
        {
            PlayerBuffManager.Instance.AddPowerStacks(card.powerStacksToAdd);
        }


        // --- Token 模式 ---
        if (card.template.triggersTokenSelection)
            TokenManager.Instance?.BeginTokenSelection();

        // =========================
        //      弃牌 + 抽牌
        // =========================

        // 关键：用实例上的数值，这些可以被 Token 修改
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

    /// <summary>
    /// 让所有伤害都经过力量 buff 修正
    /// </summary>
    private int GetModifiedDamage(int baseDamage)
    {
        if (PlayerBuffManager.Instance != null)
            return PlayerBuffManager.Instance.ModifyDamage(baseDamage);

        return baseDamage;
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
