using Verse;

namespace Seg;

public class SegCoreModSettings : ModSettings
{
    public bool useImperialFormat;
    public ImperialEraPreset selectedEra = ImperialEraPreset.EraIndomitus;
    public int yearOffset;
    public MillenniumPreset millenniumPreset = MillenniumPreset.M42;
    public string customMillennium = "M42";

    public override void ExposeData()
    {
        Scribe_Values.Look(ref useImperialFormat, "useImperialFormat");
        Scribe_Values.Look(ref selectedEra, "selectedEra", ImperialEraPreset.EraIndomitus);
        Scribe_Values.Look(ref yearOffset, "yearOffset");
        Scribe_Values.Look(ref millenniumPreset, "millenniumPreset", MillenniumPreset.M42);
        Scribe_Values.Look(ref customMillennium, "customMillennium", "M42");
    }

    public void ApplyPreset(ImperialEraPreset preset)
    {
        selectedEra = preset;
        switch (preset)
        {
            case ImperialEraPreset.GreatCrusade:
                yearOffset = 712;
                millenniumPreset = MillenniumPreset.M30;
                break;
            case ImperialEraPreset.HorusHeresy:
                yearOffset = 5;
                millenniumPreset = MillenniumPreset.M31;
                break;
            case ImperialEraPreset.TimeOfEnding:
                yearOffset = 744;
                millenniumPreset = MillenniumPreset.M41;
                break;
            case ImperialEraPreset.EraIndomitus:
                yearOffset = 0;
                millenniumPreset = MillenniumPreset.M42;
                break;
        }
    }

    public string EffectiveMillenniumLabel => millenniumPreset.ToString();
}