//All-in-one Audio Manager script by SeleniumSoul for DREditor

using System.Collections;
using UnityEngine;
using DREditor.Dialogues.Events;
using DREditor.Audio;
using System;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer current;

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

    private void Awake()
    {
        if (current != null) Destroy(this);
        else current = this;

        DialogueEventSystem.StartListening("PlayBGM", OnPlayBGM);
        DialogueEventSystem.StartListening("PlayVoice", OnPlayVoice);
        DialogueEventSystem.StartListening("PlaySFX", OnPlaySFX);
        DialogueEventSystem.StartListening("PlaySystemSFX", OnPlaySystemSFX);
    }

    private void OnDestroy()
    {
        if (current == this) current = null;
        DialogueEventSystem.StopListening("PlayBGM", OnPlayBGM);
        DialogueEventSystem.StopListening("PlayVoice", OnPlayVoice);
        DialogueEventSystem.StopListening("PlaySFX", OnPlaySFX);
        DialogueEventSystem.StopListening("PlaySystemSFX", OnPlaySystemSFX);
    }

    private void OnPlayBGM(object BGMnum)
    {
        int _num;
        float _fadeout = 0f;

        if (BGMnum is CMTuple _numpass)
        {
            _num = _numpass.MusicNum;
            _fadeout = _numpass.fadeOut;
        }
        else
        {
            _num = (int)BGMnum;
        }

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

        currentBGMNum = _num + 1;
    }

    private void OnPlaySFX(object sound)
    {
        SFXSource?.PlayOneShot((AudioClip)sound);
    }

    private void OnPlayVoice(object voice)
    {
        VoiceSource?.Stop();
        VoiceSource.clip = (AudioClip)voice;
        VoiceSource?.Play();
    }

    private void OnPlaySystemSFX(object sound)
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