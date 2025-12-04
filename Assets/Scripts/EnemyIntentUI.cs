using UnityEngine;
using TMPro;

public class EnemyIntentUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text descriptionText;
    public EnemyIntentAmountToken amountToken;   // 伤害数值
    [Tooltip("可选：用来显示剩余蓄力回合数的 token")]
    public EnemyIntentAmountToken chargeTurnsToken; // 蓄力回合数

    private EnemyIntentRuntime runtime;

    // 由 EnemyAI 在生成 UI 时调用
    public void Setup(EnemyIntentRuntime rt)
    {
        runtime = rt;

        if (descriptionText != null && rt.definition != null)
            descriptionText.text = rt.definition.descriptionText;

        if (amountToken != null)
            amountToken.BindRuntime(rt);  // 让 token 知道改的是哪条 runtime

        // 第一次把蓄力回合显示出来
        UpdateChargeTurns(rt.remainingChargeTurns);
    }

    // 如果有地方直接改 runtime.amount，也可以调用这个刷新数字
    public void RefreshAmount()
    {
        if (runtime == null || amountToken == null) return;
        amountToken.SetNumber(runtime.amount);
    }

    // 新增：更新 UI 上的蓄力回合 token
    public void UpdateChargeTurns(int turns)
    {
        if (chargeTurnsToken == null) return;

        // 蓄力结束后，你也可以选择隐藏 token，这里简单显示 0
        chargeTurnsToken.SetNumber(turns);
    }
}
