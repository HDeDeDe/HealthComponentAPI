using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace HDeMods {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    public static partial class HealthComponentAPI {
        // Plugin details
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "HDeDeDe";
        public const string PluginName = "HealthComponentAPI";
        public const string PluginVersion = "1.0.0";
        
        public class UpdateHealthEventArgs : EventArgs {
            public float finalRegenMultAdd = 0f;
            public float finalRegenFlatAdd = 0f;

            public float barrierDecayRateMultAdd = 0f;
            public float barrierDecayRateFlatAdd = 0f;

            public float shieldRechargeRateMultAdd = 0f;
            public float shieldRechargeRateFlatAdd = 0f;
			
            public float adaptiveArmorDecayRateMultAdd = 0f;
            public float adaptiveArmorDecayRateFlatAdd = 0f;

        }
		
        public class HealEventArgs : EventArgs {
            public bool enableEclipseHealReduction = false;

            public float damageCoyoteTimerMultAdd = 0f;
            public float damageCoyoteTimerFlatAdd = 0f;

            public float critHealMultAdd = 0f;
            public float critHealFlatAdd = 0f;

            public float finalHealAmountMultAdd = 0f;
            public float finalHealAmountFlatAdd = 0f;
        }
        
        public class TakeDamageArgs : EventArgs {
            public bool rejectDamage = false;
            public bool rejectForce = false;

            public float adaptiveArmorBuildRateMultAdd = 0f;
            public float adaptiveArmorBuildRateFlatAdd = 0f;
			
            public float adaptiveArmorMaxMultAdd = 0f;
            public float adaptiveArmorMaxFlatAdd = 0f;

            public float finalDamageAmountMultAdd = 0f;
            public float finalDamageAmountFlatAdd = 0f;

            public float damageForceMultAdd = 0f;
            public Vector3 damageForceFlatAdd = Vector3.zero;
			
        }
        
        public static event UpdateHealthEventHandler GetHealthStats {
            add {
                SetHealthHook();
                _getHealthStats += value;
            }
            remove {
                _getHealthStats -= value;
                if(_getHealthStats == null || _getHealthStats.GetInvocationList()?.Length == 0) UnsetHealthHook();
            }
        }
		
        public static event HealEventHandler GetHealStats {
            add {
                SetHealHooks();
                _getHealStats += value;
            }
            remove {
                _getHealStats -= value;
                if(_getHealStats == null || _getHealStats.GetInvocationList()?.Length == 0) UnsetHealHooks();
            }
        }
		
        public static event TakeDamageEventHandler GetTakeDamageStats {
            add {
                SetTakeDamageHooks();
                _getTakeDamageStats += value;
            }
            remove {
                _getTakeDamageStats -= value;
                if(_getTakeDamageStats == null || _getTakeDamageStats.GetInvocationList()?.Length == 0) UnsetTakeDamageHooks();
            }
        }
    }
}