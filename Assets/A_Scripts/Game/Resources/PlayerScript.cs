using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public int score = 0;
    public bool myturn = false;
    public bool cardSelected = false;
    public bool firstAreaSelected = false;
    public bool cardEntry = false;
    public List<GameManager.Card> cards = new List<GameManager.Card>();
    // NetworkManager NM;
    PhotonView PV;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(myturn);
            stream.SendNext(cardSelected);
            stream.SendNext(firstAreaSelected);
            stream.SendNext(cardEntry);
            stream.SendNext(score);
        }
        else
        {
            myturn = (bool)stream.ReceiveNext();
            cardSelected = (bool)stream.ReceiveNext();
            firstAreaSelected = (bool)stream.ReceiveNext();
            cardEntry = (bool)stream.ReceiveNext();
            score = (int)stream.ReceiveNext();
        }
    }

    private void Start()
    {
        PV = photonView;
        // NM = GameObject.FindWithTag("GameManager").GetComponent<NetworkManager>();
    }
    private void Update()
    {
        if (!PV.IsMine) return; // 진짜 중요!! 모든 플레이어 오브젝트에서 실행되기에, 본인인 경우에만 해야함!
    }
}
