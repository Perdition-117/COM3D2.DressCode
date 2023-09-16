using I2.Loc;
using wf;

namespace COM3D2.DressCode;

internal class SceneList : GridScrollViewPanel {
	private const int ButtonExtraWidth = 20;

	private static readonly Vector2 PanelSize = new(174, 848);

	private readonly UIWFTabPanel _tabPanel;
	private readonly Dictionary<CostumeScene, UIWFTabButton> _tabButtons = new();

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
		ContentPivot = UIWidget.Pivot.Center;

		Size = PanelSize;

		Grid.cellHeight = 63;
		Grid.pivot = UIWidget.Pivot.Center;

		ScrollChild.AddComponent<UICenterOnChild>().enabled = false;

		ScrollValue = 0.5f;

		_tabPanel = ScrollChild.AddComponent<UIWFTabPanel>();

		foreach (var scene in _scenes) {
			AddButton(scene.Key, scene.Value);
		}

		_tabPanel.UpdateChildren();
	}

	public event EventHandler<SceneSelectedEventArgs> SceneSelected;

	public CostumeScene SelectedScene { get; private set; }

	protected virtual void OnSceneSelected(SceneSelectedEventArgs e) {
		SceneSelected?.Invoke(this, e);
	}

	public void SelectScene(CostumeScene scene) {
		_tabPanel.Select(_tabButtons[scene]);
	}

	public void SelectFirstAvailable() {
		foreach (var child in ScrollChild.GetComponent<UIGrid>().GetChildList()) {
			var tabButton = child.gameObject.GetComponentInChildren<UIWFTabButton>();
			if (tabButton.isEnabled) {
				_tabPanel.Select(tabButton);
				break;
			}
		}
	}

	public void SetNpcMode(bool isNpcMode) {
		foreach (var button in _tabButtons) {
			if (!_npcScenes.Contains(button.Key)) {
				if (isNpcMode) {
					button.Value.SetSelect(false);
				}
				button.Value.isEnabled = !isNpcMode;
			}
		}
	}

	public UIWFTabButton AddButton(string term, CostumeScene scene) {
		var gameObject = Utility.CreatePrefab(ScrollChild.GameObject, "SceneYotogi/SkillSelect/Prefab/CategoryBtn", true);
		gameObject.name = term;

		var frameSprite = UTY.GetChildObject(gameObject, "Frame").GetComponent<UISprite>();
		frameSprite.width += ButtonExtraWidth;

		var label = UTY.GetChildObject(gameObject, "Label").GetComponent<UILabel>();
		label.spacingX = 0;
		label.width += ButtonExtraWidth;

		var localize = label.GetComponent<Localize>();
		localize.Term = DressCode.GetTermKey(term);

		var button = UTY.GetChildObject(gameObject, "Button");

		var buttonSprite = button.GetComponent<UISprite>();
		buttonSprite.width += ButtonExtraWidth;

		var tabButton = button.GetComponent<UIWFTabButton>();
		tabButton.onClick.Add(new(() => {
			SelectedScene = scene;
			OnSceneSelected(new() { Scene = scene });
		}));
		_tabButtons.Add(scene, tabButton);

		return tabButton;
	}

	public class SceneSelectedEventArgs : EventArgs {
		public CostumeScene Scene { get; set; }
	}
}
