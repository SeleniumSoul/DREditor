//All-in-one Audio Manager script by SeleniumSoul for DREditor

using System.Collections;
using UnityEngine;
using DREditor.Dialogues.Events;
using DREditor.Audio;
using System;
using DREditor;
using System.Threading.Tasks;

public class AudioPlayer : MonoBehaviour, IModule
{
	public Action<string> MusicChanged;

	[SerializeField] [Range(0f, 1f)] private float BGMVolume;
	[SerializeField] private Playlist playlist;

	[Header("Sources")]
	[SerializeField] private AudioSource BGMSource;
	[SerializeField] private AudioSource VoiceSource;
	[SerializeField] private AudioSource SFXSource;
	[SerializeField] private AudioSource SystemSource;

	private int currentBGMNum;

	public AudioSource MusicSource { get { return BGMSource; } }
	public string CurrentBGMName => currentBGMNum >= 0 ? playlist.Musics[currentBGMNum].Title : "???!";

	public Task Init()
    {
        DialogueEventSystem.RegisterEvents(this);
        return Task.CompletedTask;
    }

	private void OnDestroy()
	{
		DialogueEventSystem.DeregisterEvents(this);
	}

	[DialogueEvent("PlayBGM")]
	private void OnPlayBGM(CMTuple BGMnum)
	{
		int _num;
		float _fadeout = 0f;

		if (BGMnum is CMTuple _numpass)
		{
			_num = _numpass.MusicNum;
			_fadeout = _numpass.fadeOut;
		}
		//else
		//{
		//    _num = (int)BGMnum;
		//}

		if ((_num + 1) == 0)
		{
			//ToggleVisualizer(false);
			if (_fadeout > 0f)
			{
				StartCoroutine(FadeOutLerp(_fadeout));
			}
			else
			{
				BGMSource.Stop();
			}
		}
		else if ((_num + 1) != currentBGMNum)
		{
			BGMSource.volume = BGMVolume;

			BGMSource.clip = playlist.Musics[_num].BGM;
			BGMSource.Play();

			MusicChanged?.Invoke(playlist.Musics[_num].Title);
		}

		currentBGMNum = _num;
	}

	[DialogueEvent("PlayVoice")]
	public void OnPlaySFX(AudioClip sound)
	{
		SFXSource?.PlayOneShot((AudioClip)sound);
	}

	[DialogueEvent("PlaySFX")]
	public void OnPlayVoice(AudioClip voice)
	{
		VoiceSource?.Stop();
		VoiceSource.clip = (AudioClip)voice;
		VoiceSource?.Play();
	}

	[DialogueEvent("PlaySystemSFX")]
	public void OnPlaySystemSFX(AudioClip sound)
	{
		SystemSource?.PlayOneShot((AudioClip)sound);
	}

	private IEnumerator FadeOutLerp(float _fadeTime)
	{
		float _eT = 0f;

		while (_eT < _fadeTime)
		{
			BGMSource.volume = Mathf.Lerp(BGMVolume, 0f, (_eT / _fadeTime));
			_eT += Time.deltaTime;
			yield return null;
		}

		BGMSource.volume = 0f;
		BGMSource.Stop();
		yield return null;
	}
}