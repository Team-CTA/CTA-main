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
            Debug.Log("Player Name : " + userName);
        }
    }

    private void OnLeaderboardReceived(GetLeaderboardResult result)
    {
        // 기존 UI 아이템 초기화
        foreach (Transform child in Content)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in result.Leaderboard)
        {
            string countryCode = "kr"; // 강민재 : 기본 값

            if (item.Profile.Locations != null)
            {
                countryCode = item.Profile.Locations[0].CountryCode.Value.ToString().ToLower();
                Debug.Log("User Country : " + countryCode);
            }
            else
            {
                Debug.LogWarning("국가 정보 없음, 기본값 'kr' 사용."); // 강민재 : 확인 -> 수정 필요
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

    private void OnError(PlayFabError error)
    {
        Debug.LogError("리더보드 에러 : " + error.GenerateErrorReport());
    }
}
