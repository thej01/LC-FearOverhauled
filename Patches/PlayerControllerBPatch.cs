using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FearOverhauled.Logger;

namespace FearOverhauled.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        static string fearState = "calm";
        static float logTimer = 0;
        static float messageTimer = 0;
        static bool playedWarningSound = false;
        static bool showAfterPanicAttackMessage = false;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void PostUpdate(ref StartOfRound ___playersManager, ref float ___movementSpeed, ref float ___sprintTime, 
                               ref float ___climbSpeed, ref float ___sprintMeter, ref bool ___isSprinting,
                               ref Animator ___playerBodyAnimator, ref float ___limpMultiplier, ref bool ___isPlayerDead)
        {

            if (___playersManager.fearLevel < 0)
                ___playersManager.fearLevel = 0;

            float fear = ___playersManager.fearLevel;
            float staminaRegen = 0;
            bool dontLimp = false;

            FearOverhauledMod.StressedProperties stressedProperties = new FearOverhauledMod.StressedProperties();

            if (fear >= stressedProperties.fearNeeded)
            {
                FearOverhauledMod.IFearProperties fearStateProperties = FearOverhauledMod.GetPropertyClassFromName(fearState);

                fearState = FearOverhauledMod.GetFearString(fear, fearStateProperties.fearNeeded);

                fearStateProperties = FearOverhauledMod.GetPropertyClassFromName(fearState);

                // Stat changes
                if (fearState != "calm")
                {
                    if (FearOverhauledMod.ConfigValues.doSpeed)
                        ___movementSpeed = FearOverhauledMod.VanillaValues.speed + fearStateProperties.speed;

                    if (FearOverhauledMod.ConfigValues.doSprintTime)
                        ___sprintTime = FearOverhauledMod.VanillaValues.sprintTime + fearStateProperties.sprintTime;

                    if (FearOverhauledMod.ConfigValues.doClimbSpeed)
                        ___climbSpeed = FearOverhauledMod.VanillaValues.climbSpeed + fearStateProperties.climbSpeed;

                    if (FearOverhauledMod.ConfigValues.doAdrenaline)
                        dontLimp = fearStateProperties.dontLimp;

                    staminaRegen = FearOverhauledMod.VanillaValues.staminaRegen + fearStateProperties.staminaRegen;
                }
            }
            else
            {
                // Set trigger for post-panic attack message
                if (fearState == "panicAttack")
                {
                    showAfterPanicAttackMessage = true;
                    playedWarningSound = false;
                    messageTimer = 0;
                }

                // Disable status effects
                if (!CheckIfStatsAreVanilla(___movementSpeed, ___sprintTime, ___climbSpeed, staminaRegen, dontLimp, ___limpMultiplier))
                {
                    if (FearOverhauledMod.ConfigValues.doSpeed)
                        ___movementSpeed = FearOverhauledMod.VanillaValues.speed;

                    if (FearOverhauledMod.ConfigValues.doSprintTime)
                        ___sprintTime = FearOverhauledMod.VanillaValues.sprintTime;

                    if (FearOverhauledMod.ConfigValues.doClimbSpeed)
                        ___climbSpeed = FearOverhauledMod.VanillaValues.climbSpeed;

                    if (FearOverhauledMod.ConfigValues.doAdrenaline)
                    {
                        ___limpMultiplier = FearOverhauledMod.VanillaValues.limpMultiplier;
                        dontLimp = FearOverhauledMod.VanillaValues.dontLimp;
                    }
                    staminaRegen = FearOverhauledMod.VanillaValues.staminaRegen;
                }


                fearState = "calm";
            }

            // Add stamina if not moving
            if (!___isSprinting)
                ___sprintMeter += (staminaRegen * Time.deltaTime);

            // Refuse to limp
            if (dontLimp)
            {
                ___playerBodyAnimator.SetBool("Limp", false);
                ___limpMultiplier = 1f;
            }


            // Displays not-so reassuring message during a panic attack
            if (!___isPlayerDead)
            {
                if (fearState == "panicAttack")
                {
                    showAfterPanicAttackMessage = false;
                    if (!playedWarningSound)
                    {
                        RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, FearOverhauledMod.Sounds.panicAttackWarning, false, 1f, 0);
                        playedWarningSound = true;
                        messageTimer = 0;
                    }

                    if (messageTimer <= 30)
                    {
                        FearOverhauledMod.Lang.ILangProperties lang = FearOverhauledMod.Lang.GetLangClassFromEnum(FearOverhauledMod.ConfigValues.lang);

                        HUDManager.Instance.DisplayStatusEffect(lang.panicAttackWarningMSG);
                    }

                }

                // Show reassuring message after having a panic attack
                if (showAfterPanicAttackMessage)
                {
                    if (!playedWarningSound)
                    {
                        HUDManager.Instance.UIAudio.PlayOneShot(FearOverhauledMod.Sounds.panicAttackRecovery);
                        playedWarningSound = true;
                        messageTimer = 0;
                    }

                    if (messageTimer <= 30)
                    {
                        FearOverhauledMod.Lang.ILangProperties lang = FearOverhauledMod.Lang.GetLangClassFromEnum(FearOverhauledMod.ConfigValues.lang);

                        HUDManager.Instance.DisplayStatusEffect(lang.panicAttackRecoveryMSG);
                    }
                    else
                    {
                        playedWarningSound = false;
                        showAfterPanicAttackMessage = false;
                    }
                }
            }
            else
            {
                messageTimer = 999;
                showAfterPanicAttackMessage = false;
            }

            messageTimer += Time.deltaTime;


            logTimer += Time.deltaTime;
            if (logTimer > 5)
            {
                logTimer = 0;

                string formatted = string.Format("Current Fear State: {0} (Cur Fear {1})", fearState, fear);
                FearOverhauledMod.fearLogger.Log(formatted, LogLevel.Info, Logger.LogLevelConfig.Everything);

                formatted = string.Format("Current Speed: {0})", ___movementSpeed);
                FearOverhauledMod.fearLogger.Log(formatted, LogLevel.Info, Logger.LogLevelConfig.Everything);

                formatted = string.Format("Current Sprint Time: {0})", ___sprintTime);
                FearOverhauledMod.fearLogger.Log(formatted, LogLevel.Info, Logger.LogLevelConfig.Everything);

                formatted = string.Format("Current ClimbSpeed: {0})", ___climbSpeed);
                FearOverhauledMod.fearLogger.Log(formatted, LogLevel.Info, Logger.LogLevelConfig.Everything);

                formatted = string.Format("Current StaminaRegen: {0})", staminaRegen);
                FearOverhauledMod.fearLogger.Log(formatted, LogLevel.Info, Logger.LogLevelConfig.Everything);

                formatted = string.Format("Current dontLimp: {0}", dontLimp);
                FearOverhauledMod.fearLogger.Log(formatted, LogLevel.Info, Logger.LogLevelConfig.Everything);
            }

        }

        static bool CheckIfStatsAreVanilla(float movementSpeed, float sprintTime, float climbSpeed, float staminaRegen, bool dontLimp, float limpMultiplier)
        {
            if (movementSpeed != FearOverhauledMod.VanillaValues.speed && FearOverhauledMod.ConfigValues.doSpeed)
                return false;

            if (sprintTime != FearOverhauledMod.VanillaValues.sprintTime && FearOverhauledMod.ConfigValues.doSprintTime)
                return false;

            if (climbSpeed != FearOverhauledMod.VanillaValues.climbSpeed && FearOverhauledMod.ConfigValues.doClimbSpeed)
                return false;

            if (limpMultiplier != FearOverhauledMod.VanillaValues.limpMultiplier && FearOverhauledMod.ConfigValues.doAdrenaline)
                return false;

            if (dontLimp != FearOverhauledMod.VanillaValues.dontLimp && FearOverhauledMod.ConfigValues.doAdrenaline)
                return false;

            if (staminaRegen != FearOverhauledMod.VanillaValues.staminaRegen)
                return false;

            return true;
        }
    }
}
