using UnityEngine;
using UnityEngine.UI;

public class DRMusicVisualizerBar : MonoBehaviour
{
    [SerializeField] private Sprite[] FrequencyLevelSprites;

    private Image barImage;

    private float levelValue;
    private float levelMaxValue;
    private float levelDamping = 1f;

    private void Start()
    {
        barImage = GetComponent<Image>();
        levelMaxValue = FrequencyLevelSprites.Length - 1;
    }

    public void InitializeBar(float damping)
    {
        levelDamping = damping;
    }

    private void FixedUpdate()
    {
        int levelFloor = Mathf.FloorToInt(levelValue);

        if (levelFloor < levelMaxValue && levelValue >= 0)
        {
            barImage.sprite = FrequencyLevelSprites[levelFloor];
        }
        else if (levelFloor >= levelMaxValue)
        {
            barImage.sprite = FrequencyLevelSprites[FrequencyLevelSprites.Length - 1];
        }
        else
        {
            barImage.sprite = FrequencyLevelSprites[0];
        }
    }

    public void UpdateValue(float value)
    {
        levelValue = Mathf.Lerp(levelValue, value, Time.deltaTime * levelDamping);
    }
}