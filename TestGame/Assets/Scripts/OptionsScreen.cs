using Elia.Unity.Components.GameObjects;
using Elia.Unity.Modules;
using Screen = Elia.Unity.Components.GUI.Screen;

public class OptionsScreen : Screen
{
    protected override void Start()
    {
        base.Start();

        Audio.Instance.PlayClipFromLibrary(Audio.AudioType.Music, "music1");
    }

    public void GoToMainMenu()
	{
		Screens.Instance.SetScreenActive<MainMenuScreen>(true);
	}

    public void PlayRandomEffect()
    {
        Audio.Instance.PlayRandomClipFromGroup(Audio.AudioType.Effect, "effectsGroup1");
    }

    public void PlayRandomLoopEffect()
    {
        Audio.Instance.PlayRandomClipFromLibrary(Audio.AudioType.LoopEffect);
    }

    public void StopLoopEffects()
    {
        Audio.Instance.StopAllClips(Audio.AudioType.LoopEffect);
    }

    public void EffectsVolumeDown()
    {
        Audio.Instance.EffectsVolume -= 0.1f;
    }

    public void EffectsVolumeUp()
    {
        Audio.Instance.EffectsVolume += 0.1f;
    }

    public void LoopEffectsVolumeDown()
    {
        Audio.Instance.LoopEffectsVolume -= 0.1f;
    }

    public void LoopEffectsVolumeUp()
    {
        Audio.Instance.LoopEffectsVolume += 0.1f;
    }

    public void MusicVolumeDown()
    {
        Audio.Instance.MusicVolume -= 0.1f;
    }

    public void MusicVolumeUp()
    {
        Audio.Instance.MusicVolume += 0.1f;
    }

    public void PlayMusic1()
    {
        Audio.Instance.PlayClipFromLibrary(Audio.AudioType.Music, "music1");
    }

    public void StopMusic1()
    {
        Audio.Instance.StopClip("music1");
    }

	public void SpawnObject()
	{
		GameObjectPool.Instance.Get("floor");
	}

	public void FadeInMusic()
	{
		Audio.Instance.FadeMusic(2, 1);
	}

	public void FadeOutMusic()
	{
		Audio.Instance.FadeMusic(2, 0);
	}
}
