using ChangeMirror.UI.Enums;
using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace ChangeMirror
{
    [DefOf]
    public static class JobDefOfCM
    {
        public static JobDef ChangeApparelColor;
        public static JobDef ChangeHairColor;
        public static JobDef ChangeHairStyle;
        public static JobDef ChangeBody;
        public static JobDef ChangeFavoriteColor;
    }

    class Building_ChangeMirror : Building
    {
        public readonly JobDef changeBodyAlienColor = DefDatabase<JobDef>.GetNamed("ChangeBodyAlienColor", true);

        public static IEnumerable<CurrentEditorEnum> GetSupportedEditors(bool isAlien)
        {
            yield return CurrentEditorEnum.ChangeMirrorApparelColor;
            yield return CurrentEditorEnum.ChangeMirrorHair;
            yield return CurrentEditorEnum.ChangeMirrorBody;

            if (Settings.ShowBodyChange && isAlien)
            {
                yield return CurrentEditorEnum.ChangeMirrorAlienSkinColor;
            }

            if (ModsConfig.IdeologyActive)
                yield return CurrentEditorEnum.ChangeMirrorFavoriteColor;
        }

        [DebuggerHidden]
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            bool isAlien = AlienRaceUtil.IsAlien(pawn);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (pawn.apparel.WornApparel.Count > 0)
            {
                list.Add(new FloatMenuOption(
                    "ChangeMirror.ChangeApparelColors".Translate(),
                    delegate
                    {
                        Job job = new Job(JobDefOfCM.ChangeApparelColor, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            if (!isAlien || AlienRaceUtil.HasHair(pawn))
            {
                list.Add(new FloatMenuOption(
                    "ChangeMirror.ChangeHair".Translate(),
                    delegate
                    {
                        Job job = new Job(JobDefOfCM.ChangeHairStyle, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            if (Settings.ShowBodyChange)
            {
                list.Add(new FloatMenuOption(
                    "ChangeMirror.ChangeBody".Translate(),
                    delegate
                    {
                        Job job = new Job(JobDefOfCM.ChangeBody, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));

                if (isAlien)
                {
                    list.Add(new FloatMenuOption(
                        "ChangeMirror.ChangeAlienBodyColor".Translate(),
                        delegate
                        {
                            Job job = new Job(this.changeBodyAlienColor, this);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }));
                }
            }

            if (ModsConfig.IdeologyActive)
            {
                list.Add(new FloatMenuOption(
                    "ChangeMirror.ChangeFavoriteColor".Translate(),
                    delegate
                    {
                        Job job = new Job(JobDefOfCM.ChangeFavoriteColor, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
                
            }
            return list;
        }
    }
}