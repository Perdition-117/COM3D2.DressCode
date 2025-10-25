using I2.Loc;
using wf;

namespace DressCode;

internal class ButtonScrollList<T> : GridScrollViewPanel {
	private static readonly Vector2 ButtonFrameInset = new(4, 4);
	private static readonly Vector2 ButtonLabelInset = new(10, 5);
	private static readonly Vector2 ButtonLabelOffset = new(0, -2);

	private readonly Dictionary<T, Button> _buttons = new();

	private readonly UIWFTabPanel _tabPanel;

	public ButtonScrollList(BaseComponent parent, string name) : base(parent, name) {
		ContentPivot = UIWidget.Pivot.Center;

		Grid.cellHeight = 63;
		Grid.pivot = UIWidget.Pivot.Center;

		ScrollChild.AddComponent<UICenterOnChild>().enabled = false;

		_tabPanel = ScrollChild.AddComponent<UIWFTabPanel>();
	}

	public event EventHandler<ValueSelectedEventArgs> ValueSelected;

	public T SelectedValue { get; private set; }

	protected IEnumerable<Button> Buttons => _buttons.Values;

	protected virtual void OnValueSelected(ValueSelectedEventArgs e) {
		ValueSelected?.Invoke(this, e);
	}

	public void SelectValue(T value) {
		_tabPanel.Select(_buttons[value].Component);
	}

	protected void UpdateChildren() => _tabPanel.UpdateChildren();

	protected Button AddButton(T value, string term, int width) {
		var gameObject = Utility.CreatePrefab(ScrollChild.GameObject, "SceneYotogi/SkillSelect/Prefab/CategoryBtn", true);
		gameObject.name = term;

		var button = UTY.GetChildObject(gameObject, "Button");

		var buttonSprite = button.GetComponent<UISprite>();
		buttonSprite.width = width;

		var tabButton = button.GetComponent<UIWFTabButton>();
		tabButton.onClick.Add(new(() => {
			SelectedValue = value;
			OnValueSelected(new() { Value = value });
		}));

		var frameSprite = UTY.GetChildObject(gameObject, "Frame").GetComponent<UISprite>();
		frameSprite.SetAnchor(button, ButtonFrameInset);

		var label = UTY.GetChildObject(gameObject, "Label").GetComponent<UILabel>();
		label.SetAnchor(button, ButtonLabelInset, ButtonLabelOffset);
		label.spacingX = 0;
		label.maxLineCount = 1;
		label.text = term;

		var localize = label.GetComponent<Localize>();
		localize.Term = DressCode.GetTermKey(term);

		var listButton = new Button {
			Value = value,
			Component = tabButton,
		};

		_buttons.Add(value, listButton);

		return listButton;
	}

	public class ValueSelectedEventArgs : EventArgs {
		public T Value { get; set; }
	}

	internal class Button {
		public T Value { get; set; }
		public UIWFTabButton Component { get; set; }

		public bool IsEnabled {
			get => Component.isEnabled;
			set => Component.isEnabled = value;
		}

		public void SetSelected(bool isSelected) => Component.SetSelect(isSelected);
	}
}
