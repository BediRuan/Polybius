using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Febucci.UI;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public abstract class TokenNumberBase : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("可选：单独覆盖全局设置，不填则使用 Resources/TokenVisualSettings")]
    public TokenVisualSettings overrideSettings;

    protected TMP_Text text;
    protected TextAnimator_TMP animator;

    protected Color originalColor;
    protected Vector3 originalScale;

    // 发光相关
    private float originalOutlineWidth;
    private Color originalOutlineColor;

    private bool lastSelecting = false;
    private bool isHovering = false;
    private bool isPopping = false;

    // Shader 属性 ID 用懒加载，避免构造期调用 FindShaderByName
    private static bool s_shaderIdsReady = false;
    private static int s_idOutlineWidth;
    private static int s_idOutlineColor;

    protected virtual void Awake()
    {
        text = GetComponent<TMP_Text>();
        animator = GetComponent<TextAnimator_TMP>();

        originalScale = transform.localScale;

        if (text != null)
        {
            originalColor = text.color;

            // 每个 Text 用自己的材质实例，避免改到 sharedMaterial
            text.fontMaterial = Instantiate(text.fontSharedMaterial);

            // 延迟初始化 Shader IDs
            if (!s_shaderIdsReady)
            {
                ShaderUtilities.GetShaderPropertyIDs();
                s_idOutlineWidth = ShaderUtilities.ID_OutlineWidth;
                s_idOutlineColor = ShaderUtilities.ID_OutlineColor;
                s_shaderIdsReady = true;
            }

            Material mat = text.fontMaterial;
            if (mat.HasProperty(s_idOutlineWidth))
                originalOutlineWidth = mat.GetFloat(s_idOutlineWidth);
            if (mat.HasProperty(s_idOutlineColor))
                originalOutlineColor = mat.GetColor(s_idOutlineColor);
        }
    }

    protected TokenVisualSettings Settings
    {
        get
        {
            if (overrideSettings != null) return overrideSettings;

            // 全局配置：Resources/TokenVisualSettings.asset
            if (_cachedGlobal == null)
            {
                _cachedGlobal = Resources.Load<TokenVisualSettings>("TokenVisualSettings");
                if (_cachedGlobal == null)
                {
                    Debug.LogWarning("找不到 Resources/TokenVisualSettings.asset，将使用默认参数");
                    _cachedGlobal = ScriptableObject.CreateInstance<TokenVisualSettings>();
                }
            }
            return _cachedGlobal;
        }
    }
    private static TokenVisualSettings _cachedGlobal;

    protected virtual bool CanUseToken()
    {
        return TokenManager.Instance != null &&
               TokenManager.Instance.isSelecting;
    }

    protected virtual void Update()
    {
        bool selecting = CanUseToken();
        var s = Settings;

        // 进入 / 退出 token 模式 时：颜色 + 发光 + 缩放复位
        if (selecting != lastSelecting)
        {
            lastSelecting = selecting;

            if (selecting)
            {
                if (text != null)
                    text.color = s.highlightColor;
                ApplyGlow(isHovering);
            }
            else
            {
                if (text != null)
                    text.color = originalColor;

                RestoreGlow();

                if (!isPopping)
                    transform.localScale = originalScale;
            }
        }

        // 呼吸动画（token 模式下）
        if (selecting && s.useBreath && !isPopping)
        {
            float speed = isHovering ? s.breathSpeedHover : s.breathSpeedNormal;
            float t = Time.time * speed;
            float scaleFactor = s.baseScale * (1f + Mathf.Sin(t) * s.breathAmplitude);
            transform.localScale = originalScale * scaleFactor;
        }
    }

    // ---------- 鼠标进入 / 离开 ----------

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanUseToken())
            return;

        isHovering = true;
        var s = Settings;

        ApplyGlow(true);

        if (s.useShakeOnHover && animator != null && text != null)
        {
            string raw = StripTag(text.text, s.shakeTag);
            animator.SetText($"<{s.shakeTag}>{raw}</{s.shakeTag}>");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CanUseToken())
            return;

        isHovering = false;
        var s = Settings;

        ApplyGlow(false);

        if (s.useShakeOnHover && animator != null && text != null)
        {
            string raw = StripTag(text.text, s.shakeTag);
            animator.SetText(raw);
        }
    }

    // ---------- 点击：抽 token + 爆闪 ----------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanUseToken())
            return;

        if (TokenManager.Instance == null)
            return;

        int token = TokenManager.Instance.DrawRandomToken();
        if (token <= 0)
        {
            TokenManager.Instance.EndTokenSelection();
            return;
        }

        // ★ 使用 3D 掉落动画 ★
        TokenDrawController.Instance.PlayTokenDrop(token, (finalValue) =>
        {
            ApplyToken(finalValue); // 动画结束后真正使用
            TokenManager.Instance.EndTokenSelection();
        });
    }


    private IEnumerator ClickPopRoutine()
    {
        isPopping = true;
        var s = Settings;

        Vector3 startScale = transform.localScale;
        Vector3 peakScale = originalScale * s.clickPopScale;

        float half = s.clickPopDuration * 0.5f;
        float t = 0f;

        // 放大
        while (t < half)
        {
            t += Time.deltaTime;
            float k = t / half;
            transform.localScale = Vector3.Lerp(startScale, peakScale, k);
            yield return null;
        }

        // 缩回
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float k = t / half;
            transform.localScale = Vector3.Lerp(peakScale, originalScale, k);
            yield return null;
        }

        isPopping = false;
    }

    // ---------- 发光（Outline） ----------

    private void ApplyGlow(bool hover)
    {
        var s = Settings;
        if (!s.useGlow || text == null || !s_shaderIdsReady) return;

        Material mat = text.fontMaterial;
        if (!mat.HasProperty(s_idOutlineWidth) || !mat.HasProperty(s_idOutlineColor))
            return;

        float width = hover ? s.glowWidthHover : s.glowWidth;
        mat.SetFloat(s_idOutlineWidth, width);
        mat.SetColor(s_idOutlineColor, s.glowColor);
    }

    private void RestoreGlow()
    {
        if (text == null || !s_shaderIdsReady) return;

        Material mat = text.fontMaterial;
        if (!mat.HasProperty(s_idOutlineWidth) || !mat.HasProperty(s_idOutlineColor))
            return;

        mat.SetFloat(s_idOutlineWidth, originalOutlineWidth);
        mat.SetColor(s_idOutlineColor, originalOutlineColor);
    }

    private string StripTag(string t, string tag)
    {
        if (string.IsNullOrEmpty(t) || string.IsNullOrEmpty(tag))
            return t;

        string open = $"<{tag}>";
        string close = $"</{tag}>";
        return t.Replace(open, "").Replace(close, "");
    }

    // 子类只需要关心：拿到 token 数字后怎么用
    protected abstract void ApplyToken(int value);
}
