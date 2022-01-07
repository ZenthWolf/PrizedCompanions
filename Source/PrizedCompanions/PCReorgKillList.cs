using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;
using RimWorld;

namespace Prized_Companions
{
    // Currently reproduces behavior
    class PCReorgKillList
    {
        [HarmonyPatch(typeof(AutoSlaughterManager), "get_AnimalsToSlaughter")]
        internal static class PrizedCompanionsGetterTester
        {
            private static List<Pawn> tmpAnimals = new List<Pawn>();
            private static List<Pawn> tmpAnimalsMale = new List<Pawn>();
            private static List<Pawn> tmpAnimalsMaleYoung = new List<Pawn>();
            private static List<Pawn> tmpAnimalsFemale = new List<Pawn>();
            private static List<Pawn> tmpAnimalsFemaleYoung = new List<Pawn>();
            private static List<Pawn> tmpAnimalsPregnant = new List<Pawn>();

            private static bool Prefix(AutoSlaughterManager __instance, ref List<Pawn> __result, bool ___cacheDirty, ref List<Pawn> ___animalsToSlaughterCached)
            {
                if (!___cacheDirty)
                {
                    return true;
                }

                try
                {
                    ___animalsToSlaughterCached.Clear();
                    foreach (AutoSlaughterConfig config in __instance.configs)
                    {
                        if (config.AnyLimit)
                        {
                            tmpAnimals.Clear();
                            tmpAnimalsMale.Clear();
                            tmpAnimalsMaleYoung.Clear();
                            tmpAnimalsFemale.Clear();
                            tmpAnimalsFemaleYoung.Clear();
                            tmpAnimalsPregnant.Clear();
                            foreach (Pawn spawnedColonyAnimal in __instance.map.mapPawns.SpawnedColonyAnimals)
                            {
                                if (spawnedColonyAnimal.def == config.animal && AutoSlaughterManager.CanAutoSlaughterNow(spawnedColonyAnimal) && (config.allowSlaughterBonded || spawnedColonyAnimal.relations.GetDirectRelationsCount(PawnRelationDefOf.Bond) <= 0))
                                {
                                    if (spawnedColonyAnimal.gender == Gender.Male)
                                    {
                                        if (spawnedColonyAnimal.ageTracker.CurLifeStage.reproductive)
                                            tmpAnimalsMale.Add(spawnedColonyAnimal);
                                        else
                                            tmpAnimalsMaleYoung.Add(spawnedColonyAnimal);
                                        tmpAnimals.Add(spawnedColonyAnimal);
                                    }
                                    else if (spawnedColonyAnimal.gender == Gender.Female)
                                    {
                                        if (spawnedColonyAnimal.ageTracker.CurLifeStage.reproductive)
                                        {
                                            if (spawnedColonyAnimal.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant) == null)
                                            {
                                                tmpAnimalsFemale.Add(spawnedColonyAnimal);
                                                tmpAnimals.Add(spawnedColonyAnimal);
                                            }
                                            else if (config.allowSlaughterPregnant)
                                                tmpAnimalsPregnant.Add(spawnedColonyAnimal);
                                        }
                                        else
                                        {
                                            tmpAnimalsFemaleYoung.Add(spawnedColonyAnimal);
                                            tmpAnimals.Add(spawnedColonyAnimal);
                                        }
                                    }
                                    else
                                        tmpAnimals.Add(spawnedColonyAnimal);
                                }
                            }
                            tmpAnimals.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.ageTracker.AgeBiologicalTicks));
                            tmpAnimalsMale.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.ageTracker.AgeBiologicalTicks));
                            tmpAnimalsMaleYoung.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.ageTracker.AgeBiologicalTicks));
                            tmpAnimalsFemale.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.ageTracker.AgeBiologicalTicks));
                            tmpAnimalsFemaleYoung.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.ageTracker.AgeBiologicalTicks));
                            if (config.allowSlaughterPregnant)
                            {
                                tmpAnimalsPregnant.SortBy<Pawn, float>((Func<Pawn, float>)(a => -a.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant).Severity));
                                tmpAnimalsFemale.AddRange((IEnumerable<Pawn>) tmpAnimalsPregnant);
                                tmpAnimals.AddRange((IEnumerable<Pawn>) tmpAnimalsPregnant);
                            }
                            if (config.maxFemales != -1)
                            {
                                while (tmpAnimalsFemale.Count > config.maxFemales)
                                {
                                    Pawn pawn = tmpAnimalsFemale.Pop<Pawn>();
                                    tmpAnimals.Remove(pawn);
                                    ___animalsToSlaughterCached.Add(pawn);
                                }
                            }
                            if (config.maxFemalesYoung != -1)
                            {
                                while (tmpAnimalsFemaleYoung.Count > config.maxFemalesYoung)
                                {
                                    Pawn pawn = tmpAnimalsFemaleYoung.Pop<Pawn>();
                                    tmpAnimals.Remove(pawn);
                                    ___animalsToSlaughterCached.Add(pawn);
                                }
                            }
                            if (config.maxMales != -1)
                            {
                                while (tmpAnimalsMale.Count > config.maxMales)
                                {
                                    Pawn pawn = tmpAnimalsMale.Pop<Pawn>();
                                    tmpAnimals.Remove(pawn);
                                    ___animalsToSlaughterCached.Add(pawn);
                                }
                            }
                            if (config.maxMalesYoung != -1)
                            {
                                while (tmpAnimalsMaleYoung.Count > config.maxMalesYoung)
                                {
                                    Pawn pawn = tmpAnimalsMaleYoung.Pop<Pawn>();
                                    tmpAnimals.Remove(pawn);
                                    ___animalsToSlaughterCached.Add(pawn);
                                }
                            }
                            if (config.maxTotal != -1)
                            {
                                while (tmpAnimals.Count > config.maxTotal)
                                {
                                    Pawn pawn = tmpAnimals.Pop<Pawn>();
                                    if (pawn.gender == Gender.Male)
                                    {
                                        if (pawn.ageTracker.CurLifeStage.reproductive)
                                            tmpAnimalsMale.Remove(pawn);
                                        else
                                            tmpAnimalsMaleYoung.Remove(pawn);
                                    }
                                    else if (pawn.gender == Gender.Female)
                                    {
                                        if (pawn.ageTracker.CurLifeStage.reproductive)
                                            tmpAnimalsFemale.Remove(pawn);
                                        else
                                            tmpAnimalsFemaleYoung.Remove(pawn);
                                    }
                                    ___animalsToSlaughterCached.Add(pawn);
                                }
                            }
                        }
                    }
                    ___cacheDirty = false;
                }
                finally
                {
                    tmpAnimals.Clear();
                    tmpAnimalsMale.Clear();
                    tmpAnimalsFemale.Clear();
                }
                __result = ___animalsToSlaughterCached;
                return false;
            }
        }
    }
}
