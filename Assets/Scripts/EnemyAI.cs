using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance { get; private set; }

    [Header("意图定义（ScriptableObjects）")]
    [Tooltip("敌人这一战可以使用的全部意图类型，从这里随机/按规则挑选")]
    public EnemyIntentDefinition[] availableIntents;  // 在 Inspector 里拖多个进来

    [Header("每回合意图数量")]
    [Tooltip("每回合至少生成多少个意图")]
    public int minIntentsPerTurn = 1;

    [Tooltip("每回合最多生成多少个意图")]
    public int maxIntentsPerTurn = 3;

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
    /// 敌人回合结束或战斗开始时：为“下一回合”生成一组意图，
    /// 通过 EnemyIntentManager 来生成 runtime + UI。
    /// </summary>
    public void PlanNextIntent()
    {
        if (EnemyIntentManager.Instance == null)
        {
            Debug.LogWarning("[EnemyAI] 场景里没有 EnemyIntentManager，无法生成多意图");
            return;
        }

        if (availableIntents == null || availableIntents.Length == 0)
        {
            Debug.LogWarning("[EnemyAI] 没有可用的意图定义（availableIntents 为空）");
            // 没有意图就清空一下
            EnemyIntentManager.Instance.SetIntentsForThisTurn(null);
            return;
        }

        int min = Mathf.Max(1, minIntentsPerTurn);
        int max = Mathf.Max(min, maxIntentsPerTurn);

        // 防止比可用意图数量还多
        max = Mathf.Min(max, availableIntents.Length);

        int count = Random.Range(min, max + 1);

        List<EnemyIntentDefinition> chosen = new List<EnemyIntentDefinition>();
        for (int i = 0; i < count; i++)
        {
            // 简单版：完全随机，你之后可以改成“不会重复”“先保底攻击再加 buff”等规则
            var def = availableIntents[Random.Range(0, availableIntents.Length)];
            chosen.Add(def);
        }

        EnemyIntentManager.Instance.SetIntentsForThisTurn(chosen);
    }

    /// <summary>
    /// 敌人回合真正执行行为：让 EnemyIntentManager 去遍历所有 runtime
    /// </summary>
    public void ExecutePlannedIntent()
    {
        if (EnemyIntentManager.Instance == null)
            return;

        EnemyIntentManager.Instance.OnEnemyTurnStart();
    }

    /// <summary>
    /// 在其它触发时机（比如玩家回合结束 / 玩家打牌）时，如果以后需要，
    /// 可以从 EnemyIntentManager.activeIntents 里筛选对应 triggerTiming 的意图执行。
    /// </summary>
    public void HandleTrigger(EnemyIntentTriggerTiming timing)
    {
        if (EnemyIntentManager.Instance == null)
            return;

        // 先找到这一类 timing 的意图并执行
        foreach (var rt in EnemyIntentManager.Instance.activeIntents)
        {
            if (rt == null || rt.definition == null) continue;

            if (rt.definition.triggerTiming == timing)
            {
                rt.ApplyEffect();
            }
        }
    }

    /// <summary>
    /// 可选：如果以后有地方还在调用 EnemyAI.CancelPlannedIntent，
    /// 这里直接转发到 EnemyIntentManager 的多意图取消接口。
    /// </summary>
    public void CancelPlannedIntent()
    {
        if (EnemyIntentManager.Instance != null)
        {
            EnemyIntentManager.Instance.CancelOneIntent();
        }
        else
        {
            Debug.LogWarning("[EnemyAI] 想取消意图，但没有 EnemyIntentManager");
        }
    }
}
