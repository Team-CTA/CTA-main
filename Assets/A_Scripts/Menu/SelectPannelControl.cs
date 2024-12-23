using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPannelControl : MonoBehaviour
{
    public GameObject rollBase, rollUI, logo;
    public Outline outline;
    public Button exit;
    public Animator ui_center;
    public Button[] ui_Pannel;
    public AudioSource[] sounds;
    public Text sel;
    bool CanChange = true, hexEnabled = false, CanClick = true;
    int curHexBtn;
    delegate void myFunc();
    void Start()
    {
        SetFunc();
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
                sounds[1].Play();
                ui_center.SetInteger("dir", dir);
                ui_center.SetTrigger("Enable");
            }
        }
        else
        {
            if (hexEnabled)
            {
                hexEnabled = false;
                sounds[1].Play();
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
            // HexEnable(true, 1);
        });


        ui_Pannel[2].onClick.AddListener(() =>
        {
            // HexEnable(true, 2);
        });

        ui_Pannel[3].onClick.AddListener(() =>
        {

        });

        ui_Pannel[4].onClick.AddListener(() =>
        {

        });

        ui_Pannel[5].onClick.AddListener(() =>
        {
            Application.OpenURL("https://ctagame.site");
        });//Web


    }
    void ChangeEnable()
    {
        CanClick = true;
    }
    void Update()
    {
        HexControl();
    }
    void HexControl()
    {
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
        sounds[0].Play();
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
}
