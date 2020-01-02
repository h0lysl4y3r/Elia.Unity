using Elia.Unity.Components.Games;

public class TestGameModeMeta : GameModeMeta
{
	public override int GetScoreToNextLevel(int level)
	{
		return (level + 1) * 30;
	}
}
