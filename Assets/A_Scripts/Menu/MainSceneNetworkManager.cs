using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using Unity.Mathematics;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class MainSceneNetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] MainSceneManager sceneManager;
    [SerializeField] Text start_startbutton;
    [SerializeField] Text start_matchruntime;
    [SerializeField] GameObject start_matchingPnl;
    [SerializeField] TransitionScript transition;

    public string nickName;
    bool start_onMatching = false;
    bool start_MATCHFOUND = false;
    bool start_matchClicked = false;
    float start_matchRuntimeFloat;


    #region Unity Method
    private void Awake()
    {
        nickName = PlayerPrefs.GetString("USERNAME");
    }
    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady || PhotonNetwork.IsConnected) return;
        ConnectToPhoton();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) // <+=========ㅇ내ㅜㄹ얀뮤럄냥ㄹ 테스트용 지워야함 나중에
        {
            SceneManager.LoadScene("inGame");
        }


        if (start_onMatching && !start_MATCHFOUND)
        {
            start_matchRuntimeFloat += Time.deltaTime;
            int transTime = (int)math.floor(start_matchRuntimeFloat);
            string time = (transTime >= 3600 ? (transTime / 3600).ToString() + ":" : "") + ((transTime % 3600 / 60) < 10 && transTime >= 3600 ? "0" : "") + (transTime >= 60 ? (transTime % 3600 / 60).ToString() + ":" : "") + (transTime % 60 < 10 && transTime >= 60 ? "0" : "") + (transTime % 60).ToString();
            start_matchruntime.text = "Matching |  " + time;
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                start_MATCHFOUND = true;

                StartCoroutine(MatchFound());
            }
        }
        if (Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.P))
        {
            start_matchRuntimeFloat += 5;
        }
    }
    #endregion

    #region Coroutine
    IEnumerator MatchPnlContol()
    {
        Vector3 plus = new Vector3(0, start_onMatching ? -10 : 10);
        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
            start_matchingPnl.transform.localPosition += plus;
        }
    }
    IEnumerator MatchFound()
    {
        start_matchruntime.text = "Match Found";
        yield return new WaitForSeconds(2f);
        start_matchruntime.text = "Game Starts in 3";
        yield return new WaitForSeconds(1f);
        start_matchruntime.text = "Game Starts in 2";
        yield return new WaitForSeconds(1f);
        start_matchruntime.text = "Game Starts in 1";
        yield return new WaitForSeconds(1f);
        start_matchruntime.text = "Game Starts in 0";
        transition.OutT("NULL");
        yield return new WaitForSeconds(3f);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("inGame");
        }
    }
    #endregion


    #region Public Methods
    public void Start_MatchMaking()
    {
        if (start_matchClicked || start_MATCHFOUND) return;
        start_matchClicked = true;
        Invoke("MatchStartClickCool", 1f);
        if (!start_onMatching)
        {
            start_onMatching = true;
            start_matchRuntimeFloat = 0;
            start_startbutton.text = "Cancel";
            StartCoroutine(MatchPnlContol());
            switch (sceneManager.start_selectedgame)
            {
                case "Normal":
                    RoomMatching("Normal");
                    break;
                case "Ranked":
                    break;
                case "Custom":
                    break;
                default:
                    Debug.Log("[!] 오류나뮤;");
                    break;
            }
        }
        else
        {
            start_onMatching = false;
            start_startbutton.text = "Start";
            StartCoroutine(MatchPnlContol());
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        }
    }
    #endregion



    #region Private Methods
    void MatchStartClickCool()
    {
        start_matchClicked = false;
    }
    void RoomMatching(string matchType)
    {
        if (start_startbutton.text == "Cancel")
        {
            Debug.Log($"[@] 매칭 시작 : {matchType}");
            string roomName = nickName + "_" + Guid.NewGuid().ToString();
            RoomOptions ro = GetRoomOptions(matchType);

            // PhotonNetwork.JoinRandomOrCreateRoom(ro.CustomRoomProperties, (byte)ro.MaxPlayers, MatchmakingMode.FillRoom, TypedLobby.Default, null, roomName, ro, null);
            // PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
            // PhotonNetwork.JoinRandomRoom(ro.CustomRoomProperties, 2, MatchmakingMode.FillRoom, TypedLobby.Default, null, null);
            // PhotonNetwork.JoinRandomOrCreateRoom(ro.CustomRoomProperties, 2, MatchmakingMode.FillRoom, TypedLobby.Default, null, roomName, ro, null);
            // ㄴ수많은 실패의 흔적...

            PhotonNetwork.JoinRandomOrCreateRoom(
                expectedCustomRoomProperties: new Hashtable() { { "GAMEMODE", matchType } }, expectedMaxPlayers: 2, // 참가할 때의 기준.
                roomOptions: ro, roomName: roomName // 생성할 때의 기준.
            );
        }
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log($"[+] 마스터 서버 접속 ({PhotonNetwork.CloudRegion})");
        if (!PhotonNetwork.InLobby) PhotonNetwork.JoinLobby();
    }
    RoomOptions GetRoomOptions(string type)
    {
        RoomOptions ro = new RoomOptions();
        ro.IsOpen = true;
        ro.IsVisible = true;
        ro.MaxPlayers = 2;


        string[] roomProperties = { "GAMEMODE" };

        Hashtable customRoomProperties = new Hashtable()
            { {"GAMEMODE", type} };//nickName+"님의 방"
        // Photon Hashtable = 방의 커스텀 속성을 저장하는 데 사용됨. 
        // 이 속성은 방을 생성하거나 변경할 때 설정할 수 있음.
        // 다른 플레이어가 방에 입장하거나 방 목록을 검색할 때 로비에서 참조할 수 있음.
        // 게임 모드: 방의 게임 모드, 레벨 정보 등 다양한 속성을 Hashtable을 통해 관리가능.
        // 로비 검색: CustomRoomPropertiesForLobby에 지정된 키를 사용해 방 목록을 필터링하거나 검색가능.

        // 나중에 바꾸든 해야지 속성
        // 랭크 만들 때 속성에 랭크 범위 넣어서 하면 되겠네.
        // 아니었어..

        ro.CustomRoomPropertiesForLobby = roomProperties;
        ro.CustomRoomProperties = customRoomProperties;
        // ro.CustomRoomPropertiesForLobby = new string[] { };  // 커스텀 속성 비활성화
        // ro.CustomRoomProperties = new Hashtable();  // 커스텀 속성 비활성화


        return ro;
    }
    void ConnectToPhoton()
    {
        Debug.Log($"[+] 포톤 접속 : {nickName}");
        PhotonNetwork.AuthValues = new AuthenticationValues(nickName);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = nickName;
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion


    #region Photon Callbacks

    public override void OnJoinedLobby()
    {
        Debug.Log("[+] 로비 접속");
    }
    public override void OnJoinedRoom()
    {
        Debug.Log($"[+] 방 접속 : {PhotonNetwork.CurrentRoom.Name}");
        for (int i = 1; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            Debug.Log($"플레이어 {i} > {PhotonNetwork.CurrentRoom.Players[i].NickName}");
        }
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[!] 방 접속 실패 ({returnCode} : {message})");
        // 재시도
        RoomMatching(sceneManager.start_selectedgame);
    }
    public override void OnCreatedRoom()
    {
        Debug.Log($"[+] 방 생성 : {PhotonNetwork.CurrentRoom.Name}");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[!] 방 접속 실패 ({message})");
    }
    public override void OnLeftRoom()
    {
        Debug.LogError($"[!] 방 접속 취소됨");
    }
    #endregion
}
