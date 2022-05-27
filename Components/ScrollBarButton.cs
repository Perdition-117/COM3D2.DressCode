namespace COM3D2.DressCode;

internal class ScrollBarButton : AnimatedButton {
	private readonly ScrollViewScroll _scroll;

	public ScrollBarButton(BaseComponent parent, string name, string spriteName) : base(parent, name) {
		_sprite.atlas = GetResource<UIAtlas>("AtlasCommon");
		_sprite.spriteName = spriteName;
		_sprite.width = 52;
		_sprite.height = 52;

		AddWidgetCollider();

		_scroll = AddComponent<ScrollViewScroll>();
		_scroll.m_Sprite = _sprite;
	}

	public float Delta {
		get => _scroll.delta;
		set => _scroll.delta = value;
	}

	public UIProgressBar ScrollBar {
		get => _scroll.m_ScrollBar;
		set => _scroll.m_ScrollBar = value;
	}

	public UIScrollView ScrollView {
		get => _scroll.scrollView;
		set => _scroll.scrollView = value;
	}
}
