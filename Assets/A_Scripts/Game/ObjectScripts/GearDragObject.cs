using UnityEngine;
using UnityEngine.EventSystems;

public class GearDragObject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] Gear gearGm;
    private RectTransform rectTransform; // UI 오브젝트의 RectTransform
    private Canvas canvas; // UI가 속한 Canvas
    private CanvasGroup canvasGroup; // 드래그 시 Raycast Blocking 관리
    private Vector2 originalPosition;

    [Header("Clamp Bounds (Local Space)")]
    public Vector2 minBounds; // 드래그 가능한 최소 x, y 위치
    public Vector2 maxBounds; // 드래그 가능한 최대 x, y 위치

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 초기 위치 저장
        originalPosition = rectTransform.anchoredPosition;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false; // Raycast 차단 해제
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!gearGm.draggable) return;
        // 드래그 중: 마우스 위치에 따라 RectTransform 이동
        Vector2 newPosition = rectTransform.anchoredPosition + eventData.delta / canvas.scaleFactor;

        // Clamp로 범위 제한 적용
        newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);

        rectTransform.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료: Raycast 차단 복구
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        // 예: 드래그 실패 시 원래 위치로 복귀
        // rectTransform.anchoredPosition = originalPosition;
        gearGm.OnDragEnd();
        Debug.Log("드래그끝!");
    }
}
