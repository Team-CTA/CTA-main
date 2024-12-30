using UnityEngine;
using UnityEngine.UI;

public class ShowMyScore : MonoBehaviour
{
    public Text MyScore;
    public Text MyPLAY;
    public Text MyWINRATE;
    public void LoadStat(int w, int l, int d, int winrate)
    {
        if (StatManager.Instance.LoadCheck == false)
        {
            // MyScore.text = score.ToString;

            MyPLAY.text = $"{w + l + d}전 {w}승 {l}패 {d}무";
            MyWINRATE.text = $"승률 : {winrate}%";


            Debug.Log(PlayerPrefs.GetInt("UserRank")); //강민재 : 유저 랭크 가져오는 법
            Debug.Log(PlayerPrefs.GetInt("UserScore")); //강민재 : 유저 스코어 가져오는 법

            Debug.Log(StatManager.Instance.GetTotalGames()); //강민재 : 총 게임 플레이수 가져오는 법
            Debug.Log(StatManager.Instance.GetUserWins()); //강민재 : 총 이긴 횟수 가져오는 법
            Debug.Log(StatManager.Instance.GetUserLosses()); //강민재 : 총 진 횟수 가져오는 법
            Debug.Log(StatManager.Instance.GetUserDraws()); //강민재 : 총 무승부한 횟수 가져오는 법
            Debug.Log(StatManager.Instance.GetUserWinRate()); //강민재 : 승률 가져오는 법
        }
    }
    public void LoatScore(int score)
    {
        MyScore.text = score.ToString();
    }
}
