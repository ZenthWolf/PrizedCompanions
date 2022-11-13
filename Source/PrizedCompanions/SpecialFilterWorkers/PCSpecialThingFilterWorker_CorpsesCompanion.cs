using Verse;

namespace RimWorld
{
    public class PCSpecialThingFilterWorker_CorpsesCompanion : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t) => t is Corpse corpse && corpse.InnerPawn.def.race.Animal && corpse.InnerPawn.Faction == Faction.OfPlayer && !corpse.InnerPawn.Name.Numerical;
    }
}
