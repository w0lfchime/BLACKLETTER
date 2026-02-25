using BL_Grid;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PointerCast : MonoBehaviour
{
    public Transform mouseObject;
    public float delay = .5f;
    float timer = 0f;
    
    public static GridDirection GetArrowKeyVector()
    {
        if (Input.GetKey(KeyCode.UpArrow)) return GridDirection.North;
        if (Input.GetKey(KeyCode.DownArrow)) return GridDirection.South;
        if (Input.GetKey(KeyCode.RightArrow)) return GridDirection.East;
        if (Input.GetKey(KeyCode.LeftArrow)) return GridDirection.West;
        return GridDirection.Null;
    }
    
    void LateUpdate()
    {
        //if(DroneView.allDrones.Count == 0) 
        //{
        //    Debug.LogWarning("allDrones is empty");
        //    return;
        //}

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            mouseObject.position = hit.point;

            if(Input.GetMouseButton(0))
            {
                //BL_Grid.Grid.I.Drones[0].SingleStepInDirection(GetArrowKeyVector(), 0.5f);
            }

            //DroneView.allDrones[0].MoveDirection(GetArrowKeyVector());
        }
    }

    void FixedUpdate()
    {
        if(GetArrowKeyVector() != GridDirection.Null)
        {
            timer += Time.fixedDeltaTime;
            if(timer >= delay)
            {
                BL_Grid.Grid.I.Drones[0].SingleStepInDirection(GetArrowKeyVector(), delay);
                timer = 0f;
            }
        }
    }
}
