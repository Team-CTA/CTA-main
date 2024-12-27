using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System.Collections.Generic;


public class RankingManager : MonoBehaviour
{
    public Text leaderboardText;  // UI 텍스트 컴포넌트, 리더보드 랭킹을 보여줄 곳
    public int numberOfEntries = 10;  // 가져올 리더보드 랭킹 개수

    void Start()
    {
        GetLeaderboard();
    }

    // 리더보드 데이터를 가져오는 함수
    void GetLeaderboard()
    {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
        {
            StatisticName = "GroundScore",  // 리더보드에서 사용할 통계 이름
            StartPosition = 0,  // 첫 번째 랭킹부터 가져오기
            MaxResultsCount = numberOfEntries  // 가져올 랭킹 수
        },
        result =>
        {
            // 요청 성공 시
            DisplayLeaderboard(result.Leaderboard);
        },
        error =>
        {
            // 요청 실패 시
            Debug.LogError("Error retrieving leaderboard: " + error.GenerateErrorReport());
        });
    }

    // 리더보드 데이터를 UI에 표시하는 함수
    void DisplayLeaderboard(List<PlayerLeaderboardEntry> leaderboard)
    {
        leaderboardText.text = "Leaderboard:\n";

        foreach (var entry in leaderboard)
        {
            leaderboardText.text += $"{entry.Position + 1}. {entry.DisplayName} - {entry.StatValue}\n";
        }
    }
    //--------------------
    void UpdatePlayerScore(int score)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
            new StatisticUpdate {
                StatisticName = "GroundScore",
                Value = score
            }
        }
        },
        result =>
        {
            Debug.Log("Player score updated successfully");
        },
        error =>
        {
            Debug.LogError("Error updating score: " + error.GenerateErrorReport());
        });
    }

}

