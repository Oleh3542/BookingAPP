using BookingAPP_Backend.Common;

namespace BookingAPP_Backend.Services;

// один рядок пояснення розрахунку скільки годин у якому тарифному поясі й за якою ставкою
public record PriceSegment(int Hour, decimal Multiplier, double DurationHours, decimal SegmentCost, string BandName);

public record PricingResult(decimal TotalCost, IReadOnlyList<PriceSegment> Segments);


public static class PricingCalculator
{
    private const int EarliestHour = 6;
    private const int LatestHour = 23;

    public static PricingResult CalculateRoomCost(decimal baseHourlyRate, DateTime start, DateTime end)
    {
        if (end <= start)
        {
            throw new ValidationFailedException("Час завершення має бути пізніше за час початку.");
        }

        if (start.TimeOfDay < TimeSpan.FromHours(EarliestHour) || end.TimeOfDay > TimeSpan.FromHours(LatestHour))
        {
            throw new ValidationFailedException(
                $"Бронювання доступне лише в межах {EarliestHour:00}:00–{LatestHour:00}:00.");
        }

        if (start.Date != end.Date)
        {
            throw new ValidationFailedException("Бронювання не може тривати через опівніч (поза межами 06:00–23:00).");
        }

        var segments = new List<PriceSegment>();
        decimal total = 0m;

        var cursor = start;
        while (cursor < end)
        {
            var hourBoundary = cursor.Date.AddHours(cursor.Hour + 1);
            var segmentEnd = hourBoundary < end ? hourBoundary : end;
            var durationHours = (segmentEnd - cursor).TotalHours;

            var (multiplier, bandName) = GetBand(cursor.Hour);
            var segmentCost = baseHourlyRate * multiplier * (decimal)durationHours;

            segments.Add(new PriceSegment(cursor.Hour, multiplier, durationHours, Math.Round(segmentCost, 2), bandName));
            total += segmentCost;

            cursor = segmentEnd;
        }

        return new PricingResult(Math.Round(total, 2), segments);
    }

    private static (decimal Multiplier, string BandName) GetBand(int hour)
    {
        return hour switch
        {
            >= 6 and < 9 => (0.90m, "Ранкові години (знижка 10%)"),
            >= 9 and < 12 => (1.00m, "Стандартні години"),
            >= 12 and < 14 => (1.15m, "Пікові години (націнка 15%)"),
            >= 14 and < 18 => (1.00m, "Стандартні години"),
            >= 18 and < 23 => (0.80m, "Вечірні години (знижка 20%)"),
            _ => throw new ValidationFailedException("Бронювання доступне лише в межах 06:00–23:00.")
        };
    }
}
