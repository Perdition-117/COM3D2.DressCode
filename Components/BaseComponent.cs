namespace COM3D2.DressCode;

internal class BaseComponent {
	public GameObject GameObject { get; protected set; }

	public BaseComponent(string name) {
		GameObject = new GameObject(name);
	}

	public BaseComponent(GameObject parent, string name) : this(name) {
		SetParent(parent);
	}

	public BaseComponent(BaseComponent parent, string name) : this(parent.GameObject, name) { }

	public string Name {
		get => GameObject.name;
		set => GameObject.name = value;
	}

	public Vector3 Position {
		get => GameObject.transform.localPosition;
		set => GameObject.transform.localPosition = value;
	}

	public Vector3 Scale {
		get => GameObject.transform.localScale;
		set => GameObject.transform.localScale = value;
	}

	public void SetParent(GameObject parent) {
		var transform = GameObject.transform;
		transform.parent = parent.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		GameObject.layer = parent.layer;
	}

	public void SetActive(bool isActive) {
		GameObject.SetActive(isActive);
	}

	public BaseComponent AddChild(string name = null) {
		var child = new BaseComponent(GameObject, name);
		return child;
	}

	public T AddChild<T>() where T : Component {
		var child = AddChild(NGUITools.GetTypeName<T>());
		return child.AddComponent<T>();
	}

	public T AddComponent<T>() where T : Component {
		return GameObject.AddComponent<T>();
	}

	public T GetComponent<T>() where T : Component {
		return GameObject.GetComponent<T>();
	}

	public T GetComponentInChildren<T>() where T : Component {
		return GameObject.GetComponentInChildren<T>();
	}

	public UIAnchor AddAnchor(GameObject anchorTarget, UIAnchor.Side anchorPoint, Vector2 offset) {
		var anchor = AddComponent<UIAnchor>();
		anchor.container = anchorTarget;
		anchor.side = anchorPoint;
		anchor.pixelOffset = offset;
		anchor.runOnlyOnce = false;
		return anchor;
	}

	public UIWidget AddAnchorWidget(GameObject anchorTarget, Vector2 inset, Vector2 offset) {
		var widget = AddComponent<UIWidget>();
		widget.SetAnchor(anchorTarget, inset, offset);
		widget.updateAnchors = UIRect.AnchorUpdate.OnEnable;
		return widget;
	}

	public UIWidget AddAnchorWidget(GameObject anchorTarget, Vector2 inset) => AddAnchorWidget(anchorTarget, inset, Vector2.zero);

	public UIWidget AddAnchorWidget(GameObject anchorTarget) => AddAnchorWidget(anchorTarget, Vector2.zero, Vector2.zero);

	public UISprite AddSprite(string atlasName, string spriteName) {
		var sprite = AddComponent<UISprite>();
		sprite.atlas = GetResource<UIAtlas>(atlasName);
		sprite.spriteName = spriteName;
		return sprite;
	}

	public Image AddImage(string spriteName) {
		var image = AddComponent<Image>();
		image.sprite = GetResource<Sprite>(spriteName);
		image.type = Image.Type.Sliced;
		return image;
	}

	public void AddWidgetCollider() {
		NGUITools.AddWidgetCollider(GameObject);
	}

	public void UpdateWidgetCollider() {
		NGUITools.UpdateWidgetCollider(GameObject);
	}

	public static T GetResource<T>(string name) where T : UnityEngine.Object {
		return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(o => o.name == name);
	}
}
