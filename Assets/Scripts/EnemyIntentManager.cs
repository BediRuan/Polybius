using System.Collections.Generic;
using UnityEngine;

public class EnemyIntentManager : MonoBehaviour
{
    public static EnemyIntentManager Instance { get; private set; }

    [Header("UI Parent")]
    public Transform uiParent;   // 用来摆放 EnemyIntentUI 的父节点（比如一个水平 Layout）

    [Header("调试用：这一回合有哪些意图")]
    public List<EnemyIntentRuntime> activeIntents = new List<EnemyIntentRuntime>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 为“下一回合”设置意图：
    /// - 保留所有仍在蓄力中的意图（IsStillCharging == true）
    /// - 清掉已经用完 / 非蓄力的旧意图
    /// - 再追加一批新的 definition → runtime + UI
    /// </summary>
    public void SetIntentsForThisTurn(IEnumerable<EnemyIntentDefinition> defs)
    {
        // 先把仍在蓄力中的意图保留下来
        List<EnemyIntentRuntime> nextIntents = new List<EnemyIntentRuntime>();

        foreach (var rt in activeIntents)
        {
            if (rt == null) continue;

            if (rt.IsStillCharging)
            {
                // 还在蓄力：保留 runtime 和它的 UI
                nextIntents.Add(rt);
            }
            else
            {
                // 不再蓄力（普通意图或刚刚释放完的蓄力攻击）→ 清掉 UI
                if (rt.ui != null)
                    Destroy(rt.ui.gameObject);
            }
        }

        // 在保留的基础上追加新的意图
        if (defs != null)
        {
            foreach (var def in defs)
            {
                if (def == null) continue;

                var rt = new EnemyIntentRuntime(def);
                nextIntents.Add(rt);

                // 用你现有的 AttachUI 来生成 UI
                rt.AttachUI(uiParent);
            }
        }

        activeIntents = nextIntents;

        Debug.Log($"[EnemyIntentManager] 本回合意图数 = {activeIntents.Count}");
    }

    /// <summary>
    /// 敌人回合开始时调用：遍历所有 runtime，让它们根据自己的 triggerTiming / charge 状态执行。
    /// </summary>
    public void OnEnemyTurnStart()
    {
        foreach (var rt in activeIntents)
        {
            rt.OnEnemyTurnStart();
        }
    }

    /// <summary>
    /// 取消一条意图：供卡牌效果调用。
    /// 这里示例：优先取消最后一条未取消的 OnEnemyTurnStart 意图。
    /// </summary>
    public void CancelOneIntent()
    {
        // 从后往前找一条还在场上的意图
        for (int i = activeIntents.Count - 1; i >= 0; i--)
        {
            var rt = activeIntents[i];
            if (rt == null || rt.definition == null) continue;

            // 例如只取消 enemy 回合开始触发的攻击（DamagePlayer）
            if (rt.definition.triggerTiming == EnemyIntentTriggerTiming.OnEnemyTurnStart)
            {
                // 删 UI
                if (rt.ui != null)
                    Destroy(rt.ui.gameObject);

                activeIntents.RemoveAt(i);

                Debug.Log($"[EnemyIntentManager] 已取消一条意图：{rt.definition.effectType}, amount={rt.amount}");
                return;
            }
        }

        Debug.Log("[EnemyIntentManager] 没有找到可以取消的意图");
    }
}
