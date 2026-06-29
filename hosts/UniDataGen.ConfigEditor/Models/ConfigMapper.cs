using System.ComponentModel;
using UniDataGen.Abstractions;

namespace UniDataGen.ConfigEditor.Models;

/// <summary>Maps between the editable view models and the core <see cref="RunConfiguration"/>.</summary>
public static class ConfigMapper
{
    public static ConfigEditModel ToEditModel(RunConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        var model = new ConfigEditModel
        {
            Run = new RunHeaderEditModel
            {
                Industry = config.Run.Industry,
                SourceType = config.Run.SourceType,
                Locale = config.Run.Locale,
                DefaultTimeZoneId = config.Run.DefaultTimeZoneId,
                Foundry = new FoundryEditModel
                {
                    Endpoint = config.Run.Foundry.Endpoint,
                    Deployment = config.Run.Foundry.Deployment,
                    AuthMethod = config.Run.Foundry.AuthMethod,
                    ApiKeySecretRef = config.Run.Foundry.ApiKeySecretRef
                }
            }
        };

        foreach (EntityProfile entity in config.Entities)
        {
            model.Entities.Add(ToEntityModel(entity));
        }

        foreach (TargetConfig target in config.Targets)
        {
            model.Targets.Add(ToTargetModel(target));
        }

        return model;
    }

    public static RunConfiguration ToRunConfiguration(ConfigEditModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new RunConfiguration
        {
            Run = new RunHeader(
                model.Run.Industry,
                model.Run.SourceType,
                new FoundrySettings(model.Run.Foundry.Endpoint, model.Run.Foundry.Deployment, model.Run.Foundry.AuthMethod, model.Run.Foundry.ApiKeySecretRef),
                string.IsNullOrWhiteSpace(model.Run.DefaultTimeZoneId) ? null : model.Run.DefaultTimeZoneId,
                string.IsNullOrWhiteSpace(model.Run.Locale) ? "en-US" : model.Run.Locale),
            Entities = model.Entities.Select(ToEntityProfile).ToList(),
            Targets = model.Targets.Select(ToTargetConfig).ToList()
        };
    }

    private static EntityEditModel ToEntityModel(EntityProfile entity)
    {
        var model = new EntityEditModel
        {
            SchemaArea = entity.SchemaArea,
            EntityName = entity.EntityName,
            Mode = entity.Mode,
            Locale = entity.Locale,
            TimeZoneId = entity.TimeZoneId,
            Schedule = new ScheduleEditModel
            {
                Split = entity.Schedule.Split,
                Week = ToWeights(entity.Schedule.Week),
                Weekday = ToWeights(entity.Schedule.Weekday),
                Weekend = ToWeights(entity.Schedule.Weekend)
            },
            RealtimePerHour = ToRates(entity.Realtime?.PerHour),
            BatchFrequency = new BatchFrequencyEditModel
            {
                Count = entity.Batch?.Frequency.Count ?? 1,
                Per = entity.Batch?.Frequency.Per ?? BatchUnit.Day
            },
            BatchPerBatch = ToRates(entity.Batch?.PerBatch)
        };

        foreach (BoostDate boost in entity.Boosts)
        {
            model.Boosts.Add(new BoostEditModel
            {
                Date = boost.Date.ToString("yyyy-MM-dd"),
                BoostPercent = boost.BoostPercent,
                DaysBefore = boost.DaysBefore,
                DaysAfter = boost.DaysAfter,
                BoostPercentOnDate = boost.BoostPercentOnDate
            });
        }

        return model;
    }

    private static EntityProfile ToEntityProfile(EntityEditModel model)
    {
        EntitySchedule schedule = model.Schedule.Split == ScheduleSplit.WholeWeek
            ? new EntitySchedule(ScheduleSplit.WholeWeek, Week: ToWeights(model.Schedule.Week))
            : new EntitySchedule(ScheduleSplit.WeekdayWeekend, Weekday: ToWeights(model.Schedule.Weekday), Weekend: ToWeights(model.Schedule.Weekend));

        return new EntityProfile
        {
            SchemaArea = model.SchemaArea,
            EntityName = model.EntityName,
            Mode = model.Mode,
            Schedule = schedule,
            Realtime = model.Mode == GenerationMode.RealTime ? new RealtimeSettings(ToRates(model.RealtimePerHour)) : null,
            Batch = model.Mode == GenerationMode.Batch
                ? new BatchSettings(new BatchFrequency(model.BatchFrequency.Count, model.BatchFrequency.Per), ToRates(model.BatchPerBatch))
                : null,
            Boosts = model.Boosts.Select(ToBoost).ToList(),
            TimeZoneId = string.IsNullOrWhiteSpace(model.TimeZoneId) ? null : model.TimeZoneId,
            Locale = string.IsNullOrWhiteSpace(model.Locale) ? null : model.Locale
        };
    }

    private static BoostDate ToBoost(BoostEditModel model)
    {
        DateOnly date = DateOnly.TryParse(model.Date, out DateOnly parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow);
        return new BoostDate(date, model.BoostPercent, model.DaysBefore, model.DaysAfter, model.BoostPercentOnDate);
    }

    private static TargetEditModel ToTargetModel(TargetConfig target)
    {
        var model = new TargetEditModel
        {
            TargetType = target.TargetType,
            Name = target.Name,
            Enabled = target.Enabled,
            AuthMethod = target.Authentication.Method,
            KeyVaultSecretRef = target.Authentication.KeyVaultSecretRef
        };

        foreach (KeyValuePair<string, object?> property in target.Properties)
        {
            model.Properties.Add(new TargetPropertyEditModel { Name = property.Key, Value = property.Value?.ToString() });
        }

        return model;
    }

    private static TargetConfig ToTargetConfig(TargetEditModel model)
    {
        var properties = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (TargetPropertyEditModel property in model.Properties)
        {
            if (!string.IsNullOrWhiteSpace(property.Name))
            {
                properties[property.Name] = property.Value;
            }
        }

        return new TargetConfig
        {
            TargetType = model.TargetType,
            Name = model.Name,
            Enabled = model.Enabled,
            Authentication = new TargetAuth(model.AuthMethod, string.IsNullOrWhiteSpace(model.KeyVaultSecretRef) ? null : model.KeyVaultSecretRef),
            Properties = properties
        };
    }

    private static DayPartWeightsEditModel ToWeights(DayPartWeights? weights)
        => weights is null
            ? new DayPartWeightsEditModel()
            : new DayPartWeightsEditModel { Morning = weights.Morning, Afternoon = weights.Afternoon, Evening = weights.Evening, Night = weights.Night };

    private static DayPartWeights ToWeights(DayPartWeightsEditModel model)
        => new(model.Morning, model.Afternoon, model.Evening, model.Night);

    private static ActionRatesEditModel ToRates(ActionRates? rates)
        => rates is null ? new ActionRatesEditModel() : new ActionRatesEditModel { New = rates.New, Update = rates.Update, Delete = rates.Delete };

    private static ActionRates ToRates(ActionRatesEditModel model) => new(model.New, model.Update, model.Delete);
}
