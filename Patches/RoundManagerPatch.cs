using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FearOverhauled.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        static void PostStart()
        {
            FearOverhauledMod fearMod = new FearOverhauledMod();
            fearMod.InitConfig(FearOverhauledMod.fearLogger);
        }
    }
}
