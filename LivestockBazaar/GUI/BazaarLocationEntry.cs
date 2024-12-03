using StardewValley;
using StardewValley.Buildings;

namespace LivestockBazaar.GUI;

public sealed record class BazaarBuildingEntry(Building Building)
{
    public string BuildingName =>
        $"{Building.buildingType.Value} ({Building.currentOccupants.Value}/{Building.maxOccupants.Value})";
}

public sealed record class BazaarLocationEntry(
    BazaarContextMain Main,
    GameLocation Location,
    List<BazaarBuildingEntry> LivestockBuildings
)
{
    public string LocationName => Location.DisplayName;

    // public void PurchaseToLocation()
    // {
    //     Main.SelectedLivestock;
    // }
}
