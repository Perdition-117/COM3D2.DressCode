namespace COM3D2.DressCode;

internal class SceneList : ButtonScrollList<CostumeScene> {
	private static readonly CostumeScene[] NpcScenes = {
		CostumeScene.Dance,
		CostumeScene.PoleDance,
		CostumeScene.NightPool,
	};

	private static readonly KeyValuePair<string, CostumeScene>[] Scenes = {
		new("SceneDance", CostumeScene.Dance),
		new("ScenePoleDance", CostumeScene.PoleDance),
		new("SceneYotogi", CostumeScene.Yotogi),
		new("SceneYotogiTalk", CostumeScene.YotogiTalk),
		new("ScenePrivateMode", CostumeScene.PrivateMode),
		new("SceneHoneymoon", CostumeScene.Honeymoon),
		new("SceneNightPool", CostumeScene.NightPool),
	};

	public SceneList(BaseComponent parent) : base(parent, nameof(SceneList)) {
		ScrollValue = 0.5f;

		foreach (var scene in Scenes) {
			AddButton(scene.Value, scene.Key, 150);
		}

		UpdateChildren();
	}

	public void SelectFirstAvailable(CostumeScene preferredScene = CostumeScene.None) {
		if (preferredScene != CostumeScene.None && Buttons.First(e => e.Value == preferredScene).IsEnabled) {
			SelectValue(preferredScene);
			return;
		}
		var button = Buttons.First(e => e.IsEnabled);
		SelectValue(button.Value);
	}

	public void SetNpcMode(bool isNpcMode) {
		foreach (var button in Buttons.Where(e => !NpcScenes.Contains(e.Value))) {
			if (isNpcMode) {
				button.SetSelected(false);
			}
			button.IsEnabled = !isNpcMode;
		}
	}
}
