using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Gear : MonoBehaviourPun
{
    public bool draggable = false;
    bool timerRunning = false, isGameEnd = false;
    int remains;
    double startTime;
    double duration;
    [SerializeField] int AbleDropDis = 40;
    [SerializeField] Text playernameText;
    [SerializeField] Text timerText;
    [SerializeField] Text clearconditionText;
    [SerializeField] Text isclearText;
    [SerializeField] GameObject goalObj;
    [SerializeField] GearDragObject dragObj;
    [SerializeField] GameObject gmaeEndObj;
    [SerializeField] GameObject playScreenObj;
    [SerializeField] GameManager gm;

    PhotonView PV;
    void Start()
    {
        PV = photonView;
    }
    private void Update()
    {
        if (!timerRunning || isGameEnd) return;
        double elapsedTime = Time.time - startTime;
        double remainingTime = duration - elapsedTime;

        if (remainingTime <= 0)
        {
            timerRunning = false;
            isGameEnd = true;
            remainingTime = 0;
            StartCoroutine(GameEnd(false));
            // 끝남 ( 실패겠지 )
        }

        timerText.text = $"제한시간 | {math.floor(remainingTime * 100) * 0.01}초";

        //코루틴써서 0.2초마다 rpc로 시간동기화
    }
    [PunRPC]
    void timerSynchronization_(string txt)
    {
        timerText.text = txt;
    }
    IEnumerator EneEnd_(bool iswin)
    {
        yield return new WaitForSeconds(2f);
        if (iswin)
            isclearText.text = "성공";
        else
            isclearText.text = "실패";
        gmaeEndObj.SetActive(true);
        yield return new WaitForSeconds(2f);
        playScreenObj.SetActive(false);
        gm.EnClose();
    }
    IEnumerator timerSynchronization()
    {
        while (!isGameEnd)
        {
            yield return new WaitForSeconds(0.15f);
            PV.RPC("timerSynchronization_", RpcTarget.Others, timerText.text);
        }
    }
    public void GameStart(int difficulty, string playername)
    {
        if (difficulty == 4 || difficulty == 0)
        {
            return;
        }
        timerRunning = false;
        isGameEnd = false;
        draggable = false;
        playernameText.text = $"게임 진행중 : {playername}";
        if (difficulty == 1) remains = 5;
        else if (difficulty == 2) remains = 8;
        else if (difficulty == 3) remains = 11;
        clearconditionText.text = $"성공까지 {remains}개 남음";
        timerText.text = "5초 후 시작";
        gmaeEndObj.SetActive(false);
        playScreenObj.SetActive(true);
        PV.RPC("Starting", RpcTarget.Others, playername, remains);
        StartCoroutine(InProgress());
    }
    [PunRPC]
    void Starting(string name, int dif)
    {
        gmaeEndObj.SetActive(false);
        playernameText.text = $"게임 진행중 : {name}";
        clearconditionText.text = $"성공까지 {dif}개 남음";
        timerText.text = "곧 게임 시작";
        playScreenObj.SetActive(true);
    }
    IEnumerator InProgress()
    {
        for (int i = 0; i < 5; i++)
        {
            timerText.text = $"{5 - i}초 후 시작";
            yield return new WaitForSeconds(1);
        }
        StartCoroutine(timerSynchronization());
        float startTime = Time.time;
        TimerStart(startTime, 11);
        draggable = true;
        DropObjects();
    }
    public void OnDragEnd()
    {
        if (isGameEnd || !draggable) return;
        if (Vector2.Distance(goalObj.transform.localPosition, dragObj.transform.localPosition) < AbleDropDis)
        {
            remains--;
            clearconditionText.text = $"성공까지 {remains}개 남음";
            PV.RPC("Dragged_", RpcTarget.Others, remains);
            DropObjects();
            if (remains == 0)
            {
                StartCoroutine(GameEnd(true));
            }
        }
    }
    void DropObjects()
    {
        int randX = Random.Range(-625, 625);
        int randy = Random.Range(-280, 280);
        goalObj.transform.localPosition = new Vector3(randX, randy);
        randX = Random.Range(-625, 625);
        randy = Random.Range(-280, 280);
        dragObj.transform.localPosition = new Vector3(randX, randy);
    }
    [PunRPC]
    void Dragged_(int remain)
    {
        clearconditionText.text = $"성공까지 {remain}개 남음";
    }
    void TimerStart(float startTime, int duration)
    {
        this.startTime = startTime;
        this.duration = duration;
        timerRunning = true;
    }
    IEnumerator GameEnd(bool iswin)
    {
        isGameEnd = true;
        PV.RPC("timerSynchronization_", RpcTarget.Others, "게임 종료");
        PV.RPC("EneEnd", RpcTarget.Others, iswin);
        yield return new WaitForSeconds(2f);
        if (iswin)
            isclearText.text = "성공";
        else
            isclearText.text = "실패";
        gmaeEndObj.SetActive(true);
        yield return new WaitForSeconds(2f);
        playScreenObj.SetActive(false);
        gm.MiniGameEnd_capture(iswin);
    }
    [PunRPC]
    void EneEnd(bool iswin)
    {
        StartCoroutine(EneEnd_(iswin));
    }
}
