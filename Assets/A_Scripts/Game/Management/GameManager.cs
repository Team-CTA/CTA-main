using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviourPunCallbacks
{
    public HexControl hexControl = new HexControl();
    // x값 두배좌표

    [SerializeField] GameObject CheckInfoPnl;
    [SerializeField] Text enNameInfo;
    [SerializeField] Text enRankInfo;
    [SerializeField] Text enWinRateInfo;
    [SerializeField] Text fuck;

    [Header("타일 생성 설정")]
    [SerializeField] GameObject hex0;
    [SerializeField] GameObject hex1;
    [SerializeField] float hex1_tile_spawnRate = 0.05f;
    [SerializeField] float hex0_tile_spawnRate = 0.1f;
    const float hex_width = 1.3625f, hex_height = 2.3599f, hex0_deepth = -1.8f;
    public int size_Hex0 = 2;
    public int size_Hex1 = 3;

    [Header("게임 상태")]
    int difficultyUp; // [ v ]
    int boost; // [ v ]
    bool safety; // [ v ]
    bool overriding; // [ v ]
    bool chainCapture; // [ v ]
    bool remoteCapture; // [ v ]
    int synchronizaion; // [ v ]
    public bool selectable = false;
    public bool infoOpenable = false;
    public bool gameInProgress = false;
    // ㅇㅇ녀ㅗㅑㅕㅁ누랴ㅕㅎㅁ댜럄ㄷ르ㅜ묮러ㅕㅐㅑ토ㅕㅌ호!???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????

    string otherUserRank;
    string otherUserName;
    // ㅇㅇ녀ㅗㅑㅕㅁ누랴ㅕㅎㅁ댜럄ㄷ르ㅜ묮러ㅕㅐㅑ토ㅕㅌ호!???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????

    public int draw = 5;
    int drawRemains;
    public int nanido;
    public int cardSelectingTime = 60;
    public string nickName, curPhase = "";
    public InGameUiController uiController;
    public SoundManager soundManager = null;
    public PlayerScript myScript, eneScript;
    Dictionary<string, bool[]> phaseSet = new Dictionary<string, bool[]>
    {
        {"checkInfo",new bool[]{false, false}},
        {"drawCards",new bool[]{false,false}},
        {"yrFirstArea",new bool[]{false,false}},
        {"myFirstArea",new bool[]{true,false}},
        {"gameSelecting",new bool[]{false,false}},
        {"yrTrun",new bool[]{false,true}},
        {"myTrun",new bool[]{true,true}},
        {"difficultySelecting",new bool[]{false,false}},
        {"CardSelect",new bool[]{false,false}},
        {"inGame",new bool[]{false,false}},
    };//selectable, infoOpenable
    Dictionary<string, string> phaseTexts = new Dictionary<string, string>()
    {
        {"checkInfo","게임 준비 단계"}, //Enemy Information
        {"drawCards","카드를 선택하세요"}, //Draw Ability Cards
        {"yrFirstArea","상대를 기다리는중"}, //Waiting Enemy's Turn
        {"myFirstArea","게임을 시작할 땅을 선택하세요"}, //Select Your Starting Area
        {"gameSelecting","게임 선택중"},
        {"yrTrun","상대를 기다리는중"}, //Waiting Enemy's Turn
        {"myTrun","점령할 땅을 선택하세요"}, //Select Area To Capture
        {"difficultySelecting","난이도 선택중"}, //Choose Difficulty
        {"CardSelect","사용할 카드를 선택하세요"}, // Choose Cards to use
        {"inGame","게임 진행중"}, //Game In Progress
    };
    public Dictionary<string, Tuple<string, int>> cards = new Dictionary<string, Tuple<string, int>>()
    {// refCardCheck
        {"데드록",Tuple.Create("상대 카드의 효과 무력화",0)},
        {"오버라이드",Tuple.Create("상대가 미니게임 실패 시 해당 구역 점령",0)},
        // {"취약점 증폭",Tuple.Create("획득 포인트를 유지하며 미니게임의 난이도 한 단계 증가 (3단계인 경우 미니게임 자동실패)",0)},
        {"체인 캡쳐링",Tuple.Create("미니게임 성공 시 상대 한 턴 스킵 (미니게임 난이도 3으로 조정)",1)},
        {"동기화",Tuple.Create("미니게임 성공 시 해당 영역 주변 빈 땅 2개를 선택하여 점령",1)},
        {"리모트 캡쳐링",Tuple.Create("미니게임 성공 시 어디든 빈 땅 1개를 선택하여 점령",1)},
        {"부스트",Tuple.Create("미니게임 성공 시 획득 포인트 x2",2)},
        {"안전망",Tuple.Create("미니게임 실패 시 획득 포인트 없이 해당 구역 점령",2)},
        {"최적화",Tuple.Create("획득 포인트를 유지하며 미니게임 난이도 한 단계 감소 (1단계인 경우 미니게임 자동성공)",2)},
    }; // 상반된 속성은 0에서 1더하거나 1빼는식으로 계산해서 작동될지 안될지 파악하자
    // 카드 보여주는 턴엔 효과 상충 안보여주고 발동될때 [카드 발동] 옆에서 패널 밀려오면서 잠깐 보여줬다 없애기
    // 획득포인트는 바로 스코어보드에 표현
    // 마지막 출가는 안표현
    string[] randomCardKeys = {
        "데드록","오버라이드",
        "체인 캡쳐링","동기화","리모트 캡쳐링",
        "부스트","안전망","최적화",
    };
    public List<Card> cardEntriesI = new List<Card>();
    public List<Card> cardEntriesEnemy = new List<Card>();

    string lastClickedhex = null;

    public class RefCardCheck
    {// 내려갈수록 코드 실행 우선순위 높아짐
        //지원
        public bool a_boost = false;
        public bool a_safetyWeb = false;
        public bool a_easier = false;

        // 점령
        public bool a_chainCapture = false;
        public bool a_remoteCapture = false;
        public int a_synchronization = 0;

        // 공격
        public bool a_overriding = false;
        public bool a_deadlock = false;
        public bool a_harder = false;
    }
    public RefCardCheck refCardCheck;
    PhotonView PV;
    public bool GameProgress = false;

    [Header("사용 오브젝트")]
    [SerializeField] GameObject phasePannel; // 결투 겜중에 없앨듯
    [SerializeField] GameObject cardBase;
    [SerializeField] GameObject playCapureGameObj;
    [SerializeField] GameObject playCapureGameScreenObj;
    [SerializeField] Transform cardCase;
    [SerializeField] Text phaseText;
    public Sprite[] cardTypeImages;
    [SerializeField] Text ScoreText;

    [Header("미니게임 스크립트")]
    [SerializeField] Dice diceGame;
    [SerializeField] Gear gearGame;
    [SerializeField] Idea ideaGame;
    [SerializeField] Ability abilityGame;

    #region Unity Default
    void Start()
    {
        playCapureGameObj.SetActive(false);
        PV = photonView;
        StartCoroutine(CreateMap(size_Hex1));
        StartCoroutine(Hex0_tile_spawn(size_Hex0, size_Hex1));
        nickName = PlayerPrefs.GetString("USERNAME");
        GameObject MyObj = null;
        try
        {
            Debug.Log("인게임 : " + PhotonNetwork.CurrentRoom.Name);

            // ㅇㅇ녀ㅗㅑㅕㅁ누랴ㅕㅎㅁ댜럄ㄷ르ㅜ묮러ㅕㅐㅑ토ㅕㅌ호!???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????

            for (int i = 1; i <= PhotonNetwork.CurrentRoom.PlayerCount; i++)
                Debug.Log($"인게임 플레이어 {i} > {PhotonNetwork.CurrentRoom.Players[i].NickName}");
            // ㅇㅇ녀ㅗㅑㅕㅁ누랴ㅕㅎㅁ댜럄ㄷ르ㅜ묮러ㅕㅐㅑ토ㅕㅌ호!???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????


            MyObj = PhotonNetwork.Instantiate("player", Vector3.zero, Quaternion.identity); // 스트링인 프리펩 이름임! 대소문자 구별 안함
            // FindPlayerScript();
        }
        catch
        {
            Debug.Log("TestCase");
        }
        if (MyObj == null)
        {
            StartCoroutine(NoPlayer());
        }
        else
        {
            NextPhase();
        }
    }
    IEnumerator NoPlayer()
    {
        GameObject obj = null;
        while (obj == null)
        {
            try
            {
                obj = PhotonNetwork.Instantiate("player", Vector3.zero, Quaternion.identity); // 스트링인 프리펩 이름임! 대소문자 구별 안함
            }
            catch
            {
                Debug.Log("[!] 플레이어 오브젝트 생성 실패");
            }
            yield return null;
        }
        NextPhase();
    }
    void EndGame()
    {
        PV.RPC("EndEne", RpcTarget.Others);
        PlayerPrefs.SetInt("MyScore", myScript.score);
        PlayerPrefs.SetInt("EnemyScore", eneScript.score);
        int myGroundScore = 0;
        int eneGroundScore = 0;
        foreach (Hex item in hexControl.hexes)
        {
            if (item.captured_by == "me") myGroundScore++;
            else if (item.captured_by == "yu") eneGroundScore++;
        }
        PlayerPrefs.SetInt("MyScoreG", myGroundScore);
        PlayerPrefs.SetInt("EnemyScoreG", eneGroundScore);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("EndGame");
    }
    [PunRPC]
    void EndEne()
    {
        PlayerPrefs.SetInt("MyScore", myScript.score);
        PlayerPrefs.SetInt("EnemyScore", eneScript.score);
        int myGroundScore = 0;
        int eneGroundScore = 0;
        foreach (Hex item in hexControl.hexes)
        {
            if (item.captured_by == "me") myGroundScore++;
            else if (item.captured_by == "yu") eneGroundScore++;
        }
        PlayerPrefs.SetInt("MyScoreG", myGroundScore);
        PlayerPrefs.SetInt("EnemyScoreG", eneGroundScore);
        SceneManager.LoadScene("EndGame");
        PhotonNetwork.LeaveRoom();
    }
    bool Blue = false;
    bool Red = false;
    [SerializeField] AudioClip[] sounds;
    private void Update()
    {
        if (soundManager == null)
        {
            soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            soundManager.PlayBGM(sounds[0]);
        }
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.R))
        {
            Red = true; //밑에 지우기
        }
        else
        {
            Red = false;
        }
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.B))
        {
            Blue = true;
        }
        else
        {
            Blue = false;
        }
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.E) || Check_Jum() && GameProgress)
        {
            Debug.Log("끝체크!!!!!!!!");
            EndGame();
        }
        if (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.Q))
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Main");
        }
        try
        {
            ScoreText.text = $"{eneScript.score} : {myScript.score} (나)";
        }
        catch (System.Exception)
        {

            throw;
        }
    }
    #endregion
    #region PunRPC
    IEnumerator FindPlayerScript()
    {
        while (eneScript == null || myScript == null)
        {
            foreach (GameObject item in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (myScript == null && item.GetPhotonView().IsMine)
                {
                    myScript = item.GetComponent<PlayerScript>();
                }
                else if (eneScript == null && !item.GetPhotonView().IsMine)
                {
                    eneScript = item.GetComponent<PlayerScript>();
                }
            }
            yield return null;
            Debug.Log("re>");
        }
        CheckInfoPnl.SetActive(true);
        enNameInfo.text = eneScript.gameObject.GetPhotonView().Owner.NickName;
        PV.RPC("RankShow", RpcTarget.Others, $"Rank #{PlayerPrefs.GetInt("UserRank")} ({"GS : " + PlayerPrefs.GetInt("UserScore")})", $"WinRate {PlayerPrefs.GetInt("WINRATE")}%");

        // 여녀ㅑ로ㅑㅁ롬7료8ㅁ됴 여깅요 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // ㅇㅇ녀ㅗㅑㅕㅁ누랴ㅕㅎㅁ댜럄ㄷ르ㅜ묮러ㅕㅐㅑ토ㅕㅌ호!???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????


        // ㅇㅇ녀ㅗㅑㅕㅁ누랴ㅕㅎㅁ댜럄ㄷ르ㅜ묮러ㅕㅐㅑ토ㅕㅌ호!???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????

        Debug.Log(PlayerPrefs.GetInt("UserRank")); //강민재 : 유저 랭크 가져오는 법
        Debug.Log(PlayerPrefs.GetInt("UserScore")); //강민재 : 유저 스코어 가져오는 법

        Debug.Log(StatManager.Instance.GetTotalGames()); //강민재 : 총 게임 플레이수 가져오는 법
        Debug.Log(StatManager.Instance.GetUserWins()); //강민재 : 총 이긴 횟수 가져오는 법
        Debug.Log(StatManager.Instance.GetUserLosses()); //강민재 : 총 진 횟수 가져오는 법
        Debug.Log(StatManager.Instance.GetUserDraws()); //강민재 : 총 무승부한 횟수 가져오는 법
        Debug.Log(StatManager.Instance.GetUserWinRate()); //강민재 : 승률 가져오는 법


        // 여녀ㅑ로ㅑㅁ롬7료8ㅁ됴 여깅요 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


        yield return new WaitForSeconds(5f);
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("FirTurnSel", RpcTarget.MasterClient, Random.Range(0, 2));
            Debug.Log("isMaster>");
        }
    }
    [PunRPC]
    void RankShow(string rank, string winrate)
    {
        enRankInfo.text = rank;
        enWinRateInfo.text = winrate;
    }

    // ㅇㅇ녀ㅗㅑㅕㅁ누랴ㅕㅎㅁ댜럄ㄷ르ㅜ묮러ㅕㅐㅑ토ㅕㅌ호!???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
    // private void GetOtherUserData(string username)
    // {
    //     var request = new GetUserDataRequest()
    //     {
    //         PlayFabId = username
    //     };

    //     PlayFabClientAPI.GetUserData(request, OnUserDataReceived, OnError);
    // }
    private void GetPlayFabIdFromUserName(string username)
    {
        Debug.Log(username);
        var request = new GetAccountInfoRequest()
        {
            Username = username
        };

        PlayFabClientAPI.GetAccountInfo(request, OnAccountInfoReceived, OnError);
    }


    // private void OnUserDataReceived(GetUserDataResult result)
    // {
    //     if (result.Data != null)
    //     {
    //         if (result.Data.ContainsKey("UserRank-Data"))
    //         {
    //             int rank = int.Parse(result.Data["UserRank-Data"].Value);
    //             otherUserRank = rank.ToString();  // 강민재 : 다른 유저의 랭크를 otherUserRank 에 String으로 담음
    //             Debug.Log("Other User Rank : " + otherUserRank);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("not found : " + otherUserName);
    //         }
    //     }
    // }
    private void OnAccountInfoReceived(GetAccountInfoResult result)
    {
        if (result != null && result.AccountInfo != null)
        {
            string playFabId = result.AccountInfo.PlayFabId;

            GetOtherUserData(playFabId);
        }
        else
        {
            Debug.LogWarning("User not found: " + otherUserName);
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
                Debug.LogWarning("not found : " + otherUserName);
            }
        }
    }
    //----------------------------

    private void OnError(PlayFabError error)
    {
        Debug.LogError("에러 : " + error.GenerateErrorReport());
    }

    // ㅇㅇ녀ㅗㅑㅕㅁ누랴ㅕㅎㅁ댜럄ㄷ르ㅜ묮러ㅕㅐㅑ토ㅕㅌ호!???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????

    [PunRPC]
    void FirTurnSel(int rnd)
    {
        Debug.Log(eneScript.myturn);
        Debug.Log(myScript.myturn);
        if (rnd == 1)
        {
            myScript.myturn = true;
        }
        else
        {
            myScript.myturn = false;
        }
        PV.RPC("NoMaster", RpcTarget.Others, rnd);
        uiController.CheckInfoSet();
    }
    [PunRPC]
    void NoMaster(int rnd)
    {
        if (rnd == 1)
        {
            myScript.myturn = false;
        }
        else
        {
            myScript.myturn = true;
        }
        uiController.CheckInfoSet();
    }
    #endregion
    #region Pun Callbacks
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 여기 나갔을때 패배처리하는거 넣기
        PlayerPrefs.SetInt("ENEXIT", 1);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("EndGame");
    }
    private void OnApplicationQuit()
    {
        StatManager.Instance.LossesScore();
        RankManager.Instance.AddScore(-20);
    }
    #endregion
    #region Creating Map
    readonly Vector3[] posChange =
        {
            new Vector3(-hex_width, 0, hex_height),
            new Vector3(-hex_width*2, 0, 0),
            new Vector3(-hex_width, 0, -hex_height),
            new Vector3(hex_width, 0, -hex_height),
            new Vector3(hex_width*2, 0, 0),
            new Vector3(hex_width, 0, hex_height),
            new Vector3(-hex_width, 0, hex_height),
        };
    readonly int[] xyChange = { 0, -1, 1, 0, 1, -1, 0, -1, -1, 0, 1, 1 };
    IEnumerator CreateMap(int size)
    {
        int x = 0, y = 0;
        Vector3 pos = Vector3.zero;
        hexControl.SpawnHex1(Instantiate(hex1), "Center", pos, x, y, 0);
        for (int i = 1; i <= size; i++)
        {
            pos += posChange[0];
            hexControl.SpawnHex1(Instantiate(hex1), $"({x - 1},{y + 1})_FirstTile[{i}]", pos, --x, ++y, i);
            yield return new WaitForSeconds(hex1_tile_spawnRate);
            for (int j = 0; j < 6; j++)
            {
                int s = (j == 0) ? i - 1 : i;
                for (int k = 0; k < s; k++)
                {
                    pos += posChange[j + 1];
                    if (j == 0 || j == 3) x += j * 4 / 3 - 2;
                    else
                    {
                        x += xyChange[j];
                        y += xyChange[j + 6];
                    }
                    hexControl.SpawnHex1(Instantiate(hex1), $"({x},{y} : {i})", pos, x, y, i);
                    yield return new WaitForSeconds(hex1_tile_spawnRate);
                }
            }
        }
    }
    IEnumerator Hex0_tile_spawn(int size_hex0, int size_Hex1)
    {
        Vector3 pos = new Vector3(0, hex0_deepth);
        int i;
        for (i = 1; i <= size_Hex1; i++)
        {
            pos += posChange[0];
            for (int j = 0; j < 6; j++)
            {
                int s = (j == 0) ? i - 1 : i;
                for (int k = 0; k < s; k++) pos += posChange[j + 1];
            }
        }
        for (int j = i; j <= size_Hex1 + size_hex0; j++)
        {
            pos += posChange[0];
            hexControl.SapwnHex0(Instantiate(hex0), "BackHex", pos);
            yield return new WaitForSeconds(hex0_tile_spawnRate);
            for (i = 0; i < 6; i++)
            {
                int s = (i == 0) ? j - 1 : j;
                for (int k = 0; k < s; k++)
                {
                    pos += posChange[i + 1];
                    hexControl.SapwnHex0(Instantiate(hex0), "BackHex", pos);
                    yield return new WaitForSeconds(hex1_tile_spawnRate);
                }
            }
        }
        yield return new WaitForSeconds(2f);
    }
    #endregion
    #region Hexe Classes
    public class HexControl
    {
        public List<Hex> hexes;
        public HexControl() { hexes = new List<Hex>(); }
        public Hex SpawnHex1(GameObject tile,
        string name, Vector3 pos, int x = 0, int y = 0, int cir = 0)
        {
            tile.name = name;
            tile.transform.position = pos;
            tile.transform.parent = GameObject.Find("Tiles_Hex1").transform;
            Hex hex = new Hex(tile, x, y, cir);
            hexes.Add(hex);
            return hex;
        }
        public GameObject SapwnHex0(GameObject tile, string name, Vector3 pos)
        {
            tile.transform.parent = GameObject.Find("Tiles_Hex0").transform;
            tile.transform.position = pos;
            tile.name = name;
            return tile;
        }
        public Hex FindHex(GameObject obj)
        {
            foreach (Hex item in hexes)
            {
                if (item.Tile.gameObject == obj.transform.parent.gameObject)
                {
                    return item;
                }
            }
            return null;
        }


        // 특정 타일이 감싸졌는지 확인 (기존 코드)


        // 모든 적 타일이 감싸졌는지 확인

    }
    public class Hex
    {
        public int x, y, cir, getPoint;
        public string captured_by;
        public GameObject Tile;
        public Hex1 script;
        public Hex(GameObject tile, int x, int y, int cir)
        {
            this.x = x;
            this.y = y;
            this.cir = cir;
            getPoint = 0;
            captured_by = "";
            Tile = tile;
            script = tile.transform.GetChild(0).GetComponent<Hex1>();
        }
    }
    #endregion
    #region HexControl
    private readonly Vector2Int[] neighborOffsets = {
        new Vector2Int(-1,  1), // 좌상
        new Vector2Int(-2,  0), // 좌
        new Vector2Int(-1, -1), // 좌하
        new Vector2Int( 1, -1), // 우하
        new Vector2Int( 2,  0), // 우
        new Vector2Int( 1,  1)  // 우상
    };
    private bool IsEncircled(Hex targetHex)
    {
        if (Count_Space(targetHex) == 0)
        {
            return true;

        }
        return false;
    }
    public List<Hex> GetEncircledTiles()
    {
        List<Hex> encircledTiles = new List<Hex>();

        foreach (Hex hex in hexControl.hexes)
        {
            if (hex.captured_by == "me" && IsEncircled(hex))
            {
                encircledTiles.Add(hex);
            }
        }

        return encircledTiles;
    }
    bool Check_Jum()
    {
        List<Hex> encircledTiles = GetEncircledTiles();
        int cnt = 0;
        foreach (Hex item in hexControl.hexes)
        {
            if (item.captured_by == "me")
            {
                cnt++;
            }
        }
        Debug.Log(encircledTiles.Count + "<<<<<<<<<<<<<<<<<<< encircledTiles");
        Debug.Log(cnt + "<<<<<<<<<<<<<<<<<<< cnt");
        if (encircledTiles.Count == cnt)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<Hex> smallerAreaHexes;
    List<Hex>[] AreaHexes = { null, null };
    List<Hex> check_array;
    public bool Check_Divide(Hex capturedHex)//반갈죽됐는지 확인
    {
        AreaHexes[0] = new List<Hex>();
        AreaHexes[1] = new List<Hex>();
        smallerAreaHexes = new List<Hex>();
        check_array = new List<Hex>();
        int order = 0;

        foreach (Hex hex in hexControl.hexes)
        {
            if (hex.captured_by != "me" && !check_array.Contains(hex))
            {
                check_array.Add(hex);
                nearhex(hex, hex.captured_by, order);
                order++;
                if (order == 2)
                    break;
            }
        }
        if (order == 1)
            return false;

        if (AreaHexes[0].Count < AreaHexes[1].Count)
        {
            foreach (Hex x in AreaHexes[0])
            {
                smallerAreaHexes.Add(x);
            }
        }
        else
        {
            foreach (Hex x in AreaHexes[1])
            {
                smallerAreaHexes.Add(x);
            }
        }
        return true;
    }
    public bool Check_Jumryeong()// 점령됐는지 확인
    {
        AreaHexes[0] = new List<Hex>();
        AreaHexes[1] = new List<Hex>();
        check_array = new List<Hex>();
        int order = 0;

        foreach (Hex hex in hexControl.hexes)
        {
            if (hex.captured_by != "me" && !check_array.Contains(hex))
            {
                check_array.Add(hex);
                nearhex(hex, hex.captured_by, order);
                order++;
                if (order == 2)
                    break;
            }
        }
        if (order == 1)
            return false;
        return true;
    }
    public bool Check_Nearhex(Hex capturedHex)//주변에 같은 hex있는지 확인
    {
        AreaHexes[0] = new List<Hex>();
        AreaHexes[1] = new List<Hex>(); // 적 = "yu" 나 = "me" 없음 = ""
        check_array = new List<Hex>();
        Dictionary<int, Dictionary<int, Hex>> hex_xy = new Dictionary<int, Dictionary<int, Hex>>(); // hexdict[1][1] = HEX;
        // List<List<Hex>> hex_xy =
        List<int> dx = new List<int>() { -1, 1, 2, 1, -1, -2 };
        List<int> dy = new List<int>() { 1, 1, 0, -1, -1, 0 };
        foreach (Hex item in hexControl.hexes)
        {
            if (!hex_xy.ContainsKey(item.x))
            {
                hex_xy[item.x] = new Dictionary<int, Hex>();
            }
            hex_xy[item.x][item.y] = item;
        }
        for (int i = 0; i < 6; i++)
        {
            if (hex_xy.ContainsKey(capturedHex.x + dx[i]) && hex_xy[capturedHex.x + dx[i]].ContainsKey(capturedHex.y + dy[i]))
            {
                if (hex_xy[capturedHex.x + dx[i]][capturedHex.y + dy[i]].captured_by == "me")
                {
                    // capturedHex.captured_by = "" 
                    //hex_xy[capturedHex.x + dx[i]][capturedHex.y + dy[i]].captured_by =""
                    return true;
                }
            }
        }
        return false;
    }

    public bool Two_Same(Hex Hex1, Hex Hex2)//주변에 같은 hex있는지 확인
    {
        AreaHexes[0] = new List<Hex>();
        AreaHexes[1] = new List<Hex>(); // 적 = "yu" 나 = "me" 없음 = ""
        check_array = new List<Hex>();
        Dictionary<int, Dictionary<int, Hex>> hex_xy = new Dictionary<int, Dictionary<int, Hex>>(); // hexdict[1][1] = HEX;
        // List<List<Hex>> hex_xy =
        if (Hex1.captured_by != "" || Hex2.captured_by != "me")// 혹시 캡쳐 안되면 ㅇ여기보셈
            return false;
        List<int> dx = new List<int>() { -1, 1, 2, 1, -1, -2 };
        List<int> dy = new List<int>() { 1, 1, 0, -1, -1, 0 };
        foreach (Hex item in hexControl.hexes)
        {
            if (!hex_xy.ContainsKey(item.x))
            {
                hex_xy[item.x] = new Dictionary<int, Hex>();
            }
            hex_xy[item.x][item.y] = item;
        }
        for (int i = 0; i < 6; i++)
        {
            if (hex_xy.ContainsKey(Hex1.x + dx[i]) && hex_xy[Hex1.x + dx[i]].ContainsKey(Hex1.y + dy[i]))
            {
                if (Hex1.x + dx[i] == Hex2.x && Hex1.y + dy[i] == Hex2.y)//hex_xy[][] )
                {
                    // capturedHex.captured_by = "" 
                    //hex_xy[capturedHex.x + dx[i]][capturedHex.y + dy[i]].captured_by =""
                    return true;
                }
            }
        }
        return false;
    }


    public int Count_Space(Hex capturedHex)
    {
        AreaHexes[0] = new List<Hex>();
        AreaHexes[1] = new List<Hex>(); // 적 = "yu" 나 = "me" 없음 = ""
        check_array = new List<Hex>();
        int count = 0;
        Dictionary<int, Dictionary<int, Hex>> hex_xy = new Dictionary<int, Dictionary<int, Hex>>(); // hexdict[1][1] = HEX;
        // List<List<Hex>> hex_xy =
        List<int> dx = new List<int>() { -1, 1, 2, 1, -1, -2 };
        List<int> dy = new List<int>() { 1, 1, 0, -1, -1, 0 };
        foreach (Hex item in hexControl.hexes)
        {
            if (!hex_xy.ContainsKey(item.x))
            {
                hex_xy[item.x] = new Dictionary<int, Hex>();
            }
            hex_xy[item.x][item.y] = item;
        }
        for (int i = 0; i < 6; i++)
        {
            if (hex_xy.ContainsKey(capturedHex.x + dx[i]) && hex_xy[capturedHex.x + dx[i]].ContainsKey(capturedHex.y + dy[i]))
            {
                if (hex_xy[capturedHex.x + dx[i]][capturedHex.y + dy[i]].captured_by == "")
                {
                    // capturedHex.captured_by = "" 
                    //hex_xy[capturedHex.x + dx[i]][capturedHex.y + dy[i]].captured_by =""
                    count++;
                }
            }
        }
        Debug.Log($"count = {count}");
        return count;
    }

    int nearhex(Hex hex, string color, int order)
    {
        Dictionary<int, Dictionary<int, Hex>> hex_xy = new Dictionary<int, Dictionary<int, Hex>>();
        List<int> dx = new List<int>() { -1, 1, 2, 1, -1, -2 };
        List<int> dy = new List<int>() { 1, 1, 0, -1, -1, 0 };
        foreach (Hex item in hexControl.hexes)
        {
            if (!hex_xy.ContainsKey(item.x))
            {
                hex_xy[item.x] = new Dictionary<int, Hex>();
            }
            hex_xy[item.x][item.y] = item;
        }
        for (int i = 0; i < 6; i++)
        {
            if (hex_xy.ContainsKey(hex.x + dx[i]) && hex_xy[hex.x + dx[i]].ContainsKey(hex.y + dy[i]))
            {
                if (hex_xy[hex.x + dx[i]][hex.y + dy[i]].captured_by == color && !check_array.Contains(hex_xy[hex.x + dx[i]][hex.y + dy[i]]))
                {
                    AreaHexes[order].Add(hex_xy[hex.x + dx[i]][hex.y + dy[i]]);
                    check_array.Add(hex_xy[hex.x + dx[i]][hex.y + dy[i]]);
                    nearhex(hex_xy[hex.x + dx[i]][hex.y + dy[i]], color, order);
                }
            }
        }
        return 0;
    }

    List<Hex> GetEmptyArea() // 빈땅 반환
    {
        List<Hex> emptyArea = new List<Hex>();
        foreach (Hex hex in hexControl.hexes)
        {
            if (hex.captured_by == "")
            {
                emptyArea.Add(hex);
            }
        }
        return emptyArea;
    }
    // void TestCase()
    // {
    //     for (int i = 0; i < 37; i++)
    //     {
    //         if (hexControl.hexes[i].cir <= 1)
    //         {
    //             hexControl.hexes[i].captured_by = "p1";
    //         }
    //         else
    //         {
    //             hexControl.hexes[i].captured_by = "p2";
    //         }
    //     }
    //     hexControl.hexes[36].captured_by = "p1";
    //     Debug.Log(Check_Nearhex(hexControl.hexes[36]));
    // }
    #endregion
    #region Card
    public class Card
    {
        public string name;
        public string info;
        public Sprite type;
        public GameObject Object;
        public Card(GameObject card, string name, string info, Sprite type)
        {
            this.name = name;
            this.info = info;
            this.type = type;
            card.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = name;
            card.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = info;
            card.transform.GetChild(0).GetChild(2).GetComponent<Image>().sprite = type;
            Object = card;
        }
    }
    public Card MakeCard(string name)
    {
        return new Card(Instantiate(cardBase, cardCase), name, cards[name].Item1, cardTypeImages[cards[name].Item2]);
    }
    public void CardDraw()
    {
        string[] rndCards = new string[]{
            randomCardKeys[Random.Range(0, 2)],
            randomCardKeys[Random.Range(2, 5)],
            randomCardKeys[Random.Range(5, 8)]
        };
        uiController.Draw(rndCards, drawRemains);
        drawRemains--;
    }
    public void CardClicked(string name)
    {
        uiController.UnDraw(drawRemains);
        uiController.EnemyCardSelected(1, draw);
        myScript.cards.Add(MakeCard(name));
    }
    IEnumerator WaitAllDraw()
    {
        while (!eneScript.cardSelected || !myScript.cardSelected)
        {
            yield return new WaitForSeconds(0.2f);
        }
        uiController.SelEnd();
        NextPhase();
    }
    public void EndDraw()
    {
        uiController.ShowCard(draw, true);
    }
    public void selectCards(string gamename)
    {
        List<Card> cardlist = new List<Card>();
        if (myScript.myturn)
        {
            foreach (Card item in myScript.cards)
            {
                if (cards[item.name].Item2 != 0)
                {
                    cardlist.Add(item);
                }
            }
        }
        else
        {
            foreach (Card item in myScript.cards)
            {
                if (cards[item.name].Item2 == 0)
                {
                    cardlist.Add(item);
                }
            }
        }
        uiController.OpenCardSelect(cardlist, cardSelectingTime, gamename);
    }
    public void EntryCard(List<Card> mylist, List<Card> enelist)
    {
        refCardCheck = new RefCardCheck();
        cardEntriesI = mylist;
        cardEntriesEnemy = enelist;
        List<string> entireEntries = new List<string>();
        for (int i = 0; i < cardEntriesI.Count; i++)
            entireEntries.Add(cardEntriesI[i].name);
        for (int i = 0; i < cardEntriesEnemy.Count; i++)
            entireEntries.Add(cardEntriesEnemy[i].name);
        foreach (string item in entireEntries)
        {
            if (item == "데드록") refCardCheck.a_deadlock = true;
            else if (item == "취약점 증폭") refCardCheck.a_harder = true;
            else if (item == "오버라이드") refCardCheck.a_overriding = true;
            else if (item == "체인 캡쳐링") refCardCheck.a_chainCapture = true;
            else if (item == "동기화") refCardCheck.a_synchronization = 2;
            else if (item == "리모트 캡쳐링") refCardCheck.a_remoteCapture = true;
            else if (item == "안전망") refCardCheck.a_safetyWeb = true;
            else if (item == "부스트") refCardCheck.a_boost = true;
            else if (item == "최적화") refCardCheck.a_easier = true;
        }
    }
    #endregion
    #region PhaseManagement
    void ChangePhase(string phase)
    {
        curPhase = phase;
        selectable = phaseSet[phase][0];
        infoOpenable = phaseSet[phase][1];
        phaseText.text = phaseTexts[phase];
        uiController.UpdateActivity();
    }

    public void NextPhase(bool gameSel = false, int a = 0)
    {
        if (!gameSel)
        {
            if (curPhase == "")
            {
                ChangePhase("checkInfo"); // 카드 드로우 생략   phasePannel.SetActive(false);
                StartCoroutine(FindPlayerScript());
            }
            else if (curPhase == "checkInfo")
            {
                ChangePhase("drawCards");
                drawRemains = draw;
                StartCoroutine(WaitAllDraw());
                uiController.EnemyCardSelected(0, draw);
                CardDraw();
            }
            else if (curPhase == "drawCards")
            {
                uiController.ShowTurn();
                if (myScript.myturn)
                {
                    // uiController.HexSelTime();
                    ChangePhase("myFirstArea");
                }
                else
                {
                    ChangePhase("yrFirstArea");
                }
            }
            else if (curPhase == "myFirstArea")
            {

                if (eneScript.firstAreaSelected)
                {
                    myScript.myturn = false;
                    eneScript.myturn = true;
                    ChangePhase("yrTrun");
                }
                else
                {
                    myScript.myturn = false;
                    eneScript.myturn = true;
                    ChangePhase("yrFirstArea");
                }
                // uiController.ShowTurn();
            }
            else if (curPhase == "yrFirstArea")
            {
                if (myScript.firstAreaSelected)
                {
                    myScript.myturn = true;
                    eneScript.myturn = false;
                    // uiController.HexSelTime();
                    ChangePhase("myTrun");
                }
                else
                {
                    myScript.myturn = true;
                    eneScript.myturn = false;
                    ChangePhase("myFirstArea");
                }
                // uiController.ShowTurn();
            }
            // else if (curPhase == "yrTrun") // 여기 턴 횟수도 선택 안했을댄 무시하고 넘ㄱ기기
            // {
            //     // uiController.HexSelTime();
            //     myScript.myturn = true; // 나중에 selectable로 간소화 가능할듯 =============================
            //     eneScript.myturn = false;
            //     ChangePhase("myTrun");
            // }
            // else if (curPhase == "myTrun")
            // {
            //     myScript.myturn = false;
            //     eneScript.myturn = true;
            //     ChangePhase("yrTrun");
            // }
        }
        else
        {
            if (a == 0)
            {
                ChangePhase("gameSelecting");
            }
            else if (a == 1)
            {
                ChangePhase("difficultySelecting");
            }
            else if (a == 2)
            {
                ChangePhase("CardSelect");
            }
            else if (a == 3)
            {
                ChangePhase("inGame");
                Debug.Log("프로그래스 트루");
                GameProgress = true;
            }
        }
    }
    [PunRPC]
    void AllChangePhase(string str)
    {
        bool isGame = false;
        int a = 0;
        if (str != "")
        {
            isGame = true;
            a = int.Parse(str);
        }
        NextPhase(isGame, a);
    }
    #endregion
    #region AreaManagement
    public void HexSelTimeEnd()
    {
        // PV.RPC("AllChangePhase", RpcTarget.All, "");
    }
    public void AreaCaptured(string capturedhex)
    {
        GameObject obj = GameObject.Find(capturedhex).transform.GetChild(0).gameObject;
        Hex thisHex = hexControl.FindHex(obj);
        thisHex.captured_by = "me";
        thisHex.script.Capture();
        PV.RPC("EnemyCaptured", RpcTarget.Others, capturedhex);

        //페이즈 넘길때 카드 넣기 카드는 자체적으로 가면 될듯 페이즈매니저 말고
        //nanio변수값만큼 포인트
        // 포인트 추가
        // 애니메이션, 크ㄹ래스 값바꾸기
    }
    public void AreaNoCaptured(string capturedhex)
    {
        GameObject obj = GameObject.Find(capturedhex).transform.GetChild(0).gameObject;
        Hex thisHex = hexControl.FindHex(obj);
        if (!overriding)
        {
            thisHex.script.UnSel();
        }
        else
        {
            OnOverriding();
        }
        myScript.myturn = false;
        eneScript.myturn = true;
        ChangePhase("yrTrun");
        PV.RPC("EnTurn", RpcTarget.Others);
        if (!overriding)
        {
            PV.RPC("EnemyNoCaptured", RpcTarget.Others, capturedhex);
        }
        // PV.RPC("AllChangePhase", RpcTarget.All, "4");
        //추가
    }
    [PunRPC]
    void EnemyNoCaptured(string tileObject)
    {
        GameObject obj = GameObject.Find(tileObject).transform.GetChild(0).gameObject;
        Hex thisHex = hexControl.FindHex(obj);
        thisHex.script.UnSel();
        //페ㅣ즈 넘기기
    }
    [PunRPC]
    void EnemyCaptured(string tileObject)
    {
        GameObject obj = GameObject.Find(tileObject).transform.GetChild(0).gameObject;
        Hex thisHex = hexControl.FindHex(obj);
        thisHex.script.CaptureEn();
        thisHex.captured_by = "yu";

        //페ㅣ즈 넘기기
    }
    Vector3 iconOffset = new Vector3(0, 0.25f, 0);
    // 롤아이콘 돌고 리턴 필요
    public bool HexClicked(GameObject tileObject)
    {
        bool execute = false;
        Hex thisHex = hexControl.FindHex(tileObject);

        if (Blue)
        {
            execute = true;
            AreaCaptured(tileObject.transform.parent.name);
            return execute;
        }
        else if (Red)
        {
            PV.RPC("EneOverride", RpcTarget.Others, lastClickedhex);
        }


        Debug.Log($"parent : {lastClickedhex}");
        if (synchronizaion > 0)
        {
            GameObject obj = GameObject.Find(lastClickedhex).transform.GetChild(0).gameObject;
            Hex LastHex = hexControl.FindHex(obj);
            if (Two_Same(thisHex, LastHex))
            {
                synchronizaion--;
                selectable = false;
                execute = true;
                AreaCaptured(tileObject.transform.parent.name);
                OnSynchronizaion();
                return execute;
            }
        }
        if (remoteCapture && synchronizaion <= 0)
        {
            remoteCapture = false;
            if (thisHex.captured_by == "")
            {
                selectable = false;
                execute = true;
                AreaCaptured(tileObject.transform.parent.name);
                OnRemoteCapture();
                return execute;
            }
        }
        switch (curPhase)
        {
            case "myFirstArea": // 이때 바깥 헥스 색 달라지면 좋을듯
                if (thisHex.cir == size_Hex1 && thisHex.captured_by == "")
                {
                    thisHex.captured_by = "me";
                    thisHex.script.Capture();
                    myScript.firstAreaSelected = true;
                    PV.RPC("EnemyClicked", RpcTarget.Others, tileObject.transform.parent.name);
                    PV.RPC("AllChangePhase", RpcTarget.All, "");
                    // uiController.HexSelTimeStop();
                    execute = true;
                }
                break;
            case "myTrun":
                if (Check_Nearhex(thisHex) && thisHex.captured_by == "")//인접한 땅 선택한건지 확인
                {
                    lastClickedhex = tileObject.transform.parent.name;
                    fuck.text = $"Last Clicked : {lastClickedhex}";
                    thisHex.script.Seled();
                    PhotonNetwork.Instantiate("Choice_capture", tileObject.transform.position + iconOffset, Quaternion.identity);
                    PV.RPC("EnemyClicked", RpcTarget.Others, tileObject.transform.parent.name);
                    // uiController.HexSelTimeStop();
                    PV.RPC("AllChangePhase", RpcTarget.All, "0");
                    execute = true;
                }
                break;
            default:
                break;
        }
        return execute;
    }
    [PunRPC]
    void EnemyClicked(string tileObject)
    {
        Debug.Log("동동화");
        GameObject obj = GameObject.Find(tileObject).transform.GetChild(0).gameObject;
        Hex thisHex = hexControl.FindHex(obj);
        switch (curPhase)
        {
            case "yrFirstArea":
                Debug.Log("응애!");
                thisHex.script.CaptureEn();
                thisHex.captured_by = "yu";
                break;
            case "yrTrun":
                thisHex.script.Seled();
                break;
            default:
                break;
        }
    }

    #endregion
    #region MinigameManagement
    public string curGamename;
    public void GameSelected(string gamename)
    {
        switch (gamename)
        {
            case "Ability_":
                gamename = "[ 기량 ]";
                break;
            case "Gear_":
                gamename = "[ 정밀 ]";
                break;
            // case "Idea_":
            // gamename = "[ 재치 ]";
            // break;
            case "Random_":
                gamename = "[ 행운 ]";
                break;
            default:
                gamename = "Unknown";
                break;
        }
        curGamename = gamename;
        PV.RPC("AllChangePhase", RpcTarget.All, "1");
        //난이도 선택 만들고 그 안에 셀렉트 카드
        if (myScript.myturn)
            uiController.NanidoSelect(curGamename);
        // selectCards(gamename);
    }
    public void NanidoSelected(string nanido)
    {
        uiController.StopTimer();
        if (nanido == "쉬움")
            this.nanido = 1;
        else if (nanido == "보통")
            this.nanido = 2;
        else if (nanido == "어려움")
            this.nanido = 3;
        PV.RPC("AllChangePhase", RpcTarget.All, "2");
        selectCards(curGamename);
        //그거 자ㅔ 스크립트로 실행 ㄱㄱ
    }
    public void CardSelectEnd()
    {
        StartCoroutine(CardSelectEnd_());
    }
    IEnumerator CardSelectEnd_()
    {
        Debug.LogFormat(difficultyUp.ToString());
        //난이도 효과 적용
        string nanText = "";
        nanido += difficultyUp;
        if (difficultyUp == -1)
        {
            difficultyUp = 1;
        }
        else if (difficultyUp == 1)
        {
            difficultyUp = -1;
        }
        if (nanido == 0) nanText = "게임 성공";
        else if (nanido == 1) nanText = "난이도 : 쉬움";
        else if (nanido == 2) nanText = "난이도 : 보통";
        else if (nanido == 3) nanText = "난이도 : 어려움";
        else if (nanido == 4) nanText = "게임 실패";
        uiController.EditNanidoText(nanText);
        yield return new WaitForSeconds(0.5f);
        //둘다 들어와있으
        if (nanido == 0)
        {
            yield return new WaitForSeconds(0.5f);
            if (myScript.myturn)
                MiniGameEnd_capture(true);
        }
        else if (nanido == 4)
        {
            yield return new WaitForSeconds(0.5f);
            if (myScript.myturn)

                MiniGameEnd_capture(false);
        }
        else
        {
            PV.RPC("AllChangePhase", RpcTarget.All, "3");
            if (myScript.myturn)
                MiniGameStart(curGamename);
        }
    }
    void MiniGameStart(string gamename)
    {
        playCapureGameObj.SetActive(true);
        playCapureGameScreenObj.SetActive(true);
        PV.RPC("GameObj", RpcTarget.Others);
        Debug.Log($"게임 시작 {gamename}");
        if (gamename == "[ 행운 ]")
        {
            diceGame.GameStart(nanido, nickName);
        }
        else if (gamename == "[ 정밀 ]")
        {
            gearGame.GameStart(nanido, nickName);
        }
        else if (gamename == "[ 재치 ]")
        {
            ideaGame.GameStart(nanido, nickName);
        }
        else if (gamename == "[ 기량 ]")
        {
            abilityGame.GameStart(nanido, nickName);
        }
    }
    [PunRPC]
    void GameObj()
    {
        playCapureGameObj.SetActive(true);
        playCapureGameScreenObj.SetActive(true);
    }
    public void EnClose()
    {
        playCapureGameObj.SetActive(false);
        playCapureGameScreenObj.SetActive(false);
    }
    public void MiniGameEnd_capture(bool win)
    {

        Debug.Log("들어왔다 MiniGameEnd_capture");
        uiController.NanidoBack();
        Debug.Log("inin");
        playCapureGameObj.SetActive(false);
        playCapureGameScreenObj.SetActive(false);
        if (!myScript.myturn)
        {
            return;
        }
        if (win)
        {
            Debug.Log("승 MiniGameEnd_capture");
            Debug.Log(difficultyUp + " ::: 승승승");
            int addPoint = (nanido + difficultyUp) * boost;
            myScript.score += addPoint;
            Debug.Log(addPoint + " ::: ++++");
            AreaCaptured(lastClickedhex);

            //턴 넘길지 확인 
            if (synchronizaion > 0)
            {
                OnSynchronizaion();
                return;
            }
            else if (remoteCapture)
            {
                OnRemoteCapture();
                return;
            }
            else if (chainCapture)
            {
                OnChain();
                return;
            }
            else
            {
                Debug.Log("턴교환 MiniGameEnd_capture");
                myScript.myturn = false;
                eneScript.myturn = true;
                ChangePhase("yrTrun");
                PV.RPC("EnTurn", RpcTarget.Others);
            }
        }
        else
        {
            Debug.Log("패 MiniGameEnd_capture");
            if (safety && !overriding)
            {
                AreaCaptured(lastClickedhex);
                myScript.myturn = false;
                eneScript.myturn = true;
                ChangePhase("yrTrun");
                PV.RPC("EnTurn", RpcTarget.Others);
                return;
            }
            AreaNoCaptured(lastClickedhex);
        }
    }
    [PunRPC]
    void EnTurn()
    {
        difficultyUp = 0;
        boost = 1; // [ v ]
        safety = false; // [ v ]
        overriding = false; // [ v ]
        chainCapture = false; // [ v ]
        remoteCapture = false; // [ v ]
        synchronizaion = 0; // [ v ]
        myScript.myturn = true;
        eneScript.myturn = false;
        ChangePhase("myTrun");
        Debug.Log(curPhase);
    }
    #endregion
    #region Ability Execute
    void OnOverriding()
    {
        PV.RPC("EneOverride", RpcTarget.Others, lastClickedhex);
    }
    void OnSynchronizaion()
    {
        if (synchronizaion <= 0)
        {
            if (remoteCapture)
            {
                OnRemoteCapture();
            }
            else if (chainCapture)
            {
                OnChain();
            }
            else
            {
                myScript.myturn = false;
                eneScript.myturn = true;
                ChangePhase("yrTrun");
                PV.RPC("EnTurn", RpcTarget.Others);
                Debug.Log(curPhase);
            }
            return;
        }
        phaseText.text = "동기화 카드 효과 발동중";
        myScript.myturn = true;
        eneScript.myturn = false;
        selectable = true;
    }
    void OnRemoteCapture()
    {
        if (!remoteCapture && synchronizaion <= 0)
        {
            if (chainCapture)
            {
                OnChain();
            }
            else
            {
                myScript.myturn = false;
                eneScript.myturn = true;
                ChangePhase("yrTrun");
                PV.RPC("EnTurn", RpcTarget.Others);
                Debug.Log(curPhase);
            }
            return;
        }
        phaseText.text = "리모트 캡쳐링 카드 효과 발동중";
        selectable = true;
    }
    void OnChain()
    {
        chainCapture = false;
        myScript.myturn = true;
        eneScript.myturn = false;
        ChangePhase("myTrun");
        PV.RPC("EneChain", RpcTarget.Others);
        Debug.Log(curPhase);
    }
    [PunRPC]
    void EneChain()
    {
        myScript.myturn = false;
        eneScript.myturn = true;
        ChangePhase("yrTrun");
    }
    [PunRPC]
    void EneOverride(string capturedhex)
    {

        // GameObject obj = GameObject.Find(capturedhex).transform.GetChild(0).gameObject;
        // Hex thisHex = hexControl.FindHex(obj);
        AreaCaptured(capturedhex);
        // thisHex.captured_by = "me";
        // thisHex.script.Capture();
        // PV.RPC("EnemyCaptured", RpcTarget.Others, capturedhex);
    }
    public void CardAbilityExecute(int seq = 0)
    {
        List<Card> executedCards = new List<Card>();
        if (refCardCheck.a_deadlock)
        {
            if (refCardCheck.a_easier)
                if (myScript.myturn) RemoveUsedCard("최적화");
            if (refCardCheck.a_boost)
                if (myScript.myturn) RemoveUsedCard("부스트");
            if (refCardCheck.a_safetyWeb)
                if (myScript.myturn) RemoveUsedCard("안전망");
            if (refCardCheck.a_chainCapture)
                if (myScript.myturn) RemoveUsedCard("체인 캡쳐링");
            if (refCardCheck.a_synchronization == 2)
                if (myScript.myturn) RemoveUsedCard("동기화");
            if (refCardCheck.a_remoteCapture)
                if (myScript.myturn) RemoveUsedCard("리모트 캡쳐링");

            refCardCheck.a_boost = false;
            refCardCheck.a_chainCapture = false;
            refCardCheck.a_easier = false;
            refCardCheck.a_remoteCapture = false;
            refCardCheck.a_safetyWeb = false;
            refCardCheck.a_synchronization = 0;
            executedCards.Add(MakeCard("데드록"));
            if (!myScript.myturn) RemoveUsedCard("데드록");
        }

        difficultyUp = 0;
        if (refCardCheck.a_harder)
        {
            difficultyUp++;
            executedCards.Add(MakeCard("취약점 증폭"));
            if (!myScript.myturn) RemoveUsedCard("취약점 증폭");
        }
        if (refCardCheck.a_easier)
        {
            difficultyUp--;
            executedCards.Add(MakeCard("최적화"));
            if (myScript.myturn) RemoveUsedCard("최적화");
        }

        boost = 1;
        if (refCardCheck.a_boost)
        {
            boost = 2;
            executedCards.Add(MakeCard("부스트"));
            if (myScript.myturn) RemoveUsedCard("부스트");
        }

        safety = false;
        if (refCardCheck.a_safetyWeb)
        {
            safety = true;
            executedCards.Add(MakeCard("안전망"));
            if (myScript.myturn) RemoveUsedCard("안전망");
        }

        overriding = false;
        if (refCardCheck.a_overriding)
        {
            overriding = true;
            executedCards.Add(MakeCard("오버라이드"));
            if (!myScript.myturn) RemoveUsedCard("오버라이드");
        }

        chainCapture = false;
        if (refCardCheck.a_chainCapture)
        {
            chainCapture = true;
            executedCards.Add(MakeCard("체인 캡쳐링"));
            if (myScript.myturn) RemoveUsedCard("체인 캡쳐링");
        }

        synchronizaion = 0;
        if (refCardCheck.a_synchronization == 2)
        {
            GameObject obj = GameObject.Find(lastClickedhex).transform.GetChild(0).gameObject;
            Hex thisHex = hexControl.FindHex(obj);
            synchronizaion = Count_Space(thisHex) < 2 ? Count_Space(thisHex) : 2;
            executedCards.Add(MakeCard("동기화"));
            if (myScript.myturn) RemoveUsedCard("동기화");
        }

        remoteCapture = false;
        if (refCardCheck.a_remoteCapture)
        {
            remoteCapture = true;
            executedCards.Add(MakeCard("리모트 캡쳐링"));
            if (myScript.myturn) RemoveUsedCard("리모트 캡쳐링");
        }
        uiController.CardAbilExeEnd(executedCards);
    }
    void RemoveUsedCard(string name)
    {
        for (int i = 0; i < myScript.cards.Count; i++)
        {
            if (myScript.cards[i].name == name)
            {
                myScript.cards.RemoveAt(i);
                return; // 첫 번째 항목만 제거 후 함수 종료
            }
        }
    }
    #endregion
}
