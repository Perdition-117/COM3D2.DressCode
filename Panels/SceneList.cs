using I2.Loc;
using wf;

namespace COM3D2.DressCode;

internal class SceneList : ScrollViewPanel {
	private const int ExtraWidth = 20;

	private readonly BaseComponent _gridParent;
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
	};

	public SceneList(BaseComponent parent) : base(parent, nameof(SceneList)) {
		ScrollWheelFactor = 3;
		ClipRegion = new(0, 0, 156, 809);
		ContentPivot = UIWidget.Pivot.Center;

		Background.GetComponent<UISprite>().width = 174;
		Background.GetComponent<UISprite>().height = 848;

		Content.Position = new(-4, 18);

		DragMat.Position = new(-910, 454);
		DragMat.GetComponent<UIWidget>().width = 147;
		DragMat.GetComponent<UIWidget>().height = 760;

		var scrollBar = new ScrollBar(this, "Scroll Bar");

		ScrollBar = scrollBar.GetComponent<UIScrollBar>();
		ScrollBar.value = 0.5f;

		_gridParent = Content.AddChild("GridParent");
		_gridParent.Position = new(0, 56);

		_gridParent.AddComponent<UICenterOnChild>().enabled = false;

		var grid = _gridParent.AddComponent<UIGrid>();
		grid.arrangement = UIGrid.Arrangement.Vertical;
		grid.cellHeight = 63;
		grid.cellWidth = 0;
		//grid.keepWithinPanel = true;
		grid.pivot = UIWidget.Pivot.Center;

		//_gridParent.AddComponent<UIWFSwitchPanel>();
		_tabPanel = _gridParent.AddComponent<UIWFTabPanel>();

		foreach (var scene in _scenes) {
			AddButton(scene.Key, scene.Value);
		}

		_tabPanel.UpdateChildren();
	}

	public event EventHandler<SceneSelectedEventArgs> SceneSelected;

	protected virtual void OnSceneSelected(SceneSelectedEventArgs e) {
		SceneSelected?.Invoke(this, e);
	}

	public void SelectFirstAvailable() {
		foreach (var child in _gridParent.GetComponent<UIGrid>().GetChildList()) {
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
		var gameObject = Utility.CreatePrefab(_gridParent.GameObject, "SceneYotogi/SkillSelect/Prefab/CategoryBtn", true);
		gameObject.name = term;

		var frameSprite = UTY.GetChildObject(gameObject, "Frame").GetComponent<UISprite>();
		frameSprite.width += ExtraWidth;

		var label = UTY.GetChildObject(gameObject, "Label").GetComponent<UILabel>();
		label.spacingX = 0;
		label.width += ExtraWidth;

		var localize = label.GetComponent<Localize>();
		localize.Term = DressCode.GetTermKey(term);

		var button = UTY.GetChildObject(gameObject, "Button");

		var buttonSprite = button.GetComponent<UISprite>();
		buttonSprite.width += ExtraWidth;

		var tabButton = button.GetComponent<UIWFTabButton>();
		tabButton.onClick.Add(new(() => OnSceneSelected(new() { Scene = scene })));
		_tabButtons.Add(scene, tabButton);

		return tabButton;
	}

	public class SceneSelectedEventArgs : EventArgs {
		public CostumeScene Scene { get; set; }
	}
}
