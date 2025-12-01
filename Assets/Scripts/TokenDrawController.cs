using UnityEngine;
using TMPro;
using System.Collections;

public class TokenDrawController : MonoBehaviour
{
    public static TokenDrawController Instance { get; private set; }

    [Header("3D 相关对象")]
    public Transform boardRoot;          // 平面位置（世界空间）
    public GameObject token3DPrefab;     // Token3D 预制体
    public float spawnHeight = 1.5f;      // Token 距平面上方多少米掉落

    [Header("随机力参数")]
    public float dropForceMin = 2f;
    public float dropForceMax = 5f;
    public float torqueForce = 5f;

    private bool isPlayingAnimation = false;
    private System.Action<int> pendingApplyCallback;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 外部调用（例如 TokenNumberBase.OnPointerClick）
    /// 传入 token 数字 + 用来执行 ApplyToken 的回调
    /// </summary>
    public void PlayTokenDrop(int tokenValue, System.Action<int> applyCallback)
    {
        if (isPlayingAnimation) return;

        pendingApplyCallback = applyCallback;
        StartCoroutine(TokenDropRoutine(tokenValue));
    }

    private IEnumerator TokenDropRoutine(int value)
    {
        isPlayingAnimation = true;

        // 1. 开板
        boardRoot.gameObject.SetActive(true);

        // 2. 生成 Token3D
        Vector3 spawnPos = boardRoot.position + Vector3.up * spawnHeight;
        Quaternion spawnRot = Random.rotation; // 随机初始旋转

        GameObject tokenObj = Instantiate(token3DPrefab, spawnPos, spawnRot);

        // 写入数字（两个 TMP）
        foreach (var tmp in tokenObj.GetComponentsInChildren<TextMeshPro>())
        {
            tmp.text = value.ToString();
        }

        // 3. 加随机力 & 扭矩
        Rigidbody rb = tokenObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 随机向下力 + 一点横向偏移
            Vector3 forceDir = Vector3.down + new Vector3(
                Random.Range(-0.2f, 0.2f),
                0,
                Random.Range(-0.2f, 0.2f)
            );

            float force = Random.Range(dropForceMin, dropForceMax);
            rb.AddForce(forceDir.normalized * force, ForceMode.Impulse);

            // 随机扭矩
            Vector3 torque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * torqueForce;

            rb.AddTorque(torque, ForceMode.Impulse);
        }

        // 4. 等 token 停下来
        yield return new WaitForSeconds(0.25f); // 等它至少接触一下

        while (!rb.IsSleeping())
            yield return null;

        // 5. token 完全静止 → 执行 ApplyToken
        pendingApplyCallback?.Invoke(value);

        // 6. 停留一点时间再关闭
        yield return new WaitForSeconds(1.5f);

        Destroy(tokenObj);
        boardRoot.gameObject.SetActive(false);

        isPlayingAnimation = false;
    }
}
