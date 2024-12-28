using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class InGameUiController : MonoBehaviourPun
{
    [Header("게임매니저")]
    [SerializeField] GameManager gm;

    [Header("애니메이터")]
    [SerializeField] Animator gameInfoAni;
    [SerializeField] Animator CheckInfoAni;
    [SerializeField] Animator cardDrawAni;
    [SerializeField] Animator showGameNameAni;
    [SerializeField] Animator CardInputAni;
    [SerializeField] Animator showCardAni;
    [SerializeField] Animator cardSelTimeAni;
    [SerializeField] Animator nanidoAni;
    [SerializeField] Animator nanidoPnlAni;

    [Header("게임오브젝트")]
    [SerializeField] GameObject screenObj;
    [SerializeField] GameObject raiseCardsObj;
    [SerializeField] GameObject inputCardBaseObj;
    [SerializeField] GameObject cardShowBaseObj;
    [SerializeField] GameObject cardSelEndObj;
    [SerializeField] GameObject nanidoObj;
    [SerializeField] GameObject cardDrawTimeObj;
    [SerializeField] GameObject gameDifObj;

    [Header("렉트 트랜스폼")]
    [SerializeField] RectTransform CardContentsRect;
    [SerializeField] RectTransform ShowMyCards;
    [SerializeField] RectTransform ShowEneCards;
    [SerializeField] RectTransform SelectingCard;

    [Header("텍스트")]
    [SerializeField] Text explainText;
    [SerializeField] Text playerNameText;
    [SerializeField] Text playerNameText2;
    [SerializeField] Text playerRankText;
    [SerializeField] Text enemyDrawCountText;
    [SerializeField] Text GameNameText;
    [SerializeField] Text selectTimeRemainingText;
    [SerializeField] Text SelCardName;
    [SerializeField] Text SelCardInfo;
    [SerializeField] Text SelCardUseable;
    [SerializeField] Text[] cardsName;
    [SerializeField] Text[] cardsInfo;
    [SerializeField] Text cardSelEndText;
    [SerializeField] Text yrShowCardText;
    [SerializeField] Text myShowCardText;
    [SerializeField] Text nanidoText;
    [SerializeField] Text selNanidoText;
    [SerializeField] Text cardDrawTimeText;

    [Header("이미지")]
    [SerializeField] Image SelCardType;
    [SerializeField] Image[] cardsType;

    List<string> curSelCards = new List<string>();
    public List<GameManager.Card> selectableCards = new List<GameManager.Card>();
    List<bool> selCardCheck = new List<bool>();
    public Dictionary<string, int> selectedCards = new Dictionary<string, int>();


    List<GameManager.Card> eneEntry;
    List<GameManager.Card> myEntry;


    int roolCnt = 20;
    int eneCardSelCount = 0;
    int cardListIndex = 0;
    float turnShowTime = 1.8f;
    float enemyEntryShowTime = 5.5f;
    bool infoOpening = false;
    bool timerRunning = false;
    bool CardChangable = true;
    double startTime;
    double duration;
    bool selFlag;

    PhotonView PV;

    #region Unity Default
    private void Start()
    {
        PV = photonView;
        CardInput();
    }
    private void Update()
    {
        if (gm.curPhase == "CardSelect")
        {
            if (gm.myScript.cardEntry && gm.eneScript.cardEntry)
            {
                if (selFlag) return;
                selFlag = true;
                timerRunning = false;
                selectTimeRemainingText.text = $"선택 종료";
                StartCoroutine(CloseCardSelect());
            }
        }
        if (!timerRunning) return;
        if (gm.curPhase == "CardSelect")
        {
            if (gm.myScript.cardEntry)
            {
                if (!gm.eneScript.cardEntry)
                {
                    selectTimeRemainingText.text = $"{gm.eneScript.gameObject.GetPhotonView().Owner.NickName} 카드 선택중";
                }
                return;
            }
            double elapsedTime = PhotonNetwork.Time - startTime;
            double remainingTime = duration - elapsedTime;

            if (remainingTime <= 0)
            {
                timerRunning = false;
                remainingTime = 0;
                gm.myScript.cardEntry = true;
                StartCoroutine(CloseCardSelect());
            }

            selectTimeRemainingText.text = $"선택 종료까지 남은시간 | {math.floor(remainingTime)}초";
        }
        else if (gm.curPhase == "drawCards")
        {
            double elapsedTime = Time.time - startTime;
            double remainingTime = duration - elapsedTime;

            if (remainingTime <= 0)
            {
                timerRunning = false;
                remainingTime = 0;
                DrawTimeEnd();
            }
            cardDrawTimeText.text = $"선택시간 | {math.floor(remainingTime)}초";
        }
        else if (gm.curPhase == "mytrun")
        {
            double elapsedTime = Time.time - startTime;
            double remainingTime = duration - elapsedTime;

            if (remainingTime <= 0)
            {
                timerRunning = false;
                remainingTime = 0;
                cardSelTimeAni.SetTrigger("back");
                HexSelEnd();
            }
            cardDrawTimeText.text = $"선택시간 | {math.floor(remainingTime)}초";
        }
        else if (gm.curPhase == "difficultySelecting")
        {
            double elapsedTime = PhotonNetwork.Time - startTime;
            double remainingTime = duration - elapsedTime;

            if (remainingTime <= 0)
            {
                timerRunning = false;
                remainingTime = 0;
                cardSelTimeAni.SetTrigger("back");
                NanidoSelEnd();
            }
            cardDrawTimeText.text = $"선택시간 | {math.floor(remainingTime)}초";
        }
        else if (gm.curPhase == "myFirstArea")
        {
            double elapsedTime = Time.time - startTime;
            double remainingTime = duration - elapsedTime;

            if (remainingTime <= 0)
            {
                timerRunning = false;
                remainingTime = 0;
                cardSelTimeAni.SetTrigger("back");
                GameExit(); // 이거 첫땅 선택 안하면 강퇴 ================================================!ㅕㅛ@(#(!@ㅛ#(@!ㅛ(#*!@&(*#!@(#&(@!))))))
            }
            cardDrawTimeText.text = $"선택시간 | {math.floor(remainingTime)}초";
        }
    }
    #endregion
    #region General Purpose
    public void UpdateActivity()
    {
        gameInfoAni.gameObject.SetActive(gm.infoOpenable);
    }
    public void ShowTurn()
    {
        if (gm.myScript.myturn)
            CheckInfoAni.SetTrigger("turn");
    }
    #endregion
    #region GameScoreInfo 수정해야함
    public void GameInfoOpen()
    {
        if (infoOpening || !gm.infoOpenable) return;
        infoOpening = true;
        gameInfoAni.SetTrigger("Enable");
        Invoke("InfoCool", 0.5f);
    }
    public void GameInfoClose()
    {
        if (infoOpening) return;
        infoOpening = true;
        gameInfoAni.SetTrigger("Disable");
        Invoke("InfoCool", 0.5f);
    }
    void GameInfoCool()
    {
        infoOpening = false;
    }
    #endregion
    #region 아직 안만듬 (아래로갈수록 우선순위 낮음)
    void GameExit()
    {
        cardSelTimeAni.SetTrigger("back");
    }
    public void ShowCard(int Draws, bool isFirst = false)
    {
        if (isFirst)
        {
            gm.myScript.cardSelected = true;// 밑에꺼 수정하면 이건 지우면 됨
            // StartCoroutine(RaiseCards(Draws)); 여기가 카드 선택 후 올라오던 부분
            return;
        }
        //아니면 카드 추가시키는 코드
    }
    #endregion
    #region OnEnterGameEvent

    public void CheckInfoSet()
    {
        CheckInfoAni.SetTrigger("go");
        StartCoroutine(EnemyPlayerName());
        // 랭크 넣기
    }
    IEnumerator EnemyPlayerName()
    {
        for (int i = 0; i < 100; i++)
        {
            try
            {
                playerNameText.text = gm.eneScript.gameObject.GetPhotonView().Owner.NickName;
            }
            catch
            {
                playerNameText.text = "Unknown";
            }
            if (playerNameText.text == "Unknown")
            {
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                continue;
            }
        }
        CheckInfoNext();
    }
    public void CheckInfoNext()
    {
        StartCoroutine(ChangeCheckInfo());
    }
    IEnumerator ChangeCheckInfo()
    {
        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
            playerNameText.transform.Rotate(0, 6, 0);
            playerRankText.transform.Rotate(0, 6, 0);
            explainText.transform.Rotate(0, 6, 0);
        }
        playerRankText.gameObject.SetActive(false);
        explainText.text = "First Turn Random Selection";
        StartCoroutine(RollFirstTurn(roolCnt));
        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
            playerNameText.transform.Rotate(0, -6, 0);
            explainText.transform.Rotate(0, -6, 0);
        }
    }
    IEnumerator RollFirstTurn(int cnt)
    {
        try
        {
            cnt += gm.myScript.myturn == true ? 1 : 0;
            Debug.Log("성공");
        }
        catch
        {
            cnt += 1;
        }
        Debug.Log(cnt);
        string me = gm.nickName, yu;
        try
        {
            yu = gm.eneScript.gameObject.GetPhotonView().Owner.NickName;
        }
        catch
        {
            yu = "Unknown";
        }
        playerNameText.text = yu;
        playerNameText2.text = me;
        while (cnt > 0)
        {
            for (int i = 0; i < 10; i++)
            {
                float duration = 0.1f / cnt; // 이동에 걸리는 시간
                float elapsed = 0f; // 경과 시간 초기화
                Vector3 startPosText1 = playerNameText.transform.localPosition;
                Vector3 startPosText2 = playerNameText2.transform.localPosition;
                Vector3 targetPosText1 = startPosText1 + new Vector3(0, 12, 0);
                Vector3 targetPosText2 = startPosText2 + new Vector3(0, 12, 0);

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration); // 0~1 비율 계산
                    playerNameText.transform.localPosition = Vector3.Lerp(startPosText1, targetPosText1, t);
                    playerNameText2.transform.localPosition = Vector3.Lerp(startPosText2, targetPosText2, t);

                    yield return null; // 다음 프레임까지 대기
                }

                // 이동 후 위치 고정 (Lerp의 마지막 보정)
                playerNameText.transform.localPosition = targetPosText1;
                playerNameText2.transform.localPosition = targetPosText2;
            }

            if (playerNameText.transform.localPosition.y == 120)
            {
                playerNameText.transform.localPosition = new Vector3(0, -120, 0);
                playerNameText.text = playerNameText2.text == yu ? me : yu;
            }
            else if (playerNameText2.transform.localPosition.y == 120)
            {
                playerNameText2.transform.localPosition = new Vector3(0, -120, 0);
                playerNameText2.text = playerNameText.text == yu ? me : yu;
            }
            cnt--;
        }
        yield return new WaitForSeconds(0.5f);//나주엥 바꿀거임
        if (playerNameText.transform.localPosition.y == 0)
        {
            playerNameText.color = Color.green;
        }
        else
        {
            playerNameText2.color = Color.green;
        }
        yield return new WaitForSeconds(turnShowTime - 0.2f);//나주엥 바꿀거임
        CheckInfoAni.SetTrigger("back");
        yield return new WaitForSeconds(0.7f);//나주엥 바꿀거임
                                              //Next=============================================
        gm.NextPhase();
    }
    #endregion
    #region CardDraw
    //카드 확인 되는지 안되는지를 infoOpenable로 정하자
    public void Draw(string[] cards, int remain)
    {
        StartCoroutine(Drawing(cards, remain));
        curSelCards = cards.ToList();
        float startTime = (float)Time.time;
        PrivateTimer(startTime, 20);
    }
    void PrivateTimer(float startTime, int duration)
    {
        this.startTime = startTime;
        this.duration = duration;
        timerRunning = true;
        cardDrawTimeObj.SetActive(true);
    }
    void DrawTimeEnd()
    {
        screenObj.SetActive(true);
        gm.CardClicked(curSelCards[Random.Range(0, 3)]);
    }
    IEnumerator Drawing(string[] cards, int remain)
    {

        for (int i = 0; i < 3; i++)
        {
            cardsName[i].text = cards[i];
            cardsInfo[i].text = gm.cards[cards[i]].Item1;
            cardsType[i].sprite = gm.cardTypeImages[gm.cards[cards[i]].Item2];
        }
        yield return new WaitForSeconds(0.7f);
        if (remain > 0)
        {
            Debug.Log("제발");
            cardDrawAni.gameObject.SetActive(true);
            cardDrawAni.SetTrigger("Draw");
            yield return new WaitForSeconds(0.1f);

            screenObj.SetActive(false);
        }
    }
    public void UnDraw(int remain)
    {
        screenObj.SetActive(true);
        StartCoroutine(Next(remain));
    }
    IEnumerator Next(int remain)
    {
        cardDrawAni.SetTrigger("Undraw");
        while (!IsAnimationFinished(cardDrawAni, "AbilitiesAni_CloseCards"))
        {
            yield return null;
        }
        gm.CardDraw();
        if (remain <= 0)
        {
            timerRunning = false;
            cardSelTimeAni.SetTrigger("back");
            yield return new WaitForSeconds(1);
            gm.EndDraw();
        }
    }
    bool IsAnimationFinished(Animator animator, string stateName)
    {
        // 현재 재생 중인 상태 확인
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 1.0f;
    }
    IEnumerator RaiseCards(int Draws)
    {
        Vector2 firstPos = new Vector2(0, -185);
        for (int i = 0; i < Draws; i++)
        {
            Transform curObj = raiseCardsObj.transform.GetChild(Draws - (i + 1));
            curObj.localPosition = (Vector3)firstPos + new Vector3(i * 10, 0);
            curObj.localRotation = Quaternion.Euler(0, 0, i * -3);
        }
        raiseCardsObj.SetActive(true);
        for (int i = 0; i < Draws; i++)
        {
            Transform curObj = raiseCardsObj.transform.GetChild(Draws - (i + 1));
            StartCoroutine(Raise(curObj, i));
            yield return new WaitForSeconds(0.2f);
        }
        gm.myScript.cardSelected = true;
    }
    IEnumerator Raise(Transform curObj, int i)
    {
        float moveSpeed = 10f;
        Vector3 targetPosition = new Vector3(curObj.localPosition.x, -15 * i);
        while (curObj.localPosition.y < -15 * i - 1)
        {
            curObj.localPosition = Vector3.Lerp(curObj.localPosition, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
    public void EnemyCardSelected(int add, int max)
    {
        PV.RPC("EnemyDrawText", RpcTarget.Others, add, max);
    }
    public void SelEnd()
    {
        Invoke("DrawPannelDisappear", 0.2f);
    }
    void DrawPannelDisappear()
    {
        cardDrawAni.gameObject.SetActive(false);
    }
    [PunRPC]
    void EnemyDrawText(int add, int max)
    {
        eneCardSelCount += add;
        enemyDrawCountText.text = $"Enemy ( {eneCardSelCount} / {max} )";
    }
    #endregion
    #region CardSelect
    public void CardInput(GameObject card = null)
    {
        CardContentsRect.sizeDelta = new Vector2(CardContentsRect.childCount * 150, 540);
    }
    [PunRPC]
    void StartTimer(float startTime, int duration)
    {
        // 네트워크 연결 상태 확인
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Network not connected. Timer cannot start.");
            return;
        }

        // 유효성 검사
        if (duration <= 0)
        {
            Debug.LogError("Invalid timer duration.");
            return;
        }

        try
        {
            this.startTime = startTime;
            this.duration = duration;
            timerRunning = true;

            // UI 상태 업데이트
            if (gm.curPhase != "CardSelect")
            {
                cardDrawTimeObj.SetActive(false);
                cardDrawTimeObj.SetActive(true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Timer start failed: {e.Message}");
        }
    }
    public void OpenCardSelect(List<GameManager.Card> cardlist, int duration, string gamename) // <============
    {
        cardSelTimeAni.SetTrigger("back");
        eneEntry = new List<GameManager.Card>();
        myEntry = new List<GameManager.Card>();
        curSelCards = new List<string>();
        selectableCards = new List<GameManager.Card>();
        selCardCheck = new List<bool>();
        selectedCards = new Dictionary<string, int>();

        for (int i = 0; i < CardContentsRect.childCount; i++)
        {
            Destroy(CardContentsRect.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < ShowMyCards.childCount; i++)
        {
            Destroy(ShowMyCards.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < ShowEneCards.childCount; i++)
        {
            Destroy(ShowEneCards.transform.GetChild(i).gameObject);
        }
        selFlag = false;
        if (cardSelEndText != null) cardSelEndText.text = "결정";
        cardSelEndObj.SetActive(false);
        gm.myScript.cardEntry = false;
        CardInputAni.gameObject.SetActive(true);
        cardListIndex = 0;
        if (cardlist.Count != 0)
        {
            SelCardName.text = cardlist[0].name;
            SelCardInfo.text = cardlist[0].info;
            SelCardType.sprite = cardlist[0].type;
            SelCardUseable.text = "사용";
        }
        else
        {
            SelCardName.text = "카드 선택 불가";
            SelCardInfo.text = "사용가능한 카드가 없습니다.";
            SelCardType.sprite = gm.cardTypeImages[3];
            SelCardUseable.text = "사용불가";
        }
        for (int i = 0; i < cardlist.Count; i++)
        {
            selCardCheck.Add(false);
        }
        selectableCards = cardlist;
        StartCoroutine(OpenCardSelect_(duration));
    }
    IEnumerator OpenCardSelect_(int duration)
    {
        yield return new WaitForSeconds(0.2f);
        CardInputAni.SetTrigger("open");
        yield return new WaitForSeconds(1f);
        if (PhotonNetwork.IsMasterClient)
        {
            float startTime = (float)PhotonNetwork.Time;
            photonView.RPC("StartTimer", RpcTarget.AllViaServer, startTime, duration);
        }
    }
    IEnumerator ChangeCard(bool isRight = true)
    {
        if (!CardChangable) yield break;
        CardChangable = false;
        Debug.Log("응애!!");
        Vector3 direction = isRight ? new Vector3(500, 0, 0) : new Vector3(-500, 0, 0);

        float duration = 0.1f;  // 이동 시간
        for (int i = 0; i < 2; i++)
        {
            float elapsed = 0f;     // 경과 시간 초기화
            Vector3 startPos = SelectingCard.anchoredPosition;
            Vector3 targetPos = startPos + direction;  // 이동 방향을 반영하여 목표 위치 설정

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration); // 0~1 비율 계산
                SelectingCard.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
                Debug.Log("이동함!!");
                yield return null; // 다음 프레임까지 대기
            }
            SelectingCard.anchoredPosition = i == 0 ? -direction : Vector3.zero;
            if (i == 0)
                ChangeCardText(isRight);
        }
        // 이동 후 위치 고정 (Lerp의 마지막 보정)
        CardChangable = true;
    }
    void ChangeCardText(bool isRight = true)
    {
        int sequence = 0;
        if (selCardCheck.Contains(false))
        {
            if (isRight)
            {
                do
                {
                    Debug.Log("에암애ㅑㅁㄴ야ㅜㅁ 여몬ㅁㅇ   오른쪽");
                    if (++cardListIndex >= selectableCards.Count) cardListIndex = 0;
                    if (sequence++ > 100)
                    {
                        SelCardName.text = "카드 선택 불가";
                        SelCardInfo.text = "사용가능한 카드가 없습니다.";
                        SelCardType.sprite = gm.cardTypeImages[3];
                        SelCardUseable.text = "사용불가";
                        break;
                    }
                } while (selCardCheck[cardListIndex]);
            }
            else
            {
                do
                {
                    Debug.Log("에암애ㅑㅁㄴ야ㅜㅁ 여몬ㅁㅇ   왼쪽");
                    if (--cardListIndex < 0) cardListIndex = selectableCards.Count - 1;
                    if (sequence++ > 100)
                    {
                        SelCardName.text = "카드 선택 불가";
                        SelCardInfo.text = "사용가능한 카드가 없습니다.";
                        SelCardType.sprite = gm.cardTypeImages[3];
                        SelCardUseable.text = "사용불가";
                        break;
                    }
                } while (selCardCheck[cardListIndex]);
            }
            SelCardName.text = selectableCards[cardListIndex].name;
            SelCardInfo.text = selectableCards[cardListIndex].info;
            SelCardType.sprite = selectableCards[cardListIndex].type;
            if (selectedCards.ContainsKey(selectableCards[cardListIndex].name))
                SelCardUseable.text = "중복";
            else
                SelCardUseable.text = "사용";
        }
        else
        {
            SelCardName.text = "카드 선택 불가";
            SelCardInfo.text = "사용가능한 카드가 없습니다.";
            SelCardType.sprite = gm.cardTypeImages[3];
            SelCardUseable.text = "사용불가";
        }
    }

    IEnumerator CloseCardSelect()
    {
        yrShowCardText.text = gm.eneScript.gameObject.GetPhotonView().Owner.NickName;
        myShowCardText.text = gm.nickName;
        if (!gm.myScript.cardEntry)
        {
            CardCloseSet();
        }
        yield return new WaitForSeconds(0.9f);
        CardInputAni.SetTrigger("close");
        gm.EntryCard(myEntry, eneEntry);
        yield return new WaitForSeconds(1);
        showCardAni.SetTrigger("go");
        StartCoroutine(AddShowCard());
        yield return new WaitForSeconds(enemyEntryShowTime);
        showCardAni.SetTrigger("back");
        yield return new WaitForSeconds(1);
        gm.CardAbilityExecute();

        //여기 적용된 까드 쫙?

    }
    public void CardAbilExeEnd(List<GameManager.Card> list)
    {
        StartCoroutine(ShowExecuted(list));
    }
    [SerializeField] Text EntryCardName;
    [SerializeField] Text EntryCardInfo;
    [SerializeField] Image EntryCardType;
    [SerializeField] RectTransform EntryCardObj;
    Vector3 entryTargetPos = new Vector3(-450, 0, 0);
    IEnumerator ShowExecuted(List<GameManager.Card> list)
    {

        for (int i = 0; i < list.Count; i++)
        {
            EntryCardName.text = list[i].name;
            EntryCardInfo.text = list[i].info;
            EntryCardType.sprite = list[i].type;

            float moveSpeed = 15f;

            while (true)
            {
                float distance = Vector2.Distance(EntryCardObj.anchoredPosition, entryTargetPos);

                if (distance > 0.01f) // 목표 위치에 거의 도달하면 멈춤
                {
                    EntryCardObj.anchoredPosition = Vector2.Lerp(
                        EntryCardObj.anchoredPosition,
                        entryTargetPos,
                        Time.deltaTime * moveSpeed
                    );
                }
                else
                {
                    EntryCardObj.anchoredPosition = entryTargetPos; // 정확한 위치 고정
                    break;
                }
                yield return null;
            }
            moveSpeed *= 2;
            yield return new WaitForSeconds(0.3f);
            while (true)
            {
                float distance = Vector2.Distance(EntryCardObj.anchoredPosition, Vector3.zero);

                if (distance > 0.01f) // 목표 위치에 거의 도달하면 멈춤
                {
                    EntryCardObj.anchoredPosition = Vector2.Lerp(
                        EntryCardObj.anchoredPosition,
                        Vector3.zero,
                        Time.deltaTime * moveSpeed
                    );
                }
                else
                {
                    EntryCardObj.anchoredPosition = Vector3.zero; // 정확한 위치 고정
                    break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(1);
        gm.CardSelectEnd();
    }
    public void EditNanidoText(string text)
    {
        nanidoText.text = text;
    }
    IEnumerator AddShowCard()
    {
        for (int i = 0; i < gm.cardEntriesEnemy.Count; i++)
        {
            GameObject card = Instantiate(cardShowBaseObj, ShowEneCards.transform);
            card.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = gm.cardEntriesEnemy[i].name;
            card.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = gm.cardEntriesEnemy[i].info;
            card.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<Image>().sprite = gm.cardEntriesEnemy[i].type;
            yield return new WaitForSeconds(0.2f);
        }
        for (int i = 0; i < gm.cardEntriesI.Count; i++)
        {
            GameObject card = Instantiate(cardShowBaseObj, ShowMyCards.transform);
            card.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = gm.cardEntriesI[i].name;
            card.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = gm.cardEntriesI[i].info;
            card.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<Image>().sprite = gm.cardEntriesI[i].type;
            yield return new WaitForSeconds(0.2f);
        }
    }
    public void ShowPlayersCardSelection()
    {

    }
    GameManager.Card copyCard = null;
    void CardCloseSet()
    {
        Debug.Log("카드선택 닫힘");
        gm.myScript.cardEntry = true;
        cardSelEndText.text = "카드 선택 완료";
        cardSelEndObj.SetActive(true);

        string[] selectedCardNames = selectableCards
            .Where((card, index) => selCardCheck[index])
            .Select(card => card.name)
            .ToArray();
        for (int i = 0; i < selectableCards.Count; i++)
        {
            if (!selCardCheck[i]) continue;
            myEntry.Add(selectableCards[i]);
        }

        PV.RPC("EneCardSel", RpcTarget.Others, string.Join(",", selectedCardNames));
    }

    [PunRPC]
    void EneCardSel(string cardNamesString)
    {
        string[] cardNames = cardNamesString.Split(',');
        for (int i = 0; i < cardNames.Length; i++)
        {
            eneEntry.Add(gm.MakeCard(cardNames[i]));
        }
    }
    public void CardTextSet(Text name, Text info, Image type)
    {
        if (copyCard == null) return;
        name.text = copyCard.name;
        info.text = copyCard.info;
        type.sprite = copyCard.type;
    }
    #endregion
    #region Hex
    public void HexSelTime()
    {
        float startTime = (float)Time.time;
        PrivateTimer(startTime, 20);
    }
    public void HexSelTimeStop()
    {
        timerRunning = false;
        cardSelTimeAni.SetTrigger("back");
    }
    void HexSelEnd()
    {
        gm.HexSelTimeEnd();
    }
    #endregion
    #region Nanido
    string curNanido;
    public void NanidoSelect(string gamename) //<=================
    {
        if (!gm.myScript.myturn) return;
        timerRunning = false;
        GameNameText.text = gamename;
        StartCoroutine(NanidoSelect_(gamename));
        photonView.RPC("GameNameShowEnemy", RpcTarget.Others, gamename);
    }
    IEnumerator NanidoSelect_(string gamename)
    {
        showGameNameAni.SetTrigger("go");

        yield return new WaitForSeconds(1.7f);

        curNanido = "보통";
        selNanidoText.text = "보통";
        gameDifObj.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        gameDifObj.SetActive(true);
        float startTime = (float)PhotonNetwork.Time;
        photonView.RPC("StartTimer", RpcTarget.AllViaServer, startTime, 15);
    }
    [PunRPC]
    void GameNameShowEnemy(string name)
    {
        GameNameText.text = name;
        showGameNameAni.SetTrigger("go");
    }
    public void StopTimer()
    {
        timerRunning = false;
        photonView.RPC("StopTimer_", RpcTarget.Others);
    }
    [PunRPC]
    void StopTimer_()
    {
        timerRunning = false;

    }
    public void NanidoSelEnd()
    {
        if (!gm.myScript.myturn) return;
        photonView.RPC("NanidoSelEndEnemy", RpcTarget.Others, selNanidoText.text);
        nanidoPnlAni.SetTrigger("back");
        nanidoObj.SetActive(false);
        nanidoObj.SetActive(true);
        nanidoText.text = $"난이도 : {selNanidoText.text}";
        gm.NanidoSelected(selNanidoText.text);
        cardSelTimeAni.SetTrigger("back");
    }
    [PunRPC]
    void NanidoSelEndEnemy(string dif)
    {
        nanidoText.text = $"난이도 : {dif}";
        nanidoObj.SetActive(false);
        nanidoObj.SetActive(true);
        gm.NanidoSelected(dif);
    }
    public void NanidoBack()
    {
        photonView.RPC("BackToo", RpcTarget.Others);
        nanidoAni.gameObject.SetActive(false);
        nanidoAni.SetTrigger("back");
    }
    [PunRPC]
    void BackToo()
    {
        nanidoAni.gameObject.SetActive(false);
        nanidoAni.SetTrigger("back");
    }
    #endregion
    #region ButtonEvents
    public void OnPointerClick(GameObject type)
    {
        if (type.name == "Left")
        {
            StartCoroutine(ChangeCard());
        }
        else if (type.name == "Right")
        {
            StartCoroutine(ChangeCard(false));
        }
        else if (type.name == "SelectButton")
        {
            if (!selCardCheck.Contains(false) || selectedCards.ContainsKey(SelCardName.text) || gm.myScript.cardEntry) return;
            copyCard = gm.MakeCard(SelCardName.text);
            GameObject newCard = Instantiate(inputCardBaseObj, CardContentsRect.transform);
            CardInput();

            // <HFAIUHIUHDIUYGFILNSLABFUYWABFNRUGJILHGWBUYKCJINBAWHIJCOSGIHAWNDI:LDHMIIS:KLMHCGIHWK:LJMHCYHSBM<ILHMWACLILSHCMLACLHMNWIU
            newCard.name = $"{cardListIndex}.InputedCard";
            selCardCheck[cardListIndex] = true; //?
            selectedCards.Add(copyCard.name, cardListIndex);
            StartCoroutine(ChangeCard(false));
        }
        else if (type.name.Substring(type.name.LastIndexOf('.') + 1) == "InputedCard")
        {
            int index = int.Parse(type.name.Substring(0, type.name.LastIndexOf('.')));
            Debug.Log($"제거 카드 인덱스 : {index}");
            selCardCheck[index] = false;
            selectedCards.Remove(selectableCards[index].name);
            StartCoroutine(ChangeCard());
            Destroy(type);
        }
        else if (type.name == "CardSelText")
        {
            cardSelEndText.text = "카드 선택 완료";
            CardCloseSet();
        }
        else if (type.name == "nanidoButtonL")
        {
            if (curNanido == "쉬움") curNanido = "어려움";
            else if (curNanido == "보통") curNanido = "쉬움";
            else if (curNanido == "어려움") curNanido = "보통";
            selNanidoText.text = curNanido;
        }
        else if (type.name == "nanidoButtonR")
        {
            if (curNanido == "쉬움") curNanido = "보통";
            else if (curNanido == "보통") curNanido = "어려움";
            else if (curNanido == "어려움") curNanido = "쉬움";
            selNanidoText.text = curNanido;
        }
    }
    public void OnPointerEnter(Image hoverImg)
    {
        hoverImg.color = new Color(hoverImg.color.r, hoverImg.color.g - 0.2f, hoverImg.color.b);
    }
    public void OnPointerExit(Image hoverImg)
    {
        hoverImg.color = new Color(hoverImg.color.r, hoverImg.color.g + 0.2f, hoverImg.color.b);
    }
    public void OnPointerEnterCard(Image hoverImg)
    {
        hoverImg.color = new Color(1, 1, 1, 0.02f);
    }
    public void OnPointerExitCard(Image hoverImg)
    {
        hoverImg.color = new Color(1, 1, 1, 0);
    }
    #endregion
}
