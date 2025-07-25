using UnityEngine.Events;

namespace COM3D2.DressCode;

internal class RadioButton : CanvasComponent {
	private readonly Toggle _toggle;
	private readonly Label _label;

	private static readonly ColorBlock ButtonColors = new() {
		normalColor = new(0.7843f, 0.7843f, 0.7843f, 1),
		highlightedColor = new(1, 1, 1, 1),
		pressedColor = new(0.7843f, 0.7843f, 0.7843f, 1),
		disabledColor = new(0.7843f, 0.7843f, 0.7843f, 0.502f),
		colorMultiplier = 1,
		fadeDuration = 0.1f,
	};

	static RadioButton() {
		if (DressCode.GameVersion.Major >= 3) {
			var propertyInfo = typeof(ColorBlock).GetProperty("selectedColor");
			object colors = ButtonColors;
			propertyInfo.SetValue(colors, Color.white, null);
			ButtonColors = (ColorBlock)colors;
		}
	}

	public RadioButton(CanvasComponent parent, string name) : base(parent, name) {
		_toggle = AddComponent<Toggle>();
		_toggle.colors = ButtonColors;

		AddComponent<uGUISelectableSound>();

		var background = AddChild("Background");
		background.SetAllPoints();

		_toggle.targetGraphic = background.AddImage("vr_frame02");

		Checkmark = background.AddChild("Checkmark");
		Checkmark.AnchorMin = new(0, 0);
		Checkmark.AnchorMax = new(1, 1);

		_toggle.graphic = Checkmark.AddImage("recipe_selectcursor");

		_label = new Label(this, "Label") {
			Color = new(0.251f, 0.251f, 0.251f, 1),
		};
		_label.SetAllPoints();
	}

	public CanvasComponent Checkmark { get; private set; }

	public bool IsOn {
		get => _toggle.isOn;
		set => _toggle.isOn = value;
	}

	public ToggleGroup Group {
		get => _toggle.group;
		set => _toggle.group = value;
	}

	public string Term {
		set => _label.Term = value;
	}

	public event UnityAction<bool> ValueChanged {
		add => _toggle.onValueChanged.AddListener(value);
		remove => _toggle.onValueChanged.RemoveListener(value);
	}
}
