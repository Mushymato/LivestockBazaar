using LivestockBazaar.Integration;
using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewValley;

namespace LivestockBazaar.GUI;

public partial record AnimalManageEntry(BazaarBuildingEntry Bld)
{
    [Notify]
    private bool held = false;
    public bool IsPlacehold => this is AnimalManagePlaceholder;
}

public sealed record AnimalManagePlaceholder(BazaarBuildingEntry Bld) : AnimalManageEntry(Bld);

public sealed record AnimalManageFarmAnimalEntry(BazaarBuildingEntry Bld, FarmAnimal Animal) : AnimalManageEntry(Bld)
{
    private const int SCALE = 4;
    private const int MAX_WIDTH = 96;
    private const int MAX_HEIGHT = 96;

    public string DisplayName => Animal.displayName;
    public string DisplayType => Animal.displayType;
    public IEnumerable<bool> Hearts
    {
        get
        {
            int heartLevel = Animal.friendshipTowardFarmer.Value / 200;
            for (int i = 0; i < heartLevel; i++)
                yield return true;
            for (int i = heartLevel; i < 5; i++)
                yield return false;
        }
    }

    public SDUISprite Sprite => new(Animal.Sprite.Texture, Animal.Sprite.sourceRect);

    public string SpriteLayout
    {
        get
        {
            Rectangle rectangle = Animal.Sprite.sourceRect;
            return $"{Math.Min(rectangle.Width * SCALE, MAX_WIDTH)}px {Math.Min(rectangle.Height * SCALE, MAX_HEIGHT)}px";
        }
    }
}

/// <summary>
/// Context for moving animals around.
/// </summary>
public sealed partial record AnimalManageContext : ITopLevelBazaarContext
{
    public readonly IReadOnlyDictionary<GameLocation, BazaarLocationEntry> AnimalHouseByLocation;
    public readonly IList<BazaarLocationEntry> LocationEntries;

    public AnimalManageContext()
    {
        IReadOnlyList<BazaarLivestockEntry> livestockEntries = AssetManager
            .LsData.Values.Select((data) => new BazaarLivestockEntry(this, null, data))
            .ToList();
        AnimalHouseByLocation = BazaarContextMain.BuildAllAnimalHouseLocations(this, livestockEntries);
        if (!AnimalHouseByLocation.Any())
        {
            throw new ArgumentException("No valid locations");
        }
        LocationEntries = AnimalHouseByLocation
            .Values.OrderByDescending((loc) => loc.TotalRemainingSpaceCount)
            .ToList();
    }

    [Notify]
    private int selectedLocationIndex = 0;

    public BazaarLocationEntry SelectedLocation => LocationEntries[SelectedLocationIndex];

    public bool ShowNav => LocationEntries.Count > 1;

    public void PrevLocation()
    {
        if (SelectedLocationIndex == 0)
            SelectedLocationIndex = LocationEntries.Count - 1;
        else
            SelectedLocationIndex--;
    }

    public void NextLocation()
    {
        if (SelectedLocationIndex >= LocationEntries.Count - 1)
            SelectedLocationIndex = 0;
        else
            SelectedLocationIndex++;
    }

    public void ScrollLocations(SDUIDirection direction)
    {
        if (!ShowNav)
            return;
        switch (direction)
        {
            case SDUIDirection.North:
                PrevLocation();
                break;
            case SDUIDirection.South:
                NextLocation();
                break;
        }
    }

    public void PageLocations(SButton button)
    {
        switch (button)
        {
            case SButton.LeftTrigger:
                PrevLocation();
                break;
            case SButton.RightTrigger:
                NextLocation();
                break;
        }
    }

    // hovered building entry
    [Notify]
    private BazaarBuildingEntry? hoveredBuilding = null;

    // selected building entry
    [Notify]
    private BazaarBuildingEntry? selectedBuilding1 = null;

    // selected building entry
    [Notify]
    private BazaarBuildingEntry? selectedBuilding2 = null;

    public BazaarLivestockEntry? SelectedLivestock => null;

    internal BazaarBuildingEntry? UpdateSelectBuilding(
        BazaarBuildingEntry building,
        BazaarBuildingEntry? existing,
        BazaarBuildingEntry? other
    )
    {
        if (other == building)
            return existing;
        if (existing != null)
        {
            existing.Select = BazaarBuildingEntry.SelectionState.None;
            existing.HeldAnimalCanLiveHere = true;
        }
        if (BazaarMenu.AMFAEEntry is AnimalManageFarmAnimalEntry amfae)
        {
            building.HeldAnimalCanLiveHere = amfae.Animal.CanLiveIn(building.Building);
        }
        Game1.playSound("drumkit6");
        return building;
    }

    public void HandleSelectBuilding1(BazaarBuildingEntry building)
    {
        SelectedBuilding1 = UpdateSelectBuilding(building, SelectedBuilding1, SelectedBuilding2);
        if (SelectedBuilding1 != null)
        {
            SelectedBuilding1.Select = BazaarBuildingEntry.SelectionState.Left;
        }
    }

    public void HandleSelectBuilding2(BazaarBuildingEntry building)
    {
        SelectedBuilding2 = UpdateSelectBuilding(building, SelectedBuilding2, SelectedBuilding1);
        if (SelectedBuilding2 != null)
        {
            SelectedBuilding2.Select = BazaarBuildingEntry.SelectionState.Right;
        }
    }

    public void UpdateCanLiveHere()
    {
        if (BazaarMenu.AMFAEEntry is AnimalManageFarmAnimalEntry amfae2)
        {
            BazaarBuildingEntry? otherBuilding = null;
            if (SelectedBuilding1 == amfae2.Bld)
            {
                otherBuilding = SelectedBuilding2;
            }
            else if (SelectedBuilding2 == amfae2.Bld)
            {
                otherBuilding = SelectedBuilding1;
            }
            if (otherBuilding != null)
            {
                amfae2.Bld.HeldAnimalCanLiveHere = true;
                otherBuilding.HeldAnimalCanLiveHere = amfae2.Animal.CanLiveIn(otherBuilding.Building);
            }
        }
        else
        {
            if (SelectedBuilding1 != null)
                SelectedBuilding1.HeldAnimalCanLiveHere = true;
            if (SelectedBuilding2 != null)
                SelectedBuilding2.HeldAnimalCanLiveHere = true;
        }
    }

    public void ClearTooltip()
    {
        if (BazaarMenu.AMFAEEntry?.Held ?? false)
            return;
        BazaarMenu.AMFAEEntry = null;
        UpdateCanLiveHere();
    }

    public void HandleShowTooltip(AnimalManageEntry selected)
    {
        if (BazaarMenu.AMFAEEntry is AnimalManageFarmAnimalEntry prev && prev.Held)
            return;
        BazaarMenu.AMFAEEntry = selected;
        UpdateCanLiveHere();
    }

    public void HandleSelectForSwap(AnimalManageEntry selected)
    {
        if (BazaarMenu.AMFAEEntry is not AnimalManageEntry prev)
        {
            BazaarMenu.AMFAEEntry = selected;
            selected.Held = true;
            return;
        }
        if (prev == selected)
        {
            selected.Held = !selected.Held;
            return;
        }

        // consider adding
        if (prev.Bld != selected.Bld)
        {
            if (prev is AnimalManageFarmAnimalEntry amfaePrev && amfaePrev.Animal.CanLiveIn(selected.Bld.Building))
            {
                if (selected is AnimalManageFarmAnimalEntry amfae)
                {
                    if (
                        amfae.Animal.CanLiveIn(prev.Bld.Building) && BazaarBuildingEntry.AMFAEListSwap(amfaePrev, amfae)
                    )
                    {
                        amfaePrev.Held = false;
                        BazaarMenu.AMFAEEntry = null;
                        return;
                    }
                }
                else if (BazaarBuildingEntry.AMFAEListMove(amfaePrev, selected))
                {
                    amfaePrev.Held = false;
                    BazaarMenu.AMFAEEntry = null;
                    return;
                }
            }
        }

        // swap held
        prev.Held = false;
        selected.Held = true;
        BazaarMenu.AMFAEEntry = selected;
    }

    public int GetCurrentlyOwnedCount(BazaarLivestockEntry livestock)
    {
        return AnimalHouseByLocation.Values.Sum(loc => loc.GetCurrentLivestockCount(livestock));
    }

    public void ClearTooltipForce() => BazaarMenu.AMFAEEntry = null;

    // not relevant for this UI
    public bool HasSpaceForLivestock(BazaarLivestockEntry livestock) =>
        throw new NotImplementedException("HasSpaceForLivestock");

    public bool HasRequiredBuilding(BazaarLivestockEntry livestock) =>
        throw new NotImplementedException("HasRequiredBuilding");
}
