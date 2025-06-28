using System.Reflection;
using System.Text;
using LivestockBazaar.Integration;
using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;

namespace LivestockBazaar.GUI;

public sealed partial record BazaarBuildingEntry(
    BazaarLocationEntry LocationEntry,
    Building Building,
    BuildingData Data
)
{
    private readonly AnimalHouse House = (AnimalHouse)Building.GetIndoors();
    public int RemainingSpace => House.animalLimit.Value - House.animalsThatLiveHere.Count;

    private readonly StringBuilder buildingTooltipSb = new();

    private void PutBuildingName()
    {
        string name = Data.Name;
        if (Building.GetSkin() is BuildingSkin skin)
            name = skin.Name ?? name;
        buildingTooltipSb.Append(Wheels.ParseTextOrDefault(name));
    }

    public string BuildingName
    {
        get
        {
            buildingTooltipSb.Clear();
            PutBuildingName();
            return buildingTooltipSb.ToString();
        }
    }

    public string BuildingLocationCoordinate
    {
        get
        {
            buildingTooltipSb.Clear();
            buildingTooltipSb.Append(LocationEntry.LocationName);
            buildingTooltipSb.Append(": ");
            buildingTooltipSb.Append(Building.tileX);
            buildingTooltipSb.Append(',');
            buildingTooltipSb.Append(Building.tileY);
            return buildingTooltipSb.ToString();
        }
    }

    public string BuildingTooltip
    {
        get
        {
            buildingTooltipSb.Clear();
            PutBuildingName();
            buildingTooltipSb.Append(" (");
            buildingTooltipSb.Append(Building.tileX);
            buildingTooltipSb.Append(',');
            buildingTooltipSb.Append(Building.tileY);
            buildingTooltipSb.Append(')');
            foreach (FarmAnimal animal in GetFarmAnimalsThatLiveHere())
            {
                buildingTooltipSb.Append('\n');
                buildingTooltipSb.Append(animal.displayType);
                buildingTooltipSb.Append(": ");
                buildingTooltipSb.Append(animal.displayName);
            }
            return buildingTooltipSb.ToString();
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
        OnPropertyChanged(new(nameof(BuildingTooltip)));
    }

    internal IEnumerable<FarmAnimal> GetFarmAnimalsThatLiveHere()
    {
        GameLocation parentLocation = Building.GetParentLocation();
        foreach (long animalId in House.animalsThatLiveHere)
        {
            if (House.animals.TryGetValue(animalId, out FarmAnimal animal))
                yield return animal;
            else if (parentLocation.animals.TryGetValue(animalId, out animal))
                yield return animal;
            else
                ModEntry.LogOnce($"Failed to find animal {animalId}", LogLevel.Warn);
        }
    }

    internal IEnumerable<FarmAnimal> FarmAnimalsThatLiveHere => GetFarmAnimalsThatLiveHere();

    internal int CountAnimal(BazaarLivestockEntry livestock)
    {
        int count = 0;
        foreach (FarmAnimal animal in GetFarmAnimalsThatLiveHere())
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

    [Notify]
    public bool isSelected2 = false;

    public Color SelectedFrameTint =>
        IsSelected2 ? Color.Blue * 0.8f
        : IsSelected ? Color.White
        : Color.Transparent;

    private IList<AnimalManageFarmAnimalEntry>? AMFAEListImpl;

    public IList<AnimalManageFarmAnimalEntry> AMFAEList =>
        AMFAEListImpl ??= (
            GetFarmAnimalsThatLiveHere().Select(farmAnimal => new AnimalManageFarmAnimalEntry(this, farmAnimal)) ?? []
        )
            .OrderBy(amfae => amfae.DisplayType)
            .ToList();

    public void RefreshAMFAE()
    {
        AMFAEListImpl = null;
        OnPropertyChanged(new(nameof(AMFAEList)));
        OnPropertyChanged(new(nameof(AMFAEPlaceholds)));
    }

    internal static bool AMFAEListSwap(AnimalManageFarmAnimalEntry oldEntry, AnimalManageFarmAnimalEntry newEntry)
    {
        ModEntry.Log(
            $"AMFAEListSwap: {oldEntry.DisplayName}({oldEntry.Bld.BuildingName}) <=> {newEntry.DisplayName}({newEntry.Bld.BuildingName})"
        );

        GameLocation oldParentLoc = oldEntry.Bld.Building.GetParentLocation();
        AnimalHouse oldHouse = oldEntry.Bld.House;
        GameLocation newParentLoc = newEntry.Bld.Building.GetParentLocation();
        AnimalHouse newHouse = newEntry.Bld.House;

        if (
            !oldHouse.animals.Remove(oldEntry.Animal.myID.Value)
            && !oldParentLoc.animals.Remove(oldEntry.Animal.myID.Value)
        )
            return false;
        if (
            !newHouse.animals.Remove(newEntry.Animal.myID.Value)
            && !newParentLoc.animals.Remove(oldEntry.Animal.myID.Value)
        )
            return false;

        ModEntry.Log(
            $"AMFAEListSwap for reals: {oldEntry.DisplayName}({oldEntry.Bld.BuildingName}) <=> {newEntry.DisplayName}({newEntry.Bld.BuildingName})"
        );

        oldHouse.animalsThatLiveHere.Remove(oldEntry.Animal.myID.Value);
        newHouse.animalsThatLiveHere.Remove(newEntry.Animal.myID.Value);

        oldEntry.Bld.AdoptAnimal(newEntry.Animal);
        newEntry.Bld.AdoptAnimal(oldEntry.Animal);

        oldEntry.Bld.RefreshAMFAE();
        newEntry.Bld.RefreshAMFAE();

        return true;
    }

    internal static bool AMFAEListMove(AnimalManageFarmAnimalEntry oldEntry, AnimalManageEntry newEntry)
    {
        ModEntry.Log(
            $"AMFAEListMove: {oldEntry.DisplayName}({oldEntry.Bld.BuildingName}) <=> {newEntry.Bld.BuildingName}"
        );
        if (newEntry.Bld.House.isFull())
            return false;

        GameLocation oldParentLoc = oldEntry.Bld.Building.GetParentLocation();
        AnimalHouse oldHouse = oldEntry.Bld.House;

        if (
            !oldHouse.animals.Remove(oldEntry.Animal.myID.Value)
            && !oldParentLoc.animals.Remove(oldEntry.Animal.myID.Value)
        )
            return false;

        ModEntry.Log(
            $"AMFAEListMove for reals: {oldEntry.DisplayName}({oldEntry.Bld.BuildingName}) <=> {newEntry.Bld.BuildingName}"
        );

        oldHouse.animalsThatLiveHere.Remove(oldEntry.Animal.myID.Value);
        oldEntry.Bld.OnPropertyChanged(new(nameof(oldEntry.Bld.BuildingOccupant)));
        newEntry.Bld.AdoptAnimal(oldEntry.Animal);

        oldEntry.Bld.RefreshAMFAE();
        newEntry.Bld.RefreshAMFAE();

        return true;
    }

    public IEnumerable<AnimalManagePlaceholder> AMFAEPlaceholds
    {
        get
        {
            for (int i = 0; i < RemainingSpace; i++)
                yield return new AnimalManagePlaceholder(this);
        }
    }
}

public sealed partial record class BazaarLocationEntry(
    ITopLevelBazaarContext Main,
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
        if (!LivestockBuildings.TryGetValue(livestock.Ls.Key, out List<BazaarBuildingEntry>? buildings))
            return false;
        return buildings.Any(
            (bld) => livestock.RequiredBuilding == null || bld.IsBuildingOrUpgrade(livestock.RequiredBuilding)
        );
    }

    public IEnumerable<BazaarBuildingEntry> GetValidLivestockBuildings(BazaarLivestockEntry livestock)
    {
        if (LivestockBuildings.TryGetValue(livestock.Ls.Key, out List<BazaarBuildingEntry>? buildings))
            return buildings.OrderByDescending((bld) => bld.RemainingSpace);
        return [];
    }

    public int GetCurrentLivestockCount(BazaarLivestockEntry livestock)
    {
        if (LivestockBuildings.TryGetValue(livestock.Ls.Key, out List<BazaarBuildingEntry>? buildings))
        {
            return buildings.Sum(bld => bld.CountAnimal(livestock));
        }
        return 0;
    }

    public HashSet<BazaarBuildingEntry> AllLivestockBuildings = [];

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
