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

    /*
    [HarmonyPatch(typeof(AutoSlaughterManager), "get_AnimalsToSlaughter")]
    internal static class PrizedCompanionsGetterTester
    {
        private static void Postfix(ref List<Pawn> __result)
        {
            //Should disable entire autoslaughter system.
            __result.Clear();
        }
    }
    */
}