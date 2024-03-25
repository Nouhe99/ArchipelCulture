using UnityEngine;

public class ItemFloor : Item
{
    [Header("Floor")]
    public ItemFloor floorOnTop;
    public ItemFloor floorOnBottom;
    public GameObject selectionImage;
    #region Position Floor

    private readonly float distanceHeight = .5f;

    // Determines the position offset for placing a floor on top of another floor
    public Vector3 GetOffsetPosition()
    {
        return new Vector3(0, distanceHeight, -1f);
    }


    // Gets the floor number of each floor
    public int GetFloorNumber()
    {
        int nb = 0;
        ItemFloor floor = floorOnBottom;
        while (floor != null && floor != floorOnTop)
        {
            nb++;
            floor = floor.floorOnBottom;
            if (nb > 100)
            {
                Debug.LogError("INFINITE WHILE 'GetFloorNumber' ");
                break;
            }
        }
        return nb;
    }

    // Checks and returns the highest floor of any given stack 
    public ItemFloor GetHighestFloor()
    {
        ItemFloor floor = this;
        while (floor.floorOnTop != null && floor != floor.floorOnTop && floor.floorOnTop.placed)
        {
            floor = floor.floorOnTop;
        }
        return floor;
    }



    // This method saves the positions of each floor and sends them to the database
    public void SaveFloors()
    {
        ItemFloor floor = this;
        int zPosition = floor.area.position.z;
        SaveDataInventory.Instance.AddPlacedObj(floor.id, floor.area.position.x, floor.area.position.y, zPosition, floor.id_inventory);
        //Debug.Log("Bottom floor saved : (" + floor.area.position.x + " , " + floor.area.position.y + " , " + zPosition + " ) " + floor.id);

        while (floor.floorOnTop != null && floor != floor.floorOnTop && floor.floorOnTop.placed)
        {
            floor = floor.floorOnTop;
            zPosition++;
            SaveDataInventory.Instance.AddPlacedObj(floor.id, floor.area.position.x, floor.area.position.y, zPosition, floor.id_inventory);
            //Debug.Log("Top floors saved : (" + floor.area.position.x + " , " + floor.area.position.y + " , " + zPosition + " ) " + floor.id);
        }
    }


    // This method allows for the placement of floors on top of another, and checks for errors while doing so || Most of these checks are technically "useless" seeing as we disable the colliders of the floors now
    public void PlaceFloorOnTop(ItemFloor newFloor)
    {
        if (newFloor == null)
        {
            Debug.LogError("Invalid floor reference.");
            return;
        }

        ItemFloor highestFloor = GetHighestFloor();

        // Check if newFloor is above its bottom floor
        if (IsAboveBottomFloor(newFloor))
        {
            Debug.LogWarning("Cannot place floor on top of any floor within its own stack.");
            return;
        }

        // Rest of the conditions for placement
        if (highestFloor != null && highestFloor != newFloor && !IsFloorAlreadyPlaced(newFloor, highestFloor) && newFloor != highestFloor && newFloor != floorOnTop)
        {
            UpdateFloorTransform(newFloor, highestFloor);
            AttachFloors(newFloor, highestFloor);
            DisableFloorColliders(newFloor);

        }
        else if (newFloor == highestFloor)
        {
            Debug.LogWarning("Cannot place floor on itself.");
        }
        else
        {
            Debug.LogWarning("Floor placement not allowed.");
        }
    }

    // Checks if floors are above the bottom floor that you have selected and deactivates those floors colliders
    private bool IsAboveBottomFloor(ItemFloor floor)
    {
        ItemFloor currentFloor = floorOnBottom;

        while (currentFloor != null && currentFloor != currentFloor.floorOnTop)
        {
            if (currentFloor == floor)
            {
                DisableFloorColliders(currentFloor.floorOnTop);
                return true;
            }
            currentFloor = currentFloor.floorOnTop;
        }
        return false;
    }

    // Disables the colliders of all floors above the bottom floor (floorOnBottom) 
    private void DisableFloorColliders(ItemFloor floor)
    {
        Collider2D[] colliders = floor.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        ItemFloor currentFloor = floor.floorOnTop;
        while (currentFloor != null)
        {
            DisableFloorColliders(currentFloor);
            currentFloor = currentFloor.floorOnTop;
        }
    }


    // Enables the colliders of all floors above the bottom floor (floorOnBottom)
    private void EnableFloorColliders(ItemFloor floor)
    {
        Collider2D[] colliders = floor.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = true;
        }

        ItemFloor currentFloor = floor.floorOnTop;
        while (currentFloor != null)
        {
            EnableFloorColliders(currentFloor);
            currentFloor = currentFloor.floorOnTop;
        }
    }

    // Checks and returns the highest floor
    private bool IsFloorAlreadyPlaced(ItemFloor floor, ItemFloor highestFloor)
    {
        return floor == highestFloor.floorOnTop || floor == highestFloor.floorOnBottom;
    }

    // This attaches the floors to one another while updating the highest floor on the stack
    private void AttachFloors(ItemFloor newFloor, ItemFloor highestFloor)
    {
        highestFloor.floorOnTop = newFloor;
        newFloor.floorOnBottom = highestFloor;
    }

    // This moves the floor you are moving to the top of the stack of floors you are be moving it to 
    private void UpdateFloorTransform(ItemFloor newFloor, ItemFloor highestFloor)
    {
        newFloor.transform.parent = highestFloor.transform;
        newFloor.transform.localPosition = highestFloor.GetOffsetPosition();
    }

    public void SelectFloor()
    {
        transform.parent = UIManager.current.itemContener.transform;
        if (floorOnBottom != null)
        {
            floorOnBottom.floorOnTop = null;
            floorOnBottom = null;
        }
    }

    #endregion


    #region Override ReplacableOnMap
    protected override void SpecificClick()
    {
        //Do nothing when click on floor | This is normally for items that change state or make a sound when clicked on, we do not wish to do so to floors at the moment
    }


    // This checks if you are in the Builder menu and where you can place items on the builder grid layout
    protected override void SpecificSelect()
    {
        // Verify user is in the Floor menu
        if (UIManager.current.inventoryUI.menuSelection.GetFirstActiveToggle().name != "Home - Toggle") return;

        if (selectionImage != null) selectionImage.SetActive(false);
        thisCollider.enabled = false;
        if (floorOnBottom == null)
        {
            // No floor existing under this one
            GridBuildingSystem.current.FreeArea(area); //reinitialize tile
        }
        else
        {
            // Floor exist under this one
            SelectFloor();
        }


        GridBuildingSystem.current.Select(this);
        GridBuildingSystem.current.prevPos = area.position;
        GridBuildingSystem.current.HighlightGrid(this);
    }

    /// <summary>
    /// Recursively adjusts the positions of all stacked floors above the current floor.
    /// Starting from the current floor, it updates the area.position of all stacked floors,
    /// ensuring proper alignment of stacked floors in the game world.
    /// </summary>
    public void FollowArea()
    {
        // Start with the current floor
        ItemFloor floor = this;

        // Traverse through all stacked floors above the current floor
        while (floor.floorOnTop != null && floor != floor.floorOnTop && floor.floorOnTop.placed)
        {
            floor = floor.floorOnTop;

            // Adjust the area.position to match the current floor's position
            floor.area.position = new Vector3Int(area.position.x, area.position.y, floor.area.position.z);
        }
    }

    /// <summary>
    /// Handles the movement and placement of the current floor based on user input.
    /// Checks mouse position, places the floor on top of existing floors if found,
    /// and updates the floor's position if no existing floor is present.
    /// </summary>
    public override void Follow()
    {
        // Select the current floor
        SelectFloor();

        // Get the mouse position in grid cell coordinates
        Vector2 touchPos = UIManager.GetMousePos();
        Vector3Int cellPos = IslandBuilder.current.placeholderTilemap.LocalToCell(touchPos);
        GridBuildingSystem builder = GridBuildingSystem.current;

        // Check if there is an existing floor at the mouse position
        ItemFloor itemFloor = builder.GetFloorAtMousePosition();
        if (itemFloor != null && itemFloor != this)
        {
            // Place the current floor on top of the existing floor
            itemFloor.PlaceFloorOnTop(this);
            area.position = new Vector3Int(itemFloor.area.position.x, itemFloor.area.position.y, GetFloorNumber());
            FollowArea();
            builder.HighlightGrid(area, type);
            builder.HighlightFloorsRoof();
        }
        else
        {
            // No existing floor at the mouse position, update the current floor's position directly
            floorOnBottom = null;
            transform.localPosition = (Vector3)((Vector2)builder.gridLayout.CellToLocalInterpolated(cellPos)) + new Vector3(0, 0, cellPos.x + cellPos.y - 1);
            builder.prevPos = cellPos;
            area.position = new Vector3Int(builder.prevPos.x, builder.prevPos.y, GetFloorNumber());
            FollowArea();
            builder.HighlightGrid(area, type);
            builder.HighlightFloorsRoof();
        }
    }

    protected override void Place()
    {
        Vector3Int positionInt = GridBuildingSystem.current.prevPos;
        // FX
        Instantiate(GridBuildingSystem.current.smokeParticles, GridBuildingSystem.current.gridLayout.CellToWorld(positionInt), GridBuildingSystem.current.smokeParticles.transform.rotation, gameObject.transform);
        PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.placeItem);

        // if (selectionImage != null) selectionImage.SetActive(true);

        SelectFloor();
        ItemFloor itemFloor = GridBuildingSystem.current.GetFloorAtMousePosition();
        if (itemFloor != null && itemFloor != this)
        {
            itemFloor.PlaceFloorOnTop(this);
        }
        else
        {
            gameObject.transform.localPosition = (Vector3)((Vector2)GridBuildingSystem.current.gridLayout.CellToLocalInterpolated(positionInt)) + new Vector3(0, 0, positionInt.x + positionInt.y - 1);
            BoundsInt areaTemp = area;
            areaTemp.position = positionInt;
            GridBuildingSystem.current.TakeArea(TileType.TakenByFloor, areaTemp);
        }
        placed = true;
        thisCollider.enabled = true;

        // Save and update positions
        FollowArea();
        SaveFloors();

        // Re-enable colliders after the floor has been placed
        EnableFloorColliders(this);



        // Verify title
        ItemFloor[] floors = UIManager.current.itemContener.GetComponentsInChildren<ItemFloor>();
        //UIManager.current.unlockTitles.Verify(floors.Length, UnlockTitle.TitleCategory.ConstructionMaison);
    }

    protected override bool CanBePlaced()
    {
        if (MouseOverMenu(UIManager.current.inventoryUI.gameObject))
        { //over ui, cannot be placed
            return false;
        }
        Vector3Int positionInt = GridBuildingSystem.current.prevPos;
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        return GridBuildingSystem.current.FloorCanTakeArea(areaTemp);
    }

    // Deleted item gets put back into inventory 
    public override void ReplaceInInventory()
    {
        StartCoroutine(RemoveAnimation());
        ItemFloor floor = this;
        while (floor != null)
        {
            if (floor.id_inventory != "")
            {
                //remove from isle but store it in inventory
                UIManager.current.inventoryUI.AddNewSlot(floor);
                SaveDataInventory.Instance.AddPlacedObj(floor.id, 0, 0, 0, floor.id_inventory, false);
            }
            floor = floor.floorOnTop;
        }
    }

    // Deletes an item from the gameworld
    public override void RemoveFromTilemap()
    {
        if (floorOnBottom == null)
        {
            // Item is at ground level
            // Free area
            GridBuildingSystem.current.FreeArea(area);
        }
        else
        {
            // Item is on top of another
            // Remove reference on below item
            floorOnBottom.floorOnTop = null;
            floorOnBottom = null;
        }

        // Send items on top back to inventory
        if (floorOnTop != null)
        {
            floorOnTop.ReplaceInInventory();
        }
    }

    #endregion
}
