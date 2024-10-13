using BepInEx;
namespace HDeMods {
	[BepInPlugin(HealthComponentAPI.PluginGUID, HealthComponentAPI.PluginName, HealthComponentAPI.PluginVersion)]
	public sealed class HealthComponentAPIPlugin : BaseUnityPlugin {
		private void Awake() {
			Log.Init(Logger);
		}

		private void OnDestroy() {
			HealthComponentAPI.UnsetHealthHook();
			HealthComponentAPI.UnsetHealHooks();
		}
	}
}