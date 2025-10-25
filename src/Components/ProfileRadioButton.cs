using UnityEngine.Events;

namespace DressCode;

internal class ProfileRadioButton : RadioButton {
	private readonly CanvasComponent _description;
	private readonly Label _descriptionLabel;
	private readonly CanvasComponent _editFrame;
	private readonly Button _editButton;

	public ProfileRadioButton(CanvasComponent parent, string name) : base(parent, name) {
		AnchorMin = new(0, 1);
		AnchorMax = new(1, 1);
		Pivot = new(0.5f, 1);
		SizeDelta = new(-16, 50);

		Checkmark.SizeDelta = new(-4, -4);

		_description = AddChild("Description");
		_description.AnchorMin = new(0, 0);
		_description.AnchorMax = new(1, 0);
		_description.Pivot = new(0.5f, 1);

		var descriptionImage = _description.AddImage("edit_frame001B");
		descriptionImage.fillCenter = false;
		descriptionImage.raycastTarget = false;

		_descriptionLabel = new Label(_description, "Text") {
			AnchorMin = Vector2.zero,
			AnchorMax = Vector2.one,
			Pivot = new(0.5f, 0),
			Alignment = TextAnchor.LowerCenter,
			FontSize = 18,
			RaycastTarget = false,
		};

		_editFrame = _description.AddChild("Edit");
		_editFrame.AnchorMin = new(0, 1);
		_editFrame.AnchorMax = new(1, 1);
		_editFrame.Pivot = new(0.5f, 1);
		_editFrame.AnchoredPosition = new(0, -11);
		_editFrame.SizeDelta = new(-25, 40);

		_editButton = _editFrame.AddComponent<Button>();
		_editButton.interactable = false;

		_editFrame.AddImage("vr_frame02");

		_editFrame.AddComponent<uGUISelectableSound>();

		var editLabel = new Label(_editFrame, "Text") {
			Color = new(0.251f, 0.251f, 0.251f, 1),
			Term = "EditButton",
		};
		editLabel.SetAllPoints();

		ShowEditButton();
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

	public void ShowEditButton() {
		_editFrame.SetActive(true);
		_description.SizeDelta = new(0, 90);
		_descriptionLabel.AnchoredPosition = new(0, 13.5f);
	}

	public void HideEditButton() {
		_editFrame.SetActive(false);
		_description.SizeDelta = new(0, 43);
		_descriptionLabel.AnchoredPosition = new(0, 12.5f);
	}
}
