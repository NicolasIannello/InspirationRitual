using RimWorld;
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
            var invoker = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
            var target = psychicRitual.assignments.FirstAssignedPawn(targetRole);
            if (invoker == null || target == null) return;
            ApplyOutcome(psychicRitual, invoker, target);
        }

        private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, Pawn target)
        {
            if (PsychicRitualDef_InspirationRitual.inspiphagyList == null) PsychicRitualDef_InspirationRitual.PopulateInspiphagy();

            var hediff = HediffMaker.MakeHediff(HediffDefOf.DarkPsychicShock, target);
            hediff.TryGetComp<Verse.HediffComp_Disappears>()?.SetDuration(comaDurationTicks);
            target.health.AddHediff(hediff);

            InspirationDef grantedInspiration = PsychicRitualDef_InspirationRitual.GetInspiphagyInspiration(invoker, target).RandomElement();
            invoker.mindState.inspirationHandler.TryStartInspiration(grantedInspiration, psychicRitual.def.label+" Ritual");
            TaggedString text = "InspirationRitual.Complete".Translate(invoker.Named("INVOKER"), psychicRitual.def.Named("RITUAL"), grantedInspiration.LabelCap);

            int deatChance = (psychicRitual.def as PsychicRitualDef_InspirationRitual)?.DeathChance(psychicRitual.power) ?? 0;
            if (Rand.Chance(deatChance / 100f))
            {
                target.Kill(null);
                text+= "\n\n" + "InspirationRitual.Death".Translate(target.Named("INVOKER"));
            }
            else
            {
                text += "\n\n" + "InspirationRitual.Coma".Translate(target.Named("INVOKER"));
            }
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