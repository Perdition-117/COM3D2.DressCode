namespace COM3D2.DressCode;

internal class AnimatedButton : BaseComponent {
	private readonly Color Hover = Color.white;
	private readonly Color Default = new(1, 1, 1, 0.6706f);
	private readonly Color Pressed = new(1, 1, 1, 0.7843f);
	private readonly Color Disabled = new(0.5f, 0.5f, 0.5f, 1);

	protected readonly UISprite _sprite;
	protected readonly UIButton _button;

	public AnimatedButton(BaseComponent parent, string name) : base(parent, name) {
		_sprite = AddComponent<UISprite>();

		_button = AddComponent<UIButton>();
		_button.hover = Hover;
		_button.defaultColor = Default;
		_button.pressed = Pressed;
		_button.disabledColor = Disabled;

		var animation = AddComponent<Animation>();
		animation.playAutomatically = false;
		animation.clip = Resources.Load<AnimationClip>("SceneEdit/MainMenu/Animation/ButtonPuyo");
		animation.AddClip(animation.clip, animation.clip.name);

		var playAnimation = AddComponent<UIPlayAnimation>();
		playAnimation.trigger = AnimationOrTween.Trigger.OnHover;
		playAnimation.target = animation;
	}
}
