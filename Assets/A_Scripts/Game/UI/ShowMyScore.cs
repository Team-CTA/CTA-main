using UnityEngine;
using UnityEngine.UI;

public class ShowMyScore : MonoBehaviour
{
    public Text MyScore;
    public Text MyWIN;
    public Text MyLOSSES;
    public Text MyDRAW;
    public Text MyPLAY;
    public Text MyWINRATE;
    void Start()
    {
        MyScore.text = PlayerPrefs.GetInt("UserScore").ToString();

        MyWIN.text = StatManager.Instance.GetUserWins().ToString();
        MyLOSSES.text = StatManager.Instance.GetUserLosses().ToString();
        MyDRAW.text = StatManager.Instance.GetUserDraws().ToString();
        MyPLAY.text = StatManager.Instance.GetTotalGames().ToString();
        MyWINRATE.text = StatManager.Instance.GetUserWinRate().ToString() + "%";


        Debug.Log(PlayerPrefs.GetInt("UserRank")); //강민재 : 유저 랭크 가져오는 법
        Debug.Log(PlayerPrefs.GetInt("UserScore")); //강민재 : 유저 스코어 가져오는 법

        Debug.Log(StatManager.Instance.GetTotalGames()); //강민재 : 총 게임 플레이수 가져오는 법
        Debug.Log(StatManager.Instance.GetUserWins()); //강민재 : 총 이긴 횟수 가져오는 법
        Debug.Log(StatManager.Instance.GetUserLosses()); //강민재 : 총 진 횟수 가져오는 법
        Debug.Log(StatManager.Instance.GetUserDraws()); //강민재 : 총 무승부한 횟수 가져오는 법
        Debug.Log(StatManager.Instance.GetUserWinRate()); //강민재 : 승률 가져오는 법
    }
}
