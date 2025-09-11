using System.Collections;
using UnityEngine;

/// <summary>
/// Versión que no obliga a tener Animator ni BoxCollider.
/// Diseñada para usarse con Kinect (llamar SimularClick desde tu código de Kinect).
/// Si no hay collider, puede usar una detección por distancia (fallback) para considerar el toque.
/// </summary>
public class PerroAnimacionesKinect3D : MonoBehaviour
{
    [Header("Animaciones (nombres de estados en la capa 0)")]
    public string[] nombresAnimaciones;

    [Header("Detección (Kinect)")]
    [Tooltip("Transform que representa la mano (pose) entregada por tu script/SDK de Kinect.")]
    public Transform origenRayo;              // asigna aquí la mano desde el SDK Kinect
    public string nombreObjetoMano = "ManoDerecha";
    public LayerMask mascaraCapas = ~0;       // capas que el rayo considerará (por defecto todo)

    [Header("Rayo")]
    public float distanciaRayo = 5f;          // distancia del raycast desde la mano
    public bool mostrarLogs = false;

    [Header("Fallback (si no hay collider)")]
    [Tooltip("Si no hay collider, permite detectar toque si el rayo pasa cerca del transform del perro.")]
    public bool usarFallbackPorDistancia = true;
    [Tooltip("Distancia máxima (m) entre el rayo y la posición del perro para considerar un toque.")]
    public float distanciaFallback = 0.5f;

    // Estado interno
    private Animator animador;                // puede ser null
    private bool estaReproduciendo = false;
    private int ultimoIndice = -1;

    private void Awake()
    {
        // Intentar obtener un Animator si existe (pero ya no es obligatorio)
        animador = GetComponent<Animator>();

        // Intento automático de encontrar el transform de la mano por nombre si no fue asignado
        if (origenRayo == null && !string.IsNullOrEmpty(nombreObjetoMano))
        {
            GameObject go = GameObject.Find(nombreObjetoMano);
            if (go != null) origenRayo = go.transform;
        }

        if (origenRayo == null && mostrarLogs)
            Debug.LogWarning("PerroAnimacionesKinect3D_SinRequisitos: origenRayo no asignado. Asigna el Transform de la mano desde tu SDK Kinect o crea un GameObject con nombre '" + nombreObjetoMano + "'.");
    }

    private void Update()
    {
        // Atajo de prueba en editor: si presionas la tecla Espacio, ejecuta el SimularClick (útil mientras integras Kinect)
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
        {
            SimularClick();
        }
    }

    /// <summary>
    /// Lanza un raycast desde la mano (origenRayo.forward) y, si golpea este objeto (o si el fallback detecta proximidad),
    /// activa la animación aleatoria del perro.
    /// </summary>
    public void SimularClick()
    {
        if (origenRayo == null)
        {
            if (mostrarLogs) Debug.LogWarning("SimularClick: origenRayo no asignado.");
            return;
        }

        if (mostrarLogs) Debug.Log("SimularClick: lanzando rayo desde " + origenRayo.name);

        Ray r = new Ray(origenRayo.position, origenRayo.forward);
        RaycastHit[] hits = Physics.RaycastAll(r, distanciaRayo, mascaraCapas);

        bool tocado = false;

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (mostrarLogs) Debug.Log($"Hit: {hit.collider.gameObject.name} dist:{hit.distance}");

                // Si el rayo golpea este objeto (o un hijo), consideramos que el perro fue tocado
                if (hit.collider.transform.IsChildOf(transform) || hit.collider.gameObject == gameObject)
                {
                    tocado = true;
                    break;
                }
            }
        }

        // Si no hubo impacto directo con colliders del perro y está activado el fallback,
        // comprobamos la distancia mínima entre el rayo y la posición del perro.
        if (!tocado && usarFallbackPorDistancia)
        {
            float distMin = DistanciaMinimaEntreRayoYPoint(r.origin, r.direction.normalized, transform.position, distanciaRayo);
            if (mostrarLogs) Debug.Log($"Distancia mínima rayo->perro: {distMin}");
            if (distMin <= distanciaFallback) tocado = true;
        }

        if (tocado)
        {
            AlTocar();
        }
        else
        {
            if (mostrarLogs) Debug.Log("SimularClick: no se detectó toque sobre el perro.");
        }
    }

    // Calcula la distancia mínima entre un rayo (origen + dirección, limitado por maxDist) y un punto dado.
    private float DistanciaMinimaEntreRayoYPoint(Vector3 origen, Vector3 direccion, Vector3 punto, float maxDist)
    {
        // t = dot(p - o, d)
        Vector3 op = punto - origen;
        float t = Vector3.Dot(op, direccion);
        t = Mathf.Clamp(t, 0f, maxDist);
        Vector3 puntoCercano = origen + direccion * t;
        return Vector3.Distance(puntoCercano, punto);
    }

    // OnMouseDown solo se llamará si hay algún Collider en este GameObject o hijos.
    private void OnMouseDown()
    {
        AlTocar();
    }

    // Método público que activa la animación aleatoria (si hay animador). Si no hay animador, solo registra el toque.
    public void AlTocar()
    {
        if (estaReproduciendo)
        {
            if (mostrarLogs) Debug.Log("Ya hay una animación en curso.");
            return;
        }

        if (nombresAnimaciones == null || nombresAnimaciones.Length == 0)
        {
            if (mostrarLogs) Debug.LogWarning("No hay animaciones configuradas en nombresAnimaciones.");
            return;
        }

        // Elegir índice aleatorio evitando la repetición inmediata
        int indice;
        if (nombresAnimaciones.Length == 1)
        {
            indice = 0;
        }
        else
        {
            int intento = 0;
            do
            {
                indice = Random.Range(0, nombresAnimaciones.Length);
                intento++;
            } while (indice == ultimoIndice && intento < 10);
        }

        ultimoIndice = indice;
        string nombre = nombresAnimaciones[indice];

        if (animador == null)
        {
            // Si no hay Animator, solo logueamos y salimos (o podrías implementar otra lógica).
            if (mostrarLogs) Debug.Log($"Toque detectado en '{gameObject.name}', pero no se encontró Animator. Animación solicitada: '{nombre}'");
            return;
        }

        if (mostrarLogs) Debug.Log($"Perro: reproducir '{nombre}'");
        StartCoroutine(ReproducirAnimacionYEsperar(nombre));
    }

    private IEnumerator ReproducirAnimacionYEsperar(string nombreAnimacion)
    {
        if (animador == null) yield break;

        estaReproduciendo = true;

        animador.Play(nombreAnimacion, 0, 0f);

        // Esperar hasta que el Animator entre en el estado pedido
        yield return null;
        AnimatorStateInfo info = animador.GetCurrentAnimatorStateInfo(0);
        int seguridad = 0;
        while (!info.IsName(nombreAnimacion) && seguridad < 60)
        {
            seguridad++;
            yield return null;
            info = animador.GetCurrentAnimatorStateInfo(0);
        }

        // Si entró al estado, esperar a que la primera pasada termine (normalizedTime >= 1)
        if (info.IsName(nombreAnimacion))
        {
            while (info.IsName(nombreAnimacion) && info.normalizedTime < 1f)
            {
                yield return null;
                info = animador.GetCurrentAnimatorStateInfo(0);
            }
        }

        estaReproduciendo = false;
    }
}
