using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;

namespace Prized_Companions
{
    [StaticConstructorOnStartup]
    public static class PrizedCompanionsMain
    {
        static PrizedCompanionsMain() => new Harmony("Prized Companions").PatchAll();
    }
}