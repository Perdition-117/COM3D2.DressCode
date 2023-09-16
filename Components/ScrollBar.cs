namespace COM3D2.DressCode;

internal class ScrollBar : BaseComponent {
	private const int ThumbSpriteSize = 33;

	private static readonly Vector2 ForegroundInset = new(0, 62);
	private static readonly Vector2 ScrollButtonOffset = new(0, 28);

	private readonly UIScrollView _scrollView;
	private readonly UIScrollBar _scrollBar;

	public ScrollBar(BaseComponent parent, string name) : base(parent, name) {
		var anchorWidget = AddAnchorWidget(parent.GameObject);
		anchorWidget.leftAnchor.relative = 1;
		anchorWidget.leftAnchor.absolute = -20;

		var foreground = AddChild("Foreground");

		var foregroundWidget = foreground.AddAnchorWidget(GameObject, ForegroundInset);

		var thumb = AddChild("Thumb");

		var thumbSprite = thumb.AddSprite("AtlasCommon", "cm3d2_common_scrollcursor");
		thumbSprite.width = ThumbSpriteSize;
		thumbSprite.height = ThumbSpriteSize;

		thumb.AddWidgetCollider();

		_scrollView = parent.GetComponentInChildren<UIScrollView>();

		_scrollBar = AddComponent<UIScrollBar>();
		_scrollBar.mEnableFixSize = true;
		_scrollBar.mFixSizePixcel = 10;
		_scrollBar.fillDirection = UIProgressBar.FillDirection.TopToBottom;
		_scrollBar.foregroundWidget = foregroundWidget;
		_scrollBar.thumb = thumb.GameObject.transform;

		AddScrollButton("Up", "cm3d2_common_scrollbutton_up", 1, UIAnchor.Side.Top, -ScrollButtonOffset);
		AddScrollButton("Down", "cm3d2_common_scrollbutton_down", -1, UIAnchor.Side.Bottom, ScrollButtonOffset);
	}

	private void AddScrollButton(string componentName, string spriteName, int scrollDelta, UIAnchor.Side anchorPoint, Vector2 offset) {
		var scrollButton = new ScrollBarButton(this, componentName, spriteName);

		scrollButton.ScrollViewScroll.scrollView = _scrollView;
		scrollButton.ScrollViewScroll.m_ScrollBar = _scrollBar;
		scrollButton.ScrollViewScroll.delta = scrollDelta;

		scrollButton.AddAnchor(_scrollBar.gameObject, anchorPoint, offset);
	}
}
