using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace DREditor
{
	public class DRPanelCamera : MonoBehaviour
	{
		private Transform currentTarget;
		private Coroutine cameraLerpRoutine;

		public void SwitchFocus(Transform target, bool immediate, float fov, Vector3 forcedPosition = default)
		{
			var isProp = target.CompareTag("Prop");
			var prevTarget = currentTarget;
			var offsetPosition = forcedPosition;
			var offsetRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));

			if (target == null)
				return;

			//Override offset if the force position is zero
			if (target.TryGetComponent<DRBillboardProperties>(out var bp))
			{
				offsetPosition = forcedPosition == Vector3.zero ? bp.CameraPosition : forcedPosition;
				offsetRotation = Quaternion.Angle(bp.CameraRotation, Quaternion.identity) < 0.01f ? offsetRotation : bp.CameraRotation;
			}

			//Change color of previous character
			if (prevTarget != null || prevTarget == target)
			{
				if (prevTarget.TryGetComponent<DRSpriteBillboard>(out var prevSpb))
					prevSpb.Focus(false);
			}

			currentTarget = target;
			transform.SetParent(currentTarget);

			if (cameraLerpRoutine != null)
				StopCoroutine(cameraLerpRoutine);

			if (immediate) //If skipping, just teleport.
			{
				GetComponent<UnityEngine.Camera>().fieldOfView = fov;
				Debug.Log("Updating location to: " + offsetPosition);
				transform.SetLocalPositionAndRotation(offsetPosition, offsetRotation);
			}
			else //If not skipping, lerp.
			{
				if (cameraLerpRoutine != null)
					StopCoroutine(cameraLerpRoutine);

				cameraLerpRoutine = StartCoroutine(SmoothRefocus(gameObject, offsetPosition, offsetRotation, fov, 0.5f));
			}

			GetComponent<UnityEngine.Camera>().GetUniversalAdditionalCameraData().renderPostProcessing = !isProp;

			//Change color of character
			if (!isProp && target.TryGetComponent<DRSpriteBillboard>(out var spb))
			{
				spb.Focus(true);
			}
		}

		private IEnumerator SmoothRefocus(GameObject camera, Vector3 position, Quaternion rotation, float fov, float speed)
		{
			var cam = camera.GetComponent<UnityEngine.Camera>();
			float InitFOV = cam.fieldOfView;
			float _StartTime = 0;

			while (_StartTime < speed)
			{
				cam.fieldOfView = Mathf.Lerp(InitFOV, fov, _StartTime / speed);
				camera.transform.SetLocalPositionAndRotation(Vector3.Lerp(camera.transform.localPosition, position, (_StartTime / speed)), 
															 Quaternion.Lerp(camera.transform.localRotation, rotation, (_StartTime / speed)));

				_StartTime += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}

			camera.transform.SetLocalPositionAndRotation(position, rotation);
		}
	}
}
