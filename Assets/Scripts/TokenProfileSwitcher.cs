using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class TokenProfileSwitcher : MonoBehaviour
{
    public VolumeProfile normalProfile;
    public VolumeProfile tokenProfile;
    public float transitionSpeed = 5f;

    private Volume volume;
    private float targetWeight;
    private bool isInTokenProfile = false;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        if (volume == null) return;

        // 初始用 normalProfile
        volume.profile = normalProfile;
        volume.weight = 1f;
    }

    private void Update()
    {
        if (volume == null) return;
        if (TokenManager.Instance == null) return;

        bool selecting = TokenManager.Instance.isSelecting;

        // 简单方案：选中时用 tokenProfile，不选中时用 normalProfile
        // 并通过 weight 过渡
        if (selecting && !isInTokenProfile)
        {
            // 从 normal 淡出到 0，再切 profile，再从 0 淡入
            StartCoroutine(SwitchProfileSmooth(tokenProfile));
            isInTokenProfile = true;
        }
        else if (!selecting && isInTokenProfile)
        {
            StartCoroutine(SwitchProfileSmooth(normalProfile));
            isInTokenProfile = false;
        }
    }

    private System.Collections.IEnumerator SwitchProfileSmooth(VolumeProfile newProfile)
    {
        // 先把 weight 从 1 -> 0
        while (volume.weight > 0f)
        {
            volume.weight = Mathf.MoveTowards(volume.weight, 0f, transitionSpeed * Time.deltaTime);
            yield return null;
        }

        // 切换 profile
        volume.profile = newProfile;

        // 再把 weight 从 0 -> 1
        while (volume.weight < 1f)
        {
            volume.weight = Mathf.MoveTowards(volume.weight, 1f, transitionSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
