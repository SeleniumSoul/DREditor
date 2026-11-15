//Audio Visualizer script by SeleniumSoul for DREditor

using UnityEngine;
using TMPro;
using UnityEngine.Events;
using DREditor;

/// <summary>
/// UI Script used to display the music visualizer typically on the top left corner of the screen.
/// </summary>
public class DRMusicVisualizer : MonoBehaviour
{
	[Module] 
	private AudioPlayer audioPlayer;

	[Header("Bars")]
	//[SerializeField] GameObject MusicBarBase;
	[SerializeField] private DRMusicVisualizerBar[] MusicBars;
	[SerializeField] private float BarSensitivity, BarSmoothness, BarMaxHeight = 5f, GapWidth;
	
	[Header("Sampling")]
	[SerializeField] private FFTWindow SampleMethod;
	[SerializeField] private int SampleCount = 512;

	private AudioSource musicSource;
	private float[] frequencyBands, audioSamples;

	[SerializeField] private UnityEvent<string> OnChangeMusic;

	private void Awake()
	{
		DRModuleSystem.Inject(this);

        musicSource = audioPlayer.MusicSource;

        frequencyBands = new float[MusicBars.Length];
		audioSamples = new float[SampleCount];

		for (int bar = 0; bar < MusicBars.Length; bar++)
		{
			MusicBars[bar].InitializeBar(BarSmoothness);
		}

		//Create Music Bars
		//musicBars = new GameObject[MusicBars.Length];
		//musicBars[0] = MusicBar;

		//for (int x = 1; x < MusicBars.Length; x++)
		//{
		//    RectTransform _musicBarsPositions = musicBars[x-1].GetComponent<RectTransform>();

		//    GameObject _duplicatebar = Instantiate(musicBars[0]);
		//    RectTransform _dbrecttransform = _duplicatebar.GetComponent<RectTransform>();
		//    _duplicatebar.transform.SetParent(musicBars[0].transform.parent);
		//    _dbrecttransform.localScale = Vector3.one;
		//    _dbrecttransform.anchoredPosition = _musicBarsPositions.anchoredPosition + new Vector2(_musicBarsPositions.sizeDelta.x + GapWidth, 0f);
		//    _duplicatebar.name = "MusicBar" + x;
		//    musicBars[x] = _duplicatebar;
		//}
		
		audioPlayer.MusicChanged += OnMusicChanged;
    }

    private void OnEnable()
    {
		OnMusicChanged(audioPlayer.CurrentBGMName);
    }

    private void OnDestroy()
	{
		audioPlayer.MusicChanged -= OnMusicChanged;
	}

	private void Update()
	{
		if (musicSource == null)
			return;

		musicSource.GetSpectrumData(audioSamples, 0, SampleMethod);

		CalculateFreqBands();
	}

	private void FixedUpdate()
	{
		for (int samples = 0; samples < MusicBars.Length; samples++)
		{
			float _frequencyBand;
			//RectTransform MusicBarsScale = musicBars[samples].GetComponent<RectTransform>();
			_frequencyBand = frequencyBands[samples] * (10f * BarSensitivity);
			_frequencyBand = Mathf.Clamp(_frequencyBand, 0f, BarMaxHeight);
			MusicBars[samples].UpdateValue(_frequencyBand * BarSensitivity);
			//MusicBarsScale.localScale = Vector3.Lerp(MusicBarsScale.localScale, new Vector3(1f, _frequencyBand, 1f), Time.deltaTime * BarSmoothness);
		}
	}

	private void CalculateFreqBands()
	{
		float _bandrange = audioSamples.Length / SampleCount;

		for (int x = 0; x < MusicBars.Length; x++)
		{
			float _summation = 0f;
			for (int y = 0; y < _bandrange; y++)
			{
				_summation += audioSamples[(int)(y + (_bandrange * x))];
			}

			float _average = _summation / _bandrange;
			frequencyBands[x] = _average;
		}
	}

	public void OnMusicChanged(string name)
	{
		Debug.Log("Music Changed!");
		OnChangeMusic.Invoke(name);
	}
}
