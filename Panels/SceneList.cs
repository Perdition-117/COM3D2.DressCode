namespace COM3D2.DressCode;

internal class SceneList : ButtonScrollList<CostumeScene> {
	private static readonly Vector2 PanelSize = new(174, 848);

	private readonly CostumeScene[] _npcScenes = {
		CostumeScene.Dance,
		CostumeScene.PoleDance,
	};

	private readonly KeyValuePair<string, CostumeScene>[] _scenes = {
		new("SceneDance", CostumeScene.Dance),
		new("ScenePoleDance", CostumeScene.PoleDance),
		new("SceneYotogi", CostumeScene.Yotogi),
		new("ScenePrivateMode", CostumeScene.PrivateMode),
		new("SceneHoneymoon", CostumeScene.Honeymoon),
	};

	public SceneList(BaseComponent parent) : base(parent, nameof(SceneList)) {
		Size = PanelSize;

		ScrollValue = 0.5f;

		foreach (var scene in _scenes) {
			AddButton(scene.Value, scene.Key, 140);
		}

		UpdateChildren();
	}

	public void SelectFirstAvailable() {
		var button = Buttons.First(e => e.IsEnabled);
		SelectValue(button.Value);
	}

	public void SetNpcMode(bool isNpcMode) {
		foreach (var button in Buttons.Where(e => !_npcScenes.Contains(e.Value))) {
			if (isNpcMode) {
				button.SetSelected(false);
			}
			button.IsEnabled = !isNpcMode;
		}
	}
}
