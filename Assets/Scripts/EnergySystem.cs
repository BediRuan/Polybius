using System;
using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    public static EnergySystem Instance { get; private set; }

    [Header("Energy Settings")]
    public int maxEnergy = 3;

    public int CurrentEnergy { get; private set; }

    public event Action<int, int> OnEnergyChanged; // current, max

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
        StartTurn(); // 简单起见，游戏一开始就当作新回合
    }

    public void StartTurn()
    {
        CurrentEnergy = maxEnergy;
        NotifyChanged();
    }

    public bool CanPay(int cost)
    {
        return cost <= CurrentEnergy;
    }

    public bool TryPay(int cost)
    {
        if (!CanPay(cost))
            return false;

        CurrentEnergy -= cost;
        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Token 用：设置当前能量
    /// </summary>
    public void SetCurrentEnergy(int value)
    {
        CurrentEnergy = Mathf.Clamp(value, 0, maxEnergy);
        Debug.Log($"[Token] 设置当前能量为 {CurrentEnergy}/{maxEnergy}");
        NotifyChanged();
    }

    /// <summary>
    /// Token 用：设置最大能量
    /// </summary>
    public void SetMaxEnergy(int value, bool clampCurrent = true)
    {
        maxEnergy = Mathf.Max(0, value);
        if (clampCurrent)
        {
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
        }
        Debug.Log($"[Token] 设置最大能量为 {maxEnergy}，当前能量 {CurrentEnergy}");
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }
}
