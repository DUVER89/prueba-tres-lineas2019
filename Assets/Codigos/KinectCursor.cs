
using UnityEngine;
using UnityEngine.UI;

public class KinectCursor : MonoBehaviour
{
    public static KinectCursor Instance;

    public Transform cursor3D;   // objeto 3D que será el cursor
    public float smooth = 5f;    // suavizado del movimiento
    public float depth = 2f;
    public Transform source;
    public LayerMask layerMask;
    private Vector3 lastHitPoint = Vector3.zero;
    public bool isButton = false;

    private Vector3 cursorWorldPos = Vector3.zero;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Update()
    {
        KinectManager manager = KinectManager.Instance;

        if (manager != null && KinectManager.IsKinectInitialized())
        {
            uint playerId = manager.GetPlayer1ID();

            if (playerId != 0)
            {
                // Obtener posición de la mano derecha en espacio 3D (coordenadas Kinect)
                Vector3 handPos = manager.GetJointPosition(
                    playerId,
                    (int)KinectWrapper.NuiSkeletonPositionIndex.HandRight
                );

                if (handPos != Vector3.zero)
                {
                    // Opcional: proyectar mano frente a la cámara
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(handPos);
                    cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(
                        screenPos.x,
                        screenPos.y,
                        depth // la profundidad que quieras delante de la cámara
                    ));

                    // Mover el cursor suavizado
                    if (cursor3D != null)
                        cursor3D.position = Vector3.Lerp(cursor3D.position, cursorWorldPos, Time.deltaTime * smooth);
                }
            }
        }
    }

    public void SimularClick()
    {
        Debug.Log("Simulando click en El tres en raya");

        Ray ray = new Ray(source.position, source.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, 200, layerMask);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Resetear el punto de impacto
        lastHitPoint = Vector3.zero;
        isButton = false;

        foreach (RaycastHit hit in hits)
        {
            lastHitPoint = hit.point;
            var currentButton = hit.collider.GetComponent<Button>();

            if (currentButton != null)
            {
                isButton = true;
                currentButton.onClick?.Invoke();
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (source == null) return;

        // Dibujar el rayo completo
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(source.position, source.forward * 200);

        // Dibujar el punto de impacto si hay uno
        if (lastHitPoint != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(lastHitPoint, 0.1f);

            // Dibujar una cruz en el punto de impacto
            Gizmos.color = isButton ? Color.green : Color.red;
            float crossSize = 0.15f;
            Gizmos.DrawLine(
                lastHitPoint - Vector3.right * crossSize,
                lastHitPoint + Vector3.right * crossSize
            );
            Gizmos.DrawLine(
                lastHitPoint - Vector3.up * crossSize,
                lastHitPoint + Vector3.up * crossSize
            );
            Gizmos.DrawLine(
                lastHitPoint - Vector3.forward * crossSize,
                lastHitPoint + Vector3.forward * crossSize
            );
        }
    }

    // Opcional: Dibujar gizmos más detallados cuando el objeto está seleccionado
    private void OnDrawGizmosSelected()
    {
        if (source == null) return;

        // Dibujar un cono que muestra la dirección del rayo
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(
            source.position,
            source.rotation,
            Vector3.one
        );
        Gizmos.DrawFrustum(Vector3.zero, 5f, 200f, 0.1f, 1f);
        Gizmos.matrix = Matrix4x4.identity;

        // Dibujar el punto de origen del rayo
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(source.position, 0.05f);
    }
}
