using UnityEngine;
using UnityEngine.UI;

public class TutorialPage : MonoBehaviour
{
    public GameObject[] Page;
    public Button leftButton;
    public Button rightButton;

    private int PageIndex = 0;

    void Start()
    {
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
        for (int i = 0; i < Page.Length; i++)
        {
            Page[i].SetActive(i == PageIndex);
        }
    }
}
