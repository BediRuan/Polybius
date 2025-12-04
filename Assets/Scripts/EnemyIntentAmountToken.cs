using UnityEngine;
using TMPro;

public class EnemyIntentAmountToken : TokenNumberBase
{
    private EnemyIntentRuntime runtime;

    protected override void Awake()
    {
        base.Awake();
        // 初始显示 0，后面由 UI 调整
        if (text != null)
            text.text = "0";
    }

    public void BindRuntime(EnemyIntentRuntime rt)
    {
        runtime = rt;
        SetNumber(runtime.amount);
    }

    public void SetNumber(int value)
    {
        if (text != null)
            text.text = value.ToString();

        if (runtime != null)
            runtime.amount = value;
    }

    /// <summary>
    /// 当玩家在 token 模式点击这个数字时，会被调用
    /// </summary>
    protected override void ApplyToken(int value)
    {
        // 1. 改显示的数字
        SetNumber(value);

        // 2. 如果你希望点击后有“啪”的放大动画，
        // 可以考虑在基类里开个 protected 协程调用（现在是 private，只能复制一份逻辑过来）。
        // 这里只改数值，不动动画。
    }
}
