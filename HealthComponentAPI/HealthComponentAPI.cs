using System;
using System.Diagnostics.CodeAnalysis;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine.Networking;

namespace HDeMods {
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
	public static partial class HealthComponentAPI {
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
		
		private static event UpdateHealthEventHandler _getHealthStats;
		private static event HealEventHandler _getHealStats;
		private static event TakeDamageEventHandler _getTakeDamageStats;

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

		internal static void AddOnHooks() {
			On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
			On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
		}
		
		internal static void RemoveOnHooks() {
			On.RoR2.HealthComponent.TakeDamageProcess -= HealthComponent_TakeDamageProcess;
			On.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
		}

		private static float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, 
			float amount, ProcChainMask procChainMask, bool nonRegen = true) {
			if (!NetworkServer.active) {
				UnityEngine.Debug.LogWarning("[Server] function 'System.Single RoR2.HealthComponent::Heal(System.Single, RoR2.ProcChainMask, System.Boolean)' called on client");
				return 0f;
			}
			OnHealServerProcess?.Invoke(self, amount, procChainMask, nonRegen);
			return orig(self, amount, procChainMask, nonRegen);
		}

		private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, 
			HealthComponent self, DamageInfo damageInfo) {
			OnTakeDamageProcess?.Invoke(self, damageInfo);
			orig(self, damageInfo);
		}
	}
}