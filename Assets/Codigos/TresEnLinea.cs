using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;   // Necesario para IEnumerator

public class TresEnLinea : MonoBehaviour
{
    public enum Jugador { Ninguno, X, O }
    public Jugador jugadorActual = Jugador.X;

    [Header("Tablero y Casillas")]
    public Jugador[] tablero = new Jugador[9];
    public Button[] casillas;

    [Header("Sprites de Fichas")]
    public Sprite fichaX;
    public Sprite fichaO;

    [Header("UI Mensajes")]
    public TMP_Text textoEstado;

    [Header("Sonidos")]
    public AudioClip sonidoX;   // 🔊 Sonido al poner X
    public AudioClip sonidoO;   // 🔊 Sonido al poner O
    private AudioSource audioSource;

    private bool juegoTerminado = false;
    private bool esperandoTurno = false; // 🔥 Para bloquear clicks mientras esperamos

    // Contadores de puntos
    private int puntosX = 0;
    private int puntosO = 0;

    void Start()
    {
        // 🔊 Obtenemos o agregamos un AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        ReiniciarJuego();
    }

    public void CasillaSeleccionada(int index)
    {
        if (juegoTerminado || esperandoTurno) return;

        if (tablero[index] == Jugador.Ninguno)
        {
            tablero[index] = jugadorActual;

            // Cambiar sprite en el botón
            Image img = casillas[index].GetComponent<Image>();
            img.sprite = (jugadorActual == Jugador.X) ? fichaX : fichaO;

            // 🔊 Reproducir sonido según jugador
            if (jugadorActual == Jugador.X && sonidoX != null)
                audioSource.PlayOneShot(sonidoX);
            else if (jugadorActual == Jugador.O && sonidoO != null)
                audioSource.PlayOneShot(sonidoO);

            // Verificar victoria
            if (VerificarVictoria(jugadorActual))
            {
                if (jugadorActual == Jugador.X) puntosX++;
                else puntosO++;

                textoEstado.text = "¡Ganó " + jugadorActual + "!\nX:" + puntosX + "  O:" + puntosO;
                juegoTerminado = true;

                Invoke("ReiniciarJuego", 2f); // Reinicia después de 2s
                return;
            }

            // Verificar empate
            if (EsEmpate())
            {
                textoEstado.text = "¡Empate!\nX:" + puntosX + "  O:" + puntosO;
                juegoTerminado = true;

                Invoke("ReiniciarJuego", 2f);
                return;
            }

            // 🔥 Iniciar espera antes de cambiar turno
            StartCoroutine(CambiarTurnoConEspera());
        }
    }

    private IEnumerator CambiarTurnoConEspera()
    {
        esperandoTurno = true;

        textoEstado.text = "Esperando 2s...\nX:" + puntosX + "  O:" + puntosO;

        yield return new WaitForSeconds(2f); // ⏳ ahora son 2 segundos

        jugadorActual = (jugadorActual == Jugador.X) ? Jugador.O : Jugador.X;
        textoEstado.text = "Turno de " + jugadorActual + "\nX:" + puntosX + "  O:" + puntosO;

        esperandoTurno = false;
    }

    private bool VerificarVictoria(Jugador jugador)
    {
        int[,] combinaciones = new int[,]
        {
            {0,1,2},{3,4,5},{6,7,8}, // Filas
            {0,3,6},{1,4,7},{2,5,8}, // Columnas
            {0,4,8},{2,4,6}          // Diagonales
        };

        for (int i = 0; i < combinaciones.GetLength(0); i++)
        {
            if (tablero[combinaciones[i, 0]] == jugador &&
                tablero[combinaciones[i, 1]] == jugador &&
                tablero[combinaciones[i, 2]] == jugador)
            {
                return true;
            }
        }
        return false;
    }

    private bool EsEmpate()
    {
        foreach (Jugador j in tablero)
        {
            if (j == Jugador.Ninguno) return false;
        }
        return true;
    }

    public void ReiniciarJuego()
    {
        for (int i = 0; i < tablero.Length; i++)
        {
            tablero[i] = Jugador.Ninguno;
            casillas[i].GetComponent<Image>().sprite = null;

            // Resetear listeners
            int index = i;
            casillas[i].onClick.RemoveAllListeners();
            casillas[i].onClick.AddListener(() => CasillaSeleccionada(index));
        }

        jugadorActual = Jugador.X;
        textoEstado.text = "Turno de X\nX:" + puntosX + "  O:" + puntosO;
        juegoTerminado = false;
        esperandoTurno = false;
    }
}



