using UnityEngine.Events;

namespace COM3D2.DressCode;

internal class ProfileRadioButton : RadioButton {
	private readonly CanvasComponent _description;
	private readonly Label _descriptionLabel;
	private readonly CanvasComponent _editFrame;
	private readonly Button _editButton;

	public ProfileRadioButton(CanvasComponent parent, string name) : base(parent, name) {
		SizeDelta = new(325, 50);
		Checkmark.SizeDelta = new(-4, -4);

		_description = AddChild("Description");
		_description.AnchorMin = new(0, 0);
		_description.AnchorMax = new(1, 0);

		var descriptionImage = _description.AddImage("edit_frame001B");
		descriptionImage.fillCenter = false;
		descriptionImage.raycastTarget = false;

		_descriptionLabel = new Label(_description, "Text") {
			FontSize = 18,
			RaycastTarget = false,
		};

		_editFrame = _description.AddChild("Edit");
		_editFrame.SizeDelta = new(-25, 40);
		_editFrame.AnchorMin = new(0, 1);
		_editFrame.AnchorMax = new(1, 1);
		_editFrame.Position = new(0, 14);

		_editButton = _editFrame.AddComponent<Button>();
		_editButton.interactable = false;

		_editFrame.AddImage("vr_frame02");

		_editFrame.AddComponent<uGUISelectableSound>();

		var editLabel = new Label(_editFrame, "Text") {
			Color = new(0.251f, 0.251f, 0.251f, 1),
			Term = "EditButton",
		};
		editLabel.SetAllPoints();

		HideEditButton();
	}

	public string Description {
		set => _descriptionLabel.Term = value;
	}

	public bool EditIsEnabled {
		get => _editButton.interactable;
		set => _editButton.interactable = value;
	}

	public event UnityAction EditClicked {
		add => _editButton.onClick.AddListener(value);
		remove => _editButton.onClick.RemoveListener(value);
	}

	public void ShowButton() {
		_editFrame.SetActive(true);

		_descriptionLabel.SizeDelta = new(0, 45);
		_descriptionLabel.Position = new(0, -22.5f);

		_description.SizeDelta = new(0, 90);
		_description.Position = new(0, -70);
	}

	public void HideEditButton() {
		_editFrame.SetActive(false);

		_descriptionLabel.SetAllPoints();
		_descriptionLabel.Position = new(0, 0);

		_description.SizeDelta = new(0, 43);
		_description.Position = new(0, -46.5f);
	}
}
