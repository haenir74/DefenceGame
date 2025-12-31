using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;   // 전체 맵의 최대 가로 크기
    public int height = 20;  // 전체 맵의 최대 세로 크기
    public float cellSize = 1f;
    public LayerMask groundMask; // 'Ground' 레이어만 감지하도록 설정

    [Header("Default Tile")]
    public Tile defaultTileData; // 기본으로 깔릴 타일 데이터 (Tile.cs)

    // 마스킹 방식: 유효하지 않은 곳은 null로 비워둡니다.
    private Node[,] grid; 

    void Awake()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        grid = new Node[width, height];
        
        // 맵의 왼쪽 아래부터 스캔 시작 (원점 기준)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 1. 그리드 중앙 위치 계산
                Vector3 worldPoint = transform.position + new Vector3(x * cellSize, 0, y * cellSize) + new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
                
                // 2. 해당 위치에 땅(Collider)이 있는지 확인 (마스킹 체크)
                // 위에서 아래로 레이를 쏴서 땅이 있으면 노드 생성
                if (Physics.Raycast(worldPoint + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundMask))
                {
                    // 노드 생성 (데이터만 연결)
                    Node newNode = new Node(new Vector2Int(x, y), worldPoint, defaultTileData);
                    
                    // 시각적 오브젝트 생성 (선택 사항: 이미 맵이 배치되어 있다면 생략 가능)
                    GameObject tileObj = Instantiate(defaultTileData.prefab, worldPoint, Quaternion.identity, transform);
                    newNode.UpdateTile(defaultTileData, tileObj);

                    grid[x, y] = newNode;
                }
                else
                {
                    // 땅이 없으면 null (이동 불가 지역)
                    grid[x, y] = null;
                }
            }
        }
    }

    // 좌표로 노드 가져오기 (유효성 체크 포함)
    public Node GetNode(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return grid[x, y];
        }
        return null;
    }

    // 월드 좌표를 그리드 좌표로 변환
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // GridManager의 원점 기준 상대 좌표 계산
        Vector3 localPos = worldPosition - transform.position;
        
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.z / cellSize);

        return GetNode(x, y);
    }

    // 이웃 노드 가져오기 (상하좌우) - 길 찾기용
    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // 상, 하, 좌, 우 오프셋
        int[] xDir = { 0, 0, -1, 1 };
        int[] yDir = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.X + xDir[i];
            int checkY = node.Y + yDir[i];

            Node neighbor = GetNode(checkX, checkY);
            
            // neighbor가 null이 아니면(맵 안쪽이고 마스킹되지 않음) 추가
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
}