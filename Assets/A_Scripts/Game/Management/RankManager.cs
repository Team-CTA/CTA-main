using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RankManager : MonoBehaviour
{
    public static RankManager Instance { get; private set; }

    public GameObject User;
    public Transform Content;

    public Text myName;
    public Text myRank;

    private string userName;
    private int userRank = -1;
    private int userScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayFabSettings.TitleId = "1E0CD";
        GetLeaderboard();
    }

    public int Userrank => userRank;
    public int Userscore => userScore;

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest()
        {
            StatisticName = "GroundScore",
            StartPosition = 0,
            MaxResultsCount = 50,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowLocations = true,
                ShowDisplayName = true
            }
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
            Debug.Log("Player Name : " + userName);
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
            string countryCode = "us"; // 강민재 : 기본값 지정한거ㅓ

            if (item.Profile.Locations != null)
            {
                countryCode = item.Profile.Locations[0].CountryCode.Value.ToString().ToLower();
                Debug.Log("User Country : " + countryCode);
            }
            else
            {
                Debug.LogWarning("국가 정보 없음, 기본값 'kr' 사용.");
            }

            GameObject leaderboardItem = Instantiate(User, Content);

            Text rankingText = leaderboardItem.transform.Find("Rank").GetComponent<Text>();
            Text usernameText = leaderboardItem.transform.Find("Name").GetComponent<Text>();
            Text trophyText = leaderboardItem.transform.Find("GS").GetComponent<Text>();

            Image countryImage = leaderboardItem.transform.Find("Country").GetComponent<Image>();

            rankingText.text = GetRankSuffix(item.Position + 1);
            SetRankingColor(rankingText, item.Position);

            usernameText.text = item.DisplayName;
            trophyText.text = item.StatValue.ToString();

            Sprite countrySprite = Resources.Load<Sprite>("CountryImages/" + countryCode);
            if (countrySprite != null)
            {
                countryImage.sprite = countrySprite;
            }
            else
            {
                Debug.LogError("국가 이미지가 없음 : " + countryCode);
            }

            if (item.DisplayName == PlayerPrefs.GetString("USERNAME"))
            {
                userRank = item.Position + 1;
                userScore = item.StatValue;
                Debug.Log("rank 로컬 DB 세팅!");
                PlayerPrefs.SetInt("UserRank", userRank);
                PlayerPrefs.SetInt("UserScore", userScore);
                SaveUserDataToPlayFab(userRank, userScore);

                myName.text = PlayerPrefs.GetString("USERNAME");
                myRank.text = GetRankSuffix(userRank);
                SetRankingColor(myRank, userRank - 1);
            }
        }
    }
    private void SaveUserDataToPlayFab(int rank, int score)
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>
        {
            { "UserRank-Data", rank.ToString() },
            { "UserScore-Data", score.ToString() }
        }
        };

        PlayFabClientAPI.UpdateUserData(request, OnUserDataUpdated, OnError);
    }
    private void OnUserDataUpdated(UpdateUserDataResult result)
    {
        Debug.Log("랭크 데이터 저장!");
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
            rankingText.color = new Color(1f, 0.84f, 0f); // Gold
        }
        else if (position == 1)
        {
            rankingText.color = new Color(0.75f, 0.75f, 0.75f); // Silver
        }
        else if (position == 2)
        {
            rankingText.color = new Color(0.8f, 0.52f, 0.25f); // Bronze
        }
        else
        {
            rankingText.color = new Color(1f, 1f, 1f); // White
        }
    }

    public void AddScore(int amount) // 강민재 : 계산
    {
        userScore += amount;
        if (userScore < 0) userScore = 0;
        UpdateUserScore();
    }

    private void UpdateUserScore()
    {
        var request = new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "GroundScore", Value = userScore }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreUpdated, OnError);
    }

    private void OnScoreUpdated(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Score updated successfully");
        GetLeaderboard();
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("리더보드 에러 : " + error.GenerateErrorReport());
    }
}