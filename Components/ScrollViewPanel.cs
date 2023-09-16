namespace COM3D2.DressCode;

internal class ScrollViewPanel : BaseComponent {
	private static readonly Vector2 ContentOffset = new(-6, 0);
	private static readonly Vector2 DragMatInset = new(15, 47);

	private readonly UIWidget _uiPanel;
	private readonly UIPanel _contentPanel;
	private readonly UIScrollView _scrollView;

	public ScrollViewPanel(BaseComponent parent, string name) : base(parent, name) {
		_uiPanel = AddComponent<UIWidget>();

		var content = AddChild("Contents");
		content.Position = ContentOffset;

		_contentPanel = content.AddComponent<UIPanel>();
		_contentPanel.clipping = UIDrawCall.Clipping.SoftClip;
		_contentPanel.clipSoftness = new(0, 10);
		_contentPanel.depth = 1;

		ScrollChild = content.AddChild("ScrollChild");

		_scrollView = content.AddComponent<UIScrollView>();
		_scrollView.movement = UIScrollView.Movement.Vertical;
		_scrollView.dragEffect = UIScrollView.DragEffect.Momentum;
		_scrollView.disableDragIfFits = true;
		_scrollView.scale = Vector3.zero;
		_scrollView.scrollWheelFactor = 3;

		var background = AddChild("BG");

		var backgroundSprite = background.AddSprite("AtlasCommon", "cm3d2_common_window_l");
		backgroundSprite.type = UIBasicSprite.Type.Sliced;
		backgroundSprite.SetAnchor(_uiPanel.gameObject);
		backgroundSprite.updateAnchors = UIRect.AnchorUpdate.OnEnable;

		var dragMat = AddChild("DragMat");
		dragMat.AddAnchorWidget(GameObject, DragMatInset, ContentOffset);
		dragMat.AddWidgetCollider();

		var dragScrollView = dragMat.AddComponent<UIDragScrollView>();
		dragScrollView.scrollView = _scrollView;

		var scrollBar = new ScrollBar(this, "Scroll Bar");
		_scrollView.verticalScrollBar = scrollBar.GetComponent<UIScrollBar>();
	}

	public BaseComponent ScrollChild { get; }

	public UIWidget.Pivot ContentPivot {
		get => _scrollView.contentPivot;
		set => _scrollView.contentPivot = value;
	}

	public float ScrollValue {
		get => _scrollView.verticalScrollBar.value;
		set => _scrollView.verticalScrollBar.value = value;
	}

	public Vector2 Size {
		set {
			_uiPanel.width = (int)value.x;
			_uiPanel.height = (int)value.y;
			_contentPanel.baseClipRegion = new(0, 0, value.x, value.y - 94);
		}
	}
}
