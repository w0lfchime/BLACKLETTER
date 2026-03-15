using GameLogic;
using UnityEngine;
using UnityEngineInternal;

public abstract class EntityView : MonoBehaviour
{
    public Transform MeshTransform;

    //function 
    public virtual void GoToPosition(Vector3Int gridPosition, float time = 0f)
    {
        Vector3 worldPosition = GridView.I.GridToWorld(gridPosition);
        transform.position = worldPosition;

    }

}