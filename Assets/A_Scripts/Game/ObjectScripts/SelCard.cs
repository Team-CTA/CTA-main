using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelCard : MonoBehaviour
{
    [SerializeField] GameManager gm;
    [SerializeField] Text Cname;
    public void OnPointerClick()
    {
        gm.CardClicked(Cname.text);
    }
}
