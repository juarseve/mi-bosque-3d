using UnityEngine;
using UnityEngine.AI; // Necesario para NavMesh

public class SeguirCamino : MonoBehaviour
{
    public Transform[] puntos; // Arrastra aquí tus puntos desde la jerarquía
    private int indiceActual = 0;
    private NavMeshAgent agente;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        IrAlSiguientePunto();
    }

    void Update()
    {
        // Si el animal está cerca del punto actual, va al siguiente
        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            IrAlSiguientePunto();
        }
    }

    void IrAlSiguientePunto()
    {
        if (puntos.Length == 0) return;
        agente.destination = puntos[indiceActual].position;
        indiceActual = (indiceActual + 1) % puntos.Length; // Ciclo infinito
    }
}