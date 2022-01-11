using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    [HarmonyPatch(typeof(AutoSlaughterManager), "CanEverAutoSlaughter")]
    internal static class PrizedCompanionsCantBeSlaughteredPatch
    {
        private static void Postfix(ref bool __result, Pawn animal)
        {
            if (PrizedCompanions.Instance.settings.isActive && !PrizedCompanions.Instance.settings.isCounted)
            {
                if (__result)
                {
                    __result = (animal.Name.Numerical);
                }
            }
        }
    }
}