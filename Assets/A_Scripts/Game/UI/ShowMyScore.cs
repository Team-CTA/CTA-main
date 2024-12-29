using UnityEngine;
using UnityEngine.UI;

public class ShowMyScore : MonoBehaviour
{
    public Text MyScore;
    void Start()
    {
        MyScore.text = PlayerPrefs.GetInt("UserScore").ToString();
    }
}
