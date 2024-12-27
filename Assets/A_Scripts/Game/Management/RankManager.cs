using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class RankManager : MonoBehaviour
{
    public static RankManager Instance { get; private set; }

    private int user_rank_point = 0;
    public UnityEngine.UI.Text rankText;  // UI 텍스트를 연결할 변수
    private const string RankStatisticName = "GroundScore"; // 리더보드에서 사용할 통계 이름

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadRank();
    }

    private void LoadRank()
    {
        // 기존의 UserData에서 랭크 정보를 로드
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
            Keys = new List<string> { "UserRank" }
        }, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("UserRank"))
            {
                user_rank_point = int.Parse(result.Data["UserRank"].Value);
                UpdateUI();
            }
        }, error =>
        {
            Debug.LogError("유저 랭크 로드 실패 : " + error.ErrorMessage);
        });
    }

    private void UpdateUI()
    {
        if (rankText != null)
        {
            rankText.text = "Rank: " + user_rank_point.ToString();
        }
    }

    public void OnGroundScoreVictory(int rank_amount)
    {
        AddRankPoint(rank_amount);
    }

    private void AddRankPoint(int amount)
    {
        user_rank_point += amount;
        UpdateUI();
        SavePlayerData();
        UpdateLeaderboard();
    }

    private void SavePlayerData()
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>
            {
                { "UserRank", user_rank_point.ToString() },
            }
        };
        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log("Level 업데이트!");
        }, error =>
        {
            Debug.LogError("업데이트 실패 : " + error.ErrorMessage);
        });
    }

    private void UpdateLeaderboard()
    {
        // 강민재 : 플레이어의 랭크 점수를 리더보드에 업데이트
        var request = new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>
        {
            new StatisticUpdate()
            {
                StatisticName = RankStatisticName,  // 강민재 : 리더보드에서 사용할 통계 이름
                Value = user_rank_point
            }
        }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            Debug.Log("리더보드 업데이트 성공!");
        }, error =>
        {
            Debug.LogError("리더보드 업데이트 실패: " + error.ErrorMessage);
        });
    }

}
