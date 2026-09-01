using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text FPSText;

    void Update()
    {
        FPSText.text = $"FPS: {Mathf.Round(1.0f / Time.deltaTime)}";
    }
}