using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    public int maxHP = 80;
    public int currentHP;

    public event Action<int, int> OnHPChanged; // current, max
    public event Action OnDead;

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

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"玩家受到 {amount} 点伤害，当前 HP：{currentHP}/{maxHP}");
        NotifyChanged();

        if (currentHP <= 0)
        {
            Debug.Log("玩家死亡，游戏结束（这里以后可以弹出失败界面）");
            OnDead?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;

        Debug.Log($"玩家回复 {amount} 点生命，当前 HP：{currentHP}/{maxHP}");
        NotifyChanged();
    }

    /// <summary>
    /// Token 用：直接设置当前 HP，并自动触发事件 & 夹在 0~max 范围内
    /// </summary>
    public void SetCurrentHP(int value)
    {
        currentHP = Mathf.Clamp(value, 0, maxHP);
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
        maxHP = Mathf.Max(1, value); // 至少 1
        if (clampCurrent)
        {
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        }
        Debug.Log($"[Token] 设置玩家最大 HP 为 {maxHP}，当前 HP：{currentHP}");
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnHPChanged?.Invoke(currentHP, maxHP);
    }
}
