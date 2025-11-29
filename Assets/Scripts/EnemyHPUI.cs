using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPUI : MonoBehaviour
{
    [Header("多条血条 UI")]
    public Slider[] segmentSliders;        // 每条血一个 Slider
    public TMP_Text[] segmentTexts;        // 每条血上显示当前 HP（可选）

    [Header("总血量（可选，仅用于调试或总量显示）")]
    public TMP_Text totalHPText;

    private bool subscribed = false;

    private void OnDisable()
    {
        if (!subscribed || EnemyHealth.Instance == null)
            return;

        EnemyHealth.Instance.OnSegmentsChanged -= UpdateSegments;
        EnemyHealth.Instance.OnHPChanged -= UpdateTotal;
        subscribed = false;
    }

    private void Update()
    {
        if (subscribed || EnemyHealth.Instance == null)
            return;

        subscribed = true;

        EnemyHealth.Instance.OnSegmentsChanged += UpdateSegments;
        EnemyHealth.Instance.OnHPChanged += UpdateTotal;

        // 初始刷新
        if (EnemyHealth.Instance.useSegments && EnemyHealth.Instance.segmentCurrentHPs != null)
        {
            UpdateSegments(EnemyHealth.Instance.segmentCurrentHPs, EnemyHealth.Instance.segmentMaxHPs);
        }
        else
        {
            UpdateTotal(EnemyHealth.Instance.currentHP, EnemyHealth.Instance.maxHP);
        }
    }

    private void UpdateSegments(int[] currents, int[] maxes)
    {
        if (segmentSliders == null) return;

        for (int i = 0; i < segmentSliders.Length; i++)
        {
            bool active = (currents != null && maxes != null &&
                           i < currents.Length && i < maxes.Length &&
                           maxes[i] > 0);

            if (segmentSliders[i] != null)
                segmentSliders[i].gameObject.SetActive(active);

            if (segmentTexts != null && i < segmentTexts.Length && segmentTexts[i] != null)
                segmentTexts[i].gameObject.SetActive(active);

            if (active)
            {
                segmentSliders[i].maxValue = maxes[i];
                segmentSliders[i].value = currents[i];

                if (segmentTexts != null && i < segmentTexts.Length && segmentTexts[i] != null)
                {
                    segmentTexts[i].text = currents[i].ToString();
                }
            }
        }
    }

    private void UpdateTotal(int current, int max)
    {
        if (totalHPText != null)
        {
            totalHPText.text = $"{current} / {max}";
        }

        // 如果没有使用多条血（普通小怪），就用第一个 Slider 显示
        if (!EnemyHealth.Instance.useSegments && segmentSliders != null && segmentSliders.Length > 0)
        {
            if (segmentSliders[0] != null)
            {
                segmentSliders[0].gameObject.SetActive(true);
                segmentSliders[0].maxValue = max;
                segmentSliders[0].value = current;
            }

            if (segmentTexts != null && segmentTexts.Length > 0 && segmentTexts[0] != null)
            {
                segmentTexts[0].gameObject.SetActive(true);
                segmentTexts[0].text = current.ToString();
            }

            // 把其他 Slider 隐藏
            for (int i = 1; i < segmentSliders.Length; i++)
            {
                if (segmentSliders[i] != null)
                    segmentSliders[i].gameObject.SetActive(false);
                if (segmentTexts != null && i < segmentTexts.Length && segmentTexts[i] != null)
                    segmentTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
