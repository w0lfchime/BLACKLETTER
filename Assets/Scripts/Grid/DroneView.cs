using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.XR;
using BL_Grid;

public class DroneView : ViewEntity
{

    [Header("Hover Noise")]
    [SerializeField] private float hoverAmount = 0.03f;
    [SerializeField] private float hoverSpeed = 2f;
    private float noiseOffsetX, noiseOffsetY, noiseOffsetZ;


    void Start()
    {
        noiseOffsetX = Random.value * 100f;
        noiseOffsetY = Random.value * 100f;
        noiseOffsetZ = Random.value * 100f;
    }

    void Update()
    {
        float t = Time.time * hoverSpeed;
        Vector3 offset = new Vector3(
            (Mathf.PerlinNoise(t, noiseOffsetX) - 0.5f) * 2f,
            (Mathf.PerlinNoise(t, noiseOffsetY) - 0.5f) * 2f,
            (Mathf.PerlinNoise(t, noiseOffsetZ) - 0.5f) * 2f
        ) * hoverAmount;

        MeshTransform.localPosition = offset;
    }

    public void MoveDirection(Vector2Int direction)
    {

    }

    [SerializeField] private float tiltAngle = 10f;

    public override void GoToPosition(Vector3Int gridPosition, float time = 0f)
    {
        Vector3 worldPosition = GridView.I.GridToWorld(gridPosition);
        if (time <= 0f)
        {
            transform.position = worldPosition;
        }
        else
        {
            Vector3 dir = (worldPosition - MeshTransform.position).normalized;
            Vector3 tiltAxis = Vector3.Cross(Vector3.up, dir);
            
            transform.DOMove(worldPosition, time).SetEase(Ease.InOutSine);
            transform.DORotate(tiltAxis * tiltAngle, time * 0.5f)
                .SetEase(Ease.OutSine)
                .OnComplete(() => transform.DORotate(Vector3.zero, time * 0.5f).SetEase(Ease.InSine));
        }
    }
    
}