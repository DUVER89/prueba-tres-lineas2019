using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMainLevel : MonoBehaviour
{
    public bool levelLoaded = false;
    public int scenenumber;

    public GestureListener gesture;

    private void Awake()
    {
        // Usamos FindObjectOfType en lugar de FindFirstObjectByType
        gesture = FindObjectOfType<GestureListener>();
    }

    void Update()
    {
        KinectManager manager = KinectManager.Instance;

        if (!levelLoaded && manager && KinectManager.IsKinectInitialized())
        {
            // Si quieres activar la carga con el gesto, descomenta esta línea
            // if (gesture != null && gesture.IsRiseRightHand())

            levelLoaded = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
