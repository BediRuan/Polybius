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
        StartCoroutine(StartPlayerTurn());
    }

    //=======================
    //  玩家回合开始
    //=======================
    public IEnumerator StartPlayerTurn()
    {
        state = TurnState.Busy;
        Debug.Log("==== 玩家回合开始 ====");

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

        // 回合结束时丢弃所有手牌
        handManager.DiscardHand();

        StartCoroutine(EnemyTurn());
    }

    //=======================
    //  敌人回合
    //=======================
    private IEnumerator EnemyTurn()
    {
        state = TurnState.EnemyTurn;
        Debug.Log("---- 敌人回合 ----");

        // 稍微等一下，模拟“出招动画”
        yield return new WaitForSeconds(0.8f);

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(enemyAttackDamage);
        }
        else
        {
            Debug.LogWarning("场景中没有 PlayerHealth，敌人攻击无效。");
        }

        yield return new WaitForSeconds(0.6f);

        // 如果玩家已经死了，可以不再开始新回合
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.currentHP <= 0)
        {
            state = TurnState.Busy;
            yield break;
        }

        StartCoroutine(StartPlayerTurn());
    }

    //=======================
    //  是否允许玩家打牌？
    //=======================
    public bool CanPlayerAct()
    {
        return state == TurnState.PlayerTurn;
    }
}
