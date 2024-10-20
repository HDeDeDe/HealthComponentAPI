using System;
using System.ComponentModel;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace HDeMods {
	public static class HealthComponentAPI {
		// Plugin details
		public const string PluginGUID = PluginAuthor + "." + PluginName;
		public const string PluginAuthor = "HDeDeDe";
		public const string PluginName = "HealthComponentAPI";
		public const string PluginVersion = "0.2.0";

		private static UpdateHealthEventArgs HealthStats;
		private static HealEventArgs HealStats;

		private static bool _healthHookSet = false;
		private static bool _healHooksSet = false;
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
			IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;

			_healHooksSet = true;
		}
		
		internal static void UnsetHealHooks() {
			IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
			IL.RoR2.HealthComponent.TakeDamageProcess -= HealthComponent_TakeDamageProcess;

			_healHooksSet = false;
		}
		
		public class UpdateHealthEventArgs : EventArgs {
			public float TOTALregenMultAdd = 0f;
			public float TOTALregenFlatAdd = 0f;

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

			public float TOTALhealAmountMultAdd = 0f;
			public float TOTALhealAmountFlatAdd = 0f;
		}


		public delegate void UpdateHealthEventHandler(HealthComponent sender, UpdateHealthEventArgs args);

		private static event UpdateHealthEventHandler _getHealthStats;
		
		public delegate void HealEventHandler(HealthComponent sender, HealEventArgs args);
		
		private static event HealEventHandler _getHealStats;
		
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

		private static void GetHealthMod(HealthComponent hc) {
			HealthStats = new UpdateHealthEventArgs();

			if (_getHealthStats == null) return;
			foreach (UpdateHealthEventHandler @event in _getHealthStats.GetInvocationList()) {
				try {
					@event(hc, HealthStats);
				}
				catch (Exception e) {
					Log.Error($"Exception thrown by : {@event.Method.DeclaringType.Name}.{@event.Method.Name}:\n{e}");
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
					Log.Error($"Exception thrown by : {@event.Method.DeclaringType.Name}.{@event.Method.Name}:\n{e}");
				}
			}
		}

		private static void HealthComponent_ServerFixedUpdate(ILContext il) {
			ILCursor c = new ILCursor(il);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<HealthComponent>>(GetHealthMod);

			RecalcTOTALRegenAccumulator(c);
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
			RecalcTOTALHeal(c);
		}
		
		private static void HealthComponent_TakeDamageProcess(ILContext il) {
			ILCursor c = new ILCursor(il);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<HealthComponent>>(GetHealMod);

			RecalcCoyoteTimer(c);
		}

		private static void RecalcCoyoteTimer(ILCursor c) {
			if (!c.TryGotoNext(
				moveType: MoveType.Before,
                x => x.MatchStfld<HealthComponent>("recentlyTookDamageCoyoteTimer")
			    )) {
				Log.Fatal("Failed to hook Coyote Timer!");
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((consume) => 
				RefVal.damageCoyoteTimer * (1f + HealStats.damageCoyoteTimerMultAdd) + HealStats.damageCoyoteTimerFlatAdd);
		}

		private static void RecalcCritHeal(ILCursor c) {
			if (!c.TryGotoNext(
				    x => x.MatchLdloc(1),
				    x=> x.MatchBrfalse(out _),
				    x => x.MatchLdarg(1),
				    x => x.MatchLdcR4(out _),
				    // Inserting Here
				    x => x.MatchMul()
			    )) {
				Log.Fatal("Failed to hook Crit Heal!");
				return;
			}
			c.Index += 4;
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((consume) => 
				RefVal.critHealMultiplier * (1f + HealStats.critHealMultAdd) + HealStats.critHealFlatAdd);
		}

		private static void HalveHealing(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<Run>("get_selectedDifficulty")
			    )) {
				Log.Fatal("Failed to hook Eclipse 5 healing!");
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<int, int>>(consume => {
				if (!HealStats.enableEclipseHealReduction) return consume;
				return (int)RefVal.e5;
			});
		}

		private static void RecalcTOTALHeal(ILCursor c) {
			if (!c.TryGotoNext(
				    x => x.MatchLdarg(1),
				    x => x.MatchLdloc(4),
				    x => x.MatchMul(),
				    x => x.MatchStarg(1),
				    x => x.MatchLdarg(1),
				    // Inserting here
				    x => x.MatchStloc(2)
			    )) {
				Log.Fatal("Failed to hook TOTAL Healing!");
				return;
			}
			c.Index += 5;
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((TOTALHeal) => 
				TOTALHeal * (1f + HealStats.TOTALhealAmountMultAdd) + HealStats.TOTALhealAmountFlatAdd);
			c.Emit(OpCodes.Starg, 1);
			c.Emit(OpCodes.Ldarg_1);
		}

		private static void RecalcTOTALRegenAccumulator(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<CharacterBody>("get_regen")
			    )) {
				Log.Fatal("Failed to hook Regen Accumulator!");
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((regen) => 
				regen * (1f + HealthStats.TOTALregenMultAdd) + HealthStats.TOTALregenFlatAdd);
		}
		
		private static void RecalcBarrierDecayRate(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<CharacterBody>("get_barrierDecayRate")
			    )) {
				Log.Fatal("Failed to hook Barrier Decay Rate!");
				return;
			}
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, HealthComponent, float>>((consume, hc) => hc.body.maxBarrier / (RefVal.barrierDecayRate * (1f + HealthStats.barrierDecayRateMultAdd) + HealthStats.barrierDecayRateFlatAdd));
		}

		private static void RecalcShieldRechargeRate(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<CharacterBody>("get_maxShield"),
				    x => x.MatchLdcR4(out _)
			    )) {
				Log.Fatal("Failed to hook Shield Recharge Rate!");
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((consume) => 
				RefVal.shieldRechargeRate * (1 + HealthStats.shieldRechargeRateMultAdd) + HealthStats.shieldRechargeRateFlatAdd);
		}

		private static void RecalcAdaptiveArmorDecayRate(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
					x => x.MatchLdfld<HealthComponent>("adaptiveArmorValue"),
				    x => x.MatchLdcR4(out _)
			    )) {
				Log.Fatal("Failed to hook Adaptive Armor Decay Rate!");
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((consume) => 
				RefVal.adaptiveArmorDecayRate * (1 + HealthStats.adaptiveArmorDecayRateMultAdd) + HealthStats.adaptiveArmorDecayRateFlatAdd);
		}
	}
}