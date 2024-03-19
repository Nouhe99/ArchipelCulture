using UnityEngine;
using UnityEngine.UI;

public class ResponsiveGridLayout : LayoutGroup
{
    public int columns;
    public Vector2 spacing;
    public float height = 1;

    protected override void Start()
    {
        base.Start();
        Invoke("CalculateLayoutInputHorizontal", 0.1f);

    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float cellWidth = (rectTransform.rect.width / (float)columns) - ((spacing.x / (float)columns) * (columns - 1));

        int columnCount = 0;
        int rowCount = 0;

        //to do : implement padding

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / columns;
            columnCount = i % columns;

            var item = rectChildren[i];

            var xPos = (cellWidth * columnCount) + (spacing.x * columnCount);
            var yPos = (cellWidth * height * rowCount) + (spacing.y * rowCount);

            SetChildAlongAxis(item, 0, xPos, cellWidth);
            SetChildAlongAxis(item, 1, yPos, cellWidth * height);
        }

        rowCount++;
        //change size of contener
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, (cellWidth * height * rowCount) + (spacing.y * rowCount));


        /*
        //not supported
        //complete rows
        if (rectChildren.Count % columns != 0)
        {
            int slotToAdd = columns - (rectChildren.Count % columns);
            for (int i = 0; i < slotToAdd; i++)
            {
                tempSlot.Add(Instantiate(emptySlot, transform));

                rowCount = rectChildren.Count / columns;
                columnCount = rectChildren.Count % columns;
                var xPos = (cellWidth * columnCount) + (spacing.x * columnCount);
                var yPos = (cellWidth * height * rowCount) + (spacing.y * rowCount);

                SetChildAlongAxis(tempSlot[i].GetComponent<RectTransform>(), 0, xPos, cellWidth);
                SetChildAlongAxis(tempSlot[i].GetComponent<RectTransform>(), 1, yPos, cellWidth * height);
            }
        }
        if (tempSlot.Count >= columns)
        {
            for (int i = 1; i <= columns; i++)
            {
                DestroyImmediate(tempSlot[tempSlot.Count - i]);
            }
            tempSlot.RemoveRange(tempSlot.Count - columns, columns);
        }
        */
    }

    public override void CalculateLayoutInputVertical()
    {
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }
}
