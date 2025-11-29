using UnityEngine;
using UnityEngine.Rendering; // Volume 在这里

[RequireComponent(typeof(Volume))]
public class TokenSelectionVolumeController : MonoBehaviour
{
    [Header("目标权重（选中时）")]
    public float activeWeight = 1f;

    [Header("过渡速度（越大越快）")]
    public float transitionSpeed = 5f;

    private Volume volume;
    private float targetWeight = 0f;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        if (volume != null)
        {
            volume.weight = 0f; // 初始关闭特效
        }
    }

    private void Update()
    {
        if (volume == null)
            return;

        bool selecting = TokenManager.Instance != null && TokenManager.Instance.isSelecting;

        // 根据是否在 Token 模式，决定目标 weight
        targetWeight = selecting ? activeWeight : 0f;

        // 用 MoveTowards 做线性过渡，你也可以改成 Lerp
        volume.weight = Mathf.MoveTowards(
            volume.weight,
            targetWeight,
            transitionSpeed * Time.deltaTime
        );
    }
}
