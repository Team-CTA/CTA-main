using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        //강민재 : 랭크 로드 확인용
        Debug.Log(PlayerPrefs.GetInt("UserRank"));
        Debug.Log(PlayerPrefs.GetInt("UserScore"));
    }
    void Update()
    {
        //Debug.Log("+ rank" + PlayerPrefs.GetInt("UserRank"));
        //Debug.Log("+ score" + PlayerPrefs.GetInt("UserScore"));
    }

}
