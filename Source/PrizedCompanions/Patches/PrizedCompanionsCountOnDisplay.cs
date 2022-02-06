using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection.Emit;

// If it works, run away before it catches fire
//
// ~ Programming Sun Tsu

// This  might as well have been a destructive prefix probably.
namespace Prized_Companions
{
    [HarmonyPatch(typeof(AutoSlaughterManager), "get_AnimalsToSlaughter")]
    internal static class PrizedCompanionsCountOnDisplay
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //locals
            int spawnedColonyAnimal = 3;
            var PrizedCounter = generator.DeclareLocal(typeof(int));
            var PrizedCounterMale = generator.DeclareLocal(typeof(int));
            var PrizedCounterMaleY = generator.DeclareLocal(typeof(int));
            var PrizedCounterFemale = generator.DeclareLocal(typeof(int));
            var PrizedCounterFemaleY = generator.DeclareLocal(typeof(int));

            //counter labels
            Label NotPrized = generator.DefineLabel();
            Label MaleYoung = generator.DefineLabel();
            Label Sex = generator.DefineLabel();
            Label FemaleYoung = generator.DefineLabel();
            Label Pregnant = generator.DefineLabel();
            Label EndCounterLoop = generator.DefineLabel();
            Label Monosex = generator.DefineLabel();

            //while loop labels
            Label whileFemale = generator.DefineLabel();
            Label whileFemaleY = generator.DefineLabel();
            Label whileMale = generator.DefineLabel();
            Label whileMaleY = generator.DefineLabel();
            Label whileTot = generator.DefineLabel();

            //Age Inversion Labels
            Label oldestFirst = generator.DefineLabel();
            Label youngestFirst = generator.DefineLabel();

            //patch load
            bool zeroPrizeCounters = true;
            bool injectPrizeCounter = true;
            bool endLoop = true;
            bool invertCullAge = true;

            //Next while loop to look for
            int nextCounter = 0;
            bool firstFound = true;

            bool done = false;

            for (int i = 0; i < codes.Count(); ++i)
            {
                if (done)
                {
                    yield return codes[i];
                    continue;
                }
                
                // Init counters of Prized Animals
                else if (zeroPrizeCounters)
                {
                    if (codes[i].operand != null && codes[i].operand.ToString().Contains("tmpAnimals"))
                    {
                        zeroPrizeCounters = false;

                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterMale);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterMaleY);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemale);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemaleY);

                        yield return codes[i];
                        continue;
                    }
                    yield return codes[i];
                    continue;
                }

                // Count Prized Animals, skip sorting them into temp lists
                else if (injectPrizeCounter)
                {
                    if (codes[i].opcode == OpCodes.Bgt)
                    {
                        yield return codes[i];

                        //mod is active
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.Instance)));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.settings)));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Settings), nameof(Settings.isActive)));
                        yield return new CodeInstruction(OpCodes.Brfalse, NotPrized);
                        
                        //mod is counting Prized Companions in the AutoSlaughter system
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.Instance)));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PrizedCompanions), nameof(PrizedCompanions.settings)));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Settings), nameof(Settings.isCounted)));
                        yield return new CodeInstruction(OpCodes.Brfalse, NotPrized);

                        //Animal is Prized
                        yield return new CodeInstruction(OpCodes.Ldloc_S, spawnedColonyAnimal);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_Name"));
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Name), "get_Numerical"));
                        yield return new CodeInstruction(OpCodes.Brtrue, NotPrized);

                        //++PrizedCounter
  //                      Diverted to simplify pregnancy options
  //                      yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounter);
  //                      yield return new CodeInstruction(OpCodes.Ldc_I4_1);
  //                      yield return new CodeInstruction(OpCodes.Add);
  //                      yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter);

                        //Is Animal Male
                        yield return new CodeInstruction(OpCodes.Ldloc_S, spawnedColonyAnimal);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), "gender"));
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Bne_Un_S, Sex);

                        //Is (Male) Animal Adult
                        yield return new CodeInstruction(OpCodes.Ldloc_S, spawnedColonyAnimal);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.ageTracker)));
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_AgeTracker), "get_CurLifeStage"));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(LifeStageDef), nameof(LifeStageDef.reproductive)));
                        yield return new CodeInstruction(OpCodes.Brfalse_S, MaleYoung);


                        //++PrizedCounterMale
                        yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterMale);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterMale);

                        //++PrizedCounter
                        yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Br, EndCounterLoop);

                        //++PrizedCounterMaleY
                        CodeInstruction MYJump = new CodeInstruction(OpCodes.Ldloc, PrizedCounterMaleY);
                        MYJump.labels.Add(MaleYoung);
                        yield return MYJump;
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterMaleY);

                        //++PrizedCounter
                        yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Br, EndCounterLoop);

                        //Is Animal Female
                        CodeInstruction SexJump = new CodeInstruction(OpCodes.Ldloc_S, spawnedColonyAnimal);
                        SexJump.labels.Add(Sex);
                        yield return SexJump;
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), "gender"));
                        yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                        yield return new CodeInstruction(OpCodes.Bne_Un, Monosex);

                        //Is (Female) Animal Adult
                        yield return new CodeInstruction(OpCodes.Ldloc_S, spawnedColonyAnimal);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.ageTracker)));
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_AgeTracker), "get_CurLifeStage"));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(LifeStageDef), nameof(LifeStageDef.reproductive)));
                        yield return new CodeInstruction(OpCodes.Brfalse_S, FemaleYoung);

                        //Is Animal NOT Pregnant
                        yield return new CodeInstruction(OpCodes.Ldloc_S, spawnedColonyAnimal);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.health)));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.hediffSet)));
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(HediffDefOf), nameof(HediffDefOf.Pregnant)));
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(HediffSet), "GetFirstHediffOfDef"));
                        yield return new CodeInstruction(OpCodes.Brtrue_S, Pregnant);

                        //++PrizedCounterFemale
                        yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemale);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemale);

                        //++PrizedCounter
                        yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Br, EndCounterLoop);

                        //Are Pregnant Animals are Slaughterable
                        CodeInstruction PregnantJump = new CodeInstruction(OpCodes.Ldloc_1);
                        PregnantJump.labels.Add(Pregnant);
                        yield return PregnantJump;
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AutoSlaughterConfig), nameof(AutoSlaughterConfig.allowSlaughterPregnant)));
                        yield return new CodeInstruction(OpCodes.Brfalse_S, EndCounterLoop);

                        //Add Pregnant Animal to totals iff slaughterable
                        //++PrizedCounterFemale
                        yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemale);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemale);

                        //++PrizedCounter
                        yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Br, EndCounterLoop);

                        //++PrizedCounterFemaleY
                        CodeInstruction FemaleJump = new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemaleY);
                        FemaleJump.labels.Add(FemaleYoung);
                        yield return FemaleJump;
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemaleY);

                        //++PrizedCounter -- Catches Female Juveniles AND Asexual Creatures
                        CodeInstruction MonosexJump = new CodeInstruction(OpCodes.Ldloc, PrizedCounter);
                        MonosexJump.labels.Add(Monosex);
                        yield return MonosexJump;
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter);
                        yield return new CodeInstruction(OpCodes.Br, EndCounterLoop);

                        //End Prized Check
                        // yield return new CodeInstruction(OpCodes.Br);

                        //Modified NEXT line
                        ++i;
                        codes[i].labels.Add(NotPrized);
                        injectPrizeCounter = false;

                        yield return codes[i];
                        continue;
                    }

                    yield return codes[i];
                    continue;
                }

                // Mark end of loop for refernce
                else if (endLoop)
                {
                    if ( (codes[i].opcode == OpCodes.Ldloca_S) && (codes[i-1].opcode == OpCodes.Callvirt) )
                    {
                        codes[i].labels.Add(EndCounterLoop);
                        endLoop = false;

                        yield return codes[i];
                        continue;
                    }
                    yield return codes[i];
                    continue;
                }

                else if(invertCullAge)
                {
                    //Picks out the start of the sortings
                    if(codes[i].operand != null && codes[i].operand.ToString().Contains("tmpAnimals"))
                    {
                        var newSortStart = new CodeInstruction(OpCodes.Ldloc_1);
                        foreach( Label l in codes[i].labels)
                            newSortStart.labels.Add(l);
                        yield return newSortStart;
                        yield return new CodeInstruction(OpCodes.Call, typeof(PCSlaughterConfigPatch).GetMethod("IsYoungestFirst", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
                        yield return new CodeInstruction(OpCodes.Brfalse_S, oldestFirst);

                        var bFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
                        yield return new CodeInstruction(OpCodes.Ldsflda, typeof(AutoSlaughterManager).GetField("tmpAnimals", bFlags));
                        yield return new CodeInstruction(OpCodes.Ldsflda, typeof(AutoSlaughterManager).GetField("tmpAnimalsMale", bFlags));
                        yield return new CodeInstruction(OpCodes.Ldsflda, typeof(AutoSlaughterManager).GetField("tmpAnimalsMaleYoung", bFlags));
                        yield return new CodeInstruction(OpCodes.Ldsflda, typeof(AutoSlaughterManager).GetField("tmpAnimalsFemale", bFlags));
                        yield return new CodeInstruction(OpCodes.Ldsflda, typeof(AutoSlaughterManager).GetField("tmpAnimalsFemaleYoung", bFlags));
                        yield return new CodeInstruction(OpCodes.Call, typeof(PrizedCompanionsCountOnDisplay).GetMethod("AltSorter", bFlags));
                        yield return new CodeInstruction(OpCodes.Br_S, youngestFirst);


                        var freshcode = codes[i].Clone();
                        freshcode.labels.Add(oldestFirst);
                        yield return freshcode;
                        ++i;

                        Log.Message("[PrizedCompanions] Method name checks: ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                        while (codes[i].opcode != OpCodes.Ldloc_1)
                        {
                            Log.Message(codes[i].ToString());
                            yield return codes[i];
                            ++i;
                        }
                        Log.Message("D O N E");

                        codes[i].labels.Add(youngestFirst);
                        yield return codes[i];
                        invertCullAge = false;
                        continue;
                    }
                    yield return codes[i];
                    continue;
                }

                // Modify loop counters to include Prized Companions
                else if (codes[i+1].operand != null && codes[i+1].operand.ToString().Contains("max"))
                {
                    //Female Counter
                    if (nextCounter == 0)
                    {;
                        if (codes[i+1].operand.ToString().Contains("maxFemales"))
                        {
                            //Calculate new bound for while loop
                            if (firstFound)
                            {
                                //Don't interrupt previous call
                                //ldloc.1      // config
                                yield return codes[i];
                                //ldfld        int32 Verse.AutoSlaughterConfig::maxFemales
                                yield return codes[i + 1];

                                //Subtract PrizedCounter (Female Adult)
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemale);
                                yield return new CodeInstruction(OpCodes.Sub);

                                //max( maxAllowed - PrizedCounter , 0)
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemale); // May be irresponsible? No one is here to stop me...

                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemale);
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Bge, whileFemale);

                                //store 0
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemale); // No one CAN stop me!

                                //proceed with code:
                                var resume = new CodeInstruction(codes[i].opcode, codes[i].operand);
                                resume.labels.Add(whileFemale);
                                yield return resume;

                                ++i;
                                yield return codes[i];
                                firstFound = false;
                                continue;
                            }

                            else
                            {
                                ++i;
                                firstFound = true;
                                nextCounter++;

                                //replace while condition
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemale);

                                continue;
                            }
                        }

                        yield return codes[i];
                        continue;
                    }

                    //FemaleY Counter
                    if (nextCounter == 1)
                    {
                        if (codes[i + 1].operand.ToString().Contains("maxFemalesYoung"))
                        {
                            //Calculate new bound for while loop
                            if (firstFound)
                            {
                                //Don't interrupt previous call
                                //ldloc.1      // config
                                yield return codes[i];
                                //ldfld        int32 Verse.AutoSlaughterConfig::maxFemalesYoung
                                yield return codes[i + 1];

                                //Subtract PrizedCounter (Female Juvenile)
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemaleY);
                                yield return new CodeInstruction(OpCodes.Sub);

                                //max( maxAllowed - PrizedCounter , 0)
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemaleY); // May be irresponsible? No one is here to stop me...

                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemaleY);
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Bge, whileFemaleY);

                                //store 0
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterFemaleY); // No one CAN stop me!

                                //proceed with code:
                                var resume = new CodeInstruction(codes[i].opcode, codes[i].operand);
                                resume.labels.Add(whileFemaleY);
                                yield return resume;

                                ++i;
                                yield return codes[i];
                                firstFound = false;
                                continue;
                            }

                            else
                            {
                                ++i;
                                firstFound = true;
                                nextCounter++;

                                //replace while condition
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterFemaleY);

                                continue;
                            }
                        }

                        yield return codes[i];
                        continue;
                    }

                    //Male Counter
                    if (nextCounter == 2)
                    {
                        if (codes[i + 1].operand.ToString().Contains("maxMales"))
                        {
                            //Calculate new bound for while loop
                            if (firstFound)
                            {
                                //Don't interrupt previous call
                                //ldloc.1      // config
                                yield return codes[i];
                                //ldfld        int32 Verse.AutoSlaughterConfig::maxMales
                                yield return codes[i + 1];

                                //Subtract PrizedCounter (Male Adult)
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterMale);
                                yield return new CodeInstruction(OpCodes.Sub);

                                //max( maxAllowed - PrizedCounter , 0)
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterMale); // May be irresponsible? No one is here to stop me...

                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterMale);
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Bge, whileMale);

                                //store 0
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterMale); // No one CAN stop me!

                                //proceed with code:
                                var resume = new CodeInstruction(codes[i].opcode, codes[i].operand);
                                resume.labels.Add(whileMale);
                                yield return resume;

                                ++i;
                                yield return codes[i];
                                firstFound = false;
                                continue;
                            }

                            else
                            {
                                ++i;
                                firstFound = true;
                                nextCounter++;

                                //replace while condition
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterMale);

                                continue;
                            }
                        }

                        yield return codes[i];
                        continue;
                    }

                    //MaleY Counter
                    if (nextCounter == 3)
                    {
                        if (codes[i + 1].operand.ToString().Contains("maxMalesYoung"))
                        {
                            //Calculate new bound for while loop
                            if (firstFound)
                            {
                                //Don't interrupt previous call
                                //ldloc.1      // config
                                yield return codes[i];
                                //ldfld        int32 Verse.AutoSlaughterConfig::maxMalesYoung
                                yield return codes[i + 1];

                                //Subtract PrizedCounter (Male Juvenile)
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterMaleY);
                                yield return new CodeInstruction(OpCodes.Sub);

                                //max( maxAllowed - PrizedCounter , 0)
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterMaleY); // May be irresponsible? No one is here to stop me...

                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterMaleY);
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Bge, whileMaleY);

                                //store 0
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounterMaleY); // No one CAN stop me!

                                //proceed with code:
                                var resume = new CodeInstruction(codes[i].opcode, codes[i].operand);
                                resume.labels.Add(whileMaleY);
                                yield return resume;

                                ++i;
                                yield return codes[i];
                                firstFound = false;
                                continue;
                            }

                            else
                            {
                                ++i;
                                firstFound = true;
                                nextCounter++;

                                //replace while condition
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounterMaleY);

                                continue;
                            }
                        }

                        yield return codes[i];
                        continue;
                    }

                    //MaxTotal Counter
                    if (nextCounter == 4)
                    {
                        if (codes[i + 1].operand.ToString().Contains("maxTotal"))
                        {
                            //Calculate new bound for while loop
                            if (firstFound)
                            {
                                //Don't interrupt previous call
                                //ldloc.1      // config
                                yield return codes[i];
                                //ldfld        int32 Verse.AutoSlaughterConfig::maxTotal
                                yield return codes[i + 1];

                                //Subtract PrizedCounter (Total)
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounter);
                                yield return new CodeInstruction(OpCodes.Sub);

                                //max( maxAllowed - PrizedCounter , 0)
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter); // May be irresponsible? No one is here to stop me...
                                
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounter);
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Bge, whileTot);

                                //store 0
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                yield return new CodeInstruction(OpCodes.Stloc, PrizedCounter); // No one CAN stop me!

                                //proceed with code:
                                var resume = new CodeInstruction(codes[i].opcode, codes[i].operand);
                                resume.labels.Add(whileTot);
                                yield return resume;
                                
                                ++i;
                                yield return codes[i];
                                firstFound = false;
                                continue;
                            }

                            else
                            {
                                
                                ++i;
                                done = true;

                                //replace while condition bound
                                yield return new CodeInstruction(OpCodes.Ldloc, PrizedCounter);

                                continue;
                            }
                        }

                        yield return codes[i];
                        continue;
                    }

                    yield return codes[i];
                    continue;
                }

                else
                {
                    yield return codes[i];
                }
            }
        }

        private static void AltSorter(ref List<Pawn> tmpAnimals, ref List<Pawn> tmpAnimalsMale, ref List<Pawn> tmpAnimalsMaleYoung,
            ref List<Pawn> tmpAnimalsFemale, ref List<Pawn> tmpAnimalsFemaleYoung)
        {
            Log.Message("[Prized Companions] R e v e r s i n g . . . ");
            tmpAnimals.SortBy<Pawn, long>((Func<Pawn, long>)(a => -a.ageTracker.AgeBiologicalTicks));
            tmpAnimalsMale.SortBy<Pawn, long>((Func<Pawn, long>)(a => -a.ageTracker.AgeBiologicalTicks));
            tmpAnimalsMaleYoung.SortBy<Pawn, long>((Func<Pawn, long>)(a => -a.ageTracker.AgeBiologicalTicks));
            tmpAnimalsFemale.SortBy<Pawn, long>((Func<Pawn, long>)(a => -a.ageTracker.AgeBiologicalTicks));
            tmpAnimalsFemaleYoung.SortBy<Pawn, long>((Func<Pawn, long>)(a => -a.ageTracker.AgeBiologicalTicks));
        }
    }

    //presorter ends up not being useful for this part :(
    // Stashed here as a relic on the excuse that I might use it to reverse slaughter logic latger
    /*
        // Adds conditions to sorting autoslaughter temp lists
        internal static class PrizedCompanionsPreSorter
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

                yield return new CodeInstruction(OpCodes.Ldc_I8, long.MinValue);
                yield return new CodeInstruction(OpCodes.Ret);

                codes[0].labels.Add(UnPrizedLabel);

                for (int i = 0; i < codes.Count(); ++i)
                {
                    yield return codes[i];
                }
            }
*/

}
