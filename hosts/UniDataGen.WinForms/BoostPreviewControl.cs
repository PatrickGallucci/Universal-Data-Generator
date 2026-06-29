using UniDataGen.Abstractions;
using UniDataGen.Core;

namespace UniDataGen.WinForms;

/// <summary>
/// Draws the boost ramp curve for a single boost date so a mis-entered window is caught before a run.
/// The curve is the engine's own <see cref="BoostCalculator.PercentOn"/>, so the preview matches generation exactly.
/// </summary>
public sealed class BoostPreviewControl : Control
{
    private BoostDate? _boost;

    public BoostPreviewControl()
    {
        DoubleBuffered = true;
        MinimumSize = new Size(200, 120);
        BackColor = Color.White;
    }

    /// <summary>Sets the boost to preview and repaints.</summary>
    public void SetBoost(BoostDate? boost)
    {
        _boost = boost;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.Clear(BackColor);

        if (_boost is not { } boost)
        {
            TextRenderer.DrawText(g, "No boost selected", Font, ClientRectangle, Color.Gray,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        int pad = 24;
        var plot = new Rectangle(pad, pad / 2, Width - pad * 2, Height - pad);

        // Sample the window with two days of margin on each side.
        DateOnly start = boost.Date.AddDays(-boost.DaysBefore - 2);
        DateOnly end = boost.Date.AddDays(boost.DaysAfter + 2);
        int days = end.DayNumber - start.DayNumber + 1;

        var points = new List<(DateOnly Day, double Pct)>(days);
        double min = 0, max = 0;
        for (int i = 0; i < days; i++)
        {
            DateOnly day = start.AddDays(i);
            double pct = BoostCalculator.PercentOn(boost, day);
            points.Add((day, pct));
            min = Math.Min(min, pct);
            max = Math.Max(max, pct);
        }

        if (Math.Abs(max - min) < 1e-6)
        {
            max = min + 1;
        }

        float ScaleX(int i) => plot.Left + plot.Width * i / (float)(days - 1);
        float ScaleY(double pct) => plot.Bottom - (float)((pct - min) / (max - min)) * plot.Height;

        // Zero baseline.
        using var baseline = new Pen(Color.LightGray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
        float zeroY = ScaleY(0);
        g.DrawLine(baseline, plot.Left, zeroY, plot.Right, zeroY);

        // Curve.
        using var curve = new Pen(Color.SteelBlue, 2f);
        for (int i = 1; i < points.Count; i++)
        {
            g.DrawLine(curve, ScaleX(i - 1), ScaleY(points[i - 1].Pct), ScaleX(i), ScaleY(points[i].Pct));
        }

        // Peak marker.
        int peakIndex = boost.DaysBefore + 2;
        using var peak = new SolidBrush(Color.Firebrick);
        float px = ScaleX(peakIndex), py = ScaleY(boost.BoostPercentOnDate);
        g.FillEllipse(peak, px - 3, py - 3, 6, 6);
        TextRenderer.DrawText(g, $"{boost.Date:MM-dd}  peak {boost.BoostPercentOnDate:0}%  shoulder {boost.BoostPercent:0}%",
            Font, new Point(plot.Left, plot.Bottom + 2), Color.DimGray);
    }
}
