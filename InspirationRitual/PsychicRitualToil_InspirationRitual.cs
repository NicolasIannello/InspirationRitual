using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace InspirationRitual
{
    public class PsychicRitualToil_InspirationRitual : PsychicRitualToil
    {
        public PsychicRitualRoleDef invokerRole;

        public PsychicRitualRoleDef targetRole;

        public int comaDurationTicks = 60000 * 7;

        protected PsychicRitualToil_InspirationRitual() { }

        public PsychicRitualToil_InspirationRitual(PsychicRitualRoleDef invokerRole, PsychicRitualRoleDef targetRole)
        {
            this.invokerRole = invokerRole;
            this.targetRole = targetRole;
        }

        public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            base.Start(psychicRitual, parent);
            var def = (PsychicRitualDef_InspirationRitual)psychicRitual.def;
            var invoker = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
            var target = psychicRitual.assignments.FirstAssignedPawn(targetRole);
            if (invoker == null || target == null)
            {
                return;
            }
            ApplyOutcome(psychicRitual, invoker, target);
        }

        private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, Pawn target)
        {
            var hediff = HediffMaker.MakeHediff(HediffDefOf.RegenerationComa, target);
            hediff.TryGetComp<Verse.HediffComp_Disappears>()?.SetDuration(comaDurationTicks);
            target.health.AddHediff(hediff);

            TaggedString text = "InspirationRitual.Complete".Translate(invoker.Named("INVOKER"), psychicRitual.def.Named("RITUAL"), target.Named("TARGET"));
                //text += "\n\n" + "InhumanizeMentalBreak".Translate(target.Named("TARGET"));
            
            Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, LetterDefOf.NeutralEvent, new LookTargets(invoker, target));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref invokerRole, "invokerRole");
            Scribe_Defs.Look(ref targetRole, "targetRole");
            Scribe_Values.Look(ref comaDurationTicks, "comaDurationTicks");
        }
    }
}