using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    [HarmonyPatch(typeof(Dialog_NamePawn), "DoWindowContents")]
    internal static class PrizedCompanionsNamedNotification
    {
        private static void Postfix(Pawn ___pawn)
        {
            ___pawn.Map.autoSlaughterManager.Notify_PawnSpawned();

            //Log.Message("[Prized Companions] New Name clears list");
        }
    }
}