using Verse;

namespace RimWorld
{
    public class PCSpecialThingFilterWorker_CorpsesAnimal : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t) => t is Corpse corpse && corpse.InnerPawn.def.race.Animal && (corpse.InnerPawn.Name.Numerical || corpse.InnerPawn.Faction != Faction.OfPlayer);
    }
}
