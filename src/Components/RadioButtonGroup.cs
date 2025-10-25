namespace DressCode;

internal class RadioButtonGroup<TButton, TValue> where TButton : RadioButton {
	private readonly ToggleGroup _toggleGroup;
	private readonly Dictionary<TValue, TButton> _buttons = new();

	public RadioButtonGroup(CanvasComponent parent) {
		_toggleGroup = parent.AddComponent<ToggleGroup>();
	}

	public event EventHandler<ValueChangedEventArgs> ValueChanged;

	public TButton this[TValue profile] {
		get => _buttons[profile];
	}

	protected virtual void OnValueChanged(ValueChangedEventArgs e) {
		ValueChanged?.Invoke(this, e);
	}

	public void AddButton(TButton button, TValue value) {
		button.Group = _toggleGroup;
		button.ValueChanged += isSelected => OnValueChanged(new() {
			Value = value,
			IsSelected = isSelected,
		});
		_buttons.Add(value, button);
	}

	public class ValueChangedEventArgs : EventArgs {
		public TValue Value { get; set; }
		public bool IsSelected { get; set; }
	}
}
