using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Dice : MonoBehaviourPun
{
    public bool rollAble = false;
    int myNum, over;
    [SerializeField] Text numText;
    [SerializeField] Text howtoText;
    [SerializeField] Text playernameText;
    [SerializeField] Text clearconditionText;
    [SerializeField] Text isclearText;
    [SerializeField] GameObject gmaeEndObj;
    [SerializeField] GameObject playScreenObj;
    public GameManager gm;
    PhotonView PV;
    private void Start()
    {
        PV = photonView;
    }
    public void ShowG()
    {
        bool isActive = (gm.curGamename == "[ 행운 ]");
        PV.RPC("SyncShowG", RpcTarget.All, isActive);
    }

    [PunRPC]
    void SyncShowG(bool isActive)
    {
        transform.GetChild(0).gameObject.SetActive(isActive);
    }
    [PunRPC]
    void SyncShowG_()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
    public void GameStart(int difficulty, string playername)
    {
        ShowG();
        if (difficulty == 4 || difficulty == 0)
        {
            return;
        }
        rollAble = false;
        howtoText.gameObject.SetActive(false);
        howtoText.text = "상대 입장 대기중...";
        playernameText.text = $"게임 진행중 : {playername}";
        numText.text = "X";
        if (difficulty == 1) over = 2;
        else if (difficulty == 2) over = 4;
        else if (difficulty == 3) over = 6;
        clearconditionText.text = $"성공조건 : {over} 이상";
        gmaeEndObj.SetActive(false);
        playScreenObj.SetActive(true);
        PV.RPC("Starting", RpcTarget.Others, playername, over);
        StartCoroutine(InProgress());
    }
    [PunRPC]
    void OpennedPnl()
    {
        howtoText.text = "[ 위로 스와이프 ]";
        StartCoroutine(InProgress());
    }
    [PunRPC]
    void Starting(string name, int dif)
    {
        gmaeEndObj.SetActive(false);
        howtoText.gameObject.SetActive(false);
        playernameText.text = $"게임 진행중 : {name}";
        clearconditionText.text = $"성공조건 : {dif} 이상";
        numText.text = "X";
        playScreenObj.SetActive(true);
        StartCoroutine(WaitOpening());
    }
    IEnumerator WaitOpening()
    {
        while (!playScreenObj.gameObject.activeSelf)
        {
            yield return null;
        }
        PV.RPC("OpennedPnl", RpcTarget.Others);
    }
    IEnumerator InProgress()
    {
        yield return new WaitForSeconds(0.2f);
        howtoText.gameObject.SetActive(true);
        rollAble = true;
    }
    public void RollStarts()
    {
        howtoText.gameObject.SetActive(false);
    }
    public void Rolled(int number)
    {
        myNum = number;
        numText.text = number.ToString();
        StartCoroutine(GameEnd());
        PV.RPC("Rolled_", RpcTarget.Others, number, over);
    }
    [PunRPC]
    void Rolled_(int number, int ovr)
    {
        over = ovr;
        Debug.Log("음?");
        myNum = number;
        numText.text = number.ToString();

        StartCoroutine(GameEnd_());
    }
    IEnumerator GameEnd()
    {
        yield return new WaitForSeconds(2f);
        if (myNum >= over)
            isclearText.text = "성공";
        else
            isclearText.text = "실패";
        gmaeEndObj.SetActive(true);
        yield return new WaitForSeconds(2f);
        playScreenObj.SetActive(false);
        gm.MiniGameEnd_capture(myNum >= over);
        rollAble = false;
        PV.RPC("SyncShowG_", RpcTarget.All);
    }
    IEnumerator GameEnd_()
    {
        yield return new WaitForSeconds(2f);
        if (myNum >= over)
            isclearText.text = "성공";
        else
            isclearText.text = "실패";
        gmaeEndObj.SetActive(true);
        yield return new WaitForSeconds(2f);
        playScreenObj.SetActive(false);
        gm.EnClose();
    }
}
