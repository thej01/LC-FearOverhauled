using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using FearOverhauled.Patches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FearOverhauled.FearOverhauledMod;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;
using static FearOverhauled.Logger;

namespace FearOverhauled
{

    public class Logger
    {
        internal ManualLogSource MLS;

        public string modName = "No-Name";
        public string modVersion = "No-Ver";

        public enum LogLevelConfig
        {
            None,
            Important,
            Everything
        }

        public void Init(string modGUID = "")
        {
            MLS = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        }

        public bool LogLevelAllow(LogLevelConfig severity = LogLevelConfig.Important, LogLevelConfig severity2 = LogLevelConfig.Everything)
        {
            if (severity2 == LogLevelConfig.None)
                return false;

            if (severity == LogLevelConfig.Everything)
            {
                return severity2 == LogLevelConfig.Everything;
            }

            return true;
        }

        public void Log(string text = "",  LogLevel level = LogLevel.Info, LogLevelConfig severity = LogLevelConfig.Important)
        {
            bool allowed = ConfigValues.logLevel == null;
            if (!allowed)
            {
                allowed = LogLevelAllow(severity, ConfigValues.logLevel);
            }

            if (allowed)
            {
                string resultText = string.Format("[{0} v{1}] - {2}", modName, modVersion, text);
                MLS.Log(level, resultText);
            }
        }
    }

    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    public class FearOverhauledMod : BaseUnityPlugin
    {
        private const string modGUID = "thej01.lc.FearOverhauled";
        private const string modName = "Fear Overhauled";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static FearOverhauledMod Instance;

        public static Logger fearLogger = new Logger();

        public void InitConfigValues(Logger fearLogger)
        {
            fearLogger.Log("Initialising config values...");

            ConfigValues.doSpeed = ConfigBinds.doSpeed.Value;
            ConfigValues.doSprintTime = ConfigBinds.doSprintTime.Value;
            ConfigValues.doClimbSpeed = ConfigBinds.doClimbSpeed.Value;
            ConfigValues.doAdrenaline = ConfigBinds.doAdrenaline.Value;
            ConfigValues.lang = ConfigBinds.lang.Value;
            ConfigValues.logLevel = ConfigBinds.logLevel.Value;

            fearLogger.Log("Config values initialised.");
        }

        public void InitAllConfigBinds(Logger fearLogger)
        {
            fearLogger.Log("Initialising config binds...", LogLevel.Message, LogLevelConfig.Important);

            string logMsg = "";

            ConfigBinds.doSpeed = Config.Bind<bool>("General", "Speed changes", true, "Determines if your speed will increase while scared");
            logMsg = string.Format("doSpeed initialised. (Value: {0})", ConfigBinds.doSpeed.Value);
            fearLogger.Log(logMsg, LogLevel.Message, LogLevelConfig.Important);

            ConfigBinds.doSprintTime = Config.Bind<bool>("General", "Stamina changes", true, "Determines if your stamina will increase while scared");
            logMsg = string.Format("doSprintTime initialised. (Value: {0})", ConfigBinds.doSprintTime.Value);
            fearLogger.Log(logMsg, LogLevel.Message, LogLevelConfig.Important);

            ConfigBinds.doClimbSpeed = Config.Bind<bool>("General", "Climb speed changes", true, "Determines if your climb speed will increase while scared");
            logMsg = string.Format("doClimbSpeed initialised. (Value: {0})", ConfigBinds.doClimbSpeed.Value);
            fearLogger.Log(logMsg, LogLevel.Message, LogLevelConfig.Important);

            ConfigBinds.doAdrenaline = Config.Bind<bool>("General", "Adrenaline", true, "Determines if you won't limp while scared");
            logMsg = string.Format("doAdrenaline initialised. (Value: {0})", ConfigBinds.doAdrenaline.Value);
            fearLogger.Log(logMsg, LogLevel.Message, LogLevelConfig.Important);

            fearLogger.Log("Initialising LethalConfig items...");

            ConfigBinds.lang = Config.Bind("Misc", "Language", Lang.AvailableLang.En, "Doesn't affect config");

            var langDropdown = new EnumDropDownConfigItem<Lang.AvailableLang>(ConfigBinds.lang, new EnumDropDownOptions
            {
                RequiresRestart = false
            });

            LethalConfigManager.AddConfigItem(langDropdown);

            ConfigBinds.logLevel = Config.Bind("Misc", "Log Level", LogLevelConfig.Important, "Console logging level");

            var debugDropdown = new EnumDropDownConfigItem<Logger.LogLevelConfig>(ConfigBinds.logLevel, new EnumDropDownOptions
            {
                RequiresRestart = false
            });

            LethalConfigManager.AddConfigItem(debugDropdown);

            fearLogger.Log("LethalConfig items initialised.", LogLevel.Message, LogLevelConfig.Important);

            fearLogger.Log("Config binds initialised.", LogLevel.Message, LogLevelConfig.Important);
        }

        public void InitConfig(Logger fearLogger)
        {
            fearLogger.Log("Initialising config...", LogLevel.Message, LogLevelConfig.Important);
            InitAllConfigBinds(fearLogger);
            InitConfigValues(fearLogger);
            fearLogger.Log("Config initialised.", LogLevel.Message, LogLevelConfig.Important);
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            fearLogger.Init(modGUID);

            fearLogger.modName = modName;
            fearLogger.modVersion = modVersion;

            fearLogger.Log("fearLogger Initialised!", LogLevel.Info, LogLevelConfig.Everything);

            InitConfig(fearLogger);

            fearLogger.Log("Patching FearOverhauledMod...", LogLevel.Info, LogLevelConfig.Everything);
            harmony.PatchAll(typeof(FearOverhauledMod));
            fearLogger.Log("Patched FearOverhauledMod.", LogLevel.Info, LogLevelConfig.Everything);

            fearLogger.Log("Patching PlayerControllerBPatch...", LogLevel.Info, LogLevelConfig.Everything);
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            fearLogger.Log("Patched PlayerControllerBPatch.", LogLevel.Info, LogLevelConfig.Everything);

            fearLogger.Log("Patching RoundManagerPatch...", LogLevel.Info, LogLevelConfig.Everything);
            harmony.PatchAll(typeof(RoundManagerPatch));
            fearLogger.Log("Patched RoundManagerPatch.", LogLevel.Info, LogLevelConfig.Everything);
        }

        public static class ConfigValues
        {
            public static bool doSpeed;
            public static bool doSprintTime;
            public static bool doClimbSpeed;
            public static bool doAdrenaline;
            public static Lang.AvailableLang lang;
            public static Logger.LogLevelConfig logLevel;
        }

        public static class ConfigBinds
        {
            public static ConfigEntry<bool> doSpeed;
            public static ConfigEntry<bool> doSprintTime;
            public static ConfigEntry<bool> doClimbSpeed;
            public static ConfigEntry<bool> doAdrenaline;
            public static ConfigEntry<Lang.AvailableLang> lang;
            public static ConfigEntry<Logger.LogLevelConfig> logLevel;
        }

        public static class Sounds
        {
            public static AudioClip[] panicAttackWarning = HUDManager.Instance.warningSFX;
            public static AudioClip panicAttackRecovery = HUDManager.Instance.reachedQuotaSFX;
        }

        public static class VanillaValues
        {
            // if your mod changes any of these (regen, and dont limp excluded), 
            // please change them to the values in your mod to avoid incompatibility issues.

            public static float speed = 4.6f;
            public static float sprintTime = 11f;
            public static float climbSpeed = 3f;
            public static float limpMultiplier = 0.2f;
            public static float staminaRegen = 0f;
            public static bool dontLimp = false;
        }

        public interface IFearProperties
        {
            // fear needed to reach state
            float fearNeeded { get; }

            // player speed, adds to vanilla speed value
            float speed { get; }

            // stamina drain and regen speed, adds to vanilla value
            float sprintTime { get; }

            // staminaRegen is added to stamina wheel every second
            float staminaRegen { get; }

            // ladded climb speed, adds to vanilla value
            float climbSpeed { get; }

            // if enabled, the player wont limp from critical injury until calm again
            // they will still bleed out, though.
            bool dontLimp {  get; }
        }
        
        // various properties for each fear stage

        public class StressedProperties : IFearProperties
        {
            public float fearNeeded { get; } = 0.01f;
            public float speed { get; } = 0.25f;
            public float sprintTime { get; } = 1f;
            public float staminaRegen { get; } = -0.005f;
            public float climbSpeed { get; } = 1f;
            public bool dontLimp { get; } = false;
        }

        public class ScaredProperties : IFearProperties
        {
            public float fearNeeded { get; } = 0.25f;
            public float speed { get; } = 0.6f;
            public float sprintTime { get; } = 2f;
            public float staminaRegen { get; } = -0.010f;
            public float climbSpeed { get; } = 2f;
            public bool dontLimp { get; } = false;
        }

        public class HorrifiedProperties : IFearProperties
        {
            public float fearNeeded { get; } = 0.50f;
            public float speed { get; } = 0.9f;
            public float sprintTime { get; } = 4f;
            public float staminaRegen { get; } = -0.025f;
            public float climbSpeed { get; } = 3f;
            public bool dontLimp { get; } = true;
        }

        public class PanicAttackProperties : IFearProperties
        {
            public float fearNeeded { get; } = 0.85f;
            public float speed { get; } = 1.1f;
            public float sprintTime { get; } = 6f;
            public float staminaRegen { get; } = -0.0425f;
            public float climbSpeed { get; } = 4f; 
            public bool dontLimp { get; } = true;
        }


        public static IFearProperties GetPropertyClassFromName(string property = "")
        {
            switch (property)
            {
                case "scared":
                    return new ScaredProperties();
                case "horrified":
                    return new HorrifiedProperties();
                case "panicAttack":
                    return new PanicAttackProperties();
                default:
                    return new StressedProperties();
            }
        }

        public static string GetFearString(float fear = 0f, float minFear = 0)
        {
            PanicAttackProperties panicAttack = new PanicAttackProperties();
            float tmpFearNeeded = panicAttack.fearNeeded;
            if (fear >= tmpFearNeeded || minFear >= tmpFearNeeded)
                return "panicAttack";

            HorrifiedProperties horrifiedProperties = new HorrifiedProperties();
            tmpFearNeeded = horrifiedProperties.fearNeeded;

            if (fear >= tmpFearNeeded || minFear >= tmpFearNeeded)
                return "horrified";

            ScaredProperties scaredProperties = new ScaredProperties();
            tmpFearNeeded = scaredProperties.fearNeeded;

            if (fear >= tmpFearNeeded || minFear >= tmpFearNeeded)
                return "scared";

            StressedProperties stressed = new StressedProperties();
            tmpFearNeeded = stressed.fearNeeded;

            if (fear >= tmpFearNeeded || minFear >= tmpFearNeeded)
                return "stressed";

            return "calm";
        }

        public static class Lang
        {
            public enum AvailableLang
            {
                En,
                De,
                Rus
            }

            public static AvailableLang curLang = AvailableLang.En;

            public interface ILangProperties
            {
                string panicAttackWarningMSG { get; }
                string panicAttackRecoveryMSG { get; }
            }

            public class EN : ILangProperties
            {
                public string panicAttackWarningMSG { get; } = "WARNING: High heartbeat detected. Find a place to rest ASAP.";
                public string panicAttackRecoveryMSG { get; } = "INFO: Heartrate slowing down to normal levels.";
            }

            public class DE : ILangProperties
            {
                public string panicAttackWarningMSG { get; } = "WARNUNG: Hoher Herzschlag wurde festgestellt. Suchen Sie sich jetzt einen Platz zum Ausruhen.";
                public string panicAttackRecoveryMSG { get; } = "INFO: Die Herzfrequenz kehrt zur normalen Geschwindigkeit zurück.";
            }

            public class RUS : ILangProperties
            {
                public string panicAttackWarningMSG { get; } = "ПРЕДУПРЕЖДЕНИЕ: Обнаружено учащенное сердцебиение. Выздоравливайте скорее.";
                public string panicAttackRecoveryMSG { get; } = "Информация: Пульс возвращается к нормальной скорости.";
            }

            public static ILangProperties GetLangClassFromEnum(AvailableLang langEnum = AvailableLang.En)
            {
                ILangProperties lang;
                switch (langEnum)
                {
                    case AvailableLang.De:
                        lang = new DE();
                        break;
                    case AvailableLang.Rus:
                        lang = new RUS();
                        break;
                    default:
                        lang = new EN();
                        break;
                }
                return lang;
            }

        }
    }
}
