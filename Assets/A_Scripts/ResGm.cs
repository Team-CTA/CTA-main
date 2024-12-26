using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResGm : MonoBehaviour
{
    [SerializeField] Text my;
    [SerializeField] Text en;
    [SerializeField] Text resT;
    [SerializeField] GameObject winT;
    int myScor, eneScor, myGr, eneGr;

    void Start()
    {
        myScor = PlayerPrefs.GetInt("MyScore");
        eneScor = PlayerPrefs.GetInt("EnemyScore");
        myGr = PlayerPrefs.GetInt("MyScoreG");
        eneGr = PlayerPrefs.GetInt("EnemyScoreG");
        StartCoroutine(setText());
    }
    IEnumerator setText()
    {
        my.text = $"{myScor}+{myGr} (나)";
        en.text = $"상대 {eneScor}+{eneGr}";
        yield return new WaitForSeconds(4f);
        int resme = myScor + myGr;
        int resen = eneScor + eneGr;
        //end point.
        if (resme > resen)
        {
            resT.text = "승리";
        }
        else if (resme == resen)
        {
            resT.text = "무승부";
        }
        else if (resme < resen)
        {
            resT.text = "패배";
        }
        winT.SetActive(true);
        yield return new WaitForSeconds(3f);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Main");
    }
}
