using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Buildings;

namespace LivestockBazaar.GUI;

public sealed partial class BazaarBuildingEntry(BazaarContextMain Main, Building Building)
{
    public string BuildingName =>
        $"{Building.buildingType.Value} ({Building.currentOccupants.Value}/{Building.maxOccupants.Value})";

    // hover color
    [Notify]
    private Color backgroundTint = Color.White;

    // events
    public void LaneEntry_PointerEnter()
    {
        BackgroundTint = Main.Theme.ItemRowBackgroundHoverColor;
    }

    public void LaneEntry_PointerLeave()
    {
        BackgroundTint = Color.White;
    }

    public void LaneEntry_LeftClick() { }
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
