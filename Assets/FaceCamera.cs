using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform Camera;
    void LateUpdate()
    {
        transform.forward = Camera.transform.forward;
    }
}
