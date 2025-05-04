using StardewValley.GameData;

namespace LivestockBazaar.Integration;

public interface IExtraAnimalConfigApi
{
    // Get a list of item queries, in order, that can potentially replace the specified output of this animal. Returns an empty list if there are no overrides.
    // animalType: the animal type (ie. the key in Data/FarmAnimals)
    // produceId: the qualified or unqualified ID of the base produce (ie. the value in (Deluxe)ProduceItemIds)
    public List<GenericSpawnItemDataWithCondition> GetItemQueryOverrides(string animalType, string produceId);
}
