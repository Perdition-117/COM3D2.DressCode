namespace COM3D2.DressCode;

internal class MaidSelectPanel : GridScrollViewPanel {
	private readonly CharacterSelectManager _characterSelectManager;

	public MaidSelectPanel(CanvasComponent parent) : base(parent, nameof(MaidSelectPanel)) {
		ContentPivot = UIWidget.Pivot.Top;

		Grid.cellHeight = 130;
		Grid.pivot = UIWidget.Pivot.Top;
		Grid.sorting = UIGrid.Sorting.Custom;

		_characterSelectManager = AddComponent<CharacterSelectManager>();
		_characterSelectManager.MaidPlateParentGrid = Grid;
		_characterSelectManager.SetCallBackMaidList(GetMaidList);
		_characterSelectManager.SetCallBackCallBackOnSelect(maid => {
			SelectedMaid = maid;
			OnMaidSelected(new() { Maid = maid });
		});
	}

	public event EventHandler<MaidSelectedEventArgs> MaidSelected;

	public Maid SelectedMaid { get; private set; }

	protected virtual void OnMaidSelected(MaidSelectedEventArgs e) {
		MaidSelected?.Invoke(this, e);
	}

	public void Initialize() => _characterSelectManager.Create(CharacterSelectManager.Type.Select);

	public void SelectMaid(Maid maid) => _characterSelectManager.SelectMaid(maid);

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
