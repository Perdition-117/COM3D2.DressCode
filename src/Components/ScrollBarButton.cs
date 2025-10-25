namespace DressCode;

internal class ScrollBarButton : AnimatedButton {
	private const int SpriteSize = 52;

	public ScrollBarButton(BaseComponent parent, string name, string spriteName) : base(parent, name) {
		_sprite.atlas = GetResource<UIAtlas>("AtlasCommon");
		_sprite.spriteName = spriteName;
		_sprite.width = SpriteSize;
		_sprite.height = SpriteSize;

		AddWidgetCollider();

		ScrollViewScroll = AddComponent<ScrollViewScroll>();
		ScrollViewScroll.m_Sprite = _sprite;
	}

	public ScrollViewScroll ScrollViewScroll { get; }
}
