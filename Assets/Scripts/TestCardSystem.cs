using UnityEngine;

public class TestCardSystem : MonoBehaviour
{
    public CardData strikeData;

    private void Start()
    {
        // 由数据生成实际卡牌
        Card strike = new Card(strikeData);

        // 设置效果
        strike.onPlay = (card) =>
        {
            Debug.Log("⚔️ Strike：造成 6 点伤害");
        };

        // 使用卡牌
        strike.Play();
    }
}
