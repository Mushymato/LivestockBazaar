namespace LivestockBazaar.GUI;

/// <summary>
/// This class can hold a SelectedLivestock state
/// </summary>
public interface IHasSelectedLivestock
{
    BazaarLivestockEntry? SelectedLivestock { get; set; }
}
