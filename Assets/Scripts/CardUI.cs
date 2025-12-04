using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image artworkImage;
    public TMP_Text nameText;
    public TMP_Text costText;

    [Header("Optional Number Slots in Description")]
    public TMP_Text damageNumberText;
    public TMP_Text hitCountNumberText;
    public TMP_Text healNumberText;

    // 新增：弃牌 / 抽牌数字
    public TMP_Text discardCountNumberText;
    public TMP_Text drawCountNumberText;
    public TMP_Text powerStacksToAddText;

    [Header("Data")]
    public CardInstance card;   // 运行时实例，而不是 CardData

    [Header("Options")]
    public bool useAutoDescription = true;

    public UnityEvent<CardUI> onClick = new UnityEvent<CardUI>();

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(HandleClick);
        }
    }

    /// <summary>
    /// 用一份运行时实例来设置这张牌
    /// </summary>
    public void SetCard(CardInstance instance)
    {
        card = instance;
        if (card == null || card.template == null) return;

        var data = card.template;

        if (artworkImage != null)
            artworkImage.sprite = data.artwork;

        if (nameText != null)
            nameText.text = data.cardName;

        if (costText != null)
            costText.text = card.cost.ToString();

        if (damageNumberText != null)
            damageNumberText.text = card.damage.ToString();

        if (hitCountNumberText != null)
            hitCountNumberText.text = card.hitCount.ToString();

        if (healNumberText != null)
            healNumberText.text = card.healAmount.ToString();

        // 新增：弃牌数 / 抽牌数
        if (discardCountNumberText != null)
            discardCountNumberText.text = card.discardCount.ToString();

        if (drawCountNumberText != null)
            drawCountNumberText.text = card.drawCount.ToString();

        if (powerStacksToAddText != null)
            powerStacksToAddText.text = card.powerStacksToAdd.ToString();
    }

    /// <summary>
    /// 当数字被 Token 修改后，重新刷新 UI
    /// </summary>
    public void Refresh()
    {
        if (card == null || card.template == null) return;
        SetCard(card);
    }

    private void HandleClick()
    {
        onClick.Invoke(this);
    }
}
