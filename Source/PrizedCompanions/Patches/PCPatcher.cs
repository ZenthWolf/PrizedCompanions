using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    [StaticConstructorOnStartup]
    public static class PrizedCompanionsMain
    {
        static PrizedCompanionsMain()
        {
            var harmony = new Harmony("ZenthWolf.PrizedCompanions");
            harmony.PatchAll();

            /*
            for (int i = 0; i < 5; ++i)
            {
                var animalSort_original = AccessTools.FirstMethod(
                    AccessTools.FirstInner(typeof(AutoSlaughterManager), inner => inner.Name.Contains("<>c")),
                    method => method.Name.Contains("b__11_"+i.ToString()));

                var animalSort_Transpiler = typeof(PrizedCompanionsPreSorter).GetMethod("Transpiler", AccessTools.all);

                harmony.Patch(animalSort_original, transpiler: new HarmonyMethod(animalSort_Transpiler));
            }
            
            var animalFltSort_original = AccessTools.FirstMethod(
                AccessTools.FirstInner(typeof(AutoSlaughterManager), inner => inner.Name.Contains("<>c")),
                method => method.Name.Contains("b__11_5"));

            var animalFltSort_Transpiler = typeof(PrizedCompanionsPreSorter_Flt).GetMethod("Transpiler", AccessTools.all);

            harmony.Patch(animalFltSort_original, transpiler: new HarmonyMethod(animalFltSort_Transpiler));
            */
        }
    }
}
