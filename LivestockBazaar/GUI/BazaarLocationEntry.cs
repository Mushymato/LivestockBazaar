using System.Reflection;
using System.Text;
using LivestockBazaar.Integration;
using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;

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

    private readonly StringBuilder buildingNameSb = new();
    public string BuildingName
    {
        get
        {
            buildingNameSb.Clear();

            string name = Data.Name;
            if (Building.GetSkin() is BuildingSkin skin)
                name = skin.Name ?? name;
            buildingNameSb.Append(Wheels.ParseTextOrDefault(name));
            buildingNameSb.Append(" (");
            buildingNameSb.Append(Building.tileX);
            buildingNameSb.Append(',');
            buildingNameSb.Append(Building.tileY);
            buildingNameSb.Append(')');
            foreach (FarmAnimal animal in House.animals.Values)
            {
                buildingNameSb.Append('\n');
                buildingNameSb.Append(animal.displayType);
                buildingNameSb.Append(": ");
                buildingNameSb.Append(animal.displayName);
            }
            return buildingNameSb.ToString();
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

    internal int CountAnimal(BazaarLivestockEntry livestock)
    {
        int count = 0;
        foreach (FarmAnimal animal in House.animals.Values)
        {
            if (livestock.HasThisType(animal.type.Value))
                count++;
        }
        return count;
    }

    // hover color
    [Notify]
    public Color backgroundTint = Color.White;

    [Notify]
    public bool isSelected = false;
    public Color SelectedFrameTint => IsSelected ? Color.White : Color.Transparent;
}

public sealed partial record class BazaarLocationEntry(
    BazaarContextMain Main,
    GameLocation Location,
    Dictionary<string, List<BazaarBuildingEntry>> LivestockBuildings
)
{
    public string LocationName => Location.DisplayName;

    private static readonly MethodInfo? hasBuildingOrUpgradeMethod = typeof(Utility).GetMethod(
        "_HasBuildingOrUpgrade",
        BindingFlags.NonPublic | BindingFlags.Static
    );

    public bool CheckHasRequiredBuilding(BazaarLivestockEntry? livestock)
    {
        if (livestock == null)
            return false;
        // use the game's check for SVE weh
        if (hasBuildingOrUpgradeMethod != null)
            return (bool)(hasBuildingOrUpgradeMethod.Invoke(null, [Location, livestock.RequiredBuilding]) ?? false);
        // fall back impl in case something weird happens
        if (!LivestockBuildings.TryGetValue(livestock.House, out List<BazaarBuildingEntry>? buildings))
            return false;
        return buildings.Any(
            (bld) => livestock.RequiredBuilding == null || bld.IsBuildingOrUpgrade(livestock.RequiredBuilding)
        );
    }

    public IEnumerable<BazaarBuildingEntry> GetValidLivestockBuildings(BazaarLivestockEntry livestock)
    {
        if (LivestockBuildings.TryGetValue(livestock.Ls.Data.House, out List<BazaarBuildingEntry>? buildings))
            return buildings.OrderByDescending((bld) => bld.RemainingSpace);
        return [];
    }

    public int GetCurrentLivestockCount(BazaarLivestockEntry livestock)
    {
        if (LivestockBuildings.TryGetValue(livestock.Ls.Data.House, out List<BazaarBuildingEntry>? buildings))
        {
            return buildings.Sum(bld => bld.CountAnimal(livestock));
        }
        return 0;
    }

    public IEnumerable<BazaarBuildingEntry> ValidLivestockBuildings
    {
        get
        {
            if (Main.SelectedLivestock is BazaarLivestockEntry livestock)
                return GetValidLivestockBuildings(livestock);
            return [];
        }
    }

    public int TotalRemainingSpaceCount => ValidLivestockBuildings.Sum(bld => bld.RemainingSpace);

    public int GetTotalRemainingSpaceCount(BazaarLivestockEntry livestock)
    {
        return GetValidLivestockBuildings(livestock).Sum(bld => bld.RemainingSpace);
    }
}
