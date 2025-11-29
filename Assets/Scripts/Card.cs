using UnityEngine;

[System.Serializable]
public class Card
{
    public string id;
    public int cost;
    public string title;
    public string description;
    public Sprite artwork;

    // 行为由 ScriptableObject 或单独写函数驱动
    public System.Action<Card> onPlay;

    public Card(CardData data)
    {
        id = data.id;
        title = data.cardName;
        cost = data.cost;
        description = data.description;
        artwork = data.artwork;
    }

    public void Play()
    {
        Debug.Log($"Played card: {title} (cost {cost})");

        onPlay?.Invoke(this);   // 调用卡牌实际效果
    }
}
