using UnityEngine;

[CreateAssetMenu(fileName = "TokenVisualSettings", menuName = "Token/Visual Settings")]
public class TokenVisualSettings : ScriptableObject
{
    [Header("高亮颜色")]
    public Color highlightColor = Color.cyan;

    [Header("呼吸动画")]
    public bool useBreath = true;
    public float baseScale = 1f;
    public float breathAmplitude = 0.07f;   // 1 -> 1.07
    public float breathSpeedNormal = 1f;    // 普通呼吸速度
    public float breathSpeedHover = 2f;     // 悬停时呼吸更快

    [Header("点击爆闪")]
    public bool useClickPop = true;
    public float clickPopScale = 1.2f;
    public float clickPopDuration = 0.15f;

    [Header("发光(Outline)")]
    public bool useGlow = true;
    public Color glowColor = Color.cyan;
    public float glowWidth = 0.25f;         // Token 模式下的光晕宽度
    public float glowWidthHover = 0.4f;     // 悬停时更强的发光

    [Header("悬停抖动(TextAnimator)")]
    public bool useShakeOnHover = true;
    public string shakeTag = "shake";
}
