using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionScript : MonoBehaviour
{
    public Image img;
    Material SceneTransition;
    public float speed = 10;
    void Start()
    {
        SceneTransition = img.material;
        OnT();
    }
    public void OnT()
    {
        StartCoroutine(ChangeValue(-2, 3));
    }

    public void OutT(string scene)
    {
        StartCoroutine(ChangeValue(3, -2, scene));
    }

    IEnumerator ChangeValue(float current, float target, string scene = null)
    {
        float actSpeed = speed;
        if (scene != null)
            actSpeed *= 2;
        transform.SetAsLastSibling();
        SceneTransition.SetFloat("_CutOff", current);
        yield return new WaitForSeconds(0.1f);
        while (SceneTransition.GetFloat("_CutOff") != target)
        {
            yield return new WaitForSeconds(0.01f);
            SceneTransition.SetFloat("_CutOff",
                Mathf.MoveTowards(SceneTransition.GetFloat("_CutOff"),
                target, actSpeed * Time.deltaTime));
        }
        if (scene != null)
        {
            if (scene != "NULL")
                SceneManager.LoadScene(scene);
        }
        else
            transform.SetAsFirstSibling();
    }
}
