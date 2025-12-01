using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class HandCardView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float rotateSpeed = 8f;

    [Header("Layout (from HandManager)")]
    [HideInInspector] public Vector2 deckPosition;
    [HideInInspector] public float deckAngle;

    [Header("Hover")]
    public float hoverOffsetY = 40f;
    public float hoverScale = 1.1f;

    [Header("Drag")]
    public float dragScale = 0.9f;
    [Range(0f, 1f)]
    public float playYPercentMin = 0.25f;

    [Header("Optional")]
    public Camera uiCamera;

    public Action<HandCardView> onPlay;

    // 🔵 新增：用于弃牌选择
    public Action<HandCardView> onClick;
    private bool selectable = false;

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

        if (uiCamera == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                uiCamera = canvas.worldCamera;
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

        // 🔵 如果是“可选择状态”，让牌有一点提示（放大）
        if (selectable && !isDragging)
            targetScale = startScale * 1.15f;

        if (isHover && !isDragging && !selectable)
        {
            targetPos += Vector2.up * hoverOffsetY;
            targetScale = startScale * hoverScale;
        }

        if (isDragging)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                handRect,
                Input.mousePosition,
                uiCamera,
                out Vector2 localPos
            );
            targetPos = localPos;
            targetScale = startScale * dragScale;
            targetAngle = 0f;
        }

        rect.anchoredPosition =
            Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);

        rect.localScale =
            Vector3.Lerp(rect.localScale, targetScale, Time.deltaTime * moveSpeed);

        rect.localRotation =
            Quaternion.Lerp(rect.localRotation,
                Quaternion.Euler(0f, 0f, targetAngle),
                Time.deltaTime * rotateSpeed);
    }

    public void SetDeckState(Vector2 pos, float angle)
    {
        deckPosition = pos;
        deckAngle = angle;
    }

    public void SetSelectable(bool value)
    {
        selectable = value;
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

        if (selectable)
        {
            // 🔵 在选择模式下，点击 = 弃牌
            onClick?.Invoke(this);
            return;
        }

        if (IsTokenModeActive())
            return;

        isDragging = true;
        isHover = false;
        transform.SetAsLastSibling();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (selectable)
        {
            // 🔵 防止拖动触发
            return;
        }

        if (IsTokenModeActive())
            return;

        float yPercent = Input.mousePosition.y / Screen.height;
        bool shouldPlay = yPercent > playYPercentMin;

        isDragging = false;
        isHover = false;

        if (shouldPlay)
            onPlay?.Invoke(this);
    }

    private bool IsTokenModeActive()
    {
        return TokenManager.Instance != null && TokenManager.Instance.isSelecting;
    }
}
