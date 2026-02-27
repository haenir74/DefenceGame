using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PerkTreeManager : Singleton<PerkTreeManager>
{

    [SerializeField] private PerkTreeSO perkTreeData;


    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TextMeshProUGUI perkPointsText;
    [SerializeField] private Button startBattleButton;

    [SerializeField] private UILineConnector lineConnector;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float lineThickness = 2f;

    [SerializeField] private PerkNodeUI nodePrefab;
    [SerializeField] private float xSpacing = 200f;
    [SerializeField] private float ySpacing = 150f;

    private List<PerkNodeUI> treeNodes = new List<PerkNodeUI>();

    public void Initialize()
    {
        GenerateNodes();
        DrawConnections();
        UpdateUI();
        if (startBattleButton != null)
        {
            startBattleButton.onClick.RemoveAllListeners();
            startBattleButton.onClick.AddListener(() =>
            {
                Debug.Log("[PerkTreeManager] Start Battle Button Clicked. Loading Game Scene...");
                SceneController.Instance.LoadGame();
            });
        }
    }

    private void GenerateNodes()
    {
        if (MetaManager.Instance == null || nodePrefab == null || contentRoot == null) return;

        foreach (Transform child in contentRoot)
        {
            if (child.GetComponent<PerkNodeUI>() != null)
            {
                Destroy(child.gameObject);
            }
        }
        treeNodes.Clear();

        var perks = MetaManager.Instance.AvailablePerks;
        if (perks == null || perks.Count == 0) return;

        int row = 0;
        int col = 0;
        int itemsInRow = 1;

        for (int i = 0; i < perks.Count; i++)
        {
            PerkNodeUI node = Instantiate(nodePrefab, contentRoot);
            node.Setup(perks[i]);

            RectTransform rt = node.GetComponent<RectTransform>();
            if (rt != null)
            {
                float startX = -(itemsInRow - 1) * xSpacing * 0.5f;
                float px = startX + col * xSpacing;
                float py = -row * ySpacing;
                rt.anchoredPosition = new Vector2(px, py);
            }

            treeNodes.Add(node);

            col++;
            if (col >= itemsInRow)
            {
                col = 0;
                row++;
                itemsInRow = (itemsInRow == 1) ? 2 : 1;
            }
        }
    }

    private void DrawConnections()
    {
        if (lineConnector == null) return;

        foreach (var node in treeNodes)
        {
            if (node.Data == null || node.Data.Prerequisites == null) continue;

            foreach (var prereq in node.Data.Prerequisites)
            {
                if (prereq == null) continue;
                PerkNodeUI prereqNode = treeNodes.Find(n => n.Data != null && n.Data.PerkID == prereq.PerkID);
                if (prereqNode != null)
                {
                    lineConnector.Connect(prereqNode.GetComponent<RectTransform>(), node.GetComponent<RectTransform>(), lineColor, lineThickness);
                }
            }
        }
    }

    public void RefreshAllNodes()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (MetaManager.Instance != null && perkPointsText != null)
        {
            perkPointsText.text = $"SP: {MetaManager.Instance.PerkPoints}";
        }

        foreach (var node in treeNodes)
        {
            node.UpdateVisuals();
        }
    }
}



