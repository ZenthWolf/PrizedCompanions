using Verse;

namespace Prized_Companions
{
    internal static class PrizedCompanionsCantBeSlaughteredPatch
    {
        private static void Postfix(ref bool __result, Pawn animal)
        {
            if (PrizedCompanions.Instance.settings.isActive && (!PrizedCompanions.Instance.settings.isCounted | PrizedCompanions.Instance.settings.IsLiteMode))
            {
                if (__result)
                {
                    __result = (animal.Name.Numerical);
                }
            }
        }
    }
}