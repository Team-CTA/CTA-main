using System.Collections;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RollObject : MonoBehaviourPun, IPunObservable
{
    [SerializeField] float resShowTime = 3f;
    [SerializeField] int rollCount = 200;
    public Texture2D[] textures;
    public int texArr = 0;
    Vector3 camVector;
    GameManager gm;
    Image img;
    PhotonView PV;

    void Start()
    {
        PV = photonView;
        img = GetComponent<Image>();
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        int rnd = Random.Range(0, textures.Length);
        StartCoroutine(ChangeImg(rollCount, rnd));
        gm.NextPhase(true);
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.K))
        //     StartCoroutine(ChangeImg(rollCount));
        camVector = Camera.main.transform.position;
        transform.LookAt(camVector);
    }

    IEnumerator ChangeImg(int cnt, int rnd)
    {
        texArr = 0;
        cnt += rnd;
        while (cnt > 0)
        {
            // Debug.Log(cnt);

            yield return new WaitForSeconds(1.2f / cnt);
            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("SetTexture", RpcTarget.All, texArr);
            }
            if (texArr != textures.Length - 1) //4
                texArr++;
            else
                texArr = 0;
            cnt--;
        }
        yield return new WaitForSeconds(resShowTime);//나주엥 바꿀거임
        // areaManager.GameSelected(img.material.GetTexture("_MainTexture").ToString());
        gm.GameSelected(img.material.GetTexture("_MainTexture").name);
        Destroy(gameObject);
    }
    [PunRPC]
    void SetTexture(int arr)
    {
        img.material.SetTexture("_MainTexture", textures[arr]);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(texArr);
        }
        else
        {
            texArr = (int)stream.ReceiveNext();
        }
    }
}
// 텍스쳐가 같아서 여러개 나오면 이미지 통일됨 ㅋㅋ.