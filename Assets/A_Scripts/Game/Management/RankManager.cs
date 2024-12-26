using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class RankManager : MonoBehaviour
{
    public static RankManager Instance { get; private set; }

    private int user_rank = 0;
    public UnityEngine.UI.Text rankText;  // 강민재 : UI 텍스트를 연결할 변수

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
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
            Keys = new List<string> { "UserRank" }
        }, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("UserRank"))
            {
                user_rank = int.Parse(result.Data["UserRank"].Value);
                UpdateUI();
            }
        }, error =>
        {
            Debug.LogError("Failed to load user rank: " + error.ErrorMessage);
        });
    }

    private void UpdateUI()
    {
        if (rankText != null) // 강민재 : 형식상 체크 기능 추가함..ㅎ
        {
            rankText.text = "Rank: " + user_rank.ToString();
        }
    }

    public void AddRankPoint(int amount)
    {
        user_rank += amount;
        UpdateUI();  // 강민재 : UI 업데이트
        SavePlayerData();  // 강민재 : 서버에 저장(플래이팹)
    }

    private void SavePlayerData()
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>
            {
                { "UserRank", user_rank.ToString() },
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
}
