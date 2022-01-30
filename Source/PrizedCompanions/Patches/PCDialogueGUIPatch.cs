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
    // Alter the width of the firtst column label
    // Originally- calculated from the window rect
    // Now subtract the same amount as added to include new row!
    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "CalculateLabelWidth")]
    internal static class PCOhNo
    {
        private static void Postfix(ref float __result)
        {
            __result -= 132;
        }
    }

    // Make UI window larger to include space for new column
    // Originally- Set value
    // Now add space for new column- same amount must adjust initial label size!
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

                        //AddCurrentAndMaxEntries
                        //Sets the upper label 

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        //This should be a Keyed string for translation
                        yield return new CodeInstruction(OpCodes.Ldstr, "Reverse");
                        //Shifts labelbox width (Text is centered, min is -60)
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.0f);
                        //Shifts labelbox width (Text is centered, min is -56)
                        //I don't understand- this is associated with an empty string, but still affects placement of the text
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
                        yield return new CodeInstruction(OpCodes.Dup); // May not need in end if only 1 half column

                        //widgetRow.Label((str) text, width, (str) TTip, height)

                        yield return new CodeInstruction(OpCodes.Ldstr, "Test2");
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 72f);
                        yield return new CodeInstruction(OpCodes.Ldstr, "TestTip");
                        yield return new CodeInstruction(OpCodes.Ldc_R4, -1.0f); //defaults height
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

    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "DoAnimalRow")]
    internal static class PCDialogueRowPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //Locals:
            int animalCount = 1;

            //Fields
            System.Reflection.FieldInfo bonded = Type.GetType("RimWorld.Dialog_AutoSlaughter+AnimalCountRecord, Assembly-CSharp").GetField("bonded", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            //Things I don't understand, but currently need:
            //Is this the best way?
            //Is this a sign I've gone too far?
            System.Reflection.FieldInfo row =Type.GetType("RimWorld.Dialog_AutoSlaughter+<>c__DisplayClass25_0, Assembly-CSharp").GetField("row");

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
                        //Last Branch needs to be redirected
                        Label newLabel = generator.DefineLabel();
                        yield return new CodeInstruction(codes[i].opcode, newLabel);
                        ++i;
                        yield return codes[i];
                        ++i;
                        yield return codes[i];
                        ++i;

                        CodeInstruction jumpTarg = new CodeInstruction(OpCodes.Ldloc_3);
                        jumpTarg.labels.Add(newLabel);
                        yield return jumpTarg;
                        yield return new CodeInstruction(OpCodes.Ldfld, row);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 52f);
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetMethod(nameof(WidgetRow.Gap)));

                        yield return new CodeInstruction(OpCodes.Ldc_I4_4);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Text), "Anchor").GetSetMethod());
                        
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, row);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, animalCount);
                        yield return new CodeInstruction(OpCodes.Ldflda, bonded);
                        yield return new CodeInstruction(OpCodes.Call, typeof(int).GetMethod("ToString", new Type[] { }));
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 60f);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, -1f);
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetMethod("Label"));
                        yield return new CodeInstruction(OpCodes.Pop);

                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Text), "Anchor").GetSetMethod());

                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, row);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 24f);
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetMethod(nameof(WidgetRow.Gap)));

                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AutoSlaughterConfig), "allowSlaughterBonded"));
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, row);
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetProperty("FinalX").GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(AutoSlaughterConfig), "allowSlaughterBonded"));
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 24f);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Widgets).GetMethod("Checkbox", new Type[] { typeof(float), typeof(float), typeof(bool).MakeByRefType(), typeof(float), typeof(bool), typeof(bool), typeof(Texture2D), typeof(Texture2D) }));
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AutoSlaughterConfig), "allowSlaughterBonded"));
                        yield return new CodeInstruction(OpCodes.Beq_S, codes[i].labels[0]);
                        
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Dialog_AutoSlaughter).GetMethod("RecalculateAnimals",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));

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
