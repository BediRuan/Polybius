using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class HandCardView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Movement")]
    public float moveSpeed = 10f;          // 位置插值速度
    public float rotateSpeed = 8f;         // 旋转插值速度

    [Header("Layout (from HandManager)")]
    [HideInInspector] public Vector2 deckPosition; // 弧形中的目标位置
    [HideInInspector] public float deckAngle;      // 弧形中的旋转角度（Z）

    [Header("Hover")]
    public float hoverOffsetY = 40f;      // 悬停时向上抬高多少
    public float hoverScale = 1.1f;       // 悬停时放大倍数

    [Header("Drag")]
    public float dragScale = 0.9f;        // 拖拽时缩小一点点
    [Range(0f, 1f)]
    public float playYPercentMin = 0.25f; // 鼠标在屏幕高度中占比，大于这个就算“打出”

    [Header("Optional")]
    public Camera uiCamera;               // ScreenSpace-Overlay 可以留空

    public Action<HandCardView> onPlay;   // 通知 HandManager：这张牌被打出

    private RectTransform rect;
    private RectTransform handRect;
    private Vector3 startScale;
    private bool isHover;
    private bool isDragging;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        handRect = transform.parent as RectTransform;
        startScale = transform.localScale;

        // ? 如果没在 Inspector 里手动指定 uiCamera，就自动从 Canvas 拿
        if (uiCamera == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                uiCamera = canvas.worldCamera; // Screen Space - Camera/World Space 会用这个
            }
        }
    }

    private void Update()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        Vector2 targetPos = deckPosition;
        Vector3 targetScale = startScale;
        float targetAngle = deckAngle;

        // 悬停：上浮 + 放大
        if (isHover && !isDragging)
        {
            targetPos += Vector2.up * hoverOffsetY;
            targetScale = startScale * hoverScale;
        }

        // 拖拽：跟随鼠标 + 稍微缩小 + 角度归零
        if (isDragging)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                handRect,
                Input.mousePosition,
                uiCamera, // overlay 模式可为 null
                out Vector2 localPos
            );
            targetPos = localPos;
            targetScale = startScale * dragScale;
            targetAngle = 0f;
        }

        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.deltaTime * moveSpeed);
        rect.localRotation = Quaternion.Lerp(
            rect.localRotation,
            Quaternion.Euler(0f, 0f, targetAngle),
            Time.deltaTime * rotateSpeed
        );
    }

    // 由 HandManager 调用，更新这张牌在“弧形队列”中的基础位置/角度
    public void SetDeckState(Vector2 pos, float angle)
    {
        deckPosition = pos;
        deckAngle = angle;
    }

    // ========= 鼠标事件 =========

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
        {
            isHover = true;
            transform.SetAsLastSibling();
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsTokenModeActive())
            return;

        if (!isDragging)
            isHover = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // 在 Token 模式下，禁止开始拖拽，避免和点数字冲突
        if (IsTokenModeActive())
            return;

        isDragging = true;
        isHover = false;

        // 提到最上层渲染
        transform.SetAsLastSibling();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // 在 Token 模式下，完全忽略抬起事件（不打牌、不改变状态）
        if (IsTokenModeActive())
            return;

        // 根据屏幕高度判断是否“打出”
        float yPercent = Input.mousePosition.y / Screen.height;
        bool shouldPlay = yPercent > playYPercentMin;

        isDragging = false;
        isHover = false;

        if (shouldPlay)
        {
            onPlay?.Invoke(this);
        }
    }

    private bool IsTokenModeActive()
    {
        return TokenManager.Instance != null && TokenManager.Instance.isSelecting;
    }

}
