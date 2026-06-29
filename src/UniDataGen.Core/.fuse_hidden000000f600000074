namespace UniDataGen.Core;

/// <summary>
/// Accrues fractional records for a real-time action and emits whole records when the carry crosses 1.0.
/// One instance per entity per action so New, Update, and Delete accrue independently.
/// </summary>
public sealed class RateAccumulator
{
    private double _carry;

    /// <summary>The fractional remainder not yet emitted.</summary>
    public double Carry => _carry;

    /// <summary>
    /// Adds owed = ratePerHour * effective * elapsedHours to the carry and returns the whole records due.
    /// </summary>
    public long Advance(double ratePerHour, double effective, TimeSpan elapsed)
    {
        if (ratePerHour < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ratePerHour), ratePerHour, "Rate cannot be negative.");
        }

        double owed = ratePerHour * effective * elapsed.TotalHours;
        if (owed <= 0)
        {
            return 0;
        }

        _carry += owed;
        long whole = (long)Math.Floor(_carry);
        _carry -= whole;
        return whole;
    }
}
