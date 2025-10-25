namespace DressCode;

internal class GridScrollViewPanel : ScrollViewPanel {
	public GridScrollViewPanel(BaseComponent parent, string name) : base(parent, name) {
		Grid = ScrollChild.AddComponent<UIGrid>();
		Grid.arrangement = UIGrid.Arrangement.Vertical;
		Grid.cellWidth = 0;
	}

	public UIGrid Grid { get; }
}
