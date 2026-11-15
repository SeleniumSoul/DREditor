//FPSCursor script by SeleniumSoul for DRDistrust
using UnityEngine;
using TMPro;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System;
using DREditor;

[Obsolete]
public class DRFPSCursor : MonoBehaviour
{
    [SerializeField] private AudioClip[] SFX = new AudioClip[2];
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private CanvasGroup NamePanel;

	[Header("Raycasting")]
    [SerializeField] private LayerMask RaycastMask;
    [SerializeField] private float RaycastDistance = 5f;

    private Ray cursray;
    private RaycastHit[] curshit = new RaycastHit[5];

    private Animator _cursoranimator;
    private Camera _maincamera;
    private IExaminable examinable;

	private void Awake()
	{
		//_controls = new DRInput();
	}

    void Start()
	{
		_maincamera = Camera.main;
		_cursoranimator = GetComponent<Animator>();
		NamePanel.alpha = 0f;
	}

	private void OnEnable()
	{
		//_controls.Enable();
	}

	private void OnDestroy()
	{
		//_controls.Disable();
	}

	private void Examine()
	{
        if (examinable != null)
        {
            //TODO: Polish this.
            examinable.Examine();
        }
        else Debug.LogError("DREditor (PassThreadToHandler): Unable to find properties.");
	}

	void Update()
	{
		if (_maincamera == null) _maincamera = Camera.main;
		cursray = _maincamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		var amount = Physics.RaycastNonAlloc(cursray, curshit, RaycastDistance, RaycastMask);

        if (amount <= 0) return;
        if (curshit[0].collider.gameObject.layer == 3) return;

        for (int i = 0; i < curshit.Length; ++i)
        {
            try
            {
                Renderer rend = curshit[i].transform.GetComponent<Renderer>();
                Texture2D tex = rend.material.mainTexture as Texture2D;
                Vector2 pixelUV = curshit[i].textureCoord;
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;

                if (tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).a != 0f)
                {
                    NameText.text = curshit[i].transform.name;
                    examinable = curshit[i].transform.gameObject.GetComponent<IExaminable>();
                    NamePanel.alpha = 0.9f;
                    _cursoranimator.SetBool("Highlight", true);

                    if (!DialogueRuntimeData.InDialog &&
                        !DialogueRuntimeData.InMenu &&
                        !DialogueRuntimeData.IsWriting &&
                        !UIFade.OnScreenFade)
                    {
                        TriggerSound(1);
                        _cursoranimator.SetTrigger("Click");

                        //Pass: Use ClickAnim as Animation Event
                    }
                    return;
                }
                else
                {
                    NamePanel.alpha = 0f;
                    _cursoranimator.SetBool("Highlight", false);
                }
            }
            catch (UnityException)
            {
                NamePanel.alpha = 0f;
                _cursoranimator.SetBool("Highlight", false);
            }
            catch (NullReferenceException)
            {
                NamePanel.alpha = 0f;
                _cursoranimator.SetBool("Highlight", false);
            }
        }
    }

	public void ClickAnim() //Used as Animation Event
	{
		if (examinable != null)
		{
            Examine();
		}
		else
		{
			Debug.LogError("DREditor (DRFPSCursor): Dialogue File Missing! Please set it up on DR Sprite Properties.");
			return;
		}
	}

	void TriggerSound(int mode)
	{
		GetComponent<AudioSource>().PlayOneShot(SFX[mode]);
	}
}
