namespace HDeMods {
	public class OptionalMods {
		internal class SandSwept {
			private static bool enabled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamSandswept.Sandswept");
		}
		internal class Hex3 {
			private static bool enabled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Hex3.Hex3Mod");
		}

		internal class MoreStats {
			public static bool enabled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskOfBrainrot.MoreStats");
		}
	}
}