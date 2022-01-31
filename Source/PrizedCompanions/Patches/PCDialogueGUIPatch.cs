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
            __result -= 68;
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
            __result.x += 68;
            __result.y += 42; // Try and respect Phi.
                              // Bonus: Get to pretend you have a sense of design. 
        }
    }

    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "DoAnimalHeader")]
    internal static class PCDialogueHeaderPatch
    {
        static float offset;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            System.Reflection.FieldInfo rect2 = Type.GetType("RimWorld.Dialog_AutoSlaughter+<>c__DisplayClass24_0, Assembly-CSharp").GetField("rect2");

            bool doNewGUI = true;

            for (int i = 0; i < codes.Count(); ++i)
            {
                if (doNewGUI)
                {
                    if ( (codes[i]?.operand).ToStringSafe().Contains("gray") && (codes[i-1]?.operand).ToStringSafe().Contains("EndGroup"))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, rect2);
                        yield return new CodeInstruction(OpCodes.Ldloc_1);

                        yield return new CodeInstruction(OpCodes.Call, typeof(PCDialogueHeaderPatch).GetMethod("DoPCHeader", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                        yield return codes[i];
                        doNewGUI = false;
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

        static void DoPCHeader(Rect rect, float LabelWidth)
        {
            rect.y -= 24f;
            rect.height += 25;
            offset = 932 + LabelWidth;
            GUI.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(0.0f, 0.0f);
            TextAnchor anchor2 = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(string.Empty, offset);
            widgetRow.Label((string)"\nYoungest\nFirst\n", 60, (string)"Look. I never said prizing animals as companions meant NOT killing the young children...".Translate(), 48f);
            Text.Anchor = anchor2;
            GUI.color = Color.gray;
            Widgets.DrawLineVertical(offset, 0.0f, (float)((double)rect.height + 1.0));
            GUI.color = Color.white;
            GUI.EndGroup();
        }
    }

    /* BE FEARFUL
        [HarmonyPatch(typeof(Dialog_AutoSlaughter), "DoAnimalHeader")]
        internal static class PCDialogueHeaderPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                // New Locals
                var storedX = generator.DeclareLocal(typeof(float));
                var storedY = generator.DeclareLocal(typeof(float));
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

                bool doFirst = false;
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
                            yield return new CodeInstruction(OpCodes.Ldc_R4, -40.0f);
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
                            //Disregard data encapsulation, what could go wrong?
                            yield return new CodeInstruction(OpCodes.Dup); 
                            yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetProperty("FinalX").GetGetMethod());
                            yield return new CodeInstruction(OpCodes.Stloc, storedX);
                            //yield return new CodeInstruction(OpCodes.Dup);
                            yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetProperty("FinalY").GetGetMethod());

                            yield return new CodeInstruction(OpCodes.Ldc_R4, 24f);
                            yield return new CodeInstruction(OpCodes.Sub);
                            yield return new CodeInstruction(OpCodes.Stloc, storedY);
                            yield return new CodeInstruction(OpCodes.Ldloc, storedX);
                            yield return new CodeInstruction(OpCodes.Ldloc, storedY);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                            yield return new CodeInstruction(OpCodes.Ldc_R4, 99999f);
                            yield return new CodeInstruction(OpCodes.Ldc_R4, 4f);
                            yield return new CodeInstruction(OpCodes.Newobj, typeof(WidgetRow).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(Verse.UIDirection), typeof(float), typeof(float) }));

                            //widgetRow.Label((str) text, width, (str) TTip, height)
                            yield return new CodeInstruction(OpCodes.Ldstr, "Transverse\nCull");
                            yield return new CodeInstruction(OpCodes.Ldc_R4, 92f);
                            yield return new CodeInstruction(OpCodes.Ldstr, "TestTip");
                            //yield return new CodeInstruction(OpCodes.Ldc_R4, -1.0f); //defaults height
                            yield return new CodeInstruction(OpCodes.Ldc_R4, 48.0f); //double height
                            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WidgetRow), nameof(WidgetRow.Label)));
                            yield return new CodeInstruction(OpCodes.Pop);

                            //yield return new CodeInstruction(OpCodes.Ldstr, "Test3");
                            //yield return new CodeInstruction(OpCodes.Ldc_R4, 72f);
                            //yield return new CodeInstruction(OpCodes.Ldstr, "TestTip2");
                            //yield return new CodeInstruction(OpCodes.Ldc_R4, -1.0f);
                            //yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WidgetRow), nameof(WidgetRow.Label)));
                            //yield return new CodeInstruction(OpCodes.Pop);

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
        */
    [HarmonyPatch(typeof(Dialog_AutoSlaughter), "DoWindowContents")]
    internal static class PCDrawDelimiter
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            bool doDraw = true;

            for (int i = 0; i < codes.Count(); ++i)
            {
                if(doDraw)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString().Contains("End")) 
                    {
                        yield return codes[i];

                        yield return new CodeInstruction(OpCodes.Call, typeof(Color).GetProperty("gray").GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Call, typeof(GUI).GetProperty("color").GetSetMethod());

                        //yield return new CodeInstruction(OpCodes.Ldsfld, typeof(PCDialogueHeaderPatch).GetField("offset", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 994f);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.0f);
                        yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                        yield return new CodeInstruction(OpCodes.Ldflda, typeof(Dialog_AutoSlaughter).GetField("viewRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(Rect).GetProperty("height", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 6f);
                        yield return new CodeInstruction(OpCodes.Sub);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Widgets).GetMethod(nameof(Widgets.DrawLineVertical)));

                        yield return new CodeInstruction(OpCodes.Call, typeof(Color).GetProperty("white").GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Call, typeof(GUI).GetProperty("color").GetSetMethod());

                        doDraw = false;
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
            //NEW locals
            var helper = generator.DeclareLocal(typeof(bool));
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

                        /*
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
                        */
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Text), "Anchor").GetSetMethod());

                        /*Draw?     leaves 6 px gaps which can't be resolved here.
                        yield return new CodeInstruction(OpCodes.Call, typeof(Color).GetProperty("gray").GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Call, typeof(GUI).GetProperty("color").GetSetMethod());

                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, row);
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetProperty("FinalX").GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Ldc_R4, -3.0f);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 30f);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Widgets).GetMethod(nameof(Widgets.DrawLineVertical)));

                        yield return new CodeInstruction(OpCodes.Call, typeof(Color).GetProperty("white").GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Call, typeof(GUI).GetProperty("color").GetSetMethod());
                        //Draw?*/

                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, row);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 19f); //60-22 / 2
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetMethod(nameof(WidgetRow.Gap)));

                        yield return new CodeInstruction(OpCodes.Ldarg_3); //config
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PCSlaughterConfigPatch), "IsYoungestFirst"));
                        // A priori value floats around until beq in usual case!
                        yield return new CodeInstruction(OpCodes.Stloc, helper);
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldfld, row);
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(WidgetRow).GetProperty("FinalX").GetGetMethod());
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                        //yield return new CodeInstruction(OpCodes.Ldarg_3); // Config
                        //yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(AutoSlaughterConfig), "allowSlaughterBonded")); // isBonded (Address)
                        yield return new CodeInstruction(OpCodes.Ldloca, helper);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 24f);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Widgets).GetMethod("Checkbox", new Type[] { typeof(float), typeof(float), typeof(bool).MakeByRefType(), typeof(float), typeof(bool), typeof(bool), typeof(Texture2D), typeof(Texture2D) }));
                        //yield return new CodeInstruction(OpCodes.Ldarg_3);
                        //Compare against V_3 to see if changing -> Probably not most performant way to do this, but easiest to set up
                        yield return new CodeInstruction(OpCodes.Ldloc, helper);
                        yield return new CodeInstruction(OpCodes.Ldarg_3); // Config
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PCSlaughterConfigPatch), "IsYoungestFirst"));
                        yield return new CodeInstruction(OpCodes.Beq_S, codes[i].labels[0]);
                        
                        //Recalc Animals as before
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Dialog_AutoSlaughter).GetMethod("RecalculateAnimals",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                        //Also update the cull logic
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PCSlaughterConfigPatch), "InvertCullLogic"));

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
