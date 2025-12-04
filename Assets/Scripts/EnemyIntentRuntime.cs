using UnityEngine;

[System.Serializable]
public class EnemyIntentRuntime
{
    public EnemyIntentDefinition definition;
    public int amount;              // 实际数值，可以被 token 改
    public EnemyIntentUI ui;        // 运行时生成的 UI 实例

    // 新增：剩余需要蓄力的敌人回合数
    public int remainingChargeTurns;

    public EnemyIntentRuntime(EnemyIntentDefinition def)
    {
        definition = def;
        if (definition == null)
        {
            Debug.LogError("EnemyIntentRuntime: definition 为空！");
            return;
        }

        amount = def.baseAmount;
        remainingChargeTurns = def.isChargeAttack ? Mathf.Max(0, def.chargeTurns) : 0;
    }

    public void AttachUI(Transform parent)
    {
        if (definition == null || definition.uiPrefab == null || parent == null)
            return;

        ui = Object.Instantiate(definition.uiPrefab, parent);
        ui.Setup(this);

        // 如果 UI 里你想显示“剩余蓄力回合”，可以在这里第一次刷新
        if (ui != null)
        {
            ui.UpdateChargeTurns(remainingChargeTurns);
        }
    }

    /// <summary>
    /// 是否仍处于蓄力中（用于 EnemyAI 判断要不要继续 Plan）
    /// </summary>
    public bool IsStillCharging
    {
        get
        {
            return definition != null &&
                   definition.isChargeAttack &&
                   remainingChargeTurns > 0;
        }
    }

    /// <summary>
    /// 在“敌人回合开始”时调用（由 EnemyAI 调）
    /// 负责处理蓄力倒计时和最终释放。
    /// </summary>
    public void OnEnemyTurnStart()
    {
        if (definition == null)
            return;

        if (definition.triggerTiming != EnemyIntentTriggerTiming.OnEnemyTurnStart)
            return;

        if (definition.isChargeAttack && remainingChargeTurns > 0)
        {
            // 仍在蓄力：先减少 1 回合
            remainingChargeTurns--;

            // 更新 UI 上的蓄力回合显示（如果你有做）
            if (ui != null)
            {
                ui.UpdateChargeTurns(remainingChargeTurns);
            }

            // 还没蓄满，不释放
            if (remainingChargeTurns > 0)
            {
                Debug.Log($"蓄力中，还需要 {remainingChargeTurns} 回合");
                return;
            }

            Debug.Log("蓄力完成，释放攻击！");
            ApplyEffect();
            return;
        }

        // 普通 OnEnemyTurnStart 行为，直接生效
        ApplyEffect();
    }

    public void ApplyEffect()
    {
        if (definition == null) return;

        switch (definition.effectType)
        {
            case EnemyIntentEffectType.DamagePlayer:
                if (PlayerHealth.Instance != null)
                    PlayerHealth.Instance.TakeDamage(amount);
                break;

            case EnemyIntentEffectType.GainBlock:
                // 敌人获得格挡
                break;

            case EnemyIntentEffectType.GainPower:
                // 敌人获得力量
                break;

            case EnemyIntentEffectType.GainEvasion:
                // 敌人获得闪避
                break;
        }
    }
}
