using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        //강민재 : 랭크 로드 확인용
        Debug.Log(PlayerPrefs.GetInt("UserRank"));
        Debug.Log(PlayerPrefs.GetInt("UserScore"));
    }

}
