using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    [HarmonyPatch(typeof(AutoSlaughterConfig), "ExposeData")]
    public static class PCSlaughterConfigPatch
    {
        public static readonly HashSet<AutoSlaughterConfig> youngestFirst = new HashSet<AutoSlaughterConfig>();
        public static void Postfix(AutoSlaughterConfig __instance)
        {

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (__instance.IsYoungestFirst()) Scribe.saver.WriteElement("youngestFirst", "1");
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (ScribeExtractor.ValueFromNode(Scribe.loader.curXmlParent["youngestFirst"], 0) == 1)
                {
                    __instance.SetYoungestFirst();
                }
            }
        }

        //Extension methods
        public static bool IsYoungestFirst(this AutoSlaughterConfig cfg) => youngestFirst.Contains(cfg);
        public static bool IsYoungestFirst2(this AutoSlaughterConfig cfg)
        {
            Log.Message("[PRIZED COMPANIONS] IYF Check");
            Log.Message("Checking for defname: " + cfg.animal.defName);
            Log.Message("Should return: " + youngestFirst.Contains(cfg).ToString());

            return cfg.IsYoungestFirst();
        }
        public static void SetYoungestFirst(this AutoSlaughterConfig cfg) => youngestFirst.Add(cfg);
        public static void SetOldestFirst(this AutoSlaughterConfig cfg) => youngestFirst.Remove(cfg);

        public static void InvertCullLogic(this AutoSlaughterConfig cfg)
        {
            if(cfg.IsYoungestFirst())
            {
                cfg.SetOldestFirst();
            }
            else
            {
                cfg.SetYoungestFirst();
            }
        }
    }
}
