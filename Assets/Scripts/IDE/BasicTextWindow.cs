using UnityEngine;

public class TextIDE : MonoBehaviour
{
    public MeshFilter dragHandle;

    [Header("Spring Damping")]
    [SerializeField] private float springStiffness = 15f;
    [SerializeField] private float springDamping = 8f;
    
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private Vector3 velocity;
    private Vector3 targetPosition;

    void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position;
    }

    void Update()
    {
        // Match camera's X rotation
        Vector3 euler = transform.eulerAngles;
        euler.x = mainCamera.transform.eulerAngles.x;
        transform.eulerAngles = euler;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverMesh())
            {
                isDragging = true;
                offset = transform.position - GetMouseWorldPosition();
                velocity = Vector3.zero;
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        
        if (isDragging)
        {
            Vector3 newPos = GetMouseWorldPosition() + offset;
            newPos.y = transform.position.y;
            targetPosition = newPos;
        }

        // Spring-damped movement
        Vector3 displacement = targetPosition - transform.position;
        Vector3 springForce = displacement * springStiffness - velocity * springDamping;
        velocity += springForce * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
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
