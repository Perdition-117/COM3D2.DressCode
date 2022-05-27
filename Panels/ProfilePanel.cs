namespace COM3D2.DressCode;

internal class ProfilePanel : CanvasComponent {
	private readonly ThumbnailPanel _thumbnailPanel;
	private readonly CanvasComponent _buttonGroup;
	private readonly RadioButtonGroup<ProfileRadioButton, CostumeProfile> _toggleGroup;

	private CostumeProfile _selectedProfile;
	private bool _noEventTrigger = false;

	public ProfilePanel(CanvasComponent parent) : base(parent, nameof(ProfilePanel)) {
		SizeDelta = new(956, 830);
		Position = new(372, 45);

		_thumbnailPanel = new ThumbnailPanel(this, "ThumbnailFrame");

		_buttonGroup = AddChild("ButtonGroup");
		_buttonGroup.SizeDelta = new(341, 461);
		_buttonGroup.Position = new(-307.5001f, 184.5f);

		var image = _buttonGroup.AddImage("vr_frame02");
		image.color = new(0.0471f, 0.0431f, 0.0902f, 0.9412f);

		_toggleGroup = new(_buttonGroup);
		_toggleGroup.ValueChanged += (o, e) => {
			_toggleGroup[e.Value].EditIsEnabled = e.IsSelected;
			if (e.IsSelected) {
				_selectedProfile = e.Value;
				UpdateThumbnail();
				if (!_noEventTrigger) {
					OnProfileSelected(new() { Profile = e.Value });
				}
			}
		};

		var button1 = new ProfileRadioButton(_buttonGroup, "Default") {
			Position = new(15, 140),
			Term = "DefaultLabel",
			Description = "DefaultDescription",
		};
		_toggleGroup.AddButton(button1, CostumeProfile.Default);

		var button2 = new ProfileRadioButton(_buttonGroup, "Scene") {
			Position = new(15, 25),
			Term = "SceneLabel",
			Description = "SceneDescription",
		};
		button2.EditClicked += () => CostumeEdit.StartCostumeEdit(SelectedScene);
		_toggleGroup.AddButton(button2, CostumeProfile.Scene);

		var button3 = new ProfileRadioButton(_buttonGroup, "Personal") {
			Position = new(15, -90),
			Term = "PersonalLabel",
			Description = "PersonalDescription",
		};
		button3.EditClicked += () => CostumeEdit.StartCostumeEdit(SelectedMaid, SelectedScene);
		button3.ShowButton();
		_toggleGroup.AddButton(button3, CostumeProfile.Personal);

		var header = AddChild("Header");
		header.SizeDelta = new(-30, 32);
		header.AnchorMin = new(0, 1);
		header.AnchorMax = new(1, 1);
		header.Position = new(0, 383);

		var headerImage = header.AddImage("edit_frame001");
		headerImage.fillCenter = false;

		var label = new Label(header, "Label") {
			SizeDelta = new(-20, 0),
			AnchorMin = Vector2.zero,
			AnchorMax = Vector2.one,
			Alignment = TextAnchor.MiddleLeft,
			Term = "CostumeSetting",
		};
	}

	public event EventHandler<ProfileSelectedEventArgs> ProfileSelected;

	public Maid SelectedMaid { get; set; }
	public CostumeScene SelectedScene { get; set; }

	protected virtual void OnProfileSelected(ProfileSelectedEventArgs e) {
		ProfileSelected?.Invoke(this, e);
	}

	public void SetSceneMode() {
		_toggleGroup[CostumeProfile.Scene].ShowButton();
		_toggleGroup[CostumeProfile.Personal].SetActive(false);
		_toggleGroup[CostumeProfile.Personal].IsOn = false;
		_toggleGroup[CostumeProfile.Default].Position = new(15, 140 - (115 / 2));
		_toggleGroup[CostumeProfile.Scene].Position = new(15, 25 - (115 / 2));
		_buttonGroup.SizeDelta = new(341, 346);
		_buttonGroup.Position = new(-307.5001f, 242);
	}

	public void SetMaidMode() {
		_toggleGroup[CostumeProfile.Scene].HideEditButton();
		_toggleGroup[CostumeProfile.Personal].SetActive(true);
		_toggleGroup[CostumeProfile.Default].Position = new(15, 140);
		_toggleGroup[CostumeProfile.Scene].Position = new(15, 25);
		_buttonGroup.SizeDelta = new(341, 461);
		_buttonGroup.Position = new(-307.5001f, 184.5f);
	}

	public void SelectProfile(CostumeProfile profile) {
		_selectedProfile = profile;
		var toggle = _toggleGroup[profile];
		_noEventTrigger = true;
		toggle.IsOn = true;
		_noEventTrigger = false;
		UpdateThumbnail();
	}

	private void UpdateThumbnail() {
		_thumbnailPanel.ThumbnailImage = _selectedProfile switch {
			CostumeProfile.Default => SelectedMaid?.GetThumCard(),
			CostumeProfile.Scene => DressCode.GetThumbnail(SelectedScene, null),
			CostumeProfile.Personal => DressCode.GetThumbnail(SelectedScene, SelectedMaid),
			_ => throw new NotImplementedException(),
		};
	}

	public class ProfileSelectedEventArgs : EventArgs {
		public CostumeProfile Profile { get; set; }
	}
}
