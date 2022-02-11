using System.Collections.Generic;

using Verse;

namespace Prized_Companions
{
    public static class PCSlaughterConfigPatch
    {
        public static readonly HashSet<AutoSlaughterConfig> youngestFirst = new HashSet<AutoSlaughterConfig>();
        private static void Postfix(AutoSlaughterConfig __instance)
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
