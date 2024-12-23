using System;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
public class Hex1 : MonoBehaviour
{
    [Serializable]
    class References
    {
        public Material[] materials;
        public Renderer obj_renderer;
    }
    [SerializeField] References references;
    int[] matArr = { 0, 0 };
    // NetworkManager NM;
    // AreaManager areaManager;
    GameManager gameManager;
    GameObject hoveringObject = null;
    Animator ani;
    bool changed = false, isHovering = false;
    bool executed = false;
    void Awake()
    {
    }

    void Start()
    {
        // areaManager = GameObject.Find("GameManager").GetComponent<AreaManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ani = GetComponent<Animator>();
        // NM = GameObject.Find("GameManager").GetComponent<NetworkManager>();
    }
    void Update()
    {
        // if (NM.FindPlayerScript().myturn)
        // {
        Selecting();
        Clicked();
        // }
    }
    public void Seled()
    {
        ani.SetTrigger("try");
    }
    public void UnSel()
    {
        ani.SetTrigger("def");
        executed = false;
    }
    public void Capture()
    {
        ani.SetTrigger("me");
    }
    public void CaptureEn()
    {
        ani.SetTrigger("enemy");
    }
    void Selecting()
    {
        if (executed) // 혹시나 오류나면 여기 수정해
        {
            Material[] mat_arrE = references.obj_renderer.materials;
            mat_arrE[0] = references.materials[5];
            mat_arrE[1] = references.materials[0];
            matArr = new int[] { 5, 0 };
            references.obj_renderer.materials = mat_arrE;

            if (hoveringObject != null)
            {
                PhotonNetwork.Destroy(hoveringObject);
                hoveringObject = null;
            }
        }

        if (!gameManager.selectable || executed) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Material[] mat_arr = references.obj_renderer.materials;
        Physics.Raycast(ray, out hit); // if (Physics.Raycast(ray, out hit)) // 충돌체크 (충돌이면 발동됨 )
        if (hit.transform == transform)
        {
            if (!isHovering)
            {
                hoveringObject = PhotonNetwork.Instantiate("Choice_enemy", transform.position, Quaternion.identity);
                changed = true;
                isHovering = true;
                mat_arr[0] = references.materials[4];
                mat_arr[1] = references.materials[1];
                matArr = new int[] { 4, 1 };
            }
        }
        else
        {
            if (isHovering)
            {
                if (hoveringObject != null)
                {
                    PhotonNetwork.Destroy(hoveringObject);
                    hoveringObject = null;
                }
                changed = true;
                isHovering = false;
                mat_arr[0] = references.materials[2];
                mat_arr[1] = references.materials[0];
                matArr = new int[] { 2, 0 };
            }
        }
        if (changed)
        {
            references.obj_renderer.materials = mat_arr;
            changed = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (isHovering)
            {
                mat_arr[0] = references.materials[3];
                matArr[0] = 3;
                references.obj_renderer.materials = mat_arr;
            }
            else
            {
                if (mat_arr[0] != references.materials[2])
                {
                    mat_arr[0] = references.materials[2];
                    matArr[0] = 2;
                    references.obj_renderer.materials = mat_arr;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isHovering)
            {
                mat_arr[0] = references.materials[4];
                matArr[0] = 4;
                references.obj_renderer.materials = mat_arr;
            }
            else
            {
                if (mat_arr[0] != references.materials[2])
                {
                    mat_arr[0] = references.materials[2];
                    matArr[0] = 2;
                    references.obj_renderer.materials = mat_arr;
                }
            }
        }
    }
    void Clicked()
    {
        if (Input.GetMouseButtonUp(0) && isHovering && executed != true)
        {
            if (gameManager.HexClicked(gameObject)) executed = true;
        }

        //실패시 execute=false;
    }
    public void FalseExecute()
    {
        executed = false;

        //실패시 execute=false;
    }
}

