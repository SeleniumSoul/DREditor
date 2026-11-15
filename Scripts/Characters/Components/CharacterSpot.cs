using DREditor;
using DREditor.Characters;
using DREditor.Dialogues;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(Animation))]
public class CharacterSpot : MonoBehaviour
{
	private const string CHAR_ENABLE = "char_enable";
	private const string CHAR_DISABLE = "char_leave";

	[SerializeField] private int SpotID;
    [SerializeField] private bool PointAndClickMode;

	[Module]
    private DRRoomSetup roomSetup;
	private Animation animationComponent;
    private GameObject character;

    public bool AvailableSlot => character == null;
    public GameObject CurrentCharacter => character;
    public bool TemporaryCharacter { get; private set; }

    private void Start()
	{
		DRModuleSystem.Inject(this);
        animationComponent = GetComponent<Animation>();

        if (roomSetup != null)
			PointAndClickMode = roomSetup.SceneType == SceneType.PointAndClick;
	}

	public async Task LoadCharacter(int clusterID, Character character, Dialogue[] dialogues, bool temp)
	{
		//if (CharacterLog.LoadedCharacters.ContainsKey(character))
		//{
		//	Debug.Log($"{character.FullName(false)} is already loaded.");
		//	return;
		//}

		//var handle = Addressables.InstantiateAsync(character.CharacterPrefab, transform);
		//await handle.Task;

		var handle = Addressables.LoadAssetAsync<GameObject>(character.CharacterPrefab);
        await handle.Task;

        var loaded = Instantiate(handle.Result, transform);
        var loadedProperties = loaded.GetComponent<DRBillboardProperties>();

        if (PointAndClickMode)
        {
            //stupid hack to make it not flicker on load
            loaded.SetActive(false);
            animationComponent.Play(CHAR_ENABLE);
            animationComponent.Sample();
        }

        TemporaryCharacter = temp;
        loaded.name = handle.Result.name;
        loadedProperties.ClusterID = clusterID;
        loadedProperties.AddThreads(dialogues);

		if (PointAndClickMode)
		{
            loaded.SetActive(true);
            Debug.Log($"Popping out {character.FullName(false)}!");
            while (animationComponent.IsPlaying(CHAR_ENABLE))
				await Task.Yield();
		}
	}

	public void ChangeCharacter(GameObject chara)
	{
		character = chara;
		animationComponent.Play(CHAR_ENABLE);
	}

	public void Disable()
	{
		StartCoroutine(DisableAnim());
	}

	private IEnumerator DisableAnim()
	{
		if (character == null) character = gameObject.transform.GetChild(0).GetChild(0).gameObject;

		animationComponent.Play(CHAR_DISABLE);

		yield return new WaitForSeconds(animationComponent.GetClip(CHAR_DISABLE).length);

		Destroy(character);
		yield return null;
	}
}