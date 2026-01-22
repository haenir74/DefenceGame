using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Isometric Settings")]
    [SerializeField] private float angleX = 30f; // 내려다보는 각도
    [SerializeField] private float angleY = 45f; // 회전 각도 (45도가 정석)
    [SerializeField] private float padding = 2f; // 맵 외곽 여백

    private Camera cam;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    public void SetupCamera(int width, int height, float cellSize)
    {
        if (cam == null) return;

        // 1. Orthographic 모드 설정 (아이소메트릭 뷰 필수)
        cam.orthographic = true;

        // 2. 각도 설정
        transform.rotation = Quaternion.Euler(angleX, angleY, 0);

        // 3. 위치 설정 (맵의 중앙)
        float midX = (width * cellSize) * 0.5f - (cellSize * 0.5f);
        float midZ = (height * cellSize) * 0.5f - (cellSize * 0.5f);
        Vector3 centerPos = new Vector3(midX, 0, midZ);

        // 카메라는 중심에서 뒤쪽으로 멀리 떨어뜨려 놓음 (Orthographic이라 거리는 원근감에 영향 X, 클리핑만 안되면 됨)
        float distance = 100f;
        Vector3 dir = -transform.forward;
        transform.position = centerPos + dir * distance;

        // 4. 줌(Orthographic Size) 설정
        // 맵의 가로/세로 중 화면 비율에 맞춰 더 큰 쪽을 기준으로 사이즈 결정
        
        // 맵의 실제 월드 크기
        float mapWorldWidth = width * cellSize;
        float mapWorldHeight = height * cellSize; // 3D공간의 Z축 길이

        // 화면 비율 (Aspect Ratio)
        float screenRatio = (float)Screen.width / Screen.height;
        float targetSize = 0f;

        // 아이소메트릭 투영 시 화면에 보이는 높이 계산은 복잡하므로, 
        // 간단한 근사치 혹은 맵 대각선 길이를 기준으로 여유있게 잡음.
        
        // 맵의 대각선 절반 길이 + 여백
        float diagonal = Mathf.Sqrt(Mathf.Pow(mapWorldWidth, 2) + Mathf.Pow(mapWorldHeight, 2));
        targetSize = (diagonal * 0.5f) + padding;

        cam.orthographicSize = targetSize;
    }
}
