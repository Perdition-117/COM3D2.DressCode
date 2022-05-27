using I2.Loc;

namespace COM3D2.DressCode;

internal class Label : CanvasComponent {
	private readonly Text _labelText;
	private readonly Localize _localize;

	public Label(BaseComponent parent, string name) : base(parent, name) {
		_labelText = AddComponent<Text>();
		_labelText.font = GetResource<Font>("NotoSansCJKjp-DemiLight");
		_labelText.fontSize = 20;
		_labelText.alignment = TextAnchor.MiddleCenter;

		_localize = AddComponent<Localize>();
	}

	public string Text {
		get => _labelText.text;
		set => _labelText.text = value;
	}

	public string Term {
		set => _localize.Term = DressCode.GetTermKey(value);
	}

	public int FontSize {
		get => _labelText.fontSize;
		set => _labelText.fontSize = value;
	}

	public TextAnchor Alignment {
		get => _labelText.alignment;
		set => _labelText.alignment = value;
	}

	public Color Color {
		get => _labelText.color;
		set => _labelText.color = value;
	}

	public bool RaycastTarget {
		get => _labelText.raycastTarget;
		set => _labelText.raycastTarget = value;
	}
}
