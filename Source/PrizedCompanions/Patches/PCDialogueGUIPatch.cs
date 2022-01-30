using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;


namespace Prized_Companions
{
    
    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "DoWindowContents")]
    internal static class GetInfo
    {
        private static bool Prefix(Rect inRect)
        {
            Log.Message("~~GetInfo~~~");
            Log.Message("X info: " + inRect.x.ToString() + ", " + inRect.xMax.ToString() + ", " + inRect.width.ToString());
            Log.Message("Y info: " + inRect.y.ToString() + ", " + inRect.yMax.ToString() + ", " + inRect.height.ToString());

            return true;
        }
    }
    
    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "CalculateLabelWidth")]
    internal static class PCOhNo
    {
        /*
        private static bool Prefix(ref float __result, Rect rect)
        {
            Log.Message("~~PCOhNo~~~");
            Log.Message("X info: " + rect.x.ToString() + ", " + rect.xMax.ToString() + ", " + rect.width.ToString());
            Log.Message("Y info: " + rect.y.ToString() + ", " + rect.yMax.ToString() + ", " + rect.height.ToString());
            float num = 64f;
            __result = (float)((double)rect.width - 24.0 - 4.0 - 4.0 - (double)num * 8.0 - 420.0 - 32.0);
            Log.Message("result info: " + __result.ToString());
            return false;
        }
        */
        private static void Postfix(ref float __result)
        {
            __result -= 132;
        }
    }

    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "get_InitialSize")]
    internal static class PCDialogueResize
    {
        private static void Postfix(ref Vector2 __result)
        {
            __result.x += 132;
        }
    }

    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "DoAnimalHeader")]
    internal static class PCDialogueHeaderPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            bool doFirst = true;
            bool doBonusDup = true;
            bool doSecond = true;
            bool done = false;

            for (int i = 0; i < codes.Count(); ++i)
            {
                if (done)
                {
                    yield return codes[i];
                    continue;
                }
                else if (doFirst)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i - 5].operand?.ToString() == "AnimalBonded")
                    {
                        yield return codes[i];

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldstr, "Test");
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.0f);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 16f);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, 5);
                        yield return codes[i].Clone();

                        doFirst = false;
                        continue;
                    }
                    else
                    {
                        yield return codes[i];
                        continue;
                    }
                }
                else if (doBonusDup)
                {
                    if (codes[i].opcode == OpCodes.Pop && codes[i - 5].operand?.ToString() == "AutoSlaughterHeaderTooltipCurrentBonded")
                    {
                        yield return codes[i];
                        yield return new CodeInstruction(OpCodes.Dup);

                        doBonusDup = false;
                        continue;
                    }
                    else
                    {
                        yield return codes[i];
                        continue;
                    }
                        
                }
                else if(doSecond)
                {
                    if (codes[i].opcode == OpCodes.Pop && codes[i - 5].operand?.ToString() == "AutoSlaughterHeaderTooltipAllowSlaughterBonded")
                    {
                        yield return codes[i];
                        yield return new CodeInstruction(OpCodes.Dup);

                        yield return new CodeInstruction(OpCodes.Ldstr, "Test2");
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 72f);
                        yield return new CodeInstruction(OpCodes.Ldstr, "TestTip");
                        yield return new CodeInstruction(OpCodes.Ldc_R4, -1.0f);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WidgetRow), nameof(WidgetRow.Label)));
                        yield return new CodeInstruction(OpCodes.Pop);

                        yield return new CodeInstruction(OpCodes.Ldstr, "Test3");
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 72f);
                        yield return new CodeInstruction(OpCodes.Ldstr, "TestTip2");
                        yield return new CodeInstruction(OpCodes.Ldc_R4, -1.0f);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WidgetRow), nameof(WidgetRow.Label)));
                        yield return new CodeInstruction(OpCodes.Pop);

                        doSecond = false;
                        done = true;
                        continue;
                    }
                    else
                    {
                        yield return codes[i];
                        continue;
                    }
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }

    /*
    Text.Anchor = TextAnchor.MiddleCenter;
      row.Label(animalCount.pregnant.ToString(), 60f);
      Text.Anchor = TextAnchor.UpperLeft;
      int num1 = config.allowSlaughterPregnant ? 1 : 0;
      row.Gap(26f);
      Widgets.Checkbox(row.FinalX, 0.0f, ref config.allowSlaughterPregnant, paintable: true);
      int num2 = config.allowSlaughterPregnant ? 1 : 0;
      if (num1 != num2)
        this.RecalculateAnimals();
      row.Gap(52f);
    */
    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "DoAnimalRow")]
    internal static class PCDialogueRowPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            Log.Message("[Prized Companions] NEW TRANSPILER STARTING: ");

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //Locals:
            int animalCount = 1;

            bool doFirst = true;
            bool done = false;

            for (int i = 0; i < codes.Count(); ++i)
            {
                if (done)
                {
                    yield return codes[i];
                    continue;
                }
                else if (doFirst)
                {
                    if (codes[i+4].opcode == OpCodes.Ret)
                    {
                        Label newLabel = generator.DefineLabel();
                        yield return new CodeInstruction(codes[i].opcode, newLabel);
                        ++i;
                        yield return codes[i];
                        ++i;
                        yield return codes[i];
                        ++i;

                        CodeInstruction jumpTarg = codes[i - 37].Clone();
                        jumpTarg.labels.Add(newLabel);
                        yield return jumpTarg;

                        for (int j = 36; j >3; --j)
                        {
                            yield return codes[i - j].Clone();
                        }

                        yield return new CodeInstruction(codes[i - 3].opcode, codes[i].labels[0]);
                        yield return codes[i - 2];
                        yield return codes[i - 1];
                        /*
                        CodeInstruction newJumpTarg = new CodeInstruction(OpCodes.Ldc_I4_4);
                        newJumpTarg.labels.Add(newLabel);
                        yield return newJumpTarg;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Text), "Anchor").GetSetMethod());

                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return codes[i - 34].Clone(); // I...
                        yield return new CodeInstruction(OpCodes.Ldloca_S, animalCount);
                        yield return codes[i - 32].Clone(); // Stupid private struct, I don't have time to play with you
                        yield return new CodeInstruction(OpCodes.Call, typeof(int).GetMethod("ToString"));
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 60f);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, -1f);
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetMethod("Label"));
                        yield return new CodeInstruction(OpCodes.Pop);

                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Text), "Anchor").GetSetMethod());

                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return codes[i - 34].Clone(); // I...
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 24f);
                        yield return new CodeInstruction(OpCodes.Callvirt, nameof(WidgetRow.Gap));

                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AutoSlaughterConfig), "allowSlaughterBonded"));
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return codes[i - 34].Clone(); // I...
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetProperty("FinalX").GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(AutoSlaughterConfig), "allowSlaughterBonded"));
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 24f);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Widgets).GetMethod("Checkbox"));
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AutoSlaughterConfig), "allowSlaughterBonded"));
                        yield return new CodeInstruction(OpCodes.Beq_S, codes[i].ExtractLabels()[0]);

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Dialog_AutoSlaughter).GetMethod("RecalculateAnimals"));
                        */
                        yield return codes[i]; // EndGUI Group

                        doFirst = false;
                        done = true;
                        continue;
                    }
                    else
                    {
                        yield return codes[i];
                        continue;
                    }
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }
}
