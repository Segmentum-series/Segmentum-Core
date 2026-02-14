using Verse;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using RimWorld;








namespace ImperialDate
{
public class ImperialDateSettings : ModSettings
{
  public bool useImperialFormat = false;
  public ImperialEraPreset selectedEra = ImperialEraPreset.EraIndomitus;
  public int yearOffset = 0;
  public MillenniumPreset millenniumPreset = MillenniumPreset.M42;
  public string customMillennium = "M42";

  public override void ExposeData()
  {
    Scribe_Values.Look<bool>(ref this.useImperialFormat, "useImperialFormat");
    Scribe_Values.Look<ImperialEraPreset>(ref this.selectedEra, "selectedEra", ImperialEraPreset.EraIndomitus);
    Scribe_Values.Look<int>(ref this.yearOffset, "yearOffset");
    Scribe_Values.Look<MillenniumPreset>(ref this.millenniumPreset, "millenniumPreset", MillenniumPreset.M42);
    Scribe_Values.Look<string>(ref this.customMillennium, "customMillennium", "M42");
  }

  public void ApplyPreset(ImperialEraPreset preset)
  {
    this.selectedEra = preset;
    switch (preset)
    {
      case ImperialEraPreset.GreatCrusade:
        this.yearOffset = 712;
        this.millenniumPreset = MillenniumPreset.M30;
        break;
      case ImperialEraPreset.HorusHeresy:
        this.yearOffset = 5;
        this.millenniumPreset = MillenniumPreset.M31;
        break;
      case ImperialEraPreset.TimeOfEnding:
        this.yearOffset = 744;
        this.millenniumPreset = MillenniumPreset.M41;
        break;
      case ImperialEraPreset.EraIndomitus:
        this.yearOffset = 0;
        this.millenniumPreset = MillenniumPreset.M42;
        break;
    }
  }


  public string EffectiveMillenniumLabel
  {
    get
    {
      switch (this.millenniumPreset)
      {
        case MillenniumPreset.M29:
          return "M29";
        case MillenniumPreset.M30:
          return "M30";
        case MillenniumPreset.M31:
          return "M31";
        case MillenniumPreset.M41:
          return "M41";
        case MillenniumPreset.M42:
          return "M42";
        default:
          return string.IsNullOrWhiteSpace(this.customMillennium) ? "M31" : this.customMillennium;
      }
    }
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
public class ImperialDateMod : Mod
{
  public static ImperialDateSettings Settings;
  private string yearOffsetBuffer = "949";
  private string customMillenniumBuffer = "M31";

  public ImperialDateMod(ModContentPack content)
    : base(content)
  {
    ImperialDateMod.Settings = this.GetSettings<ImperialDateSettings>();
    this.yearOffsetBuffer = ImperialDateMod.Settings.yearOffset.ToString();
    this.customMillenniumBuffer = ImperialDateMod.Settings.customMillennium;
  }

  public override string SettingsCategory() => "Imperial Date System";

  public override void DoSettingsWindowContents(Rect inRect)
  {
    Listing_Standard list = new Listing_Standard();
    list.Begin(inRect);
    list.CheckboxLabeled("Use Imperial Date Format (YYY.Mxx)", ref ImperialDateMod.Settings.useImperialFormat);
    list.GapLine();
    list.Label("Era preset:");
    this.DrawEraRadio(list, ImperialEraPreset.GreatCrusade, "Great Crusade (712.M30)");
    this.DrawEraRadio(list, ImperialEraPreset.HorusHeresy, "Horus Heresy (005.M31)");
    this.DrawEraRadio(list, ImperialEraPreset.TimeOfEnding, "Time of Ending (744.M41)");
    this.DrawEraRadio(list, ImperialEraPreset.EraIndomitus, "Era Indomitus (000.M42)");
    this.DrawEraRadio(list, ImperialEraPreset.Custom, "Custom date");
    if (ImperialDateMod.Settings.selectedEra == ImperialEraPreset.Custom)
    {
      list.GapLine();
      list.Label("Custom year offset (YYY):");
      int yearOffset = ImperialDateMod.Settings.yearOffset;
      list.IntEntry(ref yearOffset, ref this.yearOffsetBuffer);
      ImperialDateMod.Settings.yearOffset = yearOffset;
      list.Gap();
      list.Label("Millennium:");
      if (list.ButtonTextLabeled("Millennium", this.GetMillenniumDisplay(ImperialDateMod.Settings.millenniumPreset)))
        this.OpenMillenniumMenu();
      if (ImperialDateMod.Settings.millenniumPreset == MillenniumPreset.Custom)
      {
        list.Label("Custom millennium label (e.g., M31):");
        this.customMillenniumBuffer = list.TextEntry(this.customMillenniumBuffer);
        ImperialDateMod.Settings.customMillennium = this.customMillenniumBuffer;
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
    bool active = ImperialDateMod.Settings.selectedEra == preset;
    if (!list.RadioButton(label, active))
      return;
    ImperialDateMod.Settings.ApplyPreset(preset);
    this.yearOffsetBuffer = ImperialDateMod.Settings.yearOffset.ToString();
    this.customMillenniumBuffer = ImperialDateMod.Settings.customMillennium;
  }

  private string GetMillenniumDisplay(MillenniumPreset preset)
  {
    switch (preset)
    {
      case MillenniumPreset.M29:
        return "M29";
      case MillenniumPreset.M30:
        return "M30";
      case MillenniumPreset.M31:
        return "M31";
      case MillenniumPreset.M41:
        return "M41";
      case MillenniumPreset.M42:
        return "M42";
      case MillenniumPreset.Custom:
        return "Custom…";
      default:
        return "M31";
    }
  }

  private void OpenMillenniumMenu()
  {
    Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
    {
      new FloatMenuOption("M29", (Action) (() => ImperialDateMod.Settings.millenniumPreset = MillenniumPreset.M29)),
      new FloatMenuOption("M30", (Action) (() => ImperialDateMod.Settings.millenniumPreset = MillenniumPreset.M30)),
      new FloatMenuOption("M31", (Action) (() => ImperialDateMod.Settings.millenniumPreset = MillenniumPreset.M31)),
      new FloatMenuOption("M41", (Action) (() => ImperialDateMod.Settings.millenniumPreset = MillenniumPreset.M41)),
      new FloatMenuOption("M42", (Action) (() => ImperialDateMod.Settings.millenniumPreset = MillenniumPreset.M42)),
      new FloatMenuOption("Custom…", (Action) (() => ImperialDateMod.Settings.millenniumPreset = MillenniumPreset.Custom))
    }));
  }
}
public static class ImperialDateUtility
{
  public static string FormatImperialYear(int vanillaYear)
  {
    int yearOffset = ImperialDateMod.Settings.yearOffset;
    int num = vanillaYear - 5500 + yearOffset;
    if (num < 0)
      num = 0;
    string effectiveMillenniumLabel = ImperialDateMod.Settings.EffectiveMillenniumLabel;
    return $"{num:000}.{effectiveMillenniumLabel}";
  }

  public static string ReplaceYearsInString(string input)
  {
    foreach (Match match in Regex.Matches(input, "\\b\\d{3,4}\\b"))
    {
      int result;
      if (int.TryParse(match.Value, out result))
      {
        string newValue = ImperialDateUtility.FormatImperialYear(result);
        input = input.Replace(match.Value, newValue);
      }
    }
    return input;
  }
}
public static class Patch_DateFullStringAt
{
  private static void Postfix(ref string __result, long absTicks, Vector2 location)
  {
    if (!ImperialDateMod.Settings.useImperialFormat)
      return;
    string str = ImperialDateUtility.FormatImperialYear(GenDate.Year(absTicks, location.x));
    int num = __result.LastIndexOf(',');
    if (num < 0)
      return;
    __result = $"{__result.Substring(0, num + 1)} {str}";
  }
}
[HarmonyPatch(typeof (GenDate), "DateReadoutStringAt")]
public static class Patch_DateReadoutStringAt
{
  private static void Postfix(ref string __result, long absTicks, Vector2 location)
  {
    if (!ImperialDateMod.Settings.useImperialFormat)
      return;
    int vanillaYear = GenDate.Year(absTicks, location.x);
    string newValue = ImperialDateUtility.FormatImperialYear(vanillaYear);
    __result = __result.Replace(vanillaYear.ToString(), newValue);
  }
}

}


