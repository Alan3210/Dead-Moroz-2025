using UnityEngine;

public class SimpleExpenseLayouter : MonoBehaviour
{
    [SerializeField] private float itemHeight = 50f;
    [SerializeField] private float spacing = 5f;
    [SerializeField] private float topOffset = 0f;

    void Start()
    {
        PositionChildren();
    }

    public void PositionChildren()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        int childCount = rectTransform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = rectTransform.GetChild(i) as RectTransform;
            if (child != null)
            {
                float yPosition = topOffset - (i * (itemHeight + spacing));
                child.anchoredPosition = new Vector2(0, yPosition);
            }
        }
    }

    void OnTransformChildrenChanged()
    {
        PositionChildren();
    }
}
