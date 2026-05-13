using UnityEngine;
using Verse;
using static Seg.SegCoreUtils;

namespace Seg;

public class SegCoreMod : Mod
{
    readonly SegCoreModSettings Settings;
    
    public static HarmonyLib.Harmony harmony;
    
    private string yearOffsetBuffer = "949";
    private string customMillenniumBuffer = "M31";
    
    public SegCoreMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<SegCoreModSettings>();
        harmony = new HarmonyLib.Harmony("SegCore.Mod");
        harmony.PatchAll();
    }
    
    public override string SettingsCategory() => "Imperial Date System";
    
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var list = new Listing_Standard();
        list.Begin(inRect);
        list.CheckboxLabeled("Use Imperial Date Format (YYY.Mxx)", ref Settings.useImperialFormat);
        list.GapLine();

        if (!Settings.useImperialFormat)
        {
            list.End();
            return;
        }
        
        list.Label("Era preset:");
        DrawEraRadio(list, ImperialEraPreset.GreatCrusade, "Great Crusade (712.M30)");
        DrawEraRadio(list, ImperialEraPreset.HorusHeresy, "Horus Heresy (005.M31)");
        DrawEraRadio(list, ImperialEraPreset.TimeOfEnding, "Time of Ending (744.M41)");
        DrawEraRadio(list, ImperialEraPreset.EraIndomitus, "Era Indomitus (000.M42)");
        DrawEraRadio(list, ImperialEraPreset.Custom, "Custom date");
        
        if (Settings.selectedEra == ImperialEraPreset.Custom)
        {
            list.GapLine();
            list.Label("Custom year offset (YYY):");
            var yearOffset = Settings.yearOffset;
            list.IntEntry(ref yearOffset, ref yearOffsetBuffer);
            Settings.yearOffset = yearOffset;
            
            list.Gap();
            if (list.ButtonTextLabeled("Millennium", GetMillenniumDisplay(Settings.millenniumPreset)))
            {
                OpenMillenniumMenu();
            }
                
            if (Settings.millenniumPreset == MillenniumPreset.Custom)
            {
                list.Label("Custom millennium label (e.g., M31):");
                customMillenniumBuffer = list.TextEntry(customMillenniumBuffer);
                Settings.customMillennium = customMillenniumBuffer;
            }
        }
        else
        {
            list.GapLine();
            list.Label("Restart Required for changes to take effect.");
        }
        list.End();
    }
    
    private void DrawEraRadio(Listing_Standard list, ImperialEraPreset preset, string label)
    {
        if (!list.RadioButton(label, Settings.selectedEra == preset))
        {
            return;
        }
            
        Settings.ApplyPreset(preset);
        yearOffsetBuffer = Settings.yearOffset.ToString();
        customMillenniumBuffer = Settings.customMillennium;
    }
}