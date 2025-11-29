using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUI : MonoBehaviour
{
    public TMP_Text currentText;
    public TMP_Text maxText;
    public Slider hpSlider;   // ÐÂÔö£ºÑªÌõ

    private bool subscribed = false;

    private void OnDisable()
    {
        if (subscribed && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnHPChanged -= UpdateUI;
            subscribed = false;
        }
    }

    private void Update()
    {
        if (!subscribed && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnHPChanged += UpdateUI;
            subscribed = true;

            UpdateUI(PlayerHealth.Instance.currentHP, PlayerHealth.Instance.maxHP);
        }
    }

    private void UpdateUI(int current, int max)
    {
        if (currentText != null)
            currentText.text = current.ToString();
        if (maxText != null)
            maxText.text = max.ToString();

        if (hpSlider != null)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }
    }
}
