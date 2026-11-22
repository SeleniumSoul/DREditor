using UnityEngine;

namespace DREditor
{
	[DefaultExecutionOrder(-1000)]
	public class ModuleInitializer : MonoBehaviour
	{
		private bool loadedFromStart = false;

		private async void Awake()
		{
			if (GameObject.FindGameObjectsWithTag("System").Length > 1)
			{
				loadedFromStart = true;
				Destroy(gameObject);
			}
			else
				DontDestroyOnLoad(this);

			var components = GetComponents<Component>();
			var moduleHubMethod = typeof(DRModuleSystem).GetMethod(nameof(DRModuleSystem.RegisterModule));

			foreach (var component in components)
			{
				var type = component.GetType();
				if (type == typeof(ModuleInitializer) || type == typeof(Transform))
					continue;

				moduleHubMethod.MakeGenericMethod(type).Invoke(null, new object[] { component });
				Debug.Log($"DREditor (DRModuleSystem): Registering {type}");

				if (component is IModule c)
					await c.Init();
			}

			if (!loadedFromStart)
			{
				var room = GameObject.FindGameObjectWithTag("Room");
				if (room != null && room.TryGetComponent<DRRoomSetup>(out var setup))
				{
					setup.SetupScene();
				}
			}
		}
	}
}
