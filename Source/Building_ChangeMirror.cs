using ChangeMirror.UI.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace ChangeMirror
{
    class Building_ChangeMirror : Building
    {
        private JobDef changeApparelColorJobDef = DefDatabase<JobDef>.GetNamed("ChangeApparelColor", true);
        private JobDef changeHairStyleJobDef = DefDatabase<JobDef>.GetNamed("ChangeHairStyle", true);
        private JobDef changeBodyJobDef = DefDatabase<JobDef>.GetNamed("ChangeBody", true);
        public readonly JobDef changeBodyAlienColor = DefDatabase<JobDef>.GetNamed("ChangeBodyAlienColor", true);

        public static IEnumerable<CurrentEditorEnum> GetSupportedEditors(bool isAlien)
        {
            yield return CurrentEditorEnum.ChangeMirrorApparelColor;
            yield return CurrentEditorEnum.ChangeMirrorHair;

            if (Settings.ShowBodyChange)
            {
                if (isAlien)
                {
                    yield return CurrentEditorEnum.ChangeMirrorAlienSkinColor;
                }
                else
                {
                    yield return CurrentEditorEnum.ChangeMirrorBody;
                }
            }
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
                        Job job = new Job(this.changeApparelColorJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            if (!isAlien || AlienRaceUtil.HasHair(pawn))
            {
                list.Add(new FloatMenuOption(
                    "ChangeMirror.ChangeHair".Translate(),
                    delegate
                    {
                        Job job = new Job(this.changeHairStyleJobDef, this);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }));
            }

            if (Settings.ShowBodyChange)
            {
                list.Add(new FloatMenuOption(
                    "ChangeMirror.ChangeBody".Translate(),
                    delegate
                    {
                        Job job = new Job(this.changeBodyJobDef, this);
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
            return list;
        }
    }
}