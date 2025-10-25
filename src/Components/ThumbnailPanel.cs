namespace DressCode;

internal class ThumbnailPanel : CanvasComponent {
	private readonly RawImage _portraitImage;

	public ThumbnailPanel(CanvasComponent parent, string name) : base(parent, name) {
		var backgroundOuter = AddImage("vr_frame02");
		backgroundOuter.color = new(0.0471f, 0.0431f, 0.0902f, 0.9412f);

		var backgroundInner = AddChild("Background");
		backgroundInner.SizeDelta = new(-105, -125);
		backgroundInner.AnchorMin = new(0, 0);
		backgroundInner.AnchorMax = new(1, 1);
		backgroundInner.Position = new(0, -10);

		backgroundInner.AddImage("vr_frame02");

		var portrait = backgroundInner.AddChild("Portrait");
		portrait.SetAllPoints();
		portrait.Scale = new(0.95f, 0.95f);

		_portraitImage = portrait.AddComponent<RawImage>();

		var frame = backgroundInner.AddChild("Frame");
		frame.SetAllPoints();

		var frameImage = frame.AddComponent<RawImage>();
		frameImage.texture = Resources.Load<Texture2D>("CharacterSelect/Atlas/DefaultFrame");
	}

	public Texture2D ThumbnailImage {
		set => _portraitImage.texture = value;
	}
}
