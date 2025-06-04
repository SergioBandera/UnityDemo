using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public List<PlayerController> players;
    public GameObject ball;
    public int goalkeeperIndex = 0; // Índice del portero en la lista
    private int currentPlayerIndex = 0;

    void Start()
    {
        var playerWithBall = FindPlayerWithBall();
        if (playerWithBall != null)
            SelectPlayer(playerWithBall);
        else
        {
            var closest = GetTwoClosestFieldPlayersToBall();
            if (closest.Count > 0)
                SelectPlayer(closest[0]);
        }
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            // Si hay un jugador con la pelota, lo selecciona
            var playerWithBall = FindPlayerWithBall();
            if (playerWithBall != null)
            {
                SelectPlayer(playerWithBall);
            }
            else if (!players[currentPlayerIndex].HasBallAttached())
            {
                SelectNextPlayer();
            }
        }
    }

    void SelectNextPlayer()
    {
        // Si el portero tiene la pelota, solo él es seleccionable
        if (players[goalkeeperIndex].HasBallAttached())
        {
            SelectPlayer(goalkeeperIndex);
            return;
        }

        var closestIndices = GetTwoClosestFieldPlayersToBall();
        if (closestIndices.Count == 0)
            return;

        int idxInList = closestIndices.IndexOf(currentPlayerIndex);
        int nextIdx;
        if (idxInList == -1)
        {
            nextIdx = closestIndices[0];
        }
        else
        {
            nextIdx = closestIndices[(idxInList + 1) % closestIndices.Count];
        }
        SelectPlayer(nextIdx);
    }

    void SelectPlayer(int index)
    {
        var allowedIndices = GetTwoClosestFieldPlayersToBall();
        // Si el portero tiene la pelota, solo él es seleccionable
        if (players[goalkeeperIndex].HasBallAttached())
        {
            allowedIndices = new List<int> { goalkeeperIndex };
        }

        for (int i = 0; i < players.Count; i++)
        {
            // Solo habilita el jugador seleccionado si está entre los permitidos
            players[i].enabled = (i == index) && allowedIndices.Contains(i);
        }
        currentPlayerIndex = index;
        Debug.Log($"Jugador seleccionado: {index}, permitidos: {string.Join(",", allowedIndices)}");
    }

    void SelectPlayer(PlayerController player)
    {
        int idx = players.IndexOf(player);
        if (idx >= 0)
            SelectPlayer(idx);
    }

    public void SelectPlayerWithBall()
    {
        var playerWithBall = FindPlayerWithBall();
        if (playerWithBall != null)
            SelectPlayer(playerWithBall);
    }

    int FindClosestPlayerToBall()
    {
        float minDist = float.MaxValue;
        int closestIdx = -1;
        for (int i = 0; i < players.Count; i++)
        {
            // Si es el portero y no tiene la pelota, lo saltamos
            if (i == goalkeeperIndex && !players[goalkeeperIndex].HasBallAttached())
                continue;
            float dist = Vector3.Distance(players[i].transform.position, ball.transform.position);
            if (dist < minDist && !players[i].HasBallAttached())
            {
                minDist = dist;
                closestIdx = i;
            }
        }
        // Si no encuentra ninguno, selecciona el portero solo si tiene la pelota
        if (closestIdx == -1 && players[goalkeeperIndex].HasBallAttached())
            return goalkeeperIndex;
        // Si no hay ninguno seleccionable, devuelve 0 por defecto (evita errores)
        return closestIdx != -1 ? closestIdx : 0;
    }

    PlayerController FindPlayerWithBall()
    {
        foreach (var p in players)
        {
            if (p.HasBallAttached())
                return p;
        }
        return null;
    }

    private List<int> GetTwoClosestFieldPlayersToBall()
    {
        // Excluye el portero
        List<int> fieldPlayerIndices = new List<int>();
        for (int i = 0; i < players.Count; i++)
        {
            if (i != goalkeeperIndex)
                fieldPlayerIndices.Add(i);
        }

        // Ordena por distancia a la pelota
        fieldPlayerIndices.Sort((a, b) =>
        {
            float distA = Vector3.Distance(players[a].transform.position, ball.transform.position);
            float distB = Vector3.Distance(players[b].transform.position, ball.transform.position);
            return distA.CompareTo(distB);
        });

        // Devuelve los 2 más cercanos (o menos si hay menos de 2)
        int count = Mathf.Min(2, fieldPlayerIndices.Count);
        return fieldPlayerIndices.GetRange(0, count);
    }
}