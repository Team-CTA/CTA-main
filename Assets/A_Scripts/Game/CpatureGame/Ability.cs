using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Ability : MonoBehaviourPun
{
    public bool canInput = false;
    int curArrayIndex;
    List<string> keyArray;
    bool timerAct = false, isGameEnd = false;
    double duration, startTime;
    List<KeyCode> keys = new List<KeyCode>(){
        KeyCode.Q,KeyCode.W,KeyCode.E,KeyCode.R,KeyCode.T,
        KeyCode.Y,KeyCode.U,KeyCode.I,KeyCode.O,KeyCode.P,
        KeyCode.A,KeyCode.S,KeyCode.D,KeyCode.F,KeyCode.G,
        KeyCode.H,KeyCode.J,KeyCode.K,KeyCode.L,KeyCode.Z,
        KeyCode.X,KeyCode.C,KeyCode.V,KeyCode.B,KeyCode.N,
        KeyCode.M,KeyCode.Space,KeyCode.Tab,
    };
    [SerializeField] Text clearconditionText;
    [SerializeField] Text playernameText;
    [SerializeField] Text timerText;
    [SerializeField] Text isclearText;
    [SerializeField] Text[] keyShowTexts;
    [SerializeField] GameObject gmaeEndObj;
    [SerializeField] GameObject playScreenObj;
    [SerializeField] GameManager gm;

    PhotonView PV;
    private void Start()
    {
        PV = photonView;
    }
    private void Update()
    {
        if (canInput)
        {
            foreach (KeyCode keyCode in keys)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    InputKey(keyCode.ToString());
                }
            }
        }
        if (timerAct)
        {
            double elapsedTime = Time.time - startTime;
            double remainingTime = duration - elapsedTime;

            if (remainingTime <= 0)
            {
                timerAct = false;
                canInput = false;
                remainingTime = 0;
                StartCoroutine(GameEnd(false));
            }
            timerText.text = $"남은시간 | {math.floor(remainingTime * 100) * 0.01}초";

            //코루틴써서 0.2초마다 rpc로 시간동기화
        }
    }
    public void GameStart(int difficulty, string playername)
    {
        if (difficulty == 4 || difficulty == 0)
        {
            return;
        }
        timerAct = false;
        isGameEnd = false;
        gmaeEndObj.SetActive(false);
        playernameText.text = $"게임 진행중 : {playername}";
        curArrayIndex = 0;
        keyArray = new List<string>();
        for (int i = 0; i < difficulty * 10; i++)
        {
            int rnd = Random.Range(0, keys.Count);
            keyArray.Add(keys[rnd].ToString());
        }
        clearconditionText.text = $"남은 키 | {keyArray.Count}";
        SetShows();
        timerText.text = "5초 후 시작";
        playScreenObj.SetActive(true);
        PV.RPC("Starting", RpcTarget.Others, playername, keyArray.Count);
        StartCoroutine(InProgress());
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
        canInput = true;
    }
    void TimerStart(float startTime, int duration)
    {
        this.startTime = startTime;
        this.duration = duration;
        timerAct = true;
    }
    IEnumerator timerSynchronization()
    {
        while (!isGameEnd)
        {
            yield return new WaitForSeconds(0.15f);
            PV.RPC("timerSynchronization_", RpcTarget.Others, timerText.text);
        }
    }
    [PunRPC]
    void timerSynchronization_(string txt)
    {
        timerText.text = txt;
    }
    [PunRPC]
    void Starting(string name, int dif)
    {
        gmaeEndObj.SetActive(false);
        playernameText.text = $"게임 진행중 : {name}";
        clearconditionText.text = $"남은 키 | {dif}";
        timerText.text = "곧 게임 시작";
        playScreenObj.SetActive(true);
    }
    void SetShows()
    {
        keyShowTexts[0].text = curArrayIndex > 1 ? keyArray[curArrayIndex - 2] : "";
        keyShowTexts[1].text = curArrayIndex > 0 ? keyArray[curArrayIndex - 1] : "";
        keyShowTexts[2].text = curArrayIndex == keyArray.Count ? "끝" : keyArray[curArrayIndex];
        keyShowTexts[3].text = curArrayIndex < keyArray.Count - 1 ? keyArray[curArrayIndex + 1] : "";
        keyShowTexts[4].text = curArrayIndex < keyArray.Count - 2 ? keyArray[curArrayIndex + 2] : "";
        List<string> arr = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            arr.Add(keyShowTexts[i].text);
        }
        PV.RPC("SetShows_", RpcTarget.All, arr.ToArray(), keyArray.Count, curArrayIndex);
    }
    [PunRPC]
    void SetShows_(string[] arr, int count, int curIndex)
    {
        clearconditionText.text = $"남은 키 | {count - curIndex}";
        for (int i = 0; i < 5; i++)
        {
            keyShowTexts[i].text = arr[i];
        }
    }
    void ArraySet()
    {

    }
    void InputKey(string keyCode)
    {
        if (keyShowTexts[2].text == keyCode)
        {
            curArrayIndex++;
            clearconditionText.text = $"남은 키 | {keyArray.Count - curArrayIndex}";
            SetShows();
            if (curArrayIndex == keyArray.Count)
            {
                StartCoroutine(GameEnd(true));
            }
        }
    }
    IEnumerator GameEnd(bool iswin)
    {
        timerAct = false;
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
}
