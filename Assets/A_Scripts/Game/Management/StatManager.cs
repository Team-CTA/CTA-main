using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    private int user_wins = 0;
    private int user_losses = 0;
    private int user_Draws = 0;

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

        PlayFabSettings.TitleId = "1E0CD";
    }

    public void WinScore()
    {
        user_wins++;
        UpdateUserStat();
    }

    public void LossesScore()
    {
        user_losses++;
        UpdateUserStat();
    }

    public void DrawsScore()
    {
        user_Draws++;
        UpdateUserStat();
    }

    private int CalculateWinRate(int wins, int losses)
    {
        if (wins + losses == 0) return 0;
        float winRate = (float)wins / (wins + losses);
        return Mathf.RoundToInt(winRate * 100);
    }

    private void UpdateUserStat()
    {
        var request = new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "Wins", Value = user_wins },
                new StatisticUpdate { StatisticName = "Losses", Value = user_losses },
                new StatisticUpdate { StatisticName = "Draws", Value = user_Draws },
                new StatisticUpdate { StatisticName = "WinRate", Value = CalculateWinRate(user_wins, user_losses) }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreUpdated, OnError);
    }

    private void OnScoreUpdated(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Score updated successfully");
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
    }

    //-------------------------------
    public int GetTotalGames()
    {
        return user_wins + user_losses + user_Draws;
    }

    public int GetWinRate()
    {
        return CalculateWinRate(user_wins, user_losses);
    }

    public int GetWins() => user_wins;
    public int GetLosses() => user_losses;
    public int GetDraws() => user_Draws;
}
