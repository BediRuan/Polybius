using System.Collections.Generic;
using UnityEngine;

public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance { get; private set; }

    [Header("测试用初始 Token 数字")]
    public List<int> tokens = new List<int> { 1, 2, 3, 4 };  // 以后你可以从别的地方生成

    [Header("状态")]
    public bool isSelecting = false;   // 是否处于“选择数字”模式

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 某个技能/按钮调用：进入 token 选择模式
    /// </summary>
    public void BeginTokenSelection()
    {
        if (tokens.Count == 0)
        {
            Debug.Log("没有可用 Token 了！");
            return;
        }

        isSelecting = true;
        Debug.Log("进入 Token 选择模式：请点击一个数字。");
    }

    /// <summary>
    /// 结束选择模式
    /// </summary>
    public void EndTokenSelection()
    {
        isSelecting = false;
        Debug.Log("结束 Token 选择模式。");
    }

    /// <summary>
    /// 从 token 池里随机抽一个数字（并移除）
    /// </summary>
    public int DrawRandomToken()
    {
        if (tokens.Count == 0)
        {
            Debug.LogWarning("Token 用完了！");
            return 0;
        }

        int index = Random.Range(0, tokens.Count);
        int value = tokens[index];
        //tokens.RemoveAt(index);
        return value;
    }
}
