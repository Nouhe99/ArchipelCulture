using System.Collections.Generic;
using UnityEngine;

public class FlexibleGridLayout : MonoBehaviour
{
    public int columns;
    public Vector2Int sizeCell;
    public Vector2 spacing;

    private void Update()
    {
        if (gameObject.activeInHierarchy)
            CalculatePlacement();
    }

    public void CalculatePlacement()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        float parentWidth = rectTransform.rect.width;

        float cellSize = (parentWidth / (float)columns) - ((spacing.x / (float)columns) * (columns - 1));

        List<bool[]> inventory = new()
        {
            new bool[columns]
        }; //true mean there is already an object in the slot

        int firstRowNotFull = 0;

        bool canBePlacedHere;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform item = gameObject.transform.GetChild(i);

            int rowCount = sizeCell.x;
            int columnCount = sizeCell.y;

            if (rowCount > columns) rowCount = columns; //objects cannot have bigger lenght than number of columns.
            int posInRow;
            int nextRow = firstRowNotFull - 1;
            do
            {
                do
                {
                    do
                    {
                        posInRow = 0;
                        nextRow++;
                        if (nextRow >= inventory.Count) inventory.Add(new bool[columns]);
                        foreach (var slot in inventory[nextRow])
                        {
                            if (slot == true) posInRow++;
                            else if (slot == false) break;
                        }
                        if (nextRow == firstRowNotFull && posInRow == columns) firstRowNotFull++;
                    } while ((posInRow + rowCount) > columns); //object cannot be in the actual row, mush be in the next one.

                    canBePlacedHere = true;
                    for (int j = 0; j < rowCount; j++) //is there place for the object in the next cells ? 
                    {
                        if (inventory[nextRow][posInRow + j])
                        {
                            canBePlacedHere = false;
                            break;
                        }
                    }
                } while (!canBePlacedHere);

                //the right row has been found (nextRow), is there enought place under it ?

                for (int j = 1; j < columnCount; j++)
                {
                    if (nextRow + j >= inventory.Count) inventory.Add(new bool[columns]);
                    for (int k = 0; k < rowCount; k++)
                    {
                        if (inventory[nextRow + j][posInRow + k]) //a slot under it has already been taken, cannot place item here.
                        {
                            canBePlacedHere = false;
                            break;
                        }
                    }
                    if (!canBePlacedHere) break;
                }
            } while (!canBePlacedHere);

            //fill array with position of item
            for (int j = 0; j < columnCount; j++)
            {
                for (int k = 0; k < rowCount; k++)
                {
                    inventory[nextRow + j][posInRow + k] = true;
                }
            }

            item.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2((cellSize * rowCount) + (spacing.x * (rowCount - 1)), (cellSize * columnCount) + (spacing.y * (columnCount - 1)));
            item.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2((posInRow * cellSize) + (spacing.x * posInRow), ((nextRow * cellSize) + (spacing.y * nextRow)) * -1);
        }

        //change size of content viewport
        rectTransform.sizeDelta = new Vector2(0, inventory.Count * (cellSize + spacing.y));

    }
}
