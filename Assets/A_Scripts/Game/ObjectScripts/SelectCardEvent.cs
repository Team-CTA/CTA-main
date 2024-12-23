using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

public class EventTriggerSetup : MonoBehaviour
{
    GameManager gameManager; // EventTrigger에서 설정한 GameManager 오브젝트
    [SerializeField] GameObject selectedCard; // Pointer Click에서 사용된 SelectedCard 오브젝트
    [SerializeField] Text cname;
    [SerializeField] Text info;
    [SerializeField] Image type;
    [SerializeField] Image myimg;

    private void Start()
    {
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        EventTrigger eventTrigger = gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        // Pointer Enter 이벤트 추가
        AddEventTrigger(eventTrigger, EventTriggerType.PointerEnter, (data) =>
        {
            gameManager.uiController.OnPointerEnterCard(myimg);
        });

        // Pointer Exit 이벤트 추가
        AddEventTrigger(eventTrigger, EventTriggerType.PointerExit, (data) =>
        {
            gameManager.uiController.OnPointerExitCard(myimg);
        });

        // Pointer Click 이벤트 추가
        AddEventTrigger(eventTrigger, EventTriggerType.PointerClick, (data) =>
        {
            gameManager.uiController.OnPointerClick(selectedCard);
        });
        gameManager.uiController.CardTextSet(cname, info, type);
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> action)
    {
        // 새로운 Entry 생성
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };

        // 이벤트에 대한 콜백 등록
        entry.callback.AddListener((eventData) => { action.Invoke(eventData); });

        // EventTrigger에 Entry 추가
        trigger.triggers.Add(entry);
    }
}

