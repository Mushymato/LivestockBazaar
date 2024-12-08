using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using xTile;

namespace LivestockBazaar.GUI;

public sealed partial record BazaarBuildingEntry(
    BazaarContextMain Main,
    BazaarLocationEntry LocationEntry,
    Building Building,
    BuildingData Data
)
{
    private readonly AnimalHouse House = (AnimalHouse)Building.GetIndoors();
    public string BuildingName =>
        $"{Building.buildingType.Value} ({House.animalsThatLiveHere.Count}/{House.animalLimit.Value})";

    public bool IsBuildingOrUpgrade(string buildingId) =>
        Building.buildingType.Value == buildingId || IsBuildingOrUpgrade(buildingId, Data);

    private static bool IsBuildingOrUpgrade(string buildingId, BuildingData? bldData)
    {
        if (bldData == null)
            return false;
        if (bldData.BuildingToUpgrade == buildingId)
            return true;
        if (
            bldData.BuildingToUpgrade != null
            && Game1.buildingData.TryGetValue(bldData.BuildingToUpgrade, out BuildingData? prevLvl)
        )
            return IsBuildingOrUpgrade(buildingId, prevLvl);
        return false;
    }

    public bool CanAcceptLivestock(BazaarLivestockEntry livestock)
    {
        if (Building.isUnderConstruction())
            return false;
        return livestock.Ls.Data.RequiredBuilding == null || IsBuildingOrUpgrade(livestock.Ls.Data.RequiredBuilding);
    }

    internal void AdoptAnimal(FarmAnimal animal)
    {
        House.adoptAnimal(animal);
        OnPropertyChanged(new(nameof(BuildingName)));
    }

    public bool IsFull => House.isFull();

    // hover color
    [Notify]
    public Color backgroundTint = Color.White;
}

public sealed partial record class BazaarLocationEntry(
    BazaarContextMain Main,
    GameLocation Location,
    Dictionary<string, List<BazaarBuildingEntry>> LivestockBuildings
)
{
    public string LocationName => Location.DisplayName;

    public bool CheckCanAcceptLivestock(BazaarLivestockEntry? livestock)
    {
        if (livestock == null)
            return false;
        if (!LivestockBuildings.TryGetValue(livestock.Ls.Data.House, out List<BazaarBuildingEntry>? buildings))
            return false;
        return buildings.Any((bld) => bld.CanAcceptLivestock(livestock));
    }

    public bool CanAcceptLivestock => CheckCanAcceptLivestock(Main.SelectedLivestock);
    public IReadOnlyList<BazaarBuildingEntry> ValidLivestockBuildings
    {
        get
        {
            var livestock = Main.SelectedLivestock;
            if (
                livestock != null
                && LivestockBuildings.TryGetValue(livestock.Ls.Data.House, out List<BazaarBuildingEntry>? buildings)
            )
                return buildings.Where((bld) => bld.CanAcceptLivestock(livestock)).ToList();
            return [];
        }
    }
}
