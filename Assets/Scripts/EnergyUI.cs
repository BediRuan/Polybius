using TMPro;
using UnityEngine;

public class EnergyUI : MonoBehaviour
{
    public TMP_Text currentText;
    public TMP_Text maxText;

    private bool subscribed = false;

    private void OnDisable()
    {
        if (subscribed && EnergySystem.Instance != null)
        {
            EnergySystem.Instance.OnEnergyChanged -= UpdateUI;
            subscribed = false;
        }
    }

    private void Update()
    {
        // 懒订阅：只要还没订阅且单例已经存在，就现在订阅 + 刷一遍
        if (!subscribed && EnergySystem.Instance != null)
        {
            EnergySystem.Instance.OnEnergyChanged += UpdateUI;
            subscribed = true;

            UpdateUI(EnergySystem.Instance.CurrentEnergy, EnergySystem.Instance.maxEnergy);
        }
    }

    private void UpdateUI(int current, int max)
    {
        if (currentText != null)
            currentText.text = current.ToString();
        if (maxText != null)
            maxText.text = max.ToString();
    }
}
