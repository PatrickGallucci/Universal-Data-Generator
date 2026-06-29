using UniDataGen.Abstractions;

namespace UniDataGen.Core;

/// <summary>Combines day-part weighting and boost factor into a single multiplier on the base rate.</summary>
public static class EffectiveMultiplier
{
    /// <summary>Returns dayPartWeight(localTime) * boostFactor(date(localTime)) for an entity profile.</summary>
    public static double At(EntityProfile profile, DateTimeOffset localTime)
    {
        double weight = ScheduleEvaluator.Weight(profile.Schedule, localTime);
        double factor = BoostCalculator.FactorOn(profile.Boosts, DateOnly.FromDateTime(localTime.DateTime));
        return weight * factor;
    }
}
