using System.Collections;
using UnityEngine;

public class PerroAnimacionesKinect3D : MonoBehaviour
{
    public static PerroAnimacionesKinect3D instancia;

    [Header("Animaciones (nombres de estados en la capa 0)")]
    public string[] nombresAnimaciones;

    [Header("Detección (Kinect)")]
    public Transform origenRayo;
    public string nombreObjetoMano = "ManoDerecha";
    public LayerMask mascaraCapas = ~0;

    [Header("Rayo")]
    public float distanciaRayo = 5f;
    public bool mostrarLogs = false;

    [Header("Fallback (si no hay collider)")]
    public bool usarFallbackPorDistancia = true;
    public float distanciaFallback = 0.5f;

    private Animator animador;
    private bool estaReproduciendo = false;
    private int ultimoIndice = -1;

    private void Awake()
    {
        animador = GetComponent<Animator>();

        if (origenRayo == null && !string.IsNullOrEmpty(nombreObjetoMano))
        {
            GameObject go = GameObject.Find(nombreObjetoMano);
            if (go != null) origenRayo = go.transform;
        }
    }

    public void SimularClick()
    {
        if (origenRayo == null)
        {
            if (mostrarLogs) Debug.LogWarning("SimularClick: origenRayo no asignado.");
            return;
        }

        Ray r = new Ray(origenRayo.position, origenRayo.forward);
        RaycastHit[] hits = Physics.RaycastAll(r, distanciaRayo, mascaraCapas);

        bool tocado = false;

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.collider.gameObject == gameObject)
                {
                    tocado = true;
                    break;
                }
            }
        }

        if (!tocado && usarFallbackPorDistancia)
        {
            float distMin = DistanciaMinimaEntreRayoYPoint(r.origin, r.direction.normalized, transform.position, distanciaRayo);
            if (distMin <= distanciaFallback) tocado = true;
        }

        if (tocado) AlTocar();
    }

    private float DistanciaMinimaEntreRayoYPoint(Vector3 origen, Vector3 direccion, Vector3 punto, float maxDist)
    {
        Vector3 op = punto - origen;
        float t = Vector3.Dot(op, direccion);
        t = Mathf.Clamp(t, 0f, maxDist);
        Vector3 puntoCercano = origen + direccion * t;
        return Vector3.Distance(puntoCercano, punto);
    }

    private void AlTocar()
    {
        if (estaReproduciendo) return;
        if (nombresAnimaciones == null || nombresAnimaciones.Length == 0) return;

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
            if (mostrarLogs) Debug.Log($"Toque detectado en '{gameObject.name}', sin Animator. Animación: '{nombre}'");
            return;
        }

        StartCoroutine(ReproducirAnimacionYEsperar(nombre));
    }

    private IEnumerator ReproducirAnimacionYEsperar(string nombreAnimacion)
    {
        if (animador == null) yield break;

        estaReproduciendo = true;
        animador.Play(nombreAnimacion, 0, 0f);

        yield return null;
        AnimatorStateInfo info = animador.GetCurrentAnimatorStateInfo(0);
        int seguridad = 0;
        while (!info.IsName(nombreAnimacion) && seguridad < 60)
        {
            seguridad++;
            yield return null;
            info = animador.GetCurrentAnimatorStateInfo(0);
        }

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
