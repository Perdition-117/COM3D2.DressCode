namespace COM3D2.DressCode;

internal class MaidSelectPanel : ScrollViewPanel {
	private readonly CharacterSelectManager _characterSelectManager;

	public MaidSelectPanel(CanvasComponent parent) : base(parent, nameof(MaidSelectPanel)) {
		Position = new(-690, 0);
		ClipRegion = new(144, 0, 800, 986);
		ScrollWheelFactor = 4;
		ContentPivot = UIWidget.Pivot.Top;

		Background.GetComponent<UISprite>().width = 502;
		Background.GetComponent<UISprite>().height = 1080;

		Content.Position = new(-4, 0);

		var scrollBar = new ScrollBar(this, "Scroll Bar", true);

		var scrollBarPanel = scrollBar.AddComponent<UIPanel>();
		scrollBarPanel.depth = 1;
		scrollBarPanel.baseClipRegion = Vector4.zero;

		ScrollBar = scrollBar.GetComponent<UIScrollBar>();
		scrollBar.Foreground.Position = new(178, 0);
		scrollBar.Foreground.GetComponent<UIWidget>().width = 36;
		scrollBar.Foreground.GetComponent<UIWidget>().height = 957;

		DragMat.Position = new(-5, 0);
		DragMat.GetComponent<UIWidget>().width = 492;
		DragMat.GetComponent<UIWidget>().height = 986;

		_characterSelectManager = AddComponent<CharacterSelectManager>();
		_characterSelectManager.SetCallBackMaidList(GetMaidList);
		_characterSelectManager.SetCallBackCallBackOnSelect(maid => {
			SelectedMaid = maid;
			OnMaidSelected(new() { Maid = maid });
		});

		var maidSkillUnitParent = Content.AddChild("MaidSkillUnitParent");
		maidSkillUnitParent.Position = new(0, 427.5f);

		var grid = maidSkillUnitParent.AddComponent<UIGrid>();
		grid.arrangement = UIGrid.Arrangement.Vertical;
		grid.cellHeight = 130;
		grid.cellWidth = 0;
		grid.keepWithinPanel = true;
		grid.pivot = UIWidget.Pivot.Top;
		grid.sorting = UIGrid.Sorting.Custom;

		_characterSelectManager.MaidPlateParentGrid = grid;

		maidSkillUnitParent.AddComponent<UIWFSwitchPanel>();
		maidSkillUnitParent.AddComponent<UIWFTabPanel>();

		DragMat.UpdateWidgetCollider();
	}

	public event EventHandler<MaidSelectedEventArgs> MaidSelected;

	public Maid SelectedMaid { get; private set; }

	protected virtual void OnMaidSelected(MaidSelectedEventArgs e) {
		MaidSelected?.Invoke(this, e);
	}

	public void Initialize(CharacterSelectManager.Type type) {
		_characterSelectManager.Create(type);
	}

	private void GetMaidList(List<Maid> drawMaidList) {
		var characterManager = GameMain.Instance.CharacterMgr;
		for (var i = 0; i < characterManager.GetStockMaidCount(); i++) {
			var maid = characterManager.GetStockMaid(i);
			if (!CharacterSelectMain.compatibilityMode || maid.status.isCompatiblePersonality) {
				drawMaidList.Add(maid);
			}
		}
	}

	public class MaidSelectedEventArgs : EventArgs {
		public Maid Maid { get; set; }
	}
}
