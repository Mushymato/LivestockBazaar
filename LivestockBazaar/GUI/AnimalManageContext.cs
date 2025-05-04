using LivestockBazaar.Integration;
using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace LivestockBazaar.GUI;

public sealed record AnimalManageFarmAnimalEntry(BazaarBuildingEntry Bld, FarmAnimal Animal)
{
    private const int SCALE = 4;
    private const int MAX_WIDTH = 32 * SCALE;
    private const int MAX_HEIGHT = 32 * SCALE;

    public string DisplayName => $"{Animal.displayName} ({Animal.displayType})";
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
public sealed partial record AnimalManageContext : IHasSelectedLivestock
{
    public IReadOnlyDictionary<GameLocation, BazaarLocationEntry> AnimalHouseByLocation;
    public IEnumerable<BazaarLocationEntry> LocationEntries =>
        AnimalHouseByLocation.Values.OrderByDescending((loc) => loc.TotalRemainingSpaceCount);

    public AnimalManageContext()
    {
        AnimalHouseByLocation = BazaarContextMain.BuildAllAnimalHouseLocations(this);
    }

    // selected livestock entry
    [Notify]
    private BazaarLivestockEntry? selectedLivestock = null;

    // hovered building entry
    [Notify]
    private BazaarBuildingEntry? hoveredBuilding = null;

    // selected building entry
    [Notify]
    private BazaarBuildingEntry? selectedBuilding1 = null;

    // selected building entry
    [Notify]
    private BazaarBuildingEntry? selectedBuilding2 = null;

    [Notify]
    private AnimalManageFarmAnimalEntry? draggedAnimal = null;

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
}
