using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class ResGm : MonoBehaviour
{
    [SerializeField] Text myScoreText;
    [SerializeField] Text enScoreText;
    [SerializeField] Text resultText;
    [SerializeField] Text infoText;
    [SerializeField] GameObject resultPannel;
    int myScore, eneScore, myGroundScore, eneGroundScore;

    void Start()
    {
        myScore = PlayerPrefs.GetInt("MyScore");
        eneScore = PlayerPrefs.GetInt("EnemyScore");
        myGroundScore = PlayerPrefs.GetInt("MyScoreG");
        eneGroundScore = PlayerPrefs.GetInt("EnemyScoreG");
        StartCoroutine(setText());
    }
    IEnumerator setText()
    {
        infoText.text = "클리어한 미니게임 포인트";
        int sI = 0, sE = 0;
        for (int i = 0; i < myScore; i++)

        {
            yield return new WaitForSeconds(0.05f);
            sI++;
            myScoreText.text = $"{sI}";
        }
        for (int i = 0; i < eneScore; i++)
        {
            yield return new WaitForSeconds(0.05f);
            sE++;
            enScoreText.text = $"{sE}";
        }
        yield return new WaitForSeconds(1f);
        infoText.text = "점령 구역 포인트";
        for (int i = 0; i < myGroundScore; i++)
        {
            yield return new WaitForSeconds(0.05f);
            sI++;
            myScoreText.text = $"{sI}";
        }
        for (int i = 0; i < eneGroundScore; i++)
        {
            yield return new WaitForSeconds(0.05f);
            sE++;
            enScoreText.text = $"{sE}";
        }
        infoText.text = "결산 종료";
        yield return new WaitForSeconds(3f);
        int resme = myScore + myGroundScore;
        int resenemy = eneScore + eneGroundScore;
        if (resme > resenemy)
        {
            resultText.text = "승리";
            //연속 = 1승 = 2승 + 2 = 3승 + 3 ~ 10스 + 10 .. 이후 그대로 10;
            RankManager.Instance.OnGroundScoreVictory(20);
        }
        else if (resme == resenemy)
        {
            resultText.text = "무승부";
        }
        else if (resme < resenemy)
        {
            resultText.text = "패배";
        }
        resultPannel.SetActive(true);
        yield return new WaitForSeconds(3f);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Main");
    }
}
