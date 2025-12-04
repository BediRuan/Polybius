using UnityEngine;

public enum EnemyIntentTriggerTiming
{
    OnEnemyTurnStart,   // 敌人回合开始触发（你现在的“预告→下回合执行”）
    OnEnemyTurnEnd,
    OnPlayerTurnEnd,    // 玩家回合结束触发
    OnPlayerCardPlayed  // 玩家打出卡牌时触发（以后用）
}

public enum EnemyIntentEffectType
{
    DamagePlayer,       // 对玩家造成伤害
    GainBlock,
    GainPower,
    GainEvasion
    // 之后还可以继续扩展
}

[CreateAssetMenu(fileName = "EnemyIntent", menuName = "Enemy/Intent Definition")]
public class EnemyIntentDefinition : ScriptableObject
{
    [Header("逻辑")]
    public string intentId;                     // 内部用 ID（可选）
    public EnemyIntentEffectType effectType;    // 行为类型
    public EnemyIntentTriggerTiming triggerTiming; // 触发时机

    [Header("数值")]
    public int baseAmount = 5;                  // 原始伤害/增益等

    [Tooltip("是否是蓄力攻击（需要经过若干个敌人回合才会释放）")]
    public bool isChargeAttack = false;

    [Tooltip("需要蓄力的敌人回合数，比如 2 表示经过 2 个敌人回合后才会攻击")]
    public int chargeTurns = 0;
    [Header("UI")]
    public EnemyIntentUI uiPrefab;              // 对应的 UI prefab（就是你现有的那个）
    [TextArea]
    public string descriptionText;              // 这条意图的描述（纯文本，不含数字）
}
