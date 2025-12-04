using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerBuffManager : MonoBehaviour
{
    public static PlayerBuffManager Instance { get; private set; }

    [Header("Power Buff（力量）")]
    [Tooltip("当前力量层数")]
    public int powerStacks = 0;

    [Tooltip("力量生效时的伤害倍率")]
    public float powerMultiplier = 1.5f;

    [Header("UI")]
    public Image powerIcon;          // 图标
    public TMP_Text powerStacksText; // 层数文本

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        RefreshPowerUI();
    }

    /// <summary>
    /// 设置力量层数（用于 token 或其它直接设定）
    /// </summary>
    public void SetPowerStacks(int value)
    {
        powerStacks = Mathf.Max(0, value);
        Debug.Log($"[PowerBuff] 直接设置力量层数 = {powerStacks}");
        RefreshPowerUI();
    }

    /// <summary>
    /// 增加力量层数（例如打出力量牌 +4）
    /// </summary>
    public void AddPowerStacks(int stacks)
    {
        if (stacks <= 0) return;

        powerStacks += stacks;
        Debug.Log($"[PowerBuff] 增加 {stacks} 层力量，当前层数 = {powerStacks}");
        RefreshPowerUI();
    }

    /// <summary>
    /// 根据当前力量层数修改伤害。
    /// 规则：只要层数 > 0，伤害 = 1.5 倍（四舍五入为整数）。
    /// </summary>
    public int ModifyDamage(int baseDamage)
    {
        if (baseDamage <= 0) return baseDamage;

        if (powerStacks <= 0)
            return baseDamage;

        float raw = baseDamage * powerMultiplier;
        int result = Mathf.RoundToInt(raw);
        result = Mathf.Max(0, result);

        Debug.Log($"[PowerBuff] 生效：基础 {baseDamage} → {result}（当前层数 {powerStacks}）");
        return result;
    }

    /// <summary>
    /// 每个玩家回合结束时调用，衰减层数：
    /// - 如果当前为 1 → 直接清空
    /// - 否则减少当前的一半（向下取整），至少减 1
    /// </summary>
    public void OnEndPlayerTurn()
    {
        if (powerStacks <= 0)
            return;

        if (powerStacks == 1)
        {
            powerStacks = 0;
            Debug.Log("[PowerBuff] 回合结束：层数为 1，直接消失");
            RefreshPowerUI();
            return;
        }

        int reduce = Mathf.Max(1, powerStacks / 2); // 整数除法：3/2 = 1
        powerStacks -= reduce;
        if (powerStacks < 0) powerStacks = 0;

        Debug.Log($"[PowerBuff] 回合结束：减少 {reduce} 层，剩余 {powerStacks}");
        RefreshPowerUI();
    }

    private void RefreshPowerUI()
    {
        if (powerIcon == null || powerStacksText == null)
            return;

        bool hasBuff = powerStacks > 0;

        powerIcon.enabled = hasBuff;
        powerStacksText.gameObject.SetActive(hasBuff);

        if (hasBuff)
        {
            powerStacksText.text = powerStacks.ToString();
        }
        else
        {
            powerStacksText.text = "";
        }
    }
}
