using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace HDeMods {
	public static partial class HealthComponentAPI {
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
			if (c.TryGotoNext(
				    moveType: MoveType.After,
				    x => x.MatchCallvirt<TeamComponent>("get_teamIndex")
			    )) {
				c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<sbyte, sbyte>>(consume => {
					if (!HealStats.eclipseHealReductionIgnoreTeam) return consume;
					return (sbyte)RefVal.plr;
				});
			} else {
				Log.Error("Failed to hook teamIndex! Attempting healing hook.");
			}
			
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
			c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, HealthComponent, float>>((consume, hc) => 
				hc.body.maxBarrier / (RefVal.barrierDecayRate * (1f + HealthStats.barrierDecayRateMultAdd) + HealthStats.barrierDecayRateFlatAdd));
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