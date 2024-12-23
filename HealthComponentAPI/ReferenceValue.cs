using RoR2;

namespace HDeMods {
	internal static class HcRefVal {
		// These are for ServerUpdate()
		public const float shieldRechargeRate = 0.5f;
		public const float barrierDecayRate = 30f;
		public const float adaptiveArmorDecayRate = 40f;
		// These are for Heal()
		public const TeamIndex plr = TeamIndex.Player;
		public const DifficultyIndex e5 = DifficultyIndex.Eclipse5;
		public const float critHealMultiplier = 2f;
		public const float damageCoyoteTimer = 0.2f;
		// These are for TakeDamageProcess()
		public const float adaptiveArmorBuildRate = 30f;
		public const float adaptiveArmorMaxValue = 400f;
	}
}