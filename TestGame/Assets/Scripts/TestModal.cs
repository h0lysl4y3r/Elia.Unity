using Elia.Unity.Components.GUI;
using Elia.Unity.Modules;

public class TestModal : Modal
{
	public void Close()
	{
		SetActive(false, null);
	}
}
