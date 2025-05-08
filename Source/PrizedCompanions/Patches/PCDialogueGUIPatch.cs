using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;


namespace Prized_Companions
{
    // Alter the width of the firtst column label
    // Originally- calculated from the window rect
    // Now subtract the same amount as added to include new row!
    internal static class PCLabelWidth
    {
        private static void Postfix(ref float __result)
        {
            __result -= 68;
            //By default, returns labelWidth back to 62 px
        }
    }

    // Make UI window larger to include space for new column
    // Originally- Set value
    // Now add space for new column- same amount must adjust initial label size!
    internal static class PCDialogueResize
    {
        private static void Postfix(ref Vector2 __result)
        {
            __result.x += 68;
            __result.y += 42; // Try and respect Phi.
                              // Bonus: Get to pretend you have a sense of design. 
        }
    }

    internal static class PCDialogueHeaderPatch
    {
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
            rect.x += rect.width - 60;
            GUI.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(0.0f, 0.0f);
            TextAnchor anchor2 = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label((string)"\nYoungest\nFirst\n", 60, (string)"Prized_Companions_GUI_TTip".Translate(), 48f);
            Text.Anchor = anchor2;
            GUI.EndGroup();
        }
    }

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
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("EndScrollView")) 
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(Dialog_AutoSlaughter).GetField("viewRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Call, typeof(PCDrawDelimiter).GetMethod("Injection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));

                        yield return codes[i];

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

        static void Injection(Rect ___viewRect)
        {
            GUI.color = Color.gray;
            Widgets.DrawLineVertical(___viewRect.xMin+___viewRect.width - 72, 0, ___viewRect.height - 8f);
            GUI.color = Color.white;
        }
    }

    internal static class PCDialogueRowPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //NEW locals
            var helper = generator.DeclareLocal(typeof(bool));
            //Fields
            System.Reflection.FieldInfo bonded = Type.GetType("RimWorld.Dialog_AutoSlaughter+AnimalCountRecord, Assembly-CSharp").GetField("bonded", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            //Is this a sign I've gone too far?
            System.Reflection.FieldInfo row =Type.GetType("RimWorld.Dialog_AutoSlaughter+<>c__DisplayClass25_1, Assembly-CSharp").GetField("row");

            bool doStaleUpdate = true;
            bool doNewColumnElement = true;
            bool done = false;

            for (int i = 0; i < codes.Count(); ++i)
            {
                if (done)
                {
                    yield return codes[i];
                    continue;
                }
                //Update when slaughtering pregnant animals is toggled
                else if (doStaleUpdate)
                {
                    if(codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("RecalculateAnimals"))
                    {
                        yield return codes[i]; //Call RecalculateAnimals

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(Dialog_AutoSlaughter).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(Map).GetField("autoSlaughterManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(AutoSlaughterManager).GetMethod("Notify_ConfigChanged", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
                        doStaleUpdate = false;
                        continue;
                    }
                    else
                    {
                        yield return codes[i];
                        continue;
                    }
                }
                else if (doNewColumnElement)
                {
                    if (codes[i+4].opcode == OpCodes.Ret)
                    {
                        //Last Branch needs to be redirected
                        Label newLabel = generator.DefineLabel();
                        //Branch if no change
                        yield return new CodeInstruction(codes[i].opcode, newLabel);
                        ++i;
                        //ldarg_0
                        yield return codes[i];
                        ++i;
                        //Call Recalculate Animals
                        yield return codes[i];
                        ++i;
                        //Update when slaughtering Bonded animals is toggled
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(Dialog_AutoSlaughter).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(Map).GetField("autoSlaughterManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Callvirt, typeof(AutoSlaughterManager).GetMethod("Notify_ConfigChanged", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));

                        //New Check GUI Element
                        CodeInstruction jumpTarg = new CodeInstruction(OpCodes.Ldloca, 4);
                        jumpTarg.labels.Add(newLabel);
                        yield return jumpTarg;
                        yield return new CodeInstruction(OpCodes.Ldflda, row);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldflda, typeof(Dialog_AutoSlaughter).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Call, typeof(PCDialogueRowPatch).GetMethod("PCRowElement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
                        yield return new CodeInstruction(OpCodes.Brfalse, codes[i].labels[0]);

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, typeof(Dialog_AutoSlaughter).GetMethod("RecalculateAnimals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));

                        yield return codes[i]; // EndGUI Group


                        doNewColumnElement = false;
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

        static bool PCRowElement(ref WidgetRow row, AutoSlaughterConfig config, ref Map map)
        {
            row.Gap(52f);
            Text.Anchor = TextAnchor.UpperLeft;
            row.Gap(19f);//60-22 / 2
            bool isYoungestFirst = config.IsYoungestFirst();
            bool wasYoungestFirst = isYoungestFirst;
            Widgets.Checkbox(row.FinalX, 0.0f, ref isYoungestFirst, paintable: true);
            if (isYoungestFirst ^ wasYoungestFirst)
            {
                config.InvertCullLogic();
                map.autoSlaughterManager.Notify_ConfigChanged();
                return true;
            }
            else return false;
        }
    }
}
