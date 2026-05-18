using System;
using Verse;

namespace Seg;

[StaticConstructorOnStartup]
public static class SegCoreUtils
{
    private static SegCoreModSettings modSettings = null;
    public static SegCoreModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<SegCoreMod>().GetSettings<SegCoreModSettings>();
    
    public static string GetMillenniumDisplay(MillenniumPreset preset)
    {
        return preset switch
        {
            MillenniumPreset.M29 => "M29",
            MillenniumPreset.M30 => "M30",
            MillenniumPreset.M31 => "M31",
            MillenniumPreset.M41 => "M41",
            MillenniumPreset.M42 => "M42",
            MillenniumPreset.Custom => "Custom…",
            _ => "M31"
        };
    }

    public static void OpenMillenniumMenu()
    {
        Find.WindowStack.Add(new FloatMenu([
            new FloatMenuOption("M29", () => ModSettings.millenniumPreset = MillenniumPreset.M29),
            new FloatMenuOption("M30", () => ModSettings.millenniumPreset = MillenniumPreset.M30),
            new FloatMenuOption("M31", () => ModSettings.millenniumPreset = MillenniumPreset.M31),
            new FloatMenuOption("M41", () => ModSettings.millenniumPreset = MillenniumPreset.M41),
            new FloatMenuOption("M42", () => ModSettings.millenniumPreset = MillenniumPreset.M42),
            new FloatMenuOption("Custom…", () => ModSettings.millenniumPreset = MillenniumPreset.Custom)
        ]));
    }
    
    public static string FormatImperialYear(int vanillaYear)
    {
        var yearOffset = ModSettings.yearOffset;
        var num = Math.Max(0, vanillaYear - 5500 + yearOffset);
            
        var effectiveMillenniumLabel = ModSettings.EffectiveMillenniumLabel;
        return $"{num:000}.{effectiveMillenniumLabel}";
    }
    
    public static bool ShouldFlipArmGraphic(Hediff hediff)
    {
        return hediff.Part.flipGraphic;
    }
}

public enum ImperialEraPreset
{
    GreatCrusade,
    HorusHeresy,
    TimeOfEnding,
    EraIndomitus,
    Custom,
}

public enum MillenniumPreset
{
    M29,
    M30,
    M31,
    M41,
    M42,
    Custom,
}