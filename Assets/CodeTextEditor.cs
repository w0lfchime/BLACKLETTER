using UnityEngine;

public class CodeTextEditor : MonoBehaviour
{
    public MeshFilter dragHandle;
    
    private bool isDragging = false;
    private Vector3 offset;
    public Camera mainCamera;

    [Header("Spring Settings")]
    [SerializeField] private float springStiffness = 15f;
    [SerializeField] private float springDamping = 5f;
    private Vector3 velocity;
    private Vector3 targetPos;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverMesh())
            {
                isDragging = true;
                offset = transform.position - GetMouseWorldPosition();
                velocity = Vector3.zero;
                targetPos = transform.position;
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        
        if (isDragging)
        {
            Vector3 newTarget = GetMouseWorldPosition() + offset;
            newTarget.y = transform.position.y;
            targetPos = newTarget;
        }

        // Spring follow — always runs so it settles after release
        Vector3 displacement = targetPos - transform.position;
        Vector3 springForce = displacement * springStiffness - velocity * springDamping;
        velocity += springForce * Time.deltaTime;
        Vector3 pos = transform.position + velocity * Time.deltaTime;
        pos.y = transform.position.y;
        transform.position = pos;
    }

    bool IsMouseOverMesh()
    {
        if (dragHandle == null) return false;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.GetComponent<MeshFilter>() == dragHandle;
        }
        return false;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    void onclick()
    {
        Application.OpenURL("https://blacklettercoding.github.io/BLACKLETTER-CODING/TextIDE/");
    }
}