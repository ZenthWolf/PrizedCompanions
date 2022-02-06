using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    //Because these aren't done in vanilla for some strange reason . . ?

    /// <summary>
    /// Notify autoslaughters when animal is known to be pregnant
    /// </summary>
    [HarmonyPatch(typeof(HediffComp_MessageAfterTicks), "CompPostTick")]
    internal static class PCPregnancyNotifications
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //Patch load
            bool notifySlaughterer = true;
            bool done = false;

            for (int i = 0; i < codes.Count(); ++i)
            {
                if (done)
                {
                    yield return codes[i];
                    continue;
                }

                else if (notifySlaughterer)
                {
                    if (codes[i].opcode == OpCodes.Brtrue)
                    {
                        yield return codes[i];

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        var pubFlag = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
                        yield return new CodeInstruction(OpCodes.Call, typeof(HediffComp).GetProperty("Pawn", pubFlag).GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Call, typeof(Thing).GetProperty("Map", pubFlag).GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(Map).GetField("autoSlaughterManager", pubFlag));
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(AutoSlaughterManager).GetMethod("Notify_PawnSpawned", pubFlag));

                        notifySlaughterer = false;
                        done = true;
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
    }

    /// <summary>  DoBirthSpawn
    /// Notify autoslaughters when animal miscarries
    /// </summary>
    [HarmonyPatch(typeof(Hediff_Pregnant), "Miscarry")]
    internal static class PCMiscarryyNotifications
    {
        static void Postfix(Pawn ___pawn)
        {
            ___pawn.Map.autoSlaughterManager.Notify_PawnDespawned();
        }
    }

    /// <summary>  DoBirthSpawn
    /// Notify autoslaughters when animal births
    /// Seems vanilla did no such check?
    /// </summary>
    [HarmonyPatch(typeof(Hediff_Pregnant), "DoBirthSpawn")]
    internal static class PCBirthNotifications
    {
        static void Postfix(Pawn ___pawn)
        {
            ___pawn.Map.autoSlaughterManager.Notify_PawnSpawned();
        }
    }
 }
