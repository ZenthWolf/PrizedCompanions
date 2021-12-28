using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    [HarmonyPatch(typeof(AutoSlaughterManager), "CanEverAutoSlaughter")]
    internal static class PrizedCompanionsCantBeSlaughteredPatch
    {
        private static void Postfix(ref bool __result, Pawn animal)
        {
            if (__result)
            {
                __result = (animal.Name.Numerical);
            }
        }
    }
}