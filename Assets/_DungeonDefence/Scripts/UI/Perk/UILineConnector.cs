using UnityEngine;
using UnityEngine.UI;

public class UILineConnector : MonoBehaviour
{
    public void Connect(RectTransform start, RectTransform end, Color color, float thickness)
    {
        GameObject lineObj = new GameObject("Line", typeof(Image));
        lineObj.transform.SetParent(transform, false);
        lineObj.transform.SetAsFirstSibling();

        Image img = lineObj.GetComponent<Image>();
        img.color = color;

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        Vector2 dir = (end.anchoredPosition - start.anchoredPosition).normalized;
        float distance = Vector2.Distance(start.anchoredPosition, end.anchoredPosition);

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(distance, thickness);
        rect.anchoredPosition = start.anchoredPosition + (dir * distance * 0.5f);
        rect.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
    }
}



