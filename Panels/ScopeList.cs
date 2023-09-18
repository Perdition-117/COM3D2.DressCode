namespace COM3D2.DressCode;

internal class ScopeList : ButtonScrollList<ProfileScope> {
	public ScopeList(BaseComponent parent) : base(parent, nameof(ScopeList)) {
		AddButton(ProfileScope.Scene, "SceneSetting", 110);
		AddButton(ProfileScope.Maid, "MaidSetting", 110);

		UpdateChildren();
	}
}
