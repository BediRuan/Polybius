using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PileViewer : MonoBehaviour
{
    public enum PileType
    {
        Draw,
        Discard
    }

    [Header("Refs")]
    public HandManager handManager;
    public RectTransform cardsRoot;     // 用来摆卡的容器
    public CardUI previewCardPrefab;    // 只读预览用的卡牌 prefab
    public TMP_Text titleText;          // 显示“抽牌堆 / 弃牌堆”的标题（可选）

    [Header("Layout")]
    //public float verticalSpacing = 120f;

    private bool isOpen = false;
    private PileType currentType;

    private void Start()
    {
        gameObject.SetActive(false);    // 初始隐藏
    }

    /// <summary>
    /// 按钮：查看抽牌堆
    /// </summary>
    public void ShowDrawPile()
    {
        currentType = PileType.Draw;
        Open();
    }

    /// <summary>
    /// 按钮：查看弃牌堆
    /// </summary>
    public void ShowDiscardPile()
    {
        currentType = PileType.Discard;
        Open();
    }

    public void Close()
    {
        isOpen = false;
        gameObject.SetActive(false);
    }

    private void Open()
    {
        if (handManager == null)
        {
            Debug.LogWarning("PileViewer has no HandManager！");
            return;
        }

        isOpen = true;
        gameObject.SetActive(true);
        RefreshView();
    }

    public void RefreshView()
    {
        ClearChildren(cardsRoot);

        IReadOnlyList<CardInstance> pile = null;
        string title = "";

        switch (currentType)
        {
            case PileType.Draw:
                pile = handManager.DrawPile;
                title = "Draw";
                break;
            case PileType.Discard:
                pile = handManager.DiscardPile;
                title = "Discard";
                break;
        }

        if (titleText != null)
            titleText.text = title;

        if (pile == null || previewCardPrefab == null || cardsRoot == null)
            return;

        for (int i = 0; i < pile.Count; i++)
        {
            var instance = pile[i];
            if (instance == null || instance.template == null) continue;

            CardUI ui = Instantiate(previewCardPrefab, cardsRoot);
            ui.SetCard(instance);

            var btn = ui.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            var view = ui.GetComponent<HandCardView>();
            if (view != null) Destroy(view);

            // ? 不再手动设置 anchoredPosition，交给 GridLayoutGroup 处理
        }
    }


    private void ClearChildren(Transform root)
    {
        if (root == null) return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }
}
