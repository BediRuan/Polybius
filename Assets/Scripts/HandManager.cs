using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Card Setup")]
    public CardUI cardPrefab;
    public RectTransform handArea;

    [Header("Layout Settings")]
    public float cardSpacing = 200f;
    public float cardAngle = 10f;
    public float cardOffsetY = 10f;

    [Header("Deck Setup")]
    public List<CardData> startingCards = new List<CardData>();

    public RectTransform drawPileAnchor;      // 发牌从这里飞出来
    public RectTransform discardPileAnchor;   // 弃牌飞向这里

    public CardUI discardFlyPrefab;

    public float discardFlyDuration = 0.25f;

    [Header("Runtime Piles")]
    [SerializeField] private List<CardInstance> drawPile = new List<CardInstance>();
    [SerializeField] private List<CardInstance> discardPile = new List<CardInstance>();
    private readonly List<HandCardView> handCards = new List<HandCardView>();
    public IReadOnlyList<CardInstance> DrawPile => drawPile;
    public IReadOnlyList<CardInstance> DiscardPile => discardPile;

    public CardEffectResolver effectResolver;
    private void Start()
    {
        ResetDeck();
    }

    public void ResetDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        foreach (var data in startingCards)
        {
            if (data == null) continue;
            drawPile.Add(new CardInstance(data));
        }

        Shuffle(drawPile);
        Debug.Log($"牌库重置完成。抽牌堆张数：{drawPile.Count}，弃牌堆张数：{discardPile.Count}");
    }

    private void Shuffle(List<CardInstance> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            CardInstance tmp = list[i];
            list[i] = list[rand];
            list[rand] = tmp;
        }
    }

    public void DrawMultiple(int count)
    {
        for (int i = 0; i < count; i++)
        {
            DrawOne();
        }
    }

    public void DrawOne()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                Debug.Log("没有牌可以抽了！");
                return;
            }

            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(drawPile);

            Debug.Log($"从弃牌堆洗牌回抽牌堆，现在有 {drawPile.Count} 张");
        }

        CardInstance top = drawPile[0];
        drawPile.RemoveAt(0);

        AddCardToHand(top);
    }

    public void AddCardToHand(CardInstance card)
    {
        CardUI prefabToUse = card.template.prefab != null ? card.template.prefab : cardPrefab;

        CardUI cardUI = Instantiate(prefabToUse, handArea);
        cardUI.SetCard(card);

        HandCardView view = cardUI.GetComponent<HandCardView>();
        if (view == null)
        {
            Debug.LogError("Card prefab 上缺少 HandCardView 组件！");
            return;
        }

        // ? 发牌动画：先把牌放到发牌堆锚点的位置
        RectTransform cardRect = cardUI.GetComponent<RectTransform>();
        if (drawPileAnchor != null)
        {
            // 注意：前提是 drawPileAnchor 和 handArea 在同一个坐标系下
            cardRect.anchoredPosition = drawPileAnchor.anchoredPosition;
        }

        view.onPlay += OnCardPlayed;
        handCards.Add(view);

        // UpdateLayout 会给每张牌计算弧形上的 deckPosition / deckAngle，
        // HandCardView.Update 里会从当前 anchoredPosition 平滑 Lerp 到目标位置，
        // 所以这张牌会从 drawPileAnchor 的位置滑进手牌弧形。
        UpdateLayout();
    }

    public void DiscardHand()
    {
        foreach (var view in handCards)
        {
            CardUI ui = view.GetComponent<CardUI>();
            if (ui != null && ui.card != null)
            {
                discardPile.Add(ui.card);

                // 每张也给个飞行动画
                PlayDiscardFlyAnimation(ui);
            }

            Destroy(view.gameObject);
        }

        handCards.Clear();
        UpdateLayout();

        Debug.Log($"弃掉整手牌。当前抽牌堆：{drawPile.Count}，弃牌堆：{discardPile.Count}");
    }


    private void OnCardPlayed(HandCardView view)
    {
        CardUI ui = view.GetComponent<CardUI>();
        if (ui == null || ui.card == null)
            return;

        CardInstance card = ui.card;

        bool playedSuccessfully = true;   // 默认当成成功

        if (effectResolver != null)
        {
            playedSuccessfully = effectResolver.Resolve(card);
        }
        else
        {
            Debug.LogWarning("未设置 CardEffectResolver，卡牌效果不会生效。");
        }

        // ❗ 如果失败（比如能量不足），卡牌留在手牌，不进入弃牌堆
        if (!playedSuccessfully)
        {
            Debug.Log($"[HandManager] 卡牌 {card.template.cardName} 使用失败，留在手牌中。");
            // HandCardView 自己应该会把位置插值回弧形上的目标位置
            return;
        }

        // 只有真正成功打出时，才进入弃牌堆并销毁 UI
        discardPile.Add(card);

        if (handCards.Contains(view))
            handCards.Remove(view);

        Destroy(view.gameObject);
        UpdateLayout();
    }



    private void UpdateLayout()
    {
        int count = handCards.Count;
        if (count == 0)
            return;

        float half = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float offset = i - half;

            Vector2 pos = new Vector2(
                offset * cardSpacing,
                offset * offset * -cardOffsetY
            );

            float angle = offset * -cardAngle;

            handCards[i].SetDeckState(pos, angle);
        }
    }
    public void DiscardSpecificCard(HandCardView view)
    {
        CardUI ui = view.GetComponent<CardUI>();
        if (ui == null || ui.card == null)
            return;

        discardPile.Add(ui.card);

        PlayDiscardFlyAnimation(ui);

        if (handCards.Contains(view))
            handCards.Remove(view);

        Destroy(view.gameObject);
        UpdateLayout();
    }


    private void PlayDiscardFlyAnimation(CardUI sourceUI)
    {
        if (discardFlyPrefab == null || discardPileAnchor == null)
            return;

        RectTransform sourceRect = sourceUI.GetComponent<RectTransform>();
        if (sourceRect == null) return;

        // 生成一张预览卡（幽灵）作为视觉动画
        // 注意 parent 用 handArea.parent，这样 discardPileAnchor 和幽灵卡在同一坐标系里：
        Transform parent = handArea.parent != null ? handArea.parent : handArea;

        CardUI ghost = Instantiate(discardFlyPrefab, parent);
        ghost.SetCard(sourceUI.card);

        RectTransform ghostRect = ghost.GetComponent<RectTransform>();
        if (ghostRect == null) return;

        // 初始位置 = 原手牌在 parent 坐标系下的位置
        ghostRect.anchoredPosition = sourceRect.anchoredPosition;

        // 飞行脚本
        var fly = ghost.gameObject.AddComponent<CardFlyToAnchor>();
        fly.Init(sourceRect.anchoredPosition, discardPileAnchor.anchoredPosition, discardFlyDuration);
    }

}
