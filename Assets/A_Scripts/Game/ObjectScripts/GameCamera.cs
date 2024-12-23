using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] Transform cam;
    void Update()
    {
        transform.position = cam.position;
        transform.rotation = cam.rotation;
    }
}