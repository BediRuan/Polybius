using UnityEngine;
using TMPro;

public class EnemySegmentTokenTMP : TokenNumberBase
{
    [Header("这是第几条血（从 0 开始）")]
    public int segmentIndex = 0;

    protected override bool CanUseToken()
    {
        // 既要在 Token 模式，也要有 EnemyHealth 实例
        return base.CanUseToken() && EnemyHealth.Instance != null;
    }

    protected override void ApplyToken(int value)
    {
        if (EnemyHealth.Instance == null)
            return;

        EnemyHealth.Instance.SetSegmentHPByToken(segmentIndex, value);
        Debug.Log($"[Token] 设置敌人第 {segmentIndex} 段血为 {value}");
    }
}
