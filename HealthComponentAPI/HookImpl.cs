using System.Diagnostics.CodeAnalysis;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace HDeMods {
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static partial class HealthComponentAPI {
		private static void RecalcCoyoteTimer(ILCursor c) {
			if (!c.TryGotoNext(
				moveType: MoveType.Before,
                x => x.MatchStfld<HealthComponent>("recentlyTookDamageCoyoteTimer")
			    )) {
				HCAPI.Log.Fatal("Failed to hook Coyote Timer!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((damageCoyoteTimer) => 
				damageCoyoteTimer * (1f + HealStats.damageCoyoteTimerMultAdd) + HealStats.damageCoyoteTimerFlatAdd);
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
				HCAPI.Log.Fatal("Failed to hook Crit Heal!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.Index += 4;
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((critHealMultiplier) => 
				critHealMultiplier * (1f + HealStats.critHealMultAdd) + HealStats.critHealFlatAdd);
		}

		private static void HalveHealing(ILCursor c) {
			if (c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<TeamComponent>("get_teamIndex")
			    )) {
				c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<sbyte, sbyte>>(teamIndex => {
					if (!HealStats.enableEclipseHealReduction) return teamIndex;
					return (sbyte)RefVal.plr;
				});
			} else {
				HCAPI.Log.Error("Failed to hook teamIndex! Attempting healing hook.");
				HCAPI.Log.Error(c.Context);
			}
			
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<Run>("get_selectedDifficulty")
			    )) {
				HCAPI.Log.Fatal("Failed to hook Eclipse 5 healing!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<int, int>>(difficulty => {
				if (!HealStats.enableEclipseHealReduction) return difficulty;
				return (int)RefVal.e5;
			});
		}

		private static void RecalcFinalHeal(ILCursor c) {
			if (!c.TryGotoNext(
				    x => x.MatchLdarg(1),
				    x => x.MatchLdloc(4),
				    x => x.MatchMul(),
				    x => x.MatchStarg(1),
				    x => x.MatchLdarg(1),
				    // Inserting here
				    x => x.MatchStloc(2)
			    )) {
				HCAPI.Log.Fatal("Failed to hook Final Healing!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.Index += 5;
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((TOTALHeal) => 
				TOTALHeal * (1f + HealStats.finalHealAmountMultAdd) + HealStats.finalHealAmountFlatAdd);
			c.Emit(OpCodes.Starg, 1);
			c.Emit(OpCodes.Ldarg_1);
		}

		private static void RecalcFinalRegenAccumulator(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<CharacterBody>("get_regen")
			    )) {
				HCAPI.Log.Fatal("Failed to hook Regen Accumulator!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((regen) => 
				regen * (1f + HealthStats.finalRegenMultAdd) + HealthStats.finalRegenFlatAdd);
		}
		
		private static void RecalcBarrierDecayRate(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<CharacterBody>("get_barrierDecayRate")
			    )) {
				HCAPI.Log.Fatal("Failed to hook Barrier Decay Rate!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, HealthComponent, float>>((barrierDecayRate, hc) => 
				hc.body.maxBarrier / (barrierDecayRate * (1f + HealthStats.barrierDecayRateMultAdd) + HealthStats.barrierDecayRateFlatAdd));
		}

		private static void RecalcShieldRechargeRate(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<CharacterBody>("get_maxShield"),
				    x => x.MatchLdcR4(out _)
			    )) {
				HCAPI.Log.Fatal("Failed to hook Shield Recharge Rate!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((shieldRechargeRate) => 
				shieldRechargeRate * (1 + HealthStats.shieldRechargeRateMultAdd) + HealthStats.shieldRechargeRateFlatAdd);
		}

		private static void RecalcAdaptiveArmorDecayRate(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
					x => x.MatchLdfld<HealthComponent>("adaptiveArmorValue"),
				    x => x.MatchLdcR4(out _)
			    )) {
				HCAPI.Log.Fatal("Failed to hook Adaptive Armor Decay Rate!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>((adaptiveArmorDecayRate) => 
				adaptiveArmorDecayRate * (1 + HealthStats.adaptiveArmorDecayRateMultAdd) + HealthStats.adaptiveArmorDecayRateFlatAdd);
		}
		
		private static void RecalcDamageForce(ILCursor c) {
			if (!c.TryGotoNext(
				    x => x.MatchLdfld<DamageInfo>("canRejectForce"),
				    // Inserting here
				    x => x.MatchBrtrue(out _)
			    )) {
				HCAPI.Log.Fatal("Failed to hook Reject Force!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.Index += 1;
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<bool, bool>>(reject => {
				if (TakeDamageStats.rejectForce) return false;
				return reject;
			});
			c.Emit(OpCodes.Ldarg_1);
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Action<DamageInfo>>((dm) => {
				if (TakeDamageStats.rejectForce) dm.canRejectForce = false;
				dm.force *= 1 + TakeDamageStats.damageForceMultAdd;
				dm.force += TakeDamageStats.damageForceFlatAdd;
			});
		}

		private static void RejectDamageCheck(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.Before,
				    x => x.MatchLdfld<DamageInfo>("rejected"),
				    x => x.MatchBrfalse(out _)
			    )) {
				HCAPI.Log.Fatal("Failed to hook Reject Damage!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Action<DamageInfo>>((dm) => {
				if (TakeDamageStats.rejectDamage) dm.rejected = true;
			});
			c.Emit(OpCodes.Ldarg_1);
		}

		private static void RecalcAdaptiveArmorBuildRate(ILCursor c) {
			if (!c.TryGotoNext(
				    x => x.MatchLdcR4(out _),
				    // Inserting here
				    x => x.MatchMul(),
				    x => x.MatchLdarg(0),
				    x => x.MatchLdflda<HealthComponent>("itemCounts")
			    )) {
				HCAPI.Log.Fatal("Failed to hook Adaptive Armor Build Rate!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.Index += 1;
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>(adaptiveArmorBuildRate =>
				adaptiveArmorBuildRate * (1 + TakeDamageStats.adaptiveArmorBuildRateMultAdd) +
				TakeDamageStats.adaptiveArmorBuildRateFlatAdd);
		}
		
		private static void RecalcAdaptiveArmorMax(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchLdfld<HealthComponent>("adaptiveArmorValue"),
				    x => x.MatchLdloc(51),
				    x => x.MatchAdd(),
				    x => x.MatchLdcR4(out _)
			    )) {
				HCAPI.Log.Fatal("Failed to hook Adaptive Armor Max Value!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, float>>(adaptiveArmorMaxValue =>
				adaptiveArmorMaxValue * (1 + TakeDamageStats.adaptiveArmorMaxMultAdd) +
				TakeDamageStats.adaptiveArmorMaxFlatAdd);
		}
		
		private static void RecalcFinalDamage(ILCursor c) {
			if (!c.TryGotoNext(
				    moveType: MoveType.Before,
				    x => x.MatchLdfld<DamageInfo>("canRejectForce"),
				    x => x.MatchBrfalse(out _)
			    )) {
				HCAPI.Log.Fatal("Failed to hook Final Damage!");
				HCAPI.Log.Fatal(c.Context);
				return;
			}
			c.Emit(OpCodes.Ldloc, 7);
			c.Emit(OpCodes.Ldarg_1);
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, DamageInfo, float>>((damageToDeal, dm) => {
				dm.damage = dm.damage * (1 + TakeDamageStats.finalDamageAmountMultAdd) +
				            TakeDamageStats.finalDamageAmountFlatAdd;
				return damageToDeal * (1 + TakeDamageStats.finalDamageAmountMultAdd) +
				       TakeDamageStats.finalDamageAmountFlatAdd;
			});
			c.Emit(OpCodes.Stloc, 7);
		}
	}
}
