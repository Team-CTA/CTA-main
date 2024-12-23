using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyChoice : MonoBehaviour
{
    Vector3 camVector;
    Vector3 rotate = new Vector3(0, 0, 180);
    void LateUpdate()
    {
        camVector = Camera.main.transform.position;
        transform.LookAt(camVector);
        transform.Rotate(rotate);
    }
}