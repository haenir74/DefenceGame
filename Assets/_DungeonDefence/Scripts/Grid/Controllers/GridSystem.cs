using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class GridSystem
{
    private JobHandle flowFieldJobHandle;
    private NativeArray<int> nodeDistances;
    private NativeArray<int> nodeObstacles;
    private bool isJobRunning = false;
    private GridMap currentMap;
    public void Generate(GridMap map, GridDataSO data)
    {
        map.Initialize(data.width, data.height);

        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y, data.cellSize);
                GridNode newNode = new GridNode(x, y, worldPos);
                map.Nodes[x, y] = newNode;

                if (x == data.spawnNodePos.x && y == data.spawnNodePos.y)
                    map.SpawnNode = newNode;
                if (x == data.coreNodePos.x && y == data.coreNodePos.y)
                    map.CoreNode = newNode;
            }
        }
    }

    private GridNode GetNode(GridMap map, GridDataSO data, int x, int y)
    {
        if (map == null || !map.IsValid(x, y)) return null;
        return map.Nodes[x, y];
    }

    public GridNode GetNode(GridMap map, GridDataSO data, Vector3 worldPosition)
    {
        if (map == null || data == null) return null;

        float halfCell = data.cellSize * 0.5f;
        int x = Mathf.FloorToInt((worldPosition.x + halfCell) / data.cellSize);
        int y = Mathf.FloorToInt((worldPosition.z + halfCell) / data.cellSize);

        return map.GetNode(x, y);
    }

    public Vector3 GetWorldPosition(int x, int y, float cellSize)
    {
        return new Vector3(x * cellSize, 0, y * cellSize);
    }

    public List<GridNode> GetNeighbors(GridMap map, GridNode node, bool includeDiagonals = false)
    {
        List<GridNode> neighbors = new List<GridNode>();
        if (map == null || node == null) return neighbors;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = node.X + dx[i];
            int ny = node.Y + dy[i];

            if (map.IsValid(nx, ny))
            {
                neighbors.Add(map.Nodes[nx, ny]);
            }
        }
        return neighbors;
    }

    [BurstCompile]
    private struct FlowFieldJob : IJob
    {
        public int width;
        public int height;
        public int coreIndex;
        public NativeArray<int> distances;
        [ReadOnly] public NativeArray<int> obstacles;
        [ReadOnly] public NativeArray<int> baseAttractiveness;

        public void Execute()
        {
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = int.MaxValue;
            }

            NativeQueue<int> queue = new NativeQueue<int>(Allocator.Temp);
            queue.Enqueue(coreIndex);
            distances[coreIndex] = 0;

            while (queue.TryDequeue(out int currentIndex))
            {
                int cx = currentIndex % width;
                int cy = currentIndex / width;
                int currentDist = distances[currentIndex];

                for (int i = 0; i < 4; i++)
                {
                    int nx = cx;
                    int ny = cy;
                    if (i == 0) ny += 1;
                    else if (i == 1) ny -= 1;
                    else if (i == 2) nx -= 1;
                    else if (i == 3) nx += 1;

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        int neighborIndex = ny * width + nx;

                        if (obstacles[neighborIndex] == 1) continue;

                        // Cost 계산: 순수 거리합(현재+1)에서 해당 타일의 BaseAttractiveness를 감산하여 더 낮고 매력적인 Cost로 변환
                        int stepCost = 1;
                        int heuristicCost = currentDist + stepCost - baseAttractiveness[neighborIndex];

                        if (distances[neighborIndex] > heuristicCost)
                        {
                            distances[neighborIndex] = heuristicCost;
                            queue.Enqueue(neighborIndex);
                        }
                    }
                }
            }
            queue.Dispose();
        }
    }

    private NativeArray<int> nodeBaseAttractiveness;

    public void CalculateFlowField(GridMap map, GridNode coreNode)
    {
        if (map == null || coreNode == null) return;

        if (isJobRunning)
        {
            flowFieldJobHandle.Complete();
            isJobRunning = false;
        }

        currentMap = map;
        int width = map.Nodes.GetLength(0);
        int height = map.Nodes.GetLength(1);
        int totalNodes = width * height;

        if (!nodeDistances.IsCreated || nodeDistances.Length != totalNodes)
        {
            if (nodeDistances.IsCreated) nodeDistances.Dispose();
            if (nodeObstacles.IsCreated) nodeObstacles.Dispose();
            if (nodeBaseAttractiveness.IsCreated) nodeBaseAttractiveness.Dispose();

            nodeDistances = new NativeArray<int>(totalNodes, Allocator.Persistent);
            nodeObstacles = new NativeArray<int>(totalNodes, Allocator.Persistent);
            nodeBaseAttractiveness = new NativeArray<int>(totalNodes, Allocator.Persistent);
        }

        for (int i = 0; i < totalNodes; i++)
        {
            int cx = i % width;
            int cy = i / width;

            nodeObstacles[i] = 0; // Obstacle data could be extracted here similarly

            GridNode node = map.Nodes[cx, cy];
            if (node != null && node.CurrentTileData != null)
            {
                nodeBaseAttractiveness[i] = node.CurrentTileData.baseAttractiveness;
            }
            else
            {
                nodeBaseAttractiveness[i] = 0;
            }
        }

        int coreIndex = coreNode.Y * width + coreNode.X;

        FlowFieldJob job = new FlowFieldJob
        {
            width = width,
            height = height,
            coreIndex = coreIndex,
            distances = nodeDistances,
            obstacles = nodeObstacles,
            baseAttractiveness = nodeBaseAttractiveness
        };

        flowFieldJobHandle = job.Schedule();
        isJobRunning = true;
    }

    public void LateUpdate()
    {
        if (isJobRunning)
        {
            flowFieldJobHandle.Complete();
            isJobRunning = false;

            if (currentMap != null && nodeDistances.IsCreated)
            {
                int width = currentMap.Nodes.GetLength(0);
                int height = currentMap.Nodes.GetLength(1);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int index = y * width + x;
                        currentMap.Nodes[x, y].DistanceToCore = nodeDistances[index];
                    }
                }
            }
        }
    }

    public void OnDestroy()
    {
        if (isJobRunning)
        {
            flowFieldJobHandle.Complete();
        }
        if (nodeDistances.IsCreated) nodeDistances.Dispose();
        if (nodeObstacles.IsCreated) nodeObstacles.Dispose();
        if (nodeBaseAttractiveness.IsCreated) nodeBaseAttractiveness.Dispose();
    }

    public void CalculateAttractivenessInfluence(GridMap map)
    {
        if (map == null) return;


        foreach (var node in map.Nodes)
        {
            node.TileInfluence = 0;
        }

        int maxRange = 5;
        float decayFactor = 0.5f;
        float radiationMultiplier = 0.5f;


        foreach (var sourceNode in map.Nodes)
        {
            int bonus = sourceNode.GetTileBonus();
            if (bonus == 0) continue;



            Queue<(GridNode node, int dist)> queue = new Queue<(GridNode, int)>();
            HashSet<GridNode> visited = new HashSet<GridNode>();

            queue.Enqueue((sourceNode, 0));
            visited.Add(sourceNode);

            while (queue.Count > 0)
            {
                var (current, dist) = queue.Dequeue();


                if (dist > 0)
                {
                    float decay = Mathf.Pow(decayFactor, dist);
                    current.TileInfluence += Mathf.RoundToInt(bonus * radiationMultiplier * decay);
                }

                if (dist < maxRange)
                {
                    foreach (var neighbor in GetNeighbors(map, current))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, dist + 1));
                        }
                    }
                }
            }
        }
    }
}



