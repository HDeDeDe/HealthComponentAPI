using System;
using System.Diagnostics.CodeAnalysis;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace HDeMods {
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
	public static partial class HealthComponentAPI {
		// Plugin details
		public const string PluginGUID = PluginAuthor + "." + PluginName;
		public const string PluginAuthor = "HDeDeDe";
		public const string PluginName = "HealthComponentAPI";
		public const string PluginVersion = "1.0.0";

		private static UpdateHealthEventArgs HealthStats;
		private static HealEventArgs HealStats;
		private static TakeDamageArgs TakeDamageStats;

		private static bool _healthHookSet = false;
		private static bool _healHooksSet = false;
		private static bool _takeDamageHooksSet = false;
		
		internal static void SetHealthHook()	{
			if (_healthHookSet) return;

			IL.RoR2.HealthComponent.ServerFixedUpdate += HealthComponent_ServerFixedUpdate;

			_healthHookSet = true;
		}
		
		internal static void UnsetHealthHook() {
			IL.RoR2.HealthComponent.ServerFixedUpdate -= HealthComponent_ServerFixedUpdate;

			_healthHookSet = false;
		}
		
		internal static void SetHealHooks() {
			if (_healHooksSet) return;

			IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
			IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess_Heal;

			_healHooksSet = true;
		}
		
		internal static void UnsetHealHooks() {
			IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
			IL.RoR2.HealthComponent.TakeDamageProcess -= HealthComponent_TakeDamageProcess_Heal;

			_healHooksSet = false;
		}
		
		internal static void SetTakeDamageHooks() {
			if (_takeDamageHooksSet) return;
            
			IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;

			_takeDamageHooksSet = true;
		}
		
		internal static void UnsetTakeDamageHooks() {
			IL.RoR2.HealthComponent.TakeDamageProcess -= HealthComponent_TakeDamageProcess;

			_takeDamageHooksSet = false;
		}
		
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


		public delegate void UpdateHealthEventHandler(HealthComponent sender, UpdateHealthEventArgs args);

		private static event UpdateHealthEventHandler _getHealthStats;
		
		public delegate void HealEventHandler(HealthComponent sender, HealEventArgs args);
		
		private static event HealEventHandler _getHealStats;
		
		public delegate void TakeDamageEventHandler(HealthComponent sender, in DamageInfo damageInfo, TakeDamageArgs args);
		
		private static event TakeDamageEventHandler _getTakeDamageStats;
		
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

		private static void GetHealthMod(HealthComponent hc) {
			HealthStats = new UpdateHealthEventArgs();

			if (_getHealthStats == null) return;
			foreach (UpdateHealthEventHandler @event in _getHealthStats.GetInvocationList()) {
				try {
					@event(hc, HealthStats);
				}
				catch (Exception e) {
					HCAPI.Log.Error($"Exception thrown by : {@event.Method.DeclaringType?.Name}.{@event.Method.Name}:\n{e}");
				}
			}
		}
		
		private static void GetHealMod(HealthComponent hc) {
			HealStats = new HealEventArgs();

			if (_getHealStats == null) return;
			foreach (HealEventHandler @event in _getHealStats.GetInvocationList()) {
				try {
					@event(hc, HealStats);
				}
				catch (Exception e) {
					HCAPI.Log.Error($"Exception thrown by : {@event.Method.DeclaringType?.Name}.{@event.Method.Name}:\n{e}");
				}
			}
		}
		
		private static void GetTakeDamageMod(HealthComponent hc, DamageInfo dm) {
			TakeDamageStats = new TakeDamageArgs();

			if (_getTakeDamageStats == null) return;
			foreach (TakeDamageEventHandler @event in _getTakeDamageStats.GetInvocationList()) {
				try {
					@event(hc, dm, TakeDamageStats);
				}
				catch (Exception e) {
					HCAPI.Log.Error($"Exception thrown by : {@event.Method.DeclaringType?.Name}.{@event.Method.Name}:\n{e}");
				}
			}
		}

		private static void HealthComponent_ServerFixedUpdate(ILContext il) {
			ILCursor c = new ILCursor(il);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<HealthComponent>>(GetHealthMod);

			RecalcFinalRegenAccumulator(c);
			RecalcBarrierDecayRate(c);
			RecalcShieldRechargeRate(c);
			RecalcAdaptiveArmorDecayRate(c);
		}
		
		private static void HealthComponent_Heal(ILContext il) {
			ILCursor c = new ILCursor(il);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<HealthComponent>>(GetHealMod);

			RecalcCoyoteTimer(c);
			RecalcCritHeal(c);
			HalveHealing(c);
			RecalcFinalHeal(c);
		}
		
		private static void HealthComponent_TakeDamageProcess_Heal(ILContext il) {
			ILCursor c = new ILCursor(il);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<HealthComponent>>(GetHealMod);

			RecalcCoyoteTimer(c);
		}
		
		private static void HealthComponent_TakeDamageProcess(ILContext il) {
			ILCursor c = new ILCursor(il);
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldarg_1);
			c.EmitDelegate<Action<HealthComponent, DamageInfo>>(GetTakeDamageMod);

			RecalcDamageForce(c);
			RejectDamageCheck(c);
			RecalcAdaptiveArmorBuildRate(c);
			RecalcAdaptiveArmorMax(c);
			RecalcFinalDamage(c);
		}
	}
}