using System;
using UnityEngine;
using DamageNumbersPro;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("HP")]
    public int maxHP = 80;
    public int currentHP;

    public event Action<int, int> OnHPChanged; // current, max
    public event Action OnDead;

    [Header("Damage / Heal Numbers")]
    public DamageNumberGUI damageNumberPrefab;   // 伤害数字 prefab
    public DamageNumberGUI healNumberPrefab;     // 治疗数字 prefab
    public RectTransform numberParent;           // 一般是主 Canvas 里的一个 RectTransform

    [Tooltip("玩家伤害数字的偏移（相对于 numberParent 的 anchoredPosition）")]
    public Vector2 damageNumberOffset = new Vector2(0, 60);

    [Tooltip("玩家治疗数字的偏移")]
    public Vector2 healNumberOffset = new Vector2(0, 80);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentHP = maxHP;
        NotifyChanged();
    }

    /// <summary>
    /// 承受伤害（正值 = 扣血）
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        int old = currentHP;

        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        int realDealt = old - currentHP;
        if (realDealt > 0)
        {
            ShowDamageNumber(realDealt);
        }

        Debug.Log($"玩家受到 {amount} 点伤害，当前 HP：{currentHP}/{maxHP}");
        NotifyChanged();

        if (currentHP <= 0)
        {
            Debug.Log("玩家死亡，游戏结束（这里以后可以弹出失败界面）");
            OnDead?.Invoke();
        }
    }

    /// <summary>
    /// 回复生命（正值 = 回复）
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int old = currentHP;

        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;

        int healed = currentHP - old;
        if (healed > 0)
        {
            ShowHealNumber(healed);
        }

        Debug.Log($"玩家回复 {amount} 点生命，当前 HP：{currentHP}/{maxHP}");
        NotifyChanged();
    }

    /// <summary>
    /// Token 用：直接设置当前 HP，并自动触发事件 & 数字显示 & 夹在 0~max 范围内
    /// </summary>
    public void SetCurrentHP(int value)
    {
        int old = currentHP;

        currentHP = Mathf.Clamp(value, 0, maxHP);

        int delta = currentHP - old;
        if (delta < 0)
        {
            ShowDamageNumber(-delta);
        }
        else if (delta > 0)
        {
            ShowHealNumber(delta);
        }

        Debug.Log($"[Token] 设置玩家当前 HP 为 {currentHP}/{maxHP}");
        NotifyChanged();

        if (currentHP <= 0)
        {
            OnDead?.Invoke();
        }
    }

    /// <summary>
    /// Token 用：设置最大 HP，可以选择是否把当前 HP 一起夹到新上限
    /// </summary>
    public void SetMaxHP(int value, bool clampCurrent = true)
    {
        int old = currentHP;

        maxHP = Mathf.Max(1, value); // 至少 1
        if (clampCurrent)
        {
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            int delta = currentHP - old;
            if (delta < 0)
            {
                ShowDamageNumber(-delta);
            }
            else if (delta > 0)
            {
                ShowHealNumber(delta);
            }
        }

        Debug.Log($"[Token] 设置玩家最大 HP 为 {maxHP}，当前 HP：{currentHP}");
        NotifyChanged();
    }

    // ========== 内部方法 ==========

    private void NotifyChanged()
    {
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    private void ShowDamageNumber(int amount)
    {
        if (amount <= 0) return;
        if (damageNumberPrefab == null)
        {
            Debug.LogWarning("[PlayerDamageNumber] damageNumberPrefab 未绑定");
            return;
        }
        if (numberParent == null)
        {
            Debug.LogWarning("[PlayerDamageNumber] numberParent 未绑定");
            return;
        }

        Vector2 anchoredPos = damageNumberOffset;
        damageNumberPrefab.SpawnGUI(numberParent, anchoredPos, amount);
    }

    private void ShowHealNumber(int amount)
    {
        if (amount <= 0) return;
        if (healNumberPrefab == null)
        {
            Debug.LogWarning("[PlayerHealNumber] healNumberPrefab 未绑定");
            return;
        }
        if (numberParent == null)
        {
            Debug.LogWarning("[PlayerHealNumber] numberParent 未绑定");
            return;
        }

        Vector2 anchoredPos = healNumberOffset;
        healNumberPrefab.SpawnGUI(numberParent, anchoredPos, amount);
    }
}
