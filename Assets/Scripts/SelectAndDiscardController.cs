using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectAndDiscardController : MonoBehaviour
{
    public static SelectAndDiscardController Instance { get; private set; }

    [Header("Reference")]
    [SerializeField] private HandManager hand;

    [Header("UI")]
    [SerializeField] private CanvasGroup hintGroup;   // 整个蒙板 + 文字
    [SerializeField] private TMP_Text hintText;       // 提示文字
    [SerializeField] private float fadeDuration = 0.25f;

    private int targetCount;
    private int selectedCount;
    private Action onFinished;

    private readonly List<HandCardView> cardViews = new List<HandCardView>();
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 确保一开始是隐藏的
        if (hintGroup != null)
        {
            hintGroup.alpha = 0f;
            hintGroup.gameObject.SetActive(false);

            // 为了不挡住卡牌点击，建议不拦截射线
            hintGroup.blocksRaycasts = false;
            hintGroup.interactable = false;
        }
    }

    /// <summary>
    /// 进入“选择弃牌”模式
    /// </summary>
    public void BeginSelectAndDiscard(int discardCount, Action callback)
    {
        targetCount = discardCount;
        selectedCount = 0;
        onFinished = callback;

        cardViews.Clear();
        cardViews.AddRange(FindObjectsOfType<HandCardView>());

        foreach (var v in cardViews)
        {
            v.SetSelectable(true);
            v.onClick += OnCardSelected;
        }

        // 更新提示文字 & 淡入
        if (hintGroup != null)
        {
            if (hintText != null)
                hintText.text = $"Select {targetCount} Card(s) to discard";

            hintGroup.gameObject.SetActive(true);
            StartFade(1f); // 淡入到完全可见
        }

        Debug.Log($"[弃牌模式] 请选择 {discardCount} 张牌");
    }

    private void OnCardSelected(HandCardView view)
    {
        if (hand == null)
        {
            Debug.LogError("SelectAndDiscardController 未设置 HandManager 引用");
            return;
        }

        hand.DiscardSpecificCard(view);
        selectedCount++;

        // 更新“还需弃几张”的提示
        if (hintText != null)
        {
            int remaining = targetCount - selectedCount;
            if (remaining > 0)
                hintText.text = $" {remaining} More...";
        }

        if (selectedCount >= targetCount)
            Finish();
    }

    private void Finish()
    {
        foreach (var v in cardViews)
        {
            v.SetSelectable(false);
            v.onClick -= OnCardSelected;
        }

        cardViews.Clear();

        // 淡出蒙板
        if (hintGroup != null)
        {
            StartFade(0f, () =>
            {
                hintGroup.gameObject.SetActive(false);
            });
        }

        Debug.Log("[弃牌模式] 完成");
        onFinished?.Invoke();
    }

    // ============ 淡入淡出逻辑 ============

    private void StartFade(float targetAlpha, Action onComplete = null)
    {
        if (hintGroup == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private System.Collections.IEnumerator FadeRoutine(float targetAlpha, Action onComplete)
    {
        float startAlpha = hintGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            hintGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, k);
            yield return null;
        }

        hintGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}
