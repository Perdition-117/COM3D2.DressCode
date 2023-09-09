namespace COM3D2.DressCode;

internal class ScopeSelectTabBar : CanvasComponent {
	private readonly RadioButtonGroup<RadioButton, ProfileScope> _radioButtonGroup;

	public ScopeSelectTabBar(CanvasComponent parent) : base(parent, nameof(ScopeSelectTabBar)) {
		SizeDelta = new(850, 50);
		Position = new(0, 505);

		var tabBg = AddImage("vr_frame02");
		tabBg.color = new(0.0471f, 0.0431f, 0.0902f, 0.9412f);

		var tabButtonList = AddChild("TabBarButtonList");
		tabButtonList.SetAllPoints();

		var gridLayout = tabButtonList.AddComponent<GridLayoutGroup>();
		gridLayout.cellSize = new(330, 30);
		gridLayout.spacing = new(5, 0);
		gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
		gridLayout.childAlignment = TextAnchor.MiddleCenter;

		_radioButtonGroup = new(tabButtonList);
		_radioButtonGroup.ValueChanged += (o, e) => {
			if (e.IsSelected) {
				SelectedScope = e.Value;
				OnScopeSelected(new() { Scope = e.Value });
			}
		};

		CreateTab(tabButtonList, "SceneSetting", ProfileScope.Scene);
		CreateTab(tabButtonList, "MaidSetting", ProfileScope.Maid);
	}

	public event EventHandler<ScopeSelectedEventArgs> ScopeSelected;

	public ProfileScope SelectedScope { get; private set; }

	protected virtual void OnScopeSelected(ScopeSelectedEventArgs e) {
		ScopeSelected?.Invoke(this, e);
	}

	public void SelectScope(ProfileScope scope) {
		_radioButtonGroup[scope].IsOn = true;
	}

	private RadioButton CreateTab(CanvasComponent parent, string text, ProfileScope profile) {
		var tab = new RadioButton(parent, text);
		tab.Term = text;
		tab.Checkmark.SizeDelta = new(-4, -2);
		_radioButtonGroup.AddButton(tab, profile);
		return tab;
	}

	public class ScopeSelectedEventArgs : EventArgs {
		public ProfileScope Scope { get; set; }
	}
}
