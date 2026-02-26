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

    private List<PerkNodeUI> treeNodes = new List<PerkNodeUI>();

    private void Start()
    {
        FindNodesInScene();
        DrawConnections();
        UpdateUI();
        if (startBattleButton != null)
            startBattleButton.onClick.AddListener(() => SceneController.Instance.LoadGame());
    }

    private void FindNodesInScene()
    {
        treeNodes.Clear();
        var nodes = GetComponentsInChildren<PerkNodeUI>(true);
        treeNodes.AddRange(nodes);
    }

    private void DrawConnections()
    {
        if (lineConnector == null) return;

        foreach (var node in treeNodes)
        {
            if (node.Data == null || node.Data.prerequisitePerks == null) continue;

            foreach (string prereqId in node.Data.prerequisitePerks)
            {
                PerkNodeUI prereqNode = treeNodes.Find(n => n.Data != null && n.Data.perkId == prereqId);
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



