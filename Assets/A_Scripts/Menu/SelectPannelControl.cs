using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectPannelControl : MonoBehaviour
{
    public GameObject rollBase, rollUI, logo, optins;
    [SerializeField] SoundManager soundManager = null;
    [SerializeField] TransitionScript transition;
    [SerializeField] GameObject tutorial;
    public Outline outline;
    public Button exit;
    public Animator ui_center;
    public Button[] ui_Pannel;
    public AudioClip[] sounds;
    [SerializeField] Slider[] sliders;
    public Text sel;
    bool CanChange = true, hexEnabled = false, CanClick = true;
    int curHexBtn;
    bool openingTutoOrOption = false;
    delegate void myFunc();
    void Start()
    {
        SetFunc();
        if (!PlayerPrefs.HasKey("First"))
        {
            PlayerPrefs.SetInt("First", 1);
            TutoOpen();
        }
    }
    public void ChangeVolBGM(Slider slider)
    {
        soundManager.ChangeBGM_Vol(slider);
    }
    public void ChangeVolSFX(Slider slider)
    {
        soundManager.ChangeSFX_Vol(slider);
    }
    public void PlaySfx(AudioClip clip)
    {
        soundManager.PlaySFX(clip);
    }
    void HexEnable(bool isOpen, int dir)
    {
        if (CanClick == false) return;
        CanClick = false;
        if (isOpen)
        {
            if (!hexEnabled)
            {
                hexEnabled = true;
                soundManager.PlaySFX(sounds[1]);
                ui_center.SetInteger("dir", dir);
                ui_center.SetTrigger("Enable");
            }
        }
        else
        {
            if (hexEnabled)
            {
                hexEnabled = false;
                soundManager.PlaySFX(sounds[1]);
                ui_center.SetInteger("dir", dir);
                ui_center.SetTrigger("Disable");
            }
        }
        Invoke("ChangeEnable", 1f);
    }
    void SetFunc()
    {
        exit.onClick.AddListener(() =>
       {
           HexEnable(false, ui_center.GetInteger("dir"));
           // Application.Quit();
       }); //Exit
        ui_Pannel[0].onClick.AddListener(() =>
        {
            HexEnable(true, 0);
        });//Start

        ui_Pannel[1].onClick.AddListener(() =>
        {
            soundManager.PlaySFX(sounds[1]);
            StartCoroutine(OpenScene("Ranking"));
        }); // ranking


        ui_Pannel[2].onClick.AddListener(() =>
        {
            // HexEnable(true, 2);
        });

        ui_Pannel[3].onClick.AddListener(() =>
        {
            if (openingTutoOrOption) return;
            openingTutoOrOption = true;
            soundManager.PlaySFX(sounds[1]);
            optins.SetActive(true);
        }); // Options

        ui_Pannel[4].onClick.AddListener(() =>
        {

        });

        ui_Pannel[5].onClick.AddListener(() =>
        {
            soundManager.PlaySFX(sounds[1]);
            Application.OpenURL("https://ctagame.site");
        });//Web


    }
    IEnumerator OpenScene(string scene)
    {
        transition.OutT("NULL");
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(scene);
    }
    void ChangeEnable()
    {
        CanClick = true;
    }
    void Update()
    {
        HexControl();
        for (int i = 0; i < 6; i++)
        {
            if (i != curHexBtn || i == 2 || i == 4)
            {
                ui_Pannel[i].interactable = false;
            }
            else
            {
                ui_Pannel[i].interactable = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (openingTutoOrOption && !optins.activeSelf) return;
            soundManager.PlaySFX(sounds[0]);
            optins.SetActive(!optins.activeSelf);
        }
        if (soundManager == null)
        {
            soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            sliders[0].value = soundManager.bgmVolume;
            sliders[1].value = soundManager.sfxVolume;

        }
        else
        {

            soundManager.PlayBGM(sounds[2]);
        }
    }
    void HexControl()
    {
        if (openingTutoOrOption) return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (CanChange && !hexEnabled)
        {
            if (Input.GetKey(KeyCode.UpArrow) || scroll < 0)
            {
                CanChange = false;
                StartCoroutine(Move(true));
            }
            else if (Input.GetKey(KeyCode.DownArrow) || scroll > 0)
            {
                CanChange = false;
                StartCoroutine(Move(false));
            }
        }
        float z = rollUI.transform.rotation.eulerAngles.z;
        if (z <= 0.001)
        {
            curHexBtn = 0;
        }
        for (int i = 1; i < 6; i++)
        {
            if (i * 60 - 0.01 <= z && z <= i * 60 + 0.01)
            {
                curHexBtn = i;
                break;
            }
        }
        sel.text = "| " + ui_Pannel[curHexBtn].gameObject.name.ToString();
        // Debug.Log(ui_Pannel[curHexBtn].gameObject.name);
        //만약 enter key를 입력받았으면
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ui_Pannel[curHexBtn].onClick.Invoke();
        }

        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (hexEnabled)
            {
                HexEnable(false, curHexBtn);
            }
            else
            {
                // 게임 나가는 부분
            }
        }
    }
    Color[] OutlineColor = {
        new Color(1, 1, 1, 1),
        new Color(0.8396226f, 0.7403533f, 0.07524917f, 1),
        new Color(0.4810431f, 0.2661089f, 0.8679245f, 1),
        new Color(0.2705882f, 0.2196079f, 0.2156863f, 1),
        new Color(0.2156863f, 0.8490566f, 0.2202741f, 5960785f),
        new Color(0.116545f, 0.9150943f, 0.8372869f, 0.7176471f),
        };
    IEnumerator Move(bool up)
    {

        float speed = 5f;
        float k = 60 / speed;
        soundManager.PlaySFX(sounds[0]);
        if (up)
        {
            for (int i = 0; i < k; i++)
            {
                rollUI.transform.Rotate(new Vector3(0, 0, speed));
                rollBase.transform.Rotate(new Vector3(0, 0, speed));
                outline.gameObject.transform.Rotate(new Vector3(0, 0, speed));
                logo.transform.Rotate(new Vector3(0, 0, ((i >= k / 2) ? speed : -speed) * 0.1f));
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            for (int i = 0; i < k; i++)
            {
                rollUI.transform.Rotate(new Vector3(0, 0, -speed));
                rollBase.transform.Rotate(new Vector3(0, 0, -speed));
                outline.gameObject.transform.Rotate(new Vector3(0, 0, -speed));
                logo.transform.Rotate(new Vector3(0, 0, ((i >= k / 2) ? -speed : speed) * 0.1f));

                yield return new WaitForFixedUpdate();
            }
        }
        CanChange = true;
        outline.effectColor = OutlineColor[curHexBtn];
    }
    public void CloseOptions()
    {
        openingTutoOrOption = false;
        optins.SetActive(false);
    }
    public void TutoOpen()
    {
        if (openingTutoOrOption) return;
        openingTutoOrOption = true;
        tutorial.SetActive(true);
    }
    public void TutoClose()
    {
        openingTutoOrOption = false;
        tutorial.SetActive(false);
    }
    public void LogOut()
    {
        PlayerPrefs.DeleteKey("USERNAME");
        PlayerPrefs.DeleteKey("PASSWORD");
        Debug.Log("로그아웃!");
        StartCoroutine(OpenScene("Login"));
    }
    public void Exit()
    {
        // 에디터에서 실행 중일 때
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서 실행 중일 때
        Application.Quit();
#endif
    }
}
