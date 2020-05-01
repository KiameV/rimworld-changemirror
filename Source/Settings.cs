using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ChangeMirror
{
    public class SettingsController : Mod
    {
        public SettingsController(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "ChangeMirror".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }

    public class Settings : ModSettings
    {
        private static bool showGenderAgeChange = true;
        private static bool showBodyChange = true;
        private static bool shareHairStyles = true;

        public static bool ShowGenderAgeChange { get { return showGenderAgeChange; } }
        public static bool ShowBodyChange { get { return showBodyChange; } }
        public static bool ShareHairStyles { get; internal set; }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref showGenderAgeChange, "ChangeMirror.ShowGenderAgeChange", true);
            Scribe_Values.Look<bool>(ref showBodyChange, "ChangeMirror.ShowBodyChange", true);
            Scribe_Values.Look<bool>(ref shareHairStyles, "ChangeMirror.ShareHairStyles", false);
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard l = new Listing_Standard(GameFont.Small)
            {
                ColumnWidth = System.Math.Min(400, rect.width / 2)
            };

            l.Begin(rect);
            l.CheckboxLabeled("ChangeMirror.ShareHairStyles".Translate(), ref shareHairStyles);
            l.Gap(48);
            l.CheckboxLabeled("ChangeMirror.ShowBodyChange".Translate(), ref showBodyChange);
            if (showBodyChange)
            {
                l.Gap(4);
                l.CheckboxLabeled("ChangeMirror.ShowGenderAgeChange".Translate(), ref showGenderAgeChange);
                l.Gap(20);
            }
            else
            {
                l.Gap(48);
            }
            l.End();
        }
    }
}
