namespace COM3D2.DressCode;

internal static class UIRectExtensions {
	public static void SetAnchor(this UIRect uiRect, GameObject anchorTarget, Vector2 inset, Vector2 offset) {
		var insetLb = inset + offset;
		var insetRt = -inset + offset;
		uiRect.SetAnchor(anchorTarget);
		uiRect.leftAnchor.absolute = (int)insetLb.x;
		uiRect.bottomAnchor.absolute = (int)insetLb.y;
		uiRect.rightAnchor.absolute = (int)insetRt.x;
		uiRect.topAnchor.absolute = (int)insetRt.y;
		uiRect.updateAnchors = UIRect.AnchorUpdate.OnEnable;
	}

	public static void SetAnchor(this UIRect uiRect, GameObject anchorTarget, Vector2 inset) => uiRect.SetAnchor(anchorTarget, inset, Vector2.zero);
}
