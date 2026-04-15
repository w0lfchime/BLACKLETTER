using UnityEngine;

public class CanvasTransform : MonoBehaviour
{
    public Camera2 camera;
    public Transform scaleTransform;
    public float mult = 1;
    public float scaleMult = 1;
    void LateUpdate()
    {
        Vector3 camPos = camera.cameraPivot.position;
        float scaleFactor = scaleMult / camera.currentZoomDistance;

        // Parent sits at camera XZ, scaled inversely to zoom — content grows around camera point
        scaleTransform.position = new Vector3(camPos.x, scaleTransform.position.y, camPos.z);
        scaleTransform.localScale = Vector3.one * scaleFactor;

        // Child offsets to cancel the parent move — divide by scale so world position stays fixed
        transform.localPosition = new Vector3(-camPos.x, -camPos.z, 0f) * mult / scaleFactor;
    }
}
