using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScoreVisualizer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool showOnGameView = true;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 20;
    [SerializeField] private float yOffset = 0.5f; // 타일보다 살짝 위에 표시

    [Header("Calculation Params (Must match Pathfinder)")]
    [SerializeField] private int coreBaseScore = 50;  // EnemyPathfinder와 맞춰주세요
    [SerializeField] private int distancePenalty = 1; // EnemyPathfinder와 맞춰주세요

    private List<TextMesh> _debugTexts = new List<TextMesh>();
    private Transform _container;

    private void Start()
    {
        // 그리드 생성이 완료될 때까지 잠시 대기
        StartCoroutine(InitRoutine());
    }

    private IEnumerator InitRoutine()
    {
        yield return new WaitForSeconds(0.5f); // 맵 생성 대기
        CreateDebugTexts();
        UpdateScores();
    }

    private void Update()
    {
        // T키를 누르거나 상황이 바뀔 때 업데이트 (테스트용)
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Space))
        {
            UpdateScores();
        }
    }

    private void CreateDebugTexts()
    {
        if (GridManager.Instance.Map == null) return;

        _container = new GameObject("DebugTextContainer").transform;
        
        var map = GridManager.Instance.Map;
        for (int x = 0; x < map.Nodes.GetLength(0); x++)
        {
            for (int y = 0; y < map.Nodes.GetLength(1); y++)
            {
                GridNode node = map.GetNode(x, y);
                if (node == null) continue;

                // 텍스트 오브젝트 생성
                GameObject textObj = new GameObject($"Score_{x}_{y}");
                textObj.transform.SetParent(_container);
                textObj.transform.position = node.WorldPosition + Vector3.up * yOffset;
                
                // 텍스트 컴포넌트 추가
                TextMesh textMesh = textObj.AddComponent<TextMesh>();
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.alignment = TextAlignment.Center;
                textMesh.characterSize = 0.2f; // 월드 스케일 조정
                textMesh.fontSize = fontSize;
                textMesh.color = textColor;

                _debugTexts.Add(textMesh);
            }
        }
    }

    // 점수 재계산 및 표시
    public void UpdateScores()
    {
        if (GridManager.Instance.Map == null || GridManager.Instance.Map.CoreNode == null) return;

        GridNode core = GridManager.Instance.Map.CoreNode;
        var map = GridManager.Instance.Map;
        int index = 0;

        for (int x = 0; x < map.Nodes.GetLength(0); x++)
        {
            for (int y = 0; y < map.Nodes.GetLength(1); y++)
            {
                if (index >= _debugTexts.Count) break;

                GridNode node = map.GetNode(x, y);
                TextMesh textMesh = _debugTexts[index];

                if (node != null)
                {
                    // === [점수 계산 로직] EnemyPathfinder와 동일한 공식 ===
                    int tileBonus = node.GetTileBonus();
                    int dist = node.GetDistance(core);
                    int score = coreBaseScore - (dist * distancePenalty) + tileBonus;

                    textMesh.text = score.ToString();
                    
                    // 점수에 따라 색상 변경 (높으면 초록, 낮으면 빨강)
                    // 시각적으로 구분이 쉽도록 보간
                    float colorRatio = Mathf.InverseLerp(coreBaseScore - 20, coreBaseScore + 10, score);
                    textMesh.color = Color.Lerp(Color.red, Color.green, colorRatio);
                }
                
                index++;
            }
        }
    }
}