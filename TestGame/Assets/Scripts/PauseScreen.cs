using Elia.Unity.Modules;
using Screen = Elia.Unity.Components.GUI.Screen;

public class PauseScreen : Screen
{
	public void GoToMenu()
	{
		App.Instance.SetGamePause(false);
		App.Instance.GoToMenu();
	}

	public void GoBackToGame()
	{
		App.Instance.SetGamePause(false);
	}
}
