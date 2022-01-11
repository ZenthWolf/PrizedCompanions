using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    // 7/10, needs improvement
    // Don't make me transpile this

    [HarmonyPatch(typeof(Dialog_NamePawn), "DoWindowContents")]
    internal static class PrizedCompanionsNamedNotification
    {
        private static void Postfix(Pawn ___pawn)
        {
            if (PrizedCompanions.Instance.settings.isActive)
            {
                ___pawn.Map.autoSlaughterManager.Notify_PawnSpawned();
            }
        }
    }
}