namespace COM3D2.DressCode;

internal class CanvasComponent : BaseComponent {
	private readonly RectTransform _transform;

	public CanvasComponent(GameObject parent, string name) : base(parent, name) {
		GameObject.transform.SetParent(parent.transform, false);
		_transform = GameObject.AddComponent<RectTransform>();
	}

	public CanvasComponent(BaseComponent parent, string name) : this(parent.GameObject, name) { }

	public Vector2 SizeDelta {
		get => _transform.sizeDelta;
		set => _transform.sizeDelta = value;
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
		AnchorMin = new(0, 0);
		AnchorMax = new(1, 1);
	}
}
