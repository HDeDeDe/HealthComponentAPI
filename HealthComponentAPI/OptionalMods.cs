namespace HDeMods {
	public class OptionalMods {
		internal class SandSwept {
			private static bool? _enabled;

			public static bool enabled {
				get {
					if (_enabled == null) {
						_enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamSandswept.Sandswept");
					}
					return (bool)_enabled;
				}
			}
		}
	}
}