using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public GameObject User;
    public Transform Content;

    public Text myName;
    public Text myRank;

    private string userName;
    private int userRank = -1;

    private void Start()
    {
        PlayFabSettings.TitleId = "1E0CD"; // 강민재 : 일단 이런식으로 세팅했는데, 수정해야할듯
        GetLeaderboard();
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest()
        {
            StatisticName = "GroundScore",
            StartPosition = 0,
            MaxResultsCount = 50
        };

        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardReceived, OnError);

        GetUserData();
    }

    private void GetUserData()
    {
        var request = new GetUserDataRequest();

        PlayFabClientAPI.GetUserData(request, OnUserDataReceived, OnError);
    }

    private void OnUserDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("PlayerName"))
        {
            userName = result.Data["PlayerName"].Value;
            Debug.Log("Player Name: " + userName);
        }
    }

    private void OnLeaderboardReceived(GetLeaderboardResult result)
    {
        foreach (Transform child in Content)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in result.Leaderboard)
        {
            GameObject leaderboardItem = Instantiate(User, Content);

            Text rankingText = leaderboardItem.transform.Find("Rank").GetComponent<Text>();
            Text usernameText = leaderboardItem.transform.Find("Name").GetComponent<Text>();
            Text trophyText = leaderboardItem.transform.Find("GS").GetComponent<Text>();

            rankingText.text = GetRankSuffix(item.Position + 1);
            SetRankingColor(rankingText, item.Position);

            usernameText.text = item.DisplayName;
            trophyText.text = item.StatValue.ToString();

            if (item.DisplayName == PlayerPrefs.GetString("USERNAME"))
            {
                Debug.Log("세팅 성공!");
                userRank = item.Position + 1;

                myName.text = PlayerPrefs.GetString("USERNAME");
                myRank.text = GetRankSuffix(userRank);
                SetRankingColor(myRank, userRank - 1);
            }
        }
    }

    private string GetRankSuffix(int rank)
    {
        if (rank % 10 == 1 && rank != 11)
            return rank + "st";
        else if (rank % 10 == 2 && rank != 12)
            return rank + "nd";
        else if (rank % 10 == 3 && rank != 13)
            return rank + "rd";
        else
            return rank + "th";
    }

    private void SetRankingColor(Text rankingText, int position)
    {
        if (position == 0)
        {
            rankingText.color = new Color(1f, 0.84f, 0f);
        }
        else if (position == 1)
        {
            rankingText.color = new Color(0.75f, 0.75f, 0.75f);
        }
        else if (position == 2)
        {
            rankingText.color = new Color(0.8f, 0.52f, 0.25f);
        }
        else
        {
            rankingText.color = new Color(1f, 1f, 1f);
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error fetching leaderboard: " + error.GenerateErrorReport());
    }
}
