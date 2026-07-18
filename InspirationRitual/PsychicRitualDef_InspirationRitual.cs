using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace InspirationRitual
{
    public class PsychicRitualDef_InspirationRitual : PsychicRitualDef_InvocationCircle
    {
        public SimpleCurve deathChanceFromQualityCurve;

        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            var list = base.CreateToils(psychicRitual, parent);
            list.Add(new PsychicRitualToil_InspirationRitual(InvokerRole, TargetRole));
            list.Add(new PsychicRitualToil_TargetCleanup(InvokerRole, TargetRole));
            return list;
        }

        public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        {
            return outcomeDescription.Formatted(Mathf.FloorToInt(deathChanceFromQualityCurve.Evaluate(qualityRange.min)));
        }
    }
}