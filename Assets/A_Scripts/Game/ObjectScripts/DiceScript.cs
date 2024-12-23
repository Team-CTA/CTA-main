using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceScript : MonoBehaviour
{
    [SerializeField] Dice diceGm;
    static Rigidbody rb;
    [SerializeField] float gravityScale = 100;
    public Vector3 diceVelocity;
    public Vector3 startPos = new Vector3(0, 0, 0);
    [SerializeField] ReturnDiceNum Badak;
    private int[] angles = { 0, 90, 180, 270, 360 };
    private Vector2 startMousePosition;
    private Vector2 endMousePosition;
    [SerializeField] float swipeThreshold = 200f; // 스와이프 최소 거리 (픽셀 단위)
    public int recentRes = 0;

    #region Unity Default
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (Badak == null)
        {
            Badak = GameObject.FindWithTag("Badak").GetComponent<ReturnDiceNum>();
        }
    }
    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);
    }
    void Update()
    {
        diceVelocity = rb.linearVelocity; // 움직임이 있는지 확인하기 위한 변수
        if (!diceGm.rollAble) return;
        if (Input.GetMouseButtonDown(0)) // 마우스 클릭 시작
        {
            startMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) // 마우스 클릭 종료
        {
            endMousePosition = Input.mousePosition;
            DetectSwipe();
        }
    }
    #endregion
    #region CheckSwipe
    private void DetectSwipe()
    {
        // 스와이프 거리 계산
        float verticalDistance = endMousePosition.y - startMousePosition.y;
        float horizontalDistance = Mathf.Abs(endMousePosition.x - startMousePosition.x);

        // 위로 스와이프 감지
        if (verticalDistance > swipeThreshold && verticalDistance > horizontalDistance)
        {
            Debug.Log("Upward Swipe Detected");
            diceGm.rollAble = false;
            diceGm.RollStarts();
            DiceRoll(verticalDistance);
        }
    }
    #endregion
    #region Roll
    public void DiceRoll(float vertical_distance)
    {
        // 회전 랜덤 변수
        float dirX = Random.Range(0, 3000);
        float dirY = Random.Range(0, 3000);
        float dirZ = Random.Range(0, 3000);

        // 초기 회전값 (랜덤 부여)
        Quaternion currentRotation = transform.localRotation;
        float randomIndex_x = Random.Range(0, angles.Length);
        float randomIndex_z = Random.Range(0, angles.Length);

        // 초기 위치와 회전 초기화
        transform.localPosition = startPos;

        transform.localRotation = Quaternion.Euler(angles[(int)randomIndex_x], currentRotation.eulerAngles.y, angles[(int)randomIndex_z]);
        // 윗 방향으로 힘을 가해 공중에 띄우면서, 랜덤한 방향으로 회전
        float randomF = Random.Range(100f, 300f);
        float Force = Mathf.Clamp(vertical_distance, 800, 1200);
        rb.AddForce(Vector3.up * Force, ForceMode.Impulse);
        rb.AddTorque(new Vector3(dirX, dirY, dirZ), ForceMode.VelocityChange); // 
        StartCoroutine(CheckRes());
    }
    Vector3 targetPos = new Vector3(0, 1f, 0);
    IEnumerator CheckRes()
    {
        yield return new WaitForSeconds(1f);

        while (rb.linearVelocity.magnitude > 0.01f || rb.angularVelocity.magnitude > 0.01f)
        {
            yield return null;
        }
        recentRes = Badak.CheckNum();
        Debug.Log(recentRes);
        diceGm.Rolled(recentRes);
        yield return new WaitForSeconds(0.6f);
        float moveDuration = 0.3f;
        StartCoroutine(MoveToLocalPosition(transform, targetPos, moveDuration));
    }
    private IEnumerator MoveToLocalPosition(Transform obj, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = obj.localPosition; // 시작 위치 저장
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 현재 진행도 계산
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // 위치 보간 (부드럽게 이동)
            obj.localPosition = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null; // 다음 프레임까지 대기
        }

        // 최종 위치를 정확히 설정
        obj.localPosition = targetPosition;
    }
    #endregion
}
