using LivestockBazaar.Integration;
using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
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
    private const int MAX_WIDTH = 32 * SCALE;
    private const int MAX_HEIGHT = 32 * SCALE;

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
    public SDUIEdges SpritePadding
    {
        get
        {
            Rectangle rectangle = Animal.Sprite.sourceRect;
            return new SDUIEdges(0, 96 - Math.Min(rectangle.Height * SCALE, 96), 0, 0);
        }
    }
    public string SpriteLayout
    {
        get
        {
            Rectangle rectangle = Animal.Sprite.sourceRect;
            return $"{Math.Min(rectangle.Width * SCALE, MAX_WIDTH)}px {Math.Min(rectangle.Height * SCALE, MAX_HEIGHT)}px";
        }
    }

    public void HandleShowTooltip()
    {
        if (BazaarMenu.AMFAEEntry is AnimalManageEntry prev && prev.Held)
            return;
        BazaarMenu.AMFAEEntry = this;
    }
}

/// <summary>
/// Context for moving animals around.
/// </summary>
public sealed partial record AnimalManageContext : ITopLevelBazaarContext
{
    public IReadOnlyDictionary<GameLocation, BazaarLocationEntry> AnimalHouseByLocation;
    public IEnumerable<BazaarLocationEntry> LocationEntries =>
        AnimalHouseByLocation.Values.OrderByDescending((loc) => loc.TotalRemainingSpaceCount);

    public AnimalManageContext()
    {
        IReadOnlyList<BazaarLivestockEntry> livestockEntries = AssetManager
            .LsData.Values.Select((data) => new BazaarLivestockEntry(this, null, data))
            .ToList();
        AnimalHouseByLocation = BazaarContextMain.BuildAllAnimalHouseLocations(this, livestockEntries);
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

    public void HandleSelectBuilding1(BazaarBuildingEntry building)
    {
        if (SelectedBuilding2 == building)
            return;
        if (SelectedBuilding1 != null)
            SelectedBuilding1.IsSelected = false;
        SelectedBuilding1 = building;
        SelectedBuilding1.IsSelected = true;
        Game1.playSound("drumkit6");
    }

    public void HandleSelectBuilding2(BazaarBuildingEntry building)
    {
        if (SelectedBuilding1 == building)
            return;
        if (SelectedBuilding2 != null)
            SelectedBuilding2.IsSelected2 = false;
        SelectedBuilding2 = building;
        SelectedBuilding2.IsSelected2 = true;
        Game1.playSound("drumkit6");
    }

    public void HandleSelectForSwap(AnimalManageEntry? selected = null)
    {
        if (BazaarMenu.AMFAEEntry is not AnimalManageEntry prev)
        {
            BazaarMenu.AMFAEEntry = selected;
            if (selected != null)
                selected.Held = true;
            return;
        }
        if (prev == selected)
        {
            selected.Held = !selected.Held;
            return;
        }

        if (selected == null)
            return;

        if (prev is AnimalManageFarmAnimalEntry amfaePrev && amfaePrev.Animal.CanLiveIn(selected.Bld.Building))
        {
            if (selected is AnimalManageFarmAnimalEntry amfae)
            {
                if (amfae.Animal.CanLiveIn(prev.Bld.Building) && BazaarBuildingEntry.AMFAEListSwap(amfaePrev, amfae))
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

        // swap held
        prev.Held = false;
        selected.Held = true;
        BazaarMenu.AMFAEEntry = selected;
    }

    public int GetCurrentlyOwnedCount(BazaarLivestockEntry livestock)
    {
        return AnimalHouseByLocation.Values.Sum(loc => loc.GetCurrentLivestockCount(livestock));
    }

    public void ClearTooltip()
    {
        if (BazaarMenu.AMFAEEntry?.Held ?? false)
            return;
        BazaarMenu.AMFAEEntry = null;
    }

    public void ClearTooltipForce() => BazaarMenu.AMFAEEntry = null;

    // not relevant for this UI
    public bool HasSpaceForLivestock(BazaarLivestockEntry livestock) =>
        throw new NotImplementedException("HasSpaceForLivestock");

    public bool HasRequiredBuilding(BazaarLivestockEntry livestock) =>
        throw new NotImplementedException("HasRequiredBuilding");
}
