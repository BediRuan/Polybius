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
    //public TMP_Text descriptionText;   // 简单卡可以直接用这一行

    [Header("Optional Number Slots in Description")]
    public TMP_Text damageNumberText;
    public TMP_Text hitCountNumberText;
    public TMP_Text healNumberText;

    // 以后还可以加 blockNumberText、drawCountText 等

    [Header("Data")]
    public CardInstance card;   // ? 运行时实例，而不是 CardData

    [Header("Options")]
    public bool useAutoDescription = true; // true = 用一整句拼出来；false = 不去改 descriptionText（你手动排）

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

        if (artworkImage != null) artworkImage.sprite = data.artwork;
        if (nameText != null) nameText.text = data.cardName;

        if (costText != null)
            costText.text = card.cost.ToString();

        if (damageNumberText != null)
            damageNumberText.text = card.damage.ToString();

        if (hitCountNumberText != null)
            hitCountNumberText.text = card.hitCount.ToString();

        // ? 回复数字
        if (healNumberText != null)
            healNumberText.text = card.healAmount.ToString();
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
