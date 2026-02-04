using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 0.5f;
    [SerializeField] private Vector2 panLimit = new Vector2(10f, 15f);

    private Camera cam;
    private Vector3 initialPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void Start()
    {
        initialPos = transform.position;
    }
    
    private void Update()
    {
        if (cam == null) return;

        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    private void HandlePan()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 right = transform.right;
            Vector3 forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 moveDir = -(right * mouseX + forwardOnPlane * mouseY);

            Vector3 targetPos = transform.position + moveDir * panSpeed;
            transform.position = GetPanLimit(targetPos);
        }
    }

    private Vector3 GetPanLimit(Vector3 targetPos)
    {
        targetPos.x = Mathf.Clamp(targetPos.x, initialPos.x - panLimit.x, initialPos.x + panLimit.x);
        targetPos.z = Mathf.Clamp(targetPos.z, initialPos.z - panLimit.y, initialPos.z + panLimit.y);

        return targetPos;
    }
}
