using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;


public class IntroManager : MonoBehaviour
{
    [SerializeField] Animator introAni;
    [SerializeField] Animator LoginStart;
    [SerializeField] Text introTxt;

    void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
    }
    void Start()
    {
        StartCoroutine(IntroAnim());
    }
    float txtDelay = 0.11f;
    string gameName = "Capture The Area";
    IEnumerator IntroAnim()
    {
        yield return new WaitForSeconds(1f);
        introTxt.text = "_";
        yield return new WaitForSeconds(0.5f);
        introTxt.text = "";
        yield return new WaitForSeconds(0.5f);
        introAni.SetTrigger("IntroStart");
        for (int i = 1; i <= gameName.Length; i++)
        {
            introTxt.text = gameName.Substring(0, i);
            yield return new WaitForSeconds(txtDelay);
        }
        yield return new WaitForSeconds(2.5f);
        print("Login"); //자동 로그인 확인 구간
        if (PlayerPrefs.HasKey("USERNAME"))
        {
            string username = PlayerPrefs.GetString("USERNAME");
            string password = PlayerPrefs.GetString("PASSWORD"); // 강민재 : 혹시 모를 보안 문제 체크 필요

            LoginWithPlayFabRequest loginRequest = new LoginWithPlayFabRequest
            {
                Username = username,
                Password = password
            };

            PlayFabClientAPI.LoginWithPlayFab(loginRequest,
                result =>
                {
                    Debug.Log($"자동 로그인 성공 : {username}");
                    SceneManager.LoadScene("Main");
                },
                error =>
                {
                    Debug.Log($"자동 로그인 실패: {error.GenerateErrorReport()}"); //강민재 : 일단 에러 띄우는걸로 대체
                    LoginStart.SetTrigger("Execute"); // 강민재 : 일단 추가 함
                }
            );
        }
        else
        {
            LoginStart.SetTrigger("Execute");
        }
    }
}
