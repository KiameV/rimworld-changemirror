using System.Collections.Generic;
using Verse;
using Verse.AI;
using ChangeMirror.UI;
using ChangeMirror.UI.Enums;
using ChangeMirror.UI.DTO;
using System;

namespace ChangeMirror.DresserJobDriver
{
    internal class JobDriver_ChangeBody : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    Find.WindowStack.Add(new DresserUI(DresserDtoFactory.Create(this.GetActor(), base.job, CurrentEditorEnum.ChangeMirrorBody)));
                }
            };
            yield break;
        }
    }
}
