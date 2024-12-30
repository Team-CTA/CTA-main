using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;

public class StatManager : MonoBehaviour
{
    public GameObject LoadError;
    public static StatManager Instance { get; private set; }
    public bool LoadCheck = false;

    private int user_wins = 0;
    private int user_losses = 0;
    private int user_Draws = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 필요한 경우 사용
        }
        else
        {
            Destroy(gameObject);
        }

        PlayFabSettings.TitleId = "1E0CD";
    }

    private void Start()
    {
        PlayerPrefs.SetInt("WINRATE", 0);
        // Login();
        LoadUserStats();

    }

    private void Login()
    {
        if (PlayerPrefs.HasKey("USERNAME"))
        {
            string username = PlayerPrefs.GetString("USERNAME");
            string password = PlayerPrefs.GetString("PASSWORD");

            LoginWithPlayFabRequest loginRequest = new LoginWithPlayFabRequest
            {
                Username = username,
                Password = password
            };
            PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSuccess, OnLoginError);
        }
        else
        {
            Debug.Log("회원가입 필요.");
        }
    }

    private void OnLoginSuccess(LoginResult result)
    {
        LoadUserStats();
    }

    private void OnLoginError(PlayFabError error)
    {
        Debug.LogError("로드 에러 : " + error.GenerateErrorReport());
        LoadError.SetActive(true);

    }

    private void LoadUserStats()
    {
        var request = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(request, OnStatsLoaded, OnError);
    }

    private void OnStatsLoaded(GetPlayerStatisticsResult result)
    {
        foreach (var stat in result.Statistics)
        {
            if (stat.StatisticName == "Wins")
            {
                user_wins = stat.Value;
            }
            else if (stat.StatisticName == "Losses")
            {
                user_losses = stat.Value;
            }
            else if (stat.StatisticName == "Draws")
            {
                user_Draws = stat.Value;
            }
        }
        UpdateUserStat();
        Debug.Log("GetUserWins : " + GetUserWins());
        Debug.Log("GetUserLosses : " + GetUserLosses());
        Debug.Log("GetUserDraws : " + GetUserDraws());
        Debug.Log("GetUserWinRate : " + GetUserWinRate());

        Debug.Log("WINRATE : " + CalculateWinRate(user_wins, user_losses));
        try
        {
            GameObject.FindWithTag("scr").GetComponent<ShowMyScore>().LoadStat(GetUserWins(), GetUserLosses(), GetUserDraws(), CalculateWinRate(user_wins, user_losses));
        }
        catch (System.Exception)
        {

            throw;
        }
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
        PlayerPrefs.SetInt("WINRATE", CalculateWinRate(user_wins, user_losses));
        PlayerPrefs.Save();
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
        LoadCheck = true;
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

    public int GetUserWins() => user_wins;
    public int GetUserLosses() => user_losses;
    public int GetUserDraws() => user_Draws;
    public int GetUserWinRate() => CalculateWinRate(user_wins, user_losses);
}
