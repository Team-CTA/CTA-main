using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] MainSceneNetworkManager networkManager;

    [Serializable]
    class RefObjects
    {
        public Text start_playername;
    }
    [SerializeField] RefObjects refObjects;

    public string start_selectedgame;
    public delegate void myFunc();


    void Start()
    {
        start_selectedgame = "Normal";
        refObjects.start_playername.text = networkManager.nickName;
    }
}
