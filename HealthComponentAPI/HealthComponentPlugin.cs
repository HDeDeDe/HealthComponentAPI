using System.Diagnostics.CodeAnalysis;
using BepInEx;
using BepInEx.Logging;
using RoR2;

namespace HDeMods {
	[BepInPlugin(HealthComponentAPI.PluginGUID, HealthComponentAPI.PluginName, HealthComponentAPI.PluginVersion)]
	public sealed class HealthComponentAPIPlugin : BaseUnityPlugin {
#if DEBUG
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
		private static bool makeEmImmortal; //Immortal
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
#endif
		
		private void Awake() {
			HCAPI.Log.Init(Logger);
			HealthComponentAPI.AddOnHooks();
#if DEBUG
			HealthComponentAPI.GetTakeDamageStats += MakeEveryoneImmortalLol;
#endif
		}

		private void OnDestroy() {
			HealthComponentAPI.UnsetHealthHook();
			HealthComponentAPI.UnsetHealHooks();
			HealthComponentAPI.UnsetTakeDamageHooks();
			HealthComponentAPI.RemoveOnHooks();
		}
#if DEBUG
		private static void MakeEveryoneImmortalLol(HealthComponent sender, in DamageInfo damageInfo,
			HealthComponentAPI.TakeDamageArgs args) {
			if (makeEmImmortal) args.rejectDamage = true;
		}
#endif
	}

	namespace HCAPI {
		internal static class Log
		{
			[SuppressMessage("ReSharper", "InconsistentNaming")] 
			private static ManualLogSource m_logSource;

			internal static void Init(ManualLogSource logSource) => m_logSource = logSource;

			internal static void Debug(object data) => m_logSource.LogDebug(data);
			internal static void Error(object data) => m_logSource.LogError(data);
			internal static void Fatal(object data) => m_logSource.LogFatal(data);
			internal static void Info(object data) => m_logSource.LogInfo(data);
			internal static void Message(object data) => m_logSource.LogMessage(data);
			internal static void Warning(object data) => m_logSource.LogWarning(data);
		}
	}
}