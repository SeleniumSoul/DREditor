//FPSCursor script by SeleniumSoul for DRDistrust
using UnityEngine;
using TMPro;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System;
using DREditor;

public class DRFPSCursor : MonoBehaviour
{
	public AudioClip[] SFX = new AudioClip[2];
	public TextMeshProUGUI NameText;
	public CanvasGroup NamePanel;
	private Animator _cursoranimator;
	private Camera _maincamera;

	public Dialogue DialogFile;

	[Header("Raycasting")]
    [SerializeField] private LayerMask RaycastMask;
    [SerializeField] private float RaycastDistance = 5f;
	private RaycastHit[] _curshit = new RaycastHit[5];
	private Ray _cursray;
	private DRBillboardProperties _properties;
	//private DRInput _controls;

	private void Awake()
	{
		//_controls = new DRInput();
	}

    void Start()
	{
		if (DialoguePlayer.current.DRCursor == null)
			DialoguePlayer.current.DRCursor = gameObject;

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

	void PassThreadToHandler()
	{
        if (_properties)
        {
            //TODO: Polish this.
            DialogueRuntimeData.CurrentCluster = GameObject.FindGameObjectWithTag("Director").GetComponent<MapSetup>().GetCluster(_properties.ClusterID);
            DialogueRuntimeData.DialogueAsset = _properties.DialogThreadPass();
        }
        else Debug.LogError("DREditor (PassThreadToHandler): Unable to find properties.");
	}

	void Update()
	{
		if (_maincamera == null) _maincamera = Camera.main;
		_cursray = _maincamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		var amount = Physics.RaycastNonAlloc(_cursray, _curshit, RaycastDistance, RaycastMask);//, 1 << 10 | 1 << 12);

        if (amount <= 0) return;
        if (_curshit[0].collider.gameObject.layer == 3) return;

        for (int i = 0; i < _curshit.Length; ++i)
        {
            try
            {
                Renderer rend = _curshit[i].transform.GetComponent<Renderer>();
                Texture2D tex = rend.material.mainTexture as Texture2D;
                Vector2 pixelUV = _curshit[i].textureCoord;
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;

                if (tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).a != 0f)
                {
                    NameText.text = _curshit[i].transform.name;
                    _properties = _curshit[i].transform.gameObject.GetComponent<DRBillboardProperties>();
                    NamePanel.alpha = 0.9f;
                    _cursoranimator.SetBool("Highlight", true);

                    if (//_controls.Global.Confirm.triggered &&
                        !DialogueRuntimeData.InDialog &&
                        !DialogueRuntimeData.InMenu &&
                        !DialogueRuntimeData.IsWriting &&
                        !UIFade.OnScreenFade)
                    {
                        DialogueRuntimeData.InDialog = true;
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
            //if (tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).a != 0f)
            //{
            //	NameText.text = _curshit[i].transform.name;
            //	_properties = _curshit[i].transform.gameObject.GetComponent<DRBillboardProperties>();
            //	NamePanel.alpha = 0.9f;
            //	_cursoranimator.SetBool("Highlight", true);

            //	if (_controls.Global.Confirm.triggered &&
            //		!DialogueHandler.InDialog &&
            //		!DialogueHandler.InMenu &&
            //		!DialogueHandler.IsWriting &&
            //		!UIFade.OnScreenFade)
            //	{
            //		DialogueHandler.InDialog = true;
            //		TriggerSound(1);
            //		_cursoranimator.SetTrigger("Click");

            //		//Pass: Use ClickAnim as Animation Event
            //	}
            //	return;
            //}
            //else
            //{
            //	NamePanel.alpha = 0f;
            //	_cursoranimator.SetBool("Highlight", false);
            //}
        }
    }

	public void ClickAnim() //Used as Animation Event
	{
		if (_properties != null && _properties.DialogThreadList[0] != null && _properties.DialogThreadList.Count != 0)
		{
			PassThreadToHandler();
			DialogueEventSystem.TriggerEvent("StartDialogue");
		}
		else
		{
			DialogueRuntimeData.InDialog = false;
			Debug.LogError("DREditor (DRFPSCursor): Dialogue File Missing! Please set it up on DR Sprite Properties.");
			return;
		}
	}

	void TriggerSound(int mode)
	{
		GetComponent<AudioSource>().PlayOneShot(SFX[mode]);
	}
}
