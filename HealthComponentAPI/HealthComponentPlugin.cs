using BepInEx;
using BepInEx.Logging;
namespace HDeMods {
	[BepInPlugin(HealthComponentAPI.PluginGUID, HealthComponentAPI.PluginName, HealthComponentAPI.PluginVersion)]
	public sealed class HealthComponentAPIPlugin : BaseUnityPlugin {
		private void Awake() {
			HCAPI.Log.Init(Logger);
		}

		private void OnDestroy() {
			HealthComponentAPI.UnsetHealthHook();
			HealthComponentAPI.UnsetHealHooks();
			HealthComponentAPI.UnsetTakeDamageHooks();
		}
	}

	namespace HCAPI {
		internal static class Log
		{
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