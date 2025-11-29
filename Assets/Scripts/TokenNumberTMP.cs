using UnityEngine;
using TMPro;

public class TokenNumberTMP : TokenNumberBase
{
    public enum TokenTargetType
    {
        PlayerCurrentHP,
        PlayerMaxHP,
        EnemyCurrentHP,
        EnemyMaxHP,
        CurrentEnergy,
        MaxEnergy
    }

    [Header("这个数字代表什么？")]
    public TokenTargetType target;

    // 不需要再 Awake / Update / OnPointerXXX，全部交给基类

    protected override void ApplyToken(int value)
    {
        switch (target)
        {
            case TokenTargetType.PlayerCurrentHP:
                if (PlayerHealth.Instance != null)
                    PlayerHealth.Instance.SetCurrentHP(value);
                break;

            case TokenTargetType.PlayerMaxHP:
                if (PlayerHealth.Instance != null)
                    PlayerHealth.Instance.SetMaxHP(value, clampCurrent: true);
                break;

            case TokenTargetType.EnemyCurrentHP:
                if (EnemyHealth.Instance != null)
                    EnemyHealth.Instance.SetCurrentHPByToken(value);
                break;

            case TokenTargetType.EnemyMaxHP:
                if (EnemyHealth.Instance != null)
                    EnemyHealth.Instance.SetMaxHPByToken(value, clampCurrent: true);
                break;

            case TokenTargetType.CurrentEnergy:
                if (EnergySystem.Instance != null)
                    EnergySystem.Instance.SetCurrentEnergy(value);
                break;

            case TokenTargetType.MaxEnergy:
                if (EnergySystem.Instance != null)
                    EnergySystem.Instance.SetMaxEnergy(value, clampCurrent: true);
                break;
        }

        Debug.Log($"[Token] 应用 Token 数值 {value} 到 {target}");
    }
}
