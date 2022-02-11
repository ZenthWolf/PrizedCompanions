using HarmonyLib;
using RimWorld;
using Verse;

namespace Prized_Companions
{
    [StaticConstructorOnStartup]
    public static class PrizedCompanionsMain
    {
        static PrizedCompanionsMain()
        {
            var harmony = new Harmony("ZenthWolf.PrizedCompanions");
            var PatchFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
            var nonpublicFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            //Always patched

            //Base Functionality
            harmony.Patch(typeof(AutoSlaughterManager).GetMethod("CanEverAutoSlaughter"),
                postfix: new HarmonyMethod(typeof(PrizedCompanionsCantBeSlaughteredPatch).GetMethod("Postfix", PatchFlags)));
            harmony.Patch(typeof(Dialog_NamePawn).GetMethod("DoWindowContents"),
                postfix: new HarmonyMethod(typeof(PrizedCompanionsNamedNotification).GetMethod("Postfix", PatchFlags)));

            if (!PrizedCompanions.Instance.settings.IsLiteMode)
            {
                //Pregnancy-related notifications to update list
                harmony.Patch(typeof(HediffComp_MessageAfterTicks).GetMethod("CompPostTick"),
                    transpiler: new HarmonyMethod(typeof(PCPregnancyNotifications).GetMethod("Transpiler", PatchFlags)));
                harmony.Patch(typeof(Hediff_Pregnant).GetMethod("Miscarry", nonpublicFlags),
                    postfix: new HarmonyMethod(typeof(PCMiscarryyNotifications).GetMethod("Postfix", PatchFlags)));
                harmony.Patch(typeof(Hediff_Pregnant).GetMethod("DoBirthSpawn"),
                    postfix: new HarmonyMethod(typeof(PCBirthNotifications).GetMethod("Postfix", PatchFlags)));
                if (PrizedCompanions.Instance.settings.IsDoGUI)
                {
                    //Count Prized Animals
                    harmony.Patch(typeof(AutoSlaughterManager).GetMethod("get_AnimalsToSlaughter"),
                        transpiler: new HarmonyMethod(typeof(PrizedCompanionsCountOnDisplay).GetMethod("Transpiler", PatchFlags)));

                    //YoungestFirst Slaughter Logic
                    harmony.Patch(typeof(AutoSlaughterConfig).GetMethod("ExposeData"),
                        postfix: new HarmonyMethod(typeof(PCSlaughterConfigPatch).GetMethod("Postfix", PatchFlags)));

                    //Youngest First GUId
                    harmony.Patch(typeof(Dialog_AutoSlaughter).GetMethod("CalculateLabelWidth", nonpublicFlags),
                        postfix: new HarmonyMethod(typeof(PCLabelWidth).GetMethod("Postfix", PatchFlags)));
                    harmony.Patch(typeof(Dialog_AutoSlaughter).GetMethod("get_InitialSize"),
                        postfix: new HarmonyMethod(typeof(PCDialogueResize).GetMethod("Postfix", PatchFlags)));
                    harmony.Patch(typeof(Dialog_AutoSlaughter).GetMethod("DoAnimalHeader", nonpublicFlags),
                        transpiler: new HarmonyMethod(typeof(PCDialogueHeaderPatch).GetMethod("Transpiler", PatchFlags)));
                    harmony.Patch(typeof(Dialog_AutoSlaughter).GetMethod("DoWindowContents"),
                        transpiler: new HarmonyMethod(typeof(PCDrawDelimiter).GetMethod("Transpiler", PatchFlags)));
                    harmony.Patch(typeof(Dialog_AutoSlaughter).GetMethod("DoAnimalRow", nonpublicFlags),
                        transpiler: new HarmonyMethod(typeof(PCDialogueRowPatch).GetMethod("Transpiler", PatchFlags)));
                }
                else Log.Message("[Prized Companions] Suppressing GUI Addon");
            }
            else Log.Message("[Prized Companions] Is running in Lite mode- most features are disabled");
        }
    }
}
