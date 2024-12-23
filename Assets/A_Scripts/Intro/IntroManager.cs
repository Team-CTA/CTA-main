using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [SerializeField] Animator introAni;
    [SerializeField] Animator LoginStart;
    [SerializeField] Text introTxt;

    void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
    }
    void Start()
    {
        StartCoroutine(IntroAnim());
    }
    float txtDelay = 0.11f;
    string gameName = "Capture The Area";
    IEnumerator IntroAnim()
    {
        yield return new WaitForSeconds(1f);
        introTxt.text = "_";
        yield return new WaitForSeconds(0.5f);
        introTxt.text = "";
        yield return new WaitForSeconds(0.5f);
        introAni.SetTrigger("IntroStart");
        for (int i = 1; i <= gameName.Length; i++)
        {
            introTxt.text = gameName.Substring(0, i);
            yield return new WaitForSeconds(txtDelay);
        }
        yield return new WaitForSeconds(2.5f);
        LoginStart.SetTrigger("Execute");
    }
}
