namespace CoachManagerPwa.Services;

public enum FeatureTier
{
    Basic = 0,
    Pro = 1,
    Enterprise = 2
}

public interface IFeatureService
{
    FeatureTier CurrentTier { get; }
    bool IsAvailable(FeatureTier requiredTier);
    Task LoadAsync(string tenantId);
}
