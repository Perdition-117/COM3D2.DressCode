namespace COM3D2.DressCode;

internal class ScrollViewPanel : BaseComponent {
	private readonly UIScrollView _scrollView;
	private readonly UIPanel _uiPanel;

	public ScrollViewPanel(BaseComponent parent, string name) : base(parent, name) {
		Content = AddChild("Contents");

		_uiPanel = Content.AddComponent<UIPanel>();
		_uiPanel.clipping = UIDrawCall.Clipping.SoftClip;
		_uiPanel.clipSoftness = new(0, 10);
		_uiPanel.depth = 1;

		_scrollView = Content.AddComponent<UIScrollView>();
		_scrollView.movement = UIScrollView.Movement.Vertical;
		_scrollView.dragEffect = UIScrollView.DragEffect.Momentum;
		_scrollView.disableDragIfFits = true;
		_scrollView.scale = Vector3.zero;

		Background = AddChild("BG");

		var bgSprite = Background.AddSprite("AtlasCommon", "cm3d2_common_window_l");
		bgSprite.type = UIBasicSprite.Type.Sliced;

		DragMat = AddChild("DragMat");
		DragMat.AddWidgetCollider();

		var dragScrollView = DragMat.AddComponent<UIDragScrollView>();
		dragScrollView.scrollView = _scrollView;

		DragMat.AddComponent<UIWidget>();
	}

	public BaseComponent Content { get; private set; }
	public BaseComponent Background { get; private set; }
	public BaseComponent DragMat { get; private set; }

	public float ScrollWheelFactor {
		get => _scrollView.scrollWheelFactor;
		set => _scrollView.scrollWheelFactor = value;
	}

	public Vector4 ClipRegion {
		get => _uiPanel.baseClipRegion;
		set => _uiPanel.baseClipRegion = value;
	}

	public UIWidget.Pivot ContentPivot {
		get => _scrollView.contentPivot;
		set => _scrollView.contentPivot = value;
	}

	public UIProgressBar ScrollBar {
		get => _scrollView.verticalScrollBar;
		set => _scrollView.verticalScrollBar = value;
	}
}
