using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace InspirationRitual
{
    public class PsychicRitualDef_InspirationRitual : PsychicRitualDef_InvocationCircle
    {
        public SimpleCurve deathChanceFromQualityCurve;
        public static Dictionary<string, List<InspirationDef>> inspiphagyList;
        private static List<SkillRecord> tmpCandidateSkills = new List<SkillRecord>();

        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            var list = base.CreateToils(psychicRitual, parent);
            list.Add(new PsychicRitualToil_InspirationRitual(InvokerRole, TargetRole));
            list.Add(new PsychicRitualToil_TargetCleanup(InvokerRole, TargetRole));
            return list;
        }

        public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        {
            TaggedString result = outcomeDescription.Formatted(DeathChance(qualityRange.min));
            if (inspiphagyList == null) PopulateInspiphagy();

            Pawn pawn = assignments.FirstAssignedPawn(InvokerRole);
            Pawn pawn2 = assignments.FirstAssignedPawn(TargetRole);
            if (pawn == null || pawn2 == null) return result;

            List<InspirationDef> inspiphagyInspiration = GetInspiphagyInspiration(pawn, pawn2);
            string inspirations="";
            for (int i = 0; i < inspiphagyInspiration.Count; i++)
            {
                inspirations += inspiphagyInspiration[i].LabelCap;
                if (i != inspiphagyInspiration.Count - 1) inspirations += " / ";
            }

            return result+= "\n\nInspiration: " + inspirations;   
        }

        public static void PopulateInspiphagy()
        {
            inspiphagyList = new Dictionary<string, List<InspirationDef>>();
            foreach (var d in DefDatabase<InspirationDef>.AllDefs)
            {
                if (d.associatedSkills != null)
                {
                    foreach (var item in d.associatedSkills)
                    {
                        if (!inspiphagyList.ContainsKey(item.defName))
                        {
                            inspiphagyList[item.defName] = new List<InspirationDef>();
                        }
                        inspiphagyList[item.defName].Add(d);
                    }
                }
                else
                {
                    if (!inspiphagyList.ContainsKey("Default"))
                    {
                        inspiphagyList["Default"] = new List<InspirationDef>();
                    }
                    inspiphagyList["Default"].Add(d);
                }
            }
        }

        public int DeathChance(float quality)
        {
            return Mathf.FloorToInt(deathChanceFromQualityCurve.Evaluate(quality));
        }

        public static List<InspirationDef> GetInspiphagyInspiration(Pawn invoker, Pawn target)
        {
            IOrderedEnumerable<SkillRecord> orderedEnumerable = from s in target.skills.skills
                                                                where !s.TotallyDisabled && !invoker.skills.GetSkill(s.def).TotallyDisabled && s.Level>2
                                                                orderby s.XpTotalEarned descending
                                                                select s;
            
            if (!orderedEnumerable.Any()) return inspiphagyList["Default"];
            tmpCandidateSkills.Clear();

            SkillRecord skillRecord = orderedEnumerable.First();
            foreach (SkillRecord item in orderedEnumerable)
            {
                if (item == skillRecord)
                {
                    tmpCandidateSkills.Add(item);
                }
                else if (Mathf.Abs(item.XpTotalEarned - skillRecord.XpTotalEarned) < 0.01f)
                {
                    tmpCandidateSkills.Add(item);
                }
            }

            SkillDef def = null;
            foreach (SkillRecord tmpCandidateSkill in tmpCandidateSkills)
            {
                if (def == null && invoker.skills.GetSkill(tmpCandidateSkill.def).Level > 2) def = tmpCandidateSkill.def;
            }

            return def!=null ? inspiphagyList.ContainsKey(def.defName) ? inspiphagyList[def.defName] : inspiphagyList["Default"] : inspiphagyList["Default"];
        }
    }
}