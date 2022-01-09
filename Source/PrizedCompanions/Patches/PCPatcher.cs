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

            var animalSort_original = AccessTools.FirstMethod(
                AccessTools.FirstInner(typeof(AutoSlaughterManager), inner => inner.Name.Contains("<>c")),
                method => method.Name.Contains("b__11_0"));

            var animalSort_Transpiler = typeof(PrizedCompanionsPreSorter).GetMethod("Transpiler", AccessTools.all);

            harmony.Patch(animalSort_original, transpiler: new HarmonyMethod(animalSort_Transpiler));
        }
    }
}
