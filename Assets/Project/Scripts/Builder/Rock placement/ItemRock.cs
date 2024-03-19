using UnityEngine;

public class ItemRock : ReplaceableOnMap
{
    public int rockType;
    private Vector3Int prevPos;

    private void Awake()
    {
        type = ItemType.Rock;

    }

    protected override void SpecificSelect()
    {
        // Verify if user is in the right menu
        if (UIManager.current.inventoryUI.menuSelection.GetFirstActiveToggle().name != "Island - Toggle") return;

        prevPos = IslandBuilder.current.islandTilemap.WorldToCell(gameObject.transform.position);

        //Database.Instance.userData.islandTile.m_TilingRules[0].m_GameObject
        GameObject tempObj = Instantiate(UIManager.GetGroundRule(rockType).m_DefaultGameObject,
                            GridBuildingSystem.current.gridLayout.CellToWorld(prevPos),
                            Quaternion.identity,
                            GridBuildingSystem.current.mainTilemap.gameObject.transform);
        IslandBuilder.current.RemoveRock(prevPos);
        //GridBuildingSystem.current.HighlightGrid(new(prevPos, Vector3Int.one), ItemType.Rock);

        //GridBuildingSystem.current.SelectAndAnimate(tempObj);
        IslandBuilder.current.Following(tempObj.GetComponent<ItemRock>());

    }

    public override void Follow()
    {
        Vector2 touchPos = UIManager.GetMousePos();
        Vector3Int cellPos = IslandBuilder.current.placeholderTilemap.LocalToCell(touchPos);

        gameObject.transform.localPosition = new Vector3(touchPos.x, touchPos.y - 0.25f, 0);

        if (prevPos != cellPos)
        {
            prevPos = cellPos;
        }
    }

    protected override void Place()
    {
        IslandBuilder.current.AddIslandTile(prevPos, rockType);
        Destroy(gameObject);
    }

    protected override bool CanBePlaced()
    {
        if (MouseOverMenu(UIManager.current.inventoryUI.gameObject))
        { //over ui, cannot be placed
            return false;
        }

        prevPos = IslandBuilder.current.islandTilemap.LocalToCell(gameObject.transform.localPosition);
        bool inBounds = IslandBuilder.current.placeholderTilemap.HasTile(prevPos);
        return (!IslandBuilder.current.islandTilemap.HasTile(prevPos) && inBounds); //there is no rock on the position, and it's inside of bounds

    }

    protected override void SpecificClick() { } //do nothing when click

    public override void ReplaceInInventory()
    {
        StartCoroutine(RemoveAnimation());
    }

    public override void RemoveFromTilemap() { }
}
