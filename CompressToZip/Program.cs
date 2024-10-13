using System;
using System.IO;
using System.IO.Compression;


string targetFile = "../HealthComponentAPI/bin/" + HDeMods.HealthComponentAPI.PluginName + ".zip";

FileInfo dll;
FileInfo readme = new FileInfo("../README.md");
FileInfo icon = new FileInfo("../Resources/icon.png");

string manifestAuthor = HDeMods.HealthComponentAPI.PluginAuthor;
string manifestName = HDeMods.HealthComponentAPI.PluginName;
string manifestVersionNumber = HDeMods.HealthComponentAPI.PluginVersion;
string manifestWebsiteUrl = "https://github.com/HDeDeDe/HealthComponentAPI";
string manifestDescription = "is api for healthcomponent. designed to be like RecalculateStatsAPI.";
string manifestDependencies = "[\n" +
                              "\t\t\"bbepis-BepInExPack-5.4.2108\",\n" + 
                              "\t\t\"RiskofThunder-HookGenPatcher-1.2.3\"\n" + 
                              "\t]";

#if DEBUG
dll = new FileInfo("../" + HDeMods.HealthComponentAPI.PluginName + "/bin/Debug/netstandard2.1/" + HDeMods.HealthComponentAPI.PluginName + ".dll");
#endif

#if RELEASE
dll = new FileInfo("../" + HDeMods.HealthComponentAPI.PluginName + "/bin/Release/netstandard2.1/" + HDeMods.HealthComponentAPI.PluginName + ".dll");
#endif

Console.WriteLine("Creating " + HDeMods.HealthComponentAPI.PluginName + ".Zip");
if (File.Exists(targetFile)) File.Delete(targetFile);

ZipArchive archive = ZipFile.Open(targetFile, ZipArchiveMode.Create);

archive.CreateEntryFromFile(readme.FullName, readme.Name, CompressionLevel.Optimal);
archive.CreateEntryFromFile(dll.FullName, dll.Name, CompressionLevel.Optimal);
archive.CreateEntryFromFile(icon.FullName, "icon.png", CompressionLevel.Optimal);
ZipArchiveEntry manifest = archive.CreateEntry("manifest.json", CompressionLevel.Optimal);
using (StreamWriter writer = new StreamWriter(manifest.Open())) {
	writer.WriteLine("{");
	writer.WriteLine("\t\"author\": \"" + manifestAuthor + "\",");
	writer.WriteLine("\t\"name\": \"" + manifestName + "\",");
	writer.WriteLine("\t\"version_number\": \"" + manifestVersionNumber + "\",");
	writer.WriteLine("\t\"website_url\": \"" + manifestWebsiteUrl + "\",");
	writer.WriteLine("\t\"description\": \"" + manifestDescription + "\",");
	writer.WriteLine("\t\"dependencies\": " + manifestDependencies);
	writer.WriteLine("}");
	
	writer.Close();
}

archive.Dispose();