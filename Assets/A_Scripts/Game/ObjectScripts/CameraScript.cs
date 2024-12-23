using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] float[] speeds = { 650.0f, 4.0f };
    [SerializeField] float[] y_Limit = { -20.0f, 45.0f, 0 };
    public GameObject stage_center;
    private float xRotate, yRotate;


    void Start()
    {
        Vector3 stagePosition = stage_center.transform.position;

        transform.RotateAround(stagePosition, Vector3.up, xRotate);
        transform.LookAt(stagePosition);
    }

    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        if (Input.GetMouseButton(1))
        {
            xRotate = Input.GetAxis("Mouse X") * Time.deltaTime * speeds[0];
            yRotate = Input.GetAxis("Mouse Y") * Time.deltaTime * speeds[0];

            Vector3 stagePosition = stage_center.transform.position;
            transform.RotateAround(stagePosition, Vector3.up, xRotate);
            if (!(yRotate > 0 && y_Limit[2] + yRotate >= y_Limit[1])
             && !(yRotate < 0 && y_Limit[2] + yRotate <= y_Limit[0]))
            {
                y_Limit[2] += yRotate;
                transform.Translate(Vector3.up * yRotate, Space.World);
            }

            transform.LookAt(stagePosition);
        }
    }
    // [SerializeField] float[] scroll_Limit = { -10.0f, 20.0f, 0 };
    // void Zoom()
    // {
    //     float scroll = Input.GetAxis("Mouse ScrollWheel") * speeds[1];
    //     if (scroll > 0 && scroll_Limit[2] + scroll >= scroll_Limit[1])
    //     {
    //         scroll_Limit[2] = scroll_Limit[1];
    //     }
    //     else if (scroll < 0 && scroll_Limit[2] + scroll <= scroll_Limit[0])
    //     {
    //         scroll_Limit[2] = scroll_Limit[0];
    //     }
    //     else
    //     {
    //         scroll_Limit[2] += scroll;
    //         transform.position += transform.forward * scroll;
    //     }
    // }
}