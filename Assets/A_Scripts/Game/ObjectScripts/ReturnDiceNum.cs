using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnDiceNum : MonoBehaviour
{
    int num = 0;
    public int CheckNum()
    {
        return num;
    }
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag != "DiceNumber") return;
        switch (col.gameObject.name)
        {
            case "1":
                num = 6;
                break;
            case "2":
                num = 5;
                break;
            case "3":
                num = 4;
                break;
            case "4":
                num = 3;
                break;
            case "5":
                num = 2;
                break;
            case "6":
                num = 1;
                break;
        }
    }
}
