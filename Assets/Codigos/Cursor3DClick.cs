using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cursor3DClick : MonoBehaviour
{
    public Camera uiCamera;
    public float maxDistance = 10f;

    void Start()
    {
        // Asegurar que la cámara tenga PhysicsRaycaster
        if (uiCamera != null && uiCamera.GetComponent<PhysicsRaycaster>() == null)
        {
            uiCamera.gameObject.AddComponent<PhysicsRaycaster>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Reemplazar con gesto de mano cerrada
        {
            PerformUIClick();
        }
    }

    public void PerformUIClick()
    {
        // Dirección CORRECTA: desde el cursor hacia la cámara
        Vector3 directionToCamera = (uiCamera.transform.position - transform.position).normalized;
        Ray ray = new Ray(transform.position, directionToCamera);

        // Alternativa: disparar hacia adelante del cursor si está bien orientado
        // Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Detectar Button
            Button button = hit.collider.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
                return;
            }

            // Detectar otros elementos interactivos
            IPointerClickHandler clickHandler = hit.collider.GetComponent<IPointerClickHandler>();
            if (clickHandler != null)
            {
                // Crear evento de click
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = uiCamera.WorldToScreenPoint(hit.point);
                pointerData.button = PointerEventData.InputButton.Left;

                clickHandler.OnPointerClick(pointerData);
            }
        }
    }

    // Método para ser llamado desde Kinect cuando se detecte mano cerrada
    public void OnHandClosedGesture()
    {
        PerformUIClick();
    }
}
