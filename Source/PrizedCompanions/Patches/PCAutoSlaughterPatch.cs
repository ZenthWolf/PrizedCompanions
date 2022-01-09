using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    [HarmonyPatch(typeof(AutoSlaughterManager), "CanEverAutoSlaughter")]
    internal static class PrizedCompanionsCantBeSlaughteredPatch
    {
        private static void Postfix(ref bool __result, Pawn animal)
        {
            if (PrizedCompanions.Instance.settings.isActive && !PrizedCompanions.Instance.settings.isAlternate)
            {
                if (__result)
                {
                    __result = (animal.Name.Numerical);
                }
            }
        }
    }

    [HarmonyPatch(typeof(AutoSlaughterManager), "CanAutoSlaughterNow")]
    internal static class PrizedCompanionsCantBeSlaughterNowPatch
    {
        private static void Postfix(ref bool __result, Pawn animal)
        {
            if (false && PrizedCompanions.Instance.settings.isActive && PrizedCompanions.Instance.settings.isAlternate)
            {
                if (__result)
                {
                    __result = (animal.Name.Numerical);
                }
            }
        }
    }
}