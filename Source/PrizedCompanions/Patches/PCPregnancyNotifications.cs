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
                        var bFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
                        yield return new CodeInstruction(OpCodes.Call, typeof(PCPregnancyNotifications).GetMethod("NotifyPregnant", bFlags));

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

        static private void NotifyPregnant(HediffComp_MessageAfterTicks that)
        {
            if (that.parent as Hediff_Pregnant != null)
            {
                if(that.Pawn != null)
                    that.Pawn.Map.autoSlaughterManager.Notify_PawnSpawned();
            }
        }
    }

    /// <summary> 
    /// Notify autoslaughters when animal miscarries
    /// </summary>
    internal static class PCMiscarryyNotifications
    {
        static void Postfix(Pawn ___pawn)
        {
            ___pawn.Map.autoSlaughterManager.Notify_PawnDespawned();
        }
    }

    /// <summary> 
    /// Notify autoslaughters when animal births
    /// Seems vanilla did no such check?
    /// </summary>
    internal static class PCBirthNotifications
    {
        static void Postfix(Pawn ___pawn)
        {
            ___pawn.Map.autoSlaughterManager.Notify_PawnSpawned();
        }
    }
 }
