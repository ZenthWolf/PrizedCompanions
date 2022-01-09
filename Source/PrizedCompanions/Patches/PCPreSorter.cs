using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection.Emit;

namespace Prized_Companions
{
    // Adds conditions to sorting autoslaughter temp lists
    internal static class PrizedCompanionsPreSorter
    {
        /*        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    var l = instructions.ToList(); // name your actual transpiler XTranspiler
                    string s = "Code:";
                    int i = 0;
                    foreach (var c in l)
                    {
                        if (c.opcode == OpCodes.Call ||
                            c.opcode == OpCodes.Callvirt)
                        { // you can make certain operations more visible
                            Log.Warning("" + i + ": " + c);
                        }
                        else
                        {
                            Log.Message("" + i + ": " + c);
                        }
                        s += "\n" + i + ": " + c;
                        i++;
                        yield return c;
                    }
                    Log.Error(s); // or just print the entire thing out to copy to a text editor.
                }*/

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label UnPrizedLabel = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_Name"));
            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Name), "get_Numerical"));
            yield return new CodeInstruction(OpCodes.Brtrue_S, UnPrizedLabel);

            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.Instance)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.settings)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Settings), nameof(Settings.isActive)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, UnPrizedLabel);

            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.Instance)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.settings)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Settings), nameof(Settings.isAlternate)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, UnPrizedLabel);

            yield return new CodeInstruction(OpCodes.Ldc_I8, long.MinValue);
            yield return new CodeInstruction(OpCodes.Ret);

            codes[0].labels.Add(UnPrizedLabel);

            for (int i = 0; i < codes.Count(); ++i)
            {
                yield return codes[i];
            }
        }

        /*
        private static List<Pawn> tmpAnimals = new List<Pawn>();
        private static List<Pawn> tmpAnimalsMale = new List<Pawn>();
        private static List<Pawn> tmpAnimalsMaleYoung = new List<Pawn>();
        private static List<Pawn> tmpAnimalsFemale = new List<Pawn>();
        private static List<Pawn> tmpAnimalsFemaleYoung = new List<Pawn>();
        private static List<Pawn> tmpAnimalsPregnant = new List<Pawn>();
        
        static bool Prefix(ref List<Pawn> __result, ref bool ___cacheDirty, AutoSlaughterManager __instance, ref List<Pawn> ___animalsToSlaughterCached)
        {
            if (___cacheDirty)
            {
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
                            tmpAnimals.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.Name.Numerical || !PrizedCompanions.Instance.settings.isActive || !PrizedCompanions.Instance.settings.isAlternate ? a.ageTracker.AgeBiologicalTicks : long.MinValue));
                            tmpAnimalsMale.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.Name.Numerical ? a.ageTracker.AgeBiologicalTicks : long.MinValue));
                            tmpAnimalsMaleYoung.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.Name.Numerical ? a.ageTracker.AgeBiologicalTicks : long.MinValue));
                            tmpAnimalsFemale.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.Name.Numerical ? a.ageTracker.AgeBiologicalTicks : long.MinValue));
                            tmpAnimalsFemaleYoung.SortBy<Pawn, long>((Func<Pawn, long>)(a => a.Name.Numerical ? a.ageTracker.AgeBiologicalTicks : long.MinValue));
                            if (config.allowSlaughterPregnant)
                            {
                                tmpAnimalsPregnant.SortBy<Pawn, float>((Func<Pawn, float>)(a => a.Name.Numerical ? -a.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant).Severity : long.MinValue));
                                tmpAnimalsFemale.AddRange((IEnumerable<Pawn>)tmpAnimalsPregnant);
                                tmpAnimals.AddRange((IEnumerable<Pawn>)tmpAnimalsPregnant);
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
            }
            __result = ___animalsToSlaughterCached;
            return false;
        }*/
    }

    //Because pregancy sorts on float- oh no please don't be a source of annoyance.
    internal static class PrizedCompanionsPreSorter_Flt
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label UnPrizedLabel = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_Name"));
            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Name), "get_Numerical"));
            yield return new CodeInstruction(OpCodes.Brtrue_S, UnPrizedLabel);

            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.Instance)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.settings)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Settings), nameof(Settings.isActive)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, UnPrizedLabel);

            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.Instance)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.settings)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Settings), nameof(Settings.isAlternate)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, UnPrizedLabel);

            yield return new CodeInstruction(OpCodes.Ldc_R4, float.MinValue);
            yield return new CodeInstruction(OpCodes.Ret);

            codes[0].labels.Add(UnPrizedLabel);
            
            for (int i = 0; i < codes.Count(); ++i)
            {
                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(AutoSlaughterManager), "get_AnimalsToSlaughter")]
    internal static class TestClass
    {
        static void Postfix(ref List<Pawn> __result)
        {
            Log.Message("PostFixing");
            for (int i = 0; i < __result.Count(); ++i)
            {
                Log.Message("Place " + i.ToString() + " is: " + __result[i].Name.ToString());
            }
        }
    }
}
