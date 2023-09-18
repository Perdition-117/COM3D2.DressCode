namespace COM3D2.DressCode;

internal class ProfilePanel : CanvasComponent {
	private const int ThumbnailPanelWidth = 615;
	private const int ButtonSpacing = 115;

	private static readonly Vector2 PanelSize = new(890, 830);

	private readonly DressCodeControl _parentControl;
	private readonly ThumbnailPanel _thumbnailPanel;
	private readonly CanvasComponent _buttonGroup;
	private readonly RadioButtonGroup<ProfileRadioButton, CostumeProfile> _toggleGroup;

	private CostumeProfile _selectedProfile;
	private bool _noEventTrigger = false;

	private static readonly ProfileDefinition[] Profiles = {
		new() {
			Profile = CostumeProfile.Default,
			LabelTerm = "DefaultLabel",
			DescriptionTerm = "DefaultDescription",
		},
		new() {
			Profile = CostumeProfile.Scene,
			LabelTerm = "SceneLabel",
			DescriptionTerm = "SceneDescription",
		},
		new() {
			Profile = CostumeProfile.Personal,
			LabelTerm = "PersonalLabel",
			DescriptionTerm = "PersonalDescription",
		},
	};

	public ProfilePanel(CanvasComponent parent, DressCodeControl parentControl) : base(parent, nameof(ProfilePanel)) {
		_parentControl = parentControl;

		AnchorMin = new(1, 0.5f);
		AnchorMax = new(1, 0.5f);
		Pivot = new(1, 0.5f);
		AnchoredPosition = new(-110, 45);
		SizeDelta = PanelSize;

		_thumbnailPanel = new ThumbnailPanel(this, "ThumbnailFrame") {
			AnchorMin = new(1, 0),
			AnchorMax = new(1, 1),
			Pivot = new(1, 0.5f),
			SizeDelta = new(ThumbnailPanelWidth, 0),
		};

		_buttonGroup = AddChild("ButtonGroup");
		_buttonGroup.AnchorMin = new(0, 1);
		_buttonGroup.AnchorMax = new(1, 1);
		_buttonGroup.Pivot = new(0, 1);
		_buttonGroup.OffsetMax = new(-ThumbnailPanelWidth, 0);

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

		for (int i = 0; i < Profiles.Length; i++) {
			var profile = Profiles[i];
			var button = new ProfileRadioButton(_buttonGroup, profile.Profile.ToString()) {
				AnchoredPosition = new(15, -(65 + ButtonSpacing * i)),
				Term = profile.LabelTerm,
				Description = profile.DescriptionTerm,
			};
			_toggleGroup.AddButton(button, profile.Profile);
		}

		_toggleGroup[CostumeProfile.Default].HideEditButton();
		_toggleGroup[CostumeProfile.Scene].EditClicked += () => CostumeEdit.StartCostumeEdit(_parentControl.SelectedScene);
		_toggleGroup[CostumeProfile.Personal].EditClicked += () => CostumeEdit.StartCostumeEdit(_parentControl.SelectedMaid, _parentControl.SelectedScene);

		var header = AddChild("Header");
		header.AnchorMin = new(0, 1);
		header.AnchorMax = new(1, 1);
		header.Pivot = new(0.5f, 1);
		header.AnchoredPosition = new(0, -16);
		header.SizeDelta = new(-30, 32);

		var headerImage = header.AddImage("edit_frame001");
		headerImage.fillCenter = false;

		var label = new Label(header, "Label") {
			AnchorMin = Vector2.zero,
			AnchorMax = Vector2.one,
			SizeDelta = new(-20, 0),
			Alignment = TextAnchor.MiddleLeft,
			Term = "CostumeSetting",
		};
	}

	public event EventHandler<ProfileSelectedEventArgs> ProfileSelected;

	protected virtual void OnProfileSelected(ProfileSelectedEventArgs e) {
		ProfileSelected?.Invoke(this, e);
	}

	public void SetSceneMode() {
		_toggleGroup[CostumeProfile.Scene].ShowEditButton();
		_toggleGroup[CostumeProfile.Personal].SetActive(false);
		_toggleGroup[CostumeProfile.Personal].IsOn = false;
		_buttonGroup.OffsetMin = new(0, -346);
	}

	public void SetMaidMode() {
		_toggleGroup[CostumeProfile.Scene].HideEditButton();
		_toggleGroup[CostumeProfile.Personal].SetActive(true);
		_buttonGroup.OffsetMin = new(0, -461);
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
			CostumeProfile.Default => (_parentControl.SelectedScope == ProfileScope.Maid ? _parentControl.SelectedMaid : DressCode.GetHeadMaid()).GetThumCard(),
			CostumeProfile.Scene => DressCode.GetThumbnail(_parentControl.SelectedScene, null),
			CostumeProfile.Personal => DressCode.GetThumbnail(_parentControl.SelectedScene, _parentControl.SelectedMaid),
			_ => throw new NotImplementedException(),
		};
	}

	public class ProfileSelectedEventArgs : EventArgs {
		public CostumeProfile Profile { get; set; }
	}

	private class ProfileDefinition {
		public CostumeProfile Profile { get; set; }
		public string LabelTerm { get; set; }
		public string DescriptionTerm { get; set; }
	}
}
