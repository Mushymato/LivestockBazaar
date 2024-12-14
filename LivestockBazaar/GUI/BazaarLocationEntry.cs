using LivestockBazaar.Integration;
using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace LivestockBazaar.GUI;

public sealed partial record BazaarBuildingEntry(
    BazaarContextMain Main,
    BazaarLocationEntry LocationEntry,
    Building Building,
    BuildingData Data
)
{
    private readonly AnimalHouse House = (AnimalHouse)Building.GetIndoors();
    public int RemainingSpace => House.animalLimit.Value - House.animalsThatLiveHere.Count;
    public string BuildingName
    {
        get
        {
            string name = Data.Name;
            if (Building.GetSkin() is BuildingSkin skin)
                name = skin.Name ?? name;
            return $"{TokenParser.ParseText(name)} ({Building.tileX},{Building.tileY})";
        }
    }
    public string BuildingOccupant => $"{House.animalsThatLiveHere.Count}/{House.animalLimit.Value}";
    public SDUISprite BuildingSprite => new(Building.texture.Value, Building.getSourceRect());
    public Color BuildingSpriteTint => Color.White * (RemainingSpace > 0 ? 1f : 0.5f);

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

    internal void AdoptAnimal(FarmAnimal animal)
    {
        House.adoptAnimal(animal);
        OnPropertyChanged(new(nameof(BuildingOccupant)));
        OnPropertyChanged(new(nameof(BuildingSpriteTint)));
    }

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

    public bool CheckHasRequiredBuilding(BazaarLivestockEntry? livestock)
    {
        if (livestock == null)
            return false;
        if (!LivestockBuildings.TryGetValue(livestock.Ls.Data.House, out List<BazaarBuildingEntry>? buildings))
            return false;
        return buildings.Any(
            (bld) =>
                livestock.Ls.Data.RequiredBuilding == null
                || bld.IsBuildingOrUpgrade(livestock.Ls.Data.RequiredBuilding)
        );
    }

    public IEnumerable<BazaarBuildingEntry> ValidLivestockBuildings
    {
        get
        {
            if (
                Main.SelectedLivestock is BazaarLivestockEntry livestock
                && LivestockBuildings.TryGetValue(livestock.Ls.Data.House, out List<BazaarBuildingEntry>? buildings)
            )
                return buildings.OrderByDescending((bld) => bld.RemainingSpace);
            return [];
        }
    }

    public int TotalRemainingSpaceCount => ValidLivestockBuildings.Sum(bld => bld.RemainingSpace);
}
