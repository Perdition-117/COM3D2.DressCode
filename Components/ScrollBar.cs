namespace COM3D2.DressCode;

internal class ScrollBar : BaseComponent {
	public ScrollBar(BaseComponent parent, string name, bool createButtons = false) : base(parent, name) {
		Position = new(64, 0);

		var scrollBar = AddComponent<UIScrollBar>();
		
		Foreground = AddChild("Foreground");

		var foregroundWidget = Foreground.AddComponent<UIWidget>();

		var thumb = AddChild("Thumb");
		thumb.Position = new(178, 474);

		var thumbSprite = thumb.AddSprite("AtlasCommon", "cm3d2_common_scrollcursor");
		thumbSprite.width = 33;
		thumbSprite.height = 33;

		thumb.AddWidgetCollider();

		scrollBar.mEnableFixSize = true;
		scrollBar.mFixSizePixcel = 10;
		scrollBar.fillDirection = UIProgressBar.FillDirection.TopToBottom;
		scrollBar.foregroundWidget = foregroundWidget;
		scrollBar.thumb = thumb.GameObject.transform;

		if (createButtons) {
			var scrollView = parent.GetComponentInChildren<UIScrollView>();

			new ScrollBarButton(this, "Up", "cm3d2_common_scrollbutton_up") {
				Position = new(178, 512),
				Delta = 1,
				ScrollView = scrollView,
				ScrollBar = scrollBar,
			};

			new ScrollBarButton(this, "Down", "cm3d2_common_scrollbutton_down") {
				Position = new(178, -512),
				Delta = -1,
				ScrollView = scrollView,
				ScrollBar = scrollBar,
			};
		}
	}

	public BaseComponent Foreground { get; private set; }
}
