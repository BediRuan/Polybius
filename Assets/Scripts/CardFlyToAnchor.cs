using UnityEngine;

public class CardFlyToAnchor : MonoBehaviour
{
    public float duration = 0.25f;

    private RectTransform rect;
    private Vector2 startPos;
    private Vector2 targetPos;
    private float t;

    public void Init(Vector2 start, Vector2 target, float dur)
    {
        rect = GetComponent<RectTransform>();
        duration = dur;
        startPos = start;
        targetPos = target;
        t = 0f;

        if (rect != null)
            rect.anchoredPosition = startPos;
    }

    private void Update()
    {
        if (rect == null) return;

        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);

        rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, k);
        rect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, k); // 可以视觉上稍微缩小一点
        rect.localRotation = Quaternion.Lerp(rect.localRotation, Quaternion.identity, k);

        if (k >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
