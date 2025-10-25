namespace DressCode;

internal class CanvasComponent : BaseComponent {
	private readonly RectTransform _transform;

	public CanvasComponent(GameObject parent, string name) : base(parent, name) {
		GameObject.transform.SetParent(parent.transform, false);
		_transform = GameObject.AddComponent<RectTransform>();
	}

	public CanvasComponent(BaseComponent parent, string name) : this(parent.GameObject, name) { }

	public Vector2 AnchoredPosition {
		get => _transform.anchoredPosition;
		set => _transform.anchoredPosition = value;
	}

	public Vector2 SizeDelta {
		get => _transform.sizeDelta;
		set => _transform.sizeDelta = value;
	}

	public Vector2 Pivot {
		get => _transform.pivot;
		set => _transform.pivot = value;
	}

	public Vector2 AnchorMin {
		get => _transform.anchorMin;
		set => _transform.anchorMin = value;
	}

	public Vector2 AnchorMax {
		get => _transform.anchorMax;
		set => _transform.anchorMax = value;
	}

	public Vector2 OffsetMin {
		get => _transform.offsetMin;
		set => _transform.offsetMin = value;
	}

	public Vector2 OffsetMax {
		get => _transform.offsetMax;
		set => _transform.offsetMax = value;
	}

	public new CanvasComponent AddChild(string name) {
		return new(GameObject, name);
	}

	public void SetAllPoints() {
		SizeDelta = Vector2.zero;
		AnchorMin = Vector2.zero;
		AnchorMax = Vector2.one;
	}
}
