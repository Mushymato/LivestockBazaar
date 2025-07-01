using System.Diagnostics.CodeAnalysis;
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
            foreach (FarmAnimal animal in GetFarmAnimalsThatLiveHere().OrderBy(animal => animal.displayType))
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

    public enum SelectionState
    {
        None,
        All,
        Left,
        Right,
    }

    [Notify]
    private SelectionState select = SelectionState.None;

    // public Color SelectedFrameTint => Select != SelectionState.None ? Color.White : Color.Transparent;
    public Color SelectedFrameTint
    {
        get
        {
            return Select switch
            {
                SelectionState.None => Color.Transparent,
                SelectionState.All => Color.White,
                SelectionState.Left => Color.Blue,
                SelectionState.Right => Color.Green,
                _ => throw new NotImplementedException(),
            };
        }
    }

    private IList<AnimalManageFarmAnimalEntry>? AMFAEListImpl;

    public IList<AnimalManageFarmAnimalEntry> AMFAEList =>
        AMFAEListImpl ??= (
            GetFarmAnimalsThatLiveHere().Select(farmAnimal => new AnimalManageFarmAnimalEntry(this, farmAnimal)) ?? []
        )
            .OrderBy(amfae => amfae.DisplayType)
            .ToList();

    [Notify]
    private bool heldAnimalCanLiveHere = true;
    public float CanLiveOpacity => HeldAnimalCanLiveHere ? 1f : 0.5f;

    public void RefreshAMFAE()
    {
        AMFAEListImpl = null;
        OnPropertyChanged(new(nameof(AMFAEList)));
        OnPropertyChanged(new(nameof(AMFAEPlaceholds)));
    }

    internal static bool TryGetLocAndHouse(
        AnimalManageFarmAnimalEntry entry,
        [NotNullWhen(true)] out (GameLocation, AnimalHouse)? locAndHouse
    )
    {
        locAndHouse = null;
        GameLocation loc = entry.Bld.Building.GetParentLocation();
        AnimalHouse house = entry.Bld.House;
        if (!loc.animals.ContainsKey(entry.Animal.myID.Value) && !house.animals.ContainsKey(entry.Animal.myID.Value))
            return false;
        locAndHouse = (loc, house);
        return true;
    }

    internal static void RemoveFromLocAndHouse(
        AnimalManageFarmAnimalEntry entry,
        (GameLocation, AnimalHouse) locAndHouse
    )
    {
        if (!locAndHouse.Item1.animals.Remove(entry.Animal.myID.Value))
        {
            locAndHouse.Item2.animals.Remove(entry.Animal.myID.Value);
        }
        locAndHouse.Item2.animalsThatLiveHere.Remove(entry.Animal.myID.Value);
        if (entry.Animal.foundGrass != null && FarmAnimal.reservedGrass.Contains(entry.Animal.foundGrass))
        {
            FarmAnimal.reservedGrass.Remove(entry.Animal.foundGrass);
        }
    }

    internal static bool AMFAEListSwap(AnimalManageFarmAnimalEntry oldEntry, AnimalManageFarmAnimalEntry newEntry)
    {
        ModEntry.Log(
            $"AMFAEListSwap: {oldEntry.DisplayName}({oldEntry.Bld.BuildingName}) <=> {newEntry.DisplayName}({newEntry.Bld.BuildingName})"
        );

        if (!TryGetLocAndHouse(oldEntry, out (GameLocation, AnimalHouse)? oldLocAndHouse))
            return false;

        if (!TryGetLocAndHouse(newEntry, out (GameLocation, AnimalHouse)? newLocAndHouse))
            return false;

        ModEntry.Log(
            $"AMFAEListSwap for reals: {oldEntry.DisplayName}({oldEntry.Bld.BuildingName}) <=> {newEntry.DisplayName}({newEntry.Bld.BuildingName})"
        );

        RemoveFromLocAndHouse(oldEntry, oldLocAndHouse.Value);
        RemoveFromLocAndHouse(newEntry, newLocAndHouse.Value);

        oldEntry.Bld.AdoptAnimal(newEntry.Animal);
        newEntry.Bld.AdoptAnimal(oldEntry.Animal);
        newEntry.Animal.makeSound();
        oldEntry.Animal.makeSound();

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

        if (!TryGetLocAndHouse(oldEntry, out (GameLocation, AnimalHouse)? oldLocAndHouse))
            return false;

        RemoveFromLocAndHouse(oldEntry, oldLocAndHouse.Value);

        ModEntry.Log(
            $"AMFAEListMove for reals: {oldEntry.DisplayName}({oldEntry.Bld.BuildingName}) <=> {newEntry.Bld.BuildingName}"
        );

        newEntry.Bld.AdoptAnimal(oldEntry.Animal);
        oldEntry.Animal.makeSound();

        oldEntry.Bld.OnPropertyChanged(new(nameof(oldEntry.Bld.BuildingOccupant)));
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
