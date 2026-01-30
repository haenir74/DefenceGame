using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Isometric Settings")]
    [SerializeField] private float angleX = 30f; 
    [SerializeField] private float angleY = 45f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 15f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 0.5f;
    [SerializeField] private bool invertPan = true;

    [Header("Limits")]
    [SerializeField] private Vector2 mapLimitPadding = new Vector2(5f, 5f);

    private Camera cam;

    private float minX, maxX, minZ, maxZ;
    private bool hasBounds = false;

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
    
    private void Update()
    {
        if (cam == null) return;

        HandleZoom();
        HandlePan();
    }

    public void SetupCamera(int width, int height, float cellSize)
    {
        if (cam == null) return;

        cam.orthographic = true;
        transform.rotation = Quaternion.Euler(angleX, angleY, 0);

        float mapWorldWidth = width * cellSize;
        float mapWorldHeight = height * cellSize;
        float midX = (mapWorldWidth * 0.5f) - (cellSize * 0.5f);
        float midZ = (mapWorldHeight * 0.5f) - (cellSize * 0.5f);
        
        Vector3 centerPos = new Vector3(midX, 0, midZ);
        float distance = 100f;
        transform.position = centerPos - transform.forward * distance;

        minX = transform.position.x - (mapWorldWidth / 2f) - mapLimitPadding.x;
        maxX = transform.position.x + (mapWorldWidth / 2f) + mapLimitPadding.x;
        minZ = transform.position.z - (mapWorldHeight / 2f) - mapLimitPadding.y;
        maxZ = transform.position.z + (mapWorldHeight / 2f) + mapLimitPadding.y;

        hasBounds = true;
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
            Vector3 moveDir = (right * mouseX + forwardOnPlane * mouseY);

            float direction = invertPan ? -1f : 1f;
            
            Vector3 targetPos = transform.position + moveDir * direction * panSpeed;

            if (hasBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
                targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);
            }

            transform.position = targetPos;
        }
    }
}
