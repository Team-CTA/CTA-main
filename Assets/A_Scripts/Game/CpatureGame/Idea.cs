using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using System;

public class Idea : MonoBehaviourPun
{
    public bool clickable = false;
    bool timerRunning = false, isGameEnd = false;
    int jungdap, remains;
    [SerializeField] float colorDif = 0;
    double startTime;
    double duration;
    [SerializeField] Text playernameText;
    [SerializeField] Text timerText;
    [SerializeField] Text clearconditionText;
    [SerializeField] Text isclearText;
    [SerializeField] Image[] images;
    [SerializeField] GameObject imgObj;
    [SerializeField] GameObject gmaeEndObj;
    [SerializeField] GameObject playScreenObj;
    [SerializeField] GameManager gm;

    PhotonView PV;
    void Start()
    {
        PV = photonView;
        if (images != null)
        {
            Debug.Log($"Images array length: {images.Length}");
        }
        else
        {
            Debug.LogError("Images array is null!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && gm.myScript == null)
        {
            jungdap = 0;
        }
        if (!timerRunning || isGameEnd) return;
        double elapsedTime = Time.time - startTime;
        double remainingTime = duration - elapsedTime;

        if (remainingTime <= 0)
        {
            timerRunning = false;
            isGameEnd = true;
            remainingTime = 0;
            GameEnd(false);
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
        timerRunning = false;
        isGameEnd = false;
        clickable = false;
        imgObj.SetActive(false);
        playernameText.text = $"게임 진행중 : {playername}";
        if (difficulty == 3)
        {
            colorDif = 0.05f;
            remains = 4;
        }
        else if (difficulty == 2)
        {
            colorDif = 0.04f;
            remains = 6;
        }
        else if (difficulty == 1)
        {
            colorDif = 0.15f;
            remains = 8;
        }
        clearconditionText.text = $"성공까지 {remains}번 남음";
        timerText.text = "5초 후 시작";
        gmaeEndObj.SetActive(false);
        playScreenObj.SetActive(true);
        ChangeColors();
        PV.RPC("Starting", RpcTarget.Others, playername, remains);
        StartCoroutine(InProgress());
    }
    [PunRPC]
    void Starting(string name, int dif)
    {
        gmaeEndObj.SetActive(false);
        playernameText.text = $"게임 진행중 : {name}";
        clearconditionText.text = $"성공까지 {dif}번 남음";
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
        TimerStart(startTime, 7);
        PV.RPC("imgobjOpen", RpcTarget.Others);
        imgObj.SetActive(true);
        clickable = true;
        // ChangeColors();
    }
    [PunRPC]
    void imgobjOpen(bool isOpen = true)
    {
        imgObj.SetActive(isOpen);
    }
    [PunRPC]
    void ChangeColEne(int i, float r, float g, float b)
    {
        images[i].color = new Color(r, g, b);
    }
    public void ImgClicked(int arr)
    {
        if (arr == jungdap)
        {
            remains--;
            clearconditionText.text = $"성공까지 {remains}개 남음";
            ChangeColors();
            PV.RPC("ImgClicked_", RpcTarget.Others, remains);
        }
        else
        {
            GameEnd(false);
        }
        if (remains == 0) GameEnd(true);
    }
    [PunRPC]
    void ImgClicked_()
    {
        clearconditionText.text = $"성공까지 {remains}개 남음";
    }
    void ChangeColors()
    {
        if (images == null || images.Length == 0)
        {
            Debug.LogError("Images array is not initialized!");
            return;
        }
        jungdap = Random.Range(0, 9);
        Debug.Log($"어어 : {jungdap}");
        float randomR = Random.Range(0.2f, 0.8f);
        float randomG = Random.Range(0.2f, 0.8f);
        float randomB = Random.Range(0.2f, 0.8f);
        Color rndColor = new Color(randomR, randomG, randomB);
        int rndRGBplus = Random.Range(0, 3);
        Color rndPlus = new Color(0, 0, 0);
        if (rndRGBplus == 0)
        {
            rndPlus = new Color(colorDif, 0, 0);
        }
        else if (rndRGBplus == 1)
        {
            rndPlus = new Color(0, colorDif, 0);
        }
        else if (rndRGBplus == 2)
        {
            rndPlus = new Color(0, 0, colorDif);
        }
        Color changedColor = rndColor + rndPlus;
        for (int i = 0; i < 9; i++)
        {
            if (i != jungdap)
            {
                images[i].color = rndColor;
                PV.RPC("ChangeColEne", RpcTarget.Others, i, rndColor.r, rndColor.g, rndColor.b);

            }
            else
            {
                images[i].color = changedColor;
                PV.RPC("ChangeColEne", RpcTarget.Others, i, changedColor.r, changedColor.g, changedColor.b);
            }
        }
    }
    void TimerStart(float startTime, int duration)
    {
        this.startTime = startTime;
        this.duration = duration;
        timerRunning = true;
    }
    void GameEnd(bool iswin)
    {
        isGameEnd = true;
        PV.RPC("imgobjOpen", RpcTarget.Others, false);
        imgObj.SetActive(false);
        PV.RPC("timerSynchronization_", RpcTarget.Others, "게임 종료");
        PV.RPC("EneEnd", RpcTarget.Others, iswin);
        StartCoroutine(End(iswin));
    }
    void EneEnd(bool iswin)
    {
        StartCoroutine(End(iswin));
    }
    IEnumerator End(bool iswin)
    {
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
}
