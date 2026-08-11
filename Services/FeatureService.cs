using CoachManagerPwa.Models;

namespace CoachManagerPwa.Services;

public class FeatureService : IFeatureService
{
    private readonly Supabase.Client _supabase;

    public FeatureTier CurrentTier { get; private set; } = FeatureTier.Basic;

    public FeatureService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public bool IsAvailable(FeatureTier requiredTier) => CurrentTier >= requiredTier;

    public async Task LoadAsync(string tenantId)
    {
        try
        {
            var response = await _supabase.From<TenantSubscription>()
                .Where(s => s.TenantId == tenantId)
                .Where(s => s.IsActive == true)
                .Order("tier", Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            var sub = response.Models.FirstOrDefault();
            if (sub != null && (sub.ExpiresAt == null || sub.ExpiresAt > DateTime.UtcNow))
            {
                CurrentTier = (FeatureTier)sub.Tier;
            }
            else
            {
                CurrentTier = FeatureTier.Basic;
            }
        }
        catch
        {
            // If table doesn't exist yet or any error — default to Basic
            CurrentTier = FeatureTier.Basic;
        }
    }
}
