using UnityEngine;

public class TokenTestInput : MonoBehaviour
{
    private void Update()
    {
        // 按 T 键进入 Token 模式
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (TokenManager.Instance != null)
            {
                TokenManager.Instance.BeginTokenSelection();
            }
        }
    }
}
