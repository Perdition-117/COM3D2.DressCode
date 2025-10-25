using I2.Loc;

namespace DressCode;

internal class CircleButton : AnimatedButton {
	private readonly UILabel _label;
	private readonly Localize _localize;

	public CircleButton(BaseComponent parent, string name) : base(parent, name) {
		_sprite.atlas = GetResource<UIAtlas>("AtlasCommon2");
		_sprite.spriteName = "main_buttom";
		_sprite.width = 120;
		_sprite.height = 120;

		AddWidgetCollider();

		var label = AddChild("Value");

		_label = label.AddComponent<UILabel>();
		_label.trueTypeFont = GetResource<Font>("NotoSansCJKjp-DemiLight");
		_label.fontSize = 22;
		_label.alignment = NGUIText.Alignment.Center;
		_label.depth = 2;
		_label.color = new(0.251f, 0.251f, 0.251f, 1);

		_localize = label.AddComponent<Localize>();
	}

	public event EventDelegate.Callback Click {
		add => EventDelegate.Add(_button.onClick, value);
		remove => EventDelegate.Remove(_button.onClick, value);
	}

	public string Text {
		get => _label.text;
		set => _label.text = value;
	}

	public string Term {
		set => _localize.Term = DressCode.GetTermKey(value);
	}
}
