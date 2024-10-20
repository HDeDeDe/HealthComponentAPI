using System.Collections;
internal static class Settings {
//-----------------------------------------------------Customize--------------------------------------------------------
		// ReSharper disable once InconsistentNaming
		public const bool giveMePDBs = true;
		public const bool weave = false;
		
		public const string pluginName = HDeMods.HealthComponentAPI.PluginName;
		public const string pluginAuthor = HDeMods.HealthComponentAPI.PluginAuthor;
		public const string pluginVersion = HDeMods.HealthComponentAPI.PluginVersion;
		public const string changelog = "../CHANGELOG.md";
		public const string readme = "../README.md";
		public const string icon = "../Resources/icon.png";
		public const string riskOfRain2Install = @"C:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2_Data\Managed\";
		public static readonly ArrayList extraFiles = new ArrayList {
		};
		public const string manifestWebsiteUrl = "https://github.com/HDeDeDe/HealthComponentAPI";
		public const string manifestDescription = "is api for healthcomponent. designed to be like RecalculateStatsAPI.";
		public const string manifestDependencies = "[\n" +
		                                           "\t\t\"bbepis-BepInExPack-5.4.2108\",\n" + 
		                                           "\t\t\"RiskofThunder-HookGenPatcher-1.2.3\"\n" + 
		                                           "\t]";
}