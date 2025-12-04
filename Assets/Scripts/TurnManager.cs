using UnityEngine;
using System.Collections;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn,
    Busy
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public TurnState state { get; private set; }
    // 🔢 新增：本回合已经成功打出的牌数量
    public int cardsPlayedThisTurn = 0;

    // 🔔 敌人意图取消请求事件（谁想处理就来订阅）
    public System.Action OnCancelEnemyIntentRequested;
    public HandManager handManager;
    public EnergySystem energySystem;

    [Header("Draw Settings")]
    public int cardsPerTurn = 5;

    public int enemyAttackDamage = 5;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (EnemyAI.Instance != null)
        {
            EnemyAI.Instance.PlanNextIntent();  // 先预告一次
        }
        StartCoroutine(StartPlayerTurn());
    }

    //=======================
    //  玩家回合开始
    //=======================
    public IEnumerator StartPlayerTurn()
    {
        state = TurnState.Busy;
        Debug.Log("==== 玩家回合开始 ====");
        // 🌟 每个玩家回合开始时把出牌数清零
        cardsPlayedThisTurn = 0;
        // 恢复能量
        energySystem.StartTurn();

        // 抽牌
        yield return new WaitForSeconds(0.2f);
        handManager.DrawMultiple(cardsPerTurn);

        state = TurnState.PlayerTurn;
        Debug.Log("玩家可以行动了");
    }

    //=======================
    //  玩家点击结束回合
    //=======================
    public void EndPlayerTurn()
    {
        if (state != TurnState.PlayerTurn)
            return;

        Debug.Log("玩家结束回合");

        // 回合结束时衰减力量 buff
        if (PlayerBuffManager.Instance != null)
        {
            PlayerBuffManager.Instance.OnEndPlayerTurn();
        }

        // 回合结束时丢弃所有手牌
        handManager.DiscardHand();

        StartCoroutine(EnemyTurn());
        // 玩家按下结束回合按钮，或你的 EndPlayerTurn() 里：
        //if (EnemyAI.Instance != null)
        //{
        //    EnemyAI.Instance.HandleTrigger(EnemyIntentTriggerTiming.OnPlayerTurnEnd);
        //}

    }

    //=======================
    //  敌人回合
    //=======================
    private IEnumerator EnemyTurn()
    {
        state = TurnState.EnemyTurn;
        Debug.Log("---- 敌人回合 ----");

        // 1. 执行已经预告好的行为
        if (EnemyAI.Instance != null)
        {
            EnemyAI.Instance.ExecutePlannedIntent();
        }

        yield return new WaitForSeconds(0.6f);

        // 这里可以判断玩家是否死亡等...

        // 2. 敌人回合结束前，预告下一回合的行为
        if (EnemyAI.Instance != null && PlayerHealth.Instance.currentHP > 0)
        {
            EnemyAI.Instance.PlanNextIntent();
        }

        // 3. 切回玩家回合，此时 UI 上已经是“下一回合敌人要干嘛”
        StartCoroutine(StartPlayerTurn());
    }
    //=======================
    //  是否允许玩家打牌？
    //=======================
    public bool CanPlayerAct()
    {
        return state == TurnState.PlayerTurn;
    }
    public void RegisterCardPlayed(CardInstance card)
    {
        cardsPlayedThisTurn++;

        if (card != null && card.template != null && card.template.cancelEnemyIntentIfOver4)
        {
            if (cardsPlayedThisTurn > 4)
            {
                Debug.Log("[TurnManager] 触发：取消本回合敌人的一个意图");

                if (EnemyIntentManager.Instance != null)
                {
                    EnemyIntentManager.Instance.CancelOneIntent();
                }
                else
                {
                    Debug.LogWarning("[TurnManager] 没有 EnemyIntentManager，无法取消意图");
                }
            }
            else
            {
                Debug.Log("[TurnManager] 出牌数未超过 4，取消意图效果不触发");
            }
        }
    }

    public void RequestCancelOneEnemyIntent()
    {
        if (OnCancelEnemyIntentRequested != null)
        {
            OnCancelEnemyIntentRequested.Invoke();
        }
        else
        {
            Debug.LogWarning("[TurnManager] 请求取消敌人意图，但没有任何系统订阅 OnCancelEnemyIntentRequested 事件");
        }
    }


}
