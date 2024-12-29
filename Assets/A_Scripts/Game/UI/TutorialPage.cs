using UnityEngine;
using UnityEngine.UI;

public class TutorialPage : MonoBehaviour
{
    public GameObject[] Page;
    public Button leftButton;
    public GameObject L_leftButton;
    public Button rightButton;
    public GameObject R_rightButton;

    private int PageIndex = 0;

    void Start()
    {
        UpdatePage();
        leftButton.onClick.AddListener(GoLastPage);
        rightButton.onClick.AddListener(GoNextPage);
    }

    void GoLastPage()
    {
        if (PageIndex > 0)
        {
            PageIndex--;
            UpdatePage();
        }
    }
    void GoNextPage()
    {
        if (PageIndex < Page.Length - 1)
        {
            PageIndex++;
            UpdatePage();
        }
    }

    void UpdatePage()
    {
        if (PageIndex == Page.Length - 1)
        {
            R_rightButton.SetActive(false);
        }
        else if (PageIndex == 0)
        {
            L_leftButton.SetActive(false);
        }
        else
        {
            R_rightButton.SetActive(true);
            L_leftButton.SetActive(true);
        }
        for (int i = 0; i < Page.Length; i++)
        {
            Page[i].SetActive(i == PageIndex);
        }
    }
}
