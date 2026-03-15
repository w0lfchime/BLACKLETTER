using UnityEngine;
using DG.Tweening;

namespace GameLogic
{
	public class DroneView : EntityView
	{
		[Header("Hover Noise")]
		[SerializeField] private float hoverAmount = 0.03f;
		[SerializeField] private float hoverSpeed = 2f;
		private float noiseOffsetX, noiseOffsetY, noiseOffsetZ;

		[Header("Tilt")]
		[SerializeField] private float tiltAngle = 10f;

		// active movement lerp
		private bool isMoving;
		private Vector3 moveStartWorld;
		private Vector3 moveTargetWorld;
		private float moveElapsedGameTime;
		private float moveDurationGameTime = 0.001f;
		private float currentAccelerationFactor = 1f;

		void Start()
		{
			noiseOffsetX = Random.value * 100f;
			noiseOffsetY = Random.value * 100f;
			noiseOffsetZ = Random.value * 100f;
		}

		void Update()
		{
			UpdateMoveLerp();
			UpdateHoverNoise();
		}

		void UpdateHoverNoise()
		{
			float t = Time.time * hoverSpeed;
			Vector3 offset = new Vector3(
				(Mathf.PerlinNoise(t, noiseOffsetX) - 0.5f) * 2f,
				(Mathf.PerlinNoise(t, noiseOffsetY) - 0.5f) * 2f,
				(Mathf.PerlinNoise(t, noiseOffsetZ) - 0.5f) * 2f
			) * hoverAmount;

			if (MeshTransform != null)
			{
				MeshTransform.localPosition = offset;
			}
		}

		void UpdateMoveLerp()
		{
			if (!isMoving) return;
			if (Clock.I == null) return;
			if (Clock.I.paused) return;

			moveElapsedGameTime += Time.unscaledDeltaTime * Clock.I.timeScale;

			float t = moveDurationGameTime > 0f ? moveElapsedGameTime / moveDurationGameTime : 1f;
			t = Mathf.Clamp01(t);

			// simple easing with acceleration factor
			float easedT = Mathf.Pow(t, Mathf.Max(0.01f, currentAccelerationFactor));

			transform.position = Vector3.LerpUnclamped(moveStartWorld, moveTargetWorld, easedT);

			// basic tilt while moving
			Vector3 dir = (moveTargetWorld - moveStartWorld).normalized;
			if (dir.sqrMagnitude > 0.0001f)
			{
				Vector3 tiltAxis = Vector3.Cross(Vector3.up, dir);
				transform.rotation = Quaternion.Euler(tiltAxis * tiltAngle * (1f - t));
			}

			if (t >= 1f)
			{
				transform.position = moveTargetWorld;
				transform.rotation = Quaternion.identity;
				isMoving = false;
			}
		}

		public void MoveDirection(Vector2Int direction, int moveDurationTicks, float accelerationFactor, int height = 0)
		{
			if (GridView.I == null) return;

			Vector3Int currentGridPos = GridView.I.WorldToGrid(transform.position);
			Vector3Int targetGridPos = new Vector3Int(
				currentGridPos.x + direction.x,
				height,
				currentGridPos.z + direction.y
			);

			moveStartWorld = transform.position;
			moveTargetWorld = GridView.I.GridToWorld(targetGridPos);

			float tickDt = Clock.I != null ? Clock.I.dt : (1f / 60f);
			moveDurationGameTime = Mathf.Max(0.0001f, moveDurationTicks * tickDt);

			moveElapsedGameTime = 0f;
			currentAccelerationFactor = accelerationFactor <= 0f ? 1f : accelerationFactor;
			isMoving = true;
		}
		public void MoveBetween(Vector2Int startLocation, Vector2Int targetLocation, int moveDurationTicks, float accelerationFactor, int height = 0)
		{
			if (GridView.I == null) return;

			moveStartWorld = GridView.I.GridToWorld(new Vector3Int(startLocation.x, height, startLocation.y));
			moveTargetWorld = GridView.I.GridToWorld(new Vector3Int(targetLocation.x, height, targetLocation.y));

			float tickDt = Clock.I != null ? Clock.I.dt : (1f / 60f);
			moveDurationGameTime = Mathf.Max(0.0001f, moveDurationTicks * tickDt);

			moveElapsedGameTime = 0f;
			currentAccelerationFactor = accelerationFactor <= 0f ? 1f : accelerationFactor;
			isMoving = true;

			transform.position = moveStartWorld;
		}

		public override void GoToPosition(Vector3Int gridPosition, float time = 0f)
		{
			Vector3 worldPosition = GridView.I.GridToWorld(gridPosition);

			isMoving = false;

			if (time <= 0f)
			{
				transform.position = worldPosition;
				transform.rotation = Quaternion.identity;
			}
			else
			{
				Vector3 dir = (worldPosition - transform.position).normalized;
				Vector3 tiltAxis = Vector3.Cross(Vector3.up, dir);

				transform.DOMove(worldPosition, time).SetEase(Ease.InOutSine);
				transform.DORotate(tiltAxis * tiltAngle, time * 0.5f)
					.SetEase(Ease.OutSine)
					.OnComplete(() => transform.DORotate(Vector3.zero, time * 0.5f).SetEase(Ease.InSine));
			}
		}
	}
}