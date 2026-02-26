using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 0.5f;
    [SerializeField] private Vector2 panLimit = new Vector2(20f, 25f);
    [SerializeField] private float smoothTime = 0.2f;

    private Camera cam;
    private Vector3 initialPos;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;

    protected override void Awake()
    {
        base.Awake();
        this.cam = GetComponent<Camera>();
        if (this.cam == null) this.cam = Camera.main;
    }

    private void Start()
    {
        this.initialPos = transform.position;
        this.targetPosition = transform.position;
    }
    
    private void Update()
    {
        if (this.cam == null) return;

        HandleInput();
        MoveCamera();
    }

    private void HandleInput()
    {
        // [FIX] 마우스가 UI 위에 있을 때는 카메라 줌(휠)을 무시하여 UI 스크롤이 가능하게 함
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            // 단, 드래그 중이거나 우클릭 팬 중일 때는 예외로 둘 수 있으나
            // 기본적으로 UI 위에서의 휠은 UI 스크롤을 위해 비워둠
            // 우클릭 팬 로직은 아래에서 계속 진행
        }
        else
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float newSize = this.cam.orthographicSize - scroll * this.zoomSpeed;
                this.cam.orthographicSize = Mathf.Clamp(newSize, this.minZoom, this.maxZoom);
            }
        }

        if (Input.GetMouseButton(1))

        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 right = transform.right;
            Vector3 forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 moveDir = -(right * mouseX + forwardOnPlane * mouseY);

            this.targetPosition += moveDir * this.panSpeed;
            this.targetPosition = GetClampedPosition(this.targetPosition);
        }
    }

    private void MoveCamera()
    {
        transform.position = Vector3.SmoothDamp(transform.position, this.targetPosition, ref this.currentVelocity, this.smoothTime);
    }

    private Vector3 GetClampedPosition(Vector3 target)
    {
        target.x = Mathf.Clamp(target.x, this.initialPos.x - this.panLimit.x, this.initialPos.x + this.panLimit.x);
        target.z = Mathf.Clamp(target.z, this.initialPos.z - this.panLimit.y, this.initialPos.z + this.panLimit.y);
        return target;
    }

    public void FocusOn(Vector3 worldPos)
    {
        Vector3 newPos = worldPos;
        newPos.y = transform.position.y; 
        this.targetPosition = GetClampedPosition(newPos);
    }

    public void ResetPosition()
    {
        this.targetPosition = this.initialPos;
    }
}
