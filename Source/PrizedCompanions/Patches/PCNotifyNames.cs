using Verse;

namespace Prized_Companions
{
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