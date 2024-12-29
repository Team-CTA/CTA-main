// using UnityEngine;

// public class Test : MonoBehaviour
// {
//     void Start()
//     {
//         //강민재 : 랭크 로드 확인용
//         Debug.Log(PlayerPrefs.GetInt("UserRank"));
//         Debug.Log(PlayerPrefs.GetInt("UserScore"));
//     }
//     void Update()
//     {
//         //Debug.Log("+ rank" + PlayerPrefs.GetInt("UserRank"));
//         //Debug.Log("+ score" + PlayerPrefs.GetInt("UserScore"));
//     }

// }

// 자신의 랭크 를 PlayFab DB에서 가져오기
// using PlayFab;
// using PlayFab.ClientModels;
// using UnityEngine;
// using UnityEngine.UI;

// public class UserRankDisplay : MonoBehaviour
// {
//     public Text myRankText; // 강민재 : 출력용
//     public Text myScoreText; // 강민재 : 출력용

//     private void Start()
//     {
//         GetMyData();
//     }

//     private void GetMyData()
//     {
//         var request = new GetUserDataRequest();

//         PlayFabClientAPI.GetUserData(request, OnUserDataReceived, OnError);
//     }

//     private void OnUserDataReceived(GetUserDataResult result)
//     {
//         if (result.Data != null)
//         {
//             if (result.Data.ContainsKey("UserRank-Data") && result.Data.ContainsKey("UserScore-Data"))
//             {
//                 int rank = int.Parse(result.Data["UserRank-Data"].Value);
//                 int score = int.Parse(result.Data["UserScore-Data"].Value);

//                 myRankText.text = "Rank: " + GetRankSuffix(rank);
//                 myScoreText.text = "Score: " + score.ToString();
//             }
//             else
//             {
//                 Debug.LogWarning("UserRank-Data or UserScore-Data is not available.");
//             }
//         }
//     }

//     private void OnError(PlayFabError error)
//     {
//         Debug.LogError("Error retrieving user data: " + error.GenerateErrorReport());
//     }

//     private string GetRankSuffix(int rank)
//     {
//         if (rank % 10 == 1 && rank != 11)
//             return rank + "st";
//         else if (rank % 10 == 2 && rank != 12)
//             return rank + "nd";
//         else if (rank % 10 == 3 && rank != 13)
//             return rank + "rd";
//         else
//             return rank + "th";
//     }
// }
//-------------------------------------------------


// 강민재 : 다른 유저 이름으로 그 유저 DB 정보 불러오기
// using PlayFab;
// using PlayFab.ClientModels;
// using UnityEngine;
// using UnityEngine.UI;

// public class OtherUserRankDisplay : MonoBehaviour
// {

//     public string targetUserName; // 강민재 : 다른 유저이름 입력용
//     string otherUserRank;

//     public void GetOtherUser()
//     {
//         GetOtherUserData(targetUserName);
//     }

//     private void GetOtherUserData(string username)
//     {
//         var request = new GetUserDataRequest()
//         {
//             PlayFabId = username
//         };

//         PlayFabClientAPI.GetUserData(request, OnUserDataReceived, OnError);
//     }

//     private void OnUserDataReceived(GetUserDataResult result)
//     {
//         if (result.Data != null)
//         {
//             if (result.Data.ContainsKey("UserRank-Data"))
//             {
//                 int rank = int.Parse(result.Data["UserRank-Data"].Value);
//                 otherUserRank = rank.ToString();  // 강민재 : 다른 유저의 랭크를 otherUserRank 에 String으로 담음
//                 Debug.Log("Other User Rank : " + otherUserRank);
//             }
//             else
//             {
//                 Debug.LogWarning("not found : " + targetUserName);
//             }
//         }
//     }

//     private void OnError(PlayFabError error)
//     {
//         Debug.LogError("에러 : " + error.GenerateErrorReport());
//     }

// }

using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class OtherUserRankDisplay : MonoBehaviour
{
    public string targetUserName;
    string otherUserRank;

    public void GetOtherUser()
    {
        GetPlayFabIdFromUserName(targetUserName);
    }

    private void GetPlayFabIdFromUserName(string username)
    {
        var request = new GetAccountInfoRequest()
        {
            Username = username
        };

        PlayFabClientAPI.GetAccountInfo(request, OnAccountInfoReceived, OnError);
    }

    private void OnAccountInfoReceived(GetAccountInfoResult result)
    {
        if (result != null && result.AccountInfo != null)
        {
            string playFabId = result.AccountInfo.PlayFabId;

            GetOtherUserData(playFabId);
        }
        else
        {
            Debug.LogWarning("User not found: " + targetUserName);
        }
    }

    private void GetOtherUserData(string playFabId)
    {
        var request = new GetUserDataRequest()
        {
            PlayFabId = playFabId
        };

        PlayFabClientAPI.GetUserData(request, OnUserDataReceived, OnError);
    }

    private void OnUserDataReceived(GetUserDataResult result)
    {
        if (result.Data != null)
        {
            if (result.Data.ContainsKey("UserRank-Data"))
            {
                int rank = int.Parse(result.Data["UserRank-Data"].Value);
                otherUserRank = rank.ToString();
                Debug.Log("Other User Rank : " + otherUserRank);
            }
            else
            {
                Debug.LogWarning("Rank data not found for user: " + targetUserName);
            }
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
    }
}
