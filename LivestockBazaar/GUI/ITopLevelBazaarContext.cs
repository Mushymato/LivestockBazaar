namespace LivestockBazaar.GUI;

/// <summary>
/// This interface is used as a parent to livestock entry and location entry
/// </summary>
public interface ITopLevelBazaarContext
{
    BazaarLivestockEntry? SelectedLivestock { get; }
    bool HasSpaceForLivestock(BazaarLivestockEntry livestock);
    int GetCurrentlyOwnedCount(BazaarLivestockEntry livestock);
    bool HasRequiredBuilding(BazaarLivestockEntry livestock);
}
