using System.Xml;
using System.Xml.Serialization;

namespace COM3D2.DressCode;

public class Configuration {
	public SceneList Scenes { get; set; } = new();
	[XmlArrayItem("Maid")]
	public List<MaidConfiguration> Maids { get; set; } = new();

	public class SceneList {
		[XmlElement("Profile")]
		public List<SceneProfile> SceneProfiles { get; set; } = new();

		public SceneProfile AddSceneProfile(CostumeScene scene, CostumeProfile profile) {
			var costume = new SceneProfile() {
				Scene = scene,
				PreferredProfile = profile,
			};
			SceneProfiles.Add(costume);
			return costume;
		}

		public bool TryGetSceneProfile(CostumeScene scene, out SceneProfile profile) {
			profile = SceneProfiles.Find(e => e.Scene == scene);
			return profile != null;
		}
	}

	public class MaidConfiguration : SceneList {
		[XmlAttribute]
		public string Guid { get; set; }
	}

	public class SceneProfile {
		[XmlAttribute]
		public CostumeScene Scene { get; set; }
		[XmlAttribute]
		public CostumeProfile PreferredProfile { get; set; }
		public Costume Costume { get; set; }

		public bool HasCostume {
			get => Costume != null;
		}
	}

	public class Costume {
		[XmlElement("Item")]
		public List<Item> Items { get; set; } = new();

		public Item AddItem(MPN mpn, string fileName, bool isEnabled = true) {
			var item = new Item {
				Slot = mpn,
				FileName = fileName,
				IsEnabled = isEnabled,
			};
			Items.Add(item);
			return item;
		}

		public bool TryGetItem(MPN mpn, out Item item) {
			item = Items.Find(e => e.Slot == mpn);
			return item != null;
		}
	}

	public class Item {
		[XmlAttribute]
		public MPN Slot { get; set; }
		[XmlAttribute]
		public string FileName { get; set; }
		[XmlAttribute]
		public bool IsEnabled { get; set; }
	}

	private MaidConfiguration AddMaid(Maid maid) {
		var maidConfig = new MaidConfiguration {
			Guid = maid.status.guid,
		};
		Maids.Add(maidConfig);
		return maidConfig;
	}

	private bool TryGetMaid(Maid maid, out MaidConfiguration maidConfig) {
		maidConfig = Maids.Find(e => e.Guid == maid.status.guid);
		return maidConfig != null;
	}

	internal bool TryGetSceneProfile(CostumeScene scene, out SceneProfile profile) {
		return Scenes.TryGetSceneProfile(scene, out profile);
	}

	internal bool TryGetMaidProfile(Maid maid, CostumeScene scene, out SceneProfile profile) {
		if (!TryGetMaid(maid, out var maidConfig)) {
			profile = null;
			return false;
		}
		return maidConfig.TryGetSceneProfile(scene, out profile);
	}

	internal void SetPreferredProfile(CostumeScene scene, CostumeProfile profile) {
		var costume = GetSceneProfile(scene);
		costume.PreferredProfile = profile;
		ConfigurationManager.Save();
	}

	internal void SetPreferredProfile(Maid maid, CostumeScene scene, CostumeProfile profile) {
		var costume = GetMaidProfile(maid, scene);
		costume.PreferredProfile = profile;
		ConfigurationManager.Save();
	}

	internal void SaveCostume(Maid maid, CostumeScene scene, Costume newCostume, CostumeProfile profile) {
		var profileConfig = profile switch {
			CostumeProfile.Personal => GetMaidProfile(maid, scene),
			CostumeProfile.Shared => GetSceneProfile(scene),
			_ => throw new NotImplementedException(),
		};
		profileConfig.Costume = newCostume;
		ConfigurationManager.Save();
	}

	internal SceneProfile GetSceneProfile(CostumeScene scene) {
		if (!TryGetSceneProfile(scene, out var costume)) {
			costume = Scenes.AddSceneProfile(scene, CostumeProfile.Personal);
		}
		return costume;
	}

	internal SceneProfile GetMaidProfile(Maid maid, CostumeScene scene) {
		if (!TryGetMaid(maid, out var maidConfig)) {
			maidConfig = AddMaid(maid);
		}
		if (!maidConfig.TryGetSceneProfile(scene, out var costume)) {
			costume = maidConfig.AddSceneProfile(scene, CostumeProfile.Personal);
		}
		return costume;
	}
}
