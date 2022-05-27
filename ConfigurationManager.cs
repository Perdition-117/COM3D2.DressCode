using System.IO;
using System.Xml;
using System.Xml.Serialization;
using BepInEx;

namespace COM3D2.DressCode;

internal class ConfigurationManager {
	private static readonly string _configPath = Path.Combine(Paths.ConfigPath, "net.perdition.com3d2.dresscode.xml");

	private static readonly XmlSerializer serializer = new(typeof(Configuration));

	private static readonly XmlWriterSettings writerSettings = new() {
		Indent = true,
		IndentChars = "\t",
	};

	private static readonly XmlSerializerNamespaces namespaces = new(new[] { XmlQualifiedName.Empty });

	public static Configuration Configuration { get; private set; }

	public static void Load() {
		if (File.Exists(_configPath)) {
			using var reader = XmlReader.Create(_configPath);
			Configuration = (Configuration)serializer.Deserialize(reader);
		} else {
			Configuration = new();
		}
	}

	public static void Save() {
		using var writer = XmlWriter.Create(_configPath, writerSettings);
		serializer.Serialize(writer, Configuration, namespaces);
	}
}
