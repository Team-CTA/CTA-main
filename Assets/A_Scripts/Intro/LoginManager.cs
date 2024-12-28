using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class Login_ : MonoBehaviour
{
    public string titleId = "1E0CD";
    [SerializeField] string username;
    [SerializeField] string password;
    [SerializeField] string emailAdress;
    [SerializeField] GameObject[] LoginObj;
    [SerializeField] GameObject[] RegisterObj;
    [SerializeField] Text currentState;
    [SerializeField] TMP_InputField user;
    [SerializeField] TMP_InputField pass;
    [SerializeField] TMP_InputField emailField;
    bool isSignin = false;

    #region  Unity Methods
    void Start()
    {
        currentState.text = "";
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = titleId;
        }
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (isSignin)
            {
                if (!user.isFocused)
                {
                    user.ActivateInputField();
                }
                else
                {
                    pass.ActivateInputField();
                }
            }
            else
            {
                if (!emailField.isFocused && !user.isFocused)
                {
                    emailField.ActivateInputField();
                }
                else if (!user.isFocused && !pass.isFocused)
                {
                    user.ActivateInputField();
                }
                else
                {
                    pass.ActivateInputField();
                }
            }
        }
    }
    #endregion
    #region UnityUse
    public void LoginOrSignin()
    {
        if (!isSignin)
        {
            LoginWithPlayFabRequest loginRequest = new LoginWithPlayFabRequest();
            loginRequest.Username = username;
            loginRequest.Password = password;
            PlayFabClientAPI.LoginWithPlayFab(loginRequest,
            result =>
            {
                Debug.Log($"로그인 성공. {username} - 접속중...");
                UpdateDisplayName(username);
                PlayerPrefs.SetString("USERNAME", username);
                PlayerPrefs.SetString("PASSWORD", password);
                currentState.text = "로그인 완료! 접속중...";
                StartCoroutine(SceneDelayChange("Main", 1f));
            }, OnFailure, null);
        }
        else
        {
            if (!CheckUsername()) return; // 여따는 이름길이 적을때 할거
            // 여기 나중에 이메일 확인도 넣든 해야지.
            RegisterPlayFabUserRequest signinRequest = new RegisterPlayFabUserRequest();
            signinRequest.Email = emailAdress;
            signinRequest.Username = username;
            signinRequest.Password = password;
            PlayFabClientAPI.RegisterPlayFabUser(signinRequest,
            result =>
            {
                Debug.Log("계정 생성됨.");
                currentState.text = "계정이 생성되었습니다. [" + username + "]";
                StartScore();
            }, OnFailure);
        }
    }
    private void StartScore()
    {
        var leaderboardRequest = new UpdatePlayerStatisticsRequest()
        {
            Statistics = new System.Collections.Generic.List<StatisticUpdate>()
        {
            new StatisticUpdate()
            {
                StatisticName = "GroundScore",
                Value = 0
            }
        }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(leaderboardRequest,
            result =>
            {
                Debug.Log("GroundScore 설정 성공");
            },
            error =>
            {
                Debug.LogError("리더보드에 GroundScore 설정 오류 : " + error.GenerateErrorReport());
            });
    }
    public void SetUsername(string name)
    {
        username = user.text;
        PlayerPrefs.SetString("USERNAME", username);

    }
    public void SetPassword(string pw)
    {
        Debug.Log(pw);
        password = pass.text;
        PlayerPrefs.SetString("PASSWORD", password); // 강민재 : 자동 로그인을 위한 패스워드 저장
    }
    public void SetEmail(string email)
    {
        Debug.Log(email);
        emailAdress = emailField.text;
    }
    public void OpenSignin()
    {
        OpenRegister(isSignin ? false : true);
        isSignin = isSignin ? false : true;
    }
    #endregion
    #region Ex
    IEnumerator SceneDelayChange(string scene, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(scene);
    }
    void OpenRegister(bool isOpen = true)
    {
        foreach (GameObject obj in RegisterObj)
        {
            obj.SetActive(isOpen ? true : false);
        }
        LoginObj[0].SetActive(isOpen ? false : true);
        LoginObj[1].GetComponent<TextMeshProUGUI>().text = isOpen ? "Sign In" : "Login";
        LoginObj[2].GetComponent<Text>().text = isOpen ? "Login" : "Sign In";
    }
    bool CheckUsername()
    {
        bool isVaild = false;
        if (username.Length >= 3 && username.Length <= 24 && !username.Contains(" ") && !ContainsSpecialCharacter(username))
            isVaild = true;
        if (!isVaild)
        {
            Debug.Log("닉네임 길이 조건 : 3자이상 24자 이하");
            if (username.Contains(" ") || ContainsSpecialCharacter(username))
                currentState.text = "유저 이름에는 공백 또는 특수문자를 사용할 수 없습니다.";
            else
                currentState.text = "유저 이름은 3자 이상 24자 이하로 설정해주세요.";
        }
        return isVaild;
    }
    void UpdateDisplayName(string displayname)
    {
        Debug.Log($"Playfab계정의 DisplayName변경중... : {displayname}");
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = displayname };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
        result => Debug.Log("displayname변경완료"), OnFailure);
    }
    void OnFailure(PlayFabError error)
    {
        Debug.Log($"오류 발생 {error.GenerateErrorReport()}");
        Debug.Log(username + "|" + password);
        currentState.text = "error |" + error.GenerateErrorReport();
    }
    bool ContainsSpecialCharacter(string input)
    {
        foreach (char c in input)
        {
            if (!char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))
            {
                return true;
            }
        }
        return false;
    }
    #endregion


}