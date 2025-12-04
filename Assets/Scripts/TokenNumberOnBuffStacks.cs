using UnityEngine;

public class TokenNumberOnBuffStacks : TokenNumberBase
{
    protected override bool CanUseToken()
    {
        // 没有 buff 管理器就不要用 token
        if (PlayerBuffManager.Instance == null)
            return false;

        return base.CanUseToken();
    }

    protected override void ApplyToken(int newValue)
    {
        if (PlayerBuffManager.Instance == null)
            return;

        newValue = Mathf.Max(0, newValue);

        // 用 token 直接设置当前力量层数
        PlayerBuffManager.Instance.SetPowerStacks(newValue);

        Debug.Log($"[Token] 将当前力量层数改为 {newValue}");
    }
}
