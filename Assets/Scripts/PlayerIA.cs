using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(NavMeshAgent))]

public class PlayerIA : MonoBehaviour
{
    public Transform ballTarget;
    public List<Transform> teamPlayers;
    Vector3 targetPosition;
    private AttachBall _attachBallScript;


    void Start()
    {
        _attachBallScript = GetComponent<AttachBall>();
    }
    void Update()
    {
        if (_attachBallScript != null)
        {
            PlayerManager.Team whoHaveBall = _attachBallScript.GetTeam();
            if (whoHaveBall != PlayerManager.Team.BlueTeam)
            {
                MoveClosestPlayerToBall();
            }

        }

    }
    //esto solo deberia funcionar en el equipo de la IA 
    private void MoveClosestPlayerToBall()
    {
        float closestDistance = Mathf.Infinity;
        NavMeshAgent closestPlayer = null;

        foreach (Transform player in teamPlayers)
        {
            float playerDistance = Vector3.Distance(player.position, ballTarget.position);
            if (playerDistance < closestDistance)
            {
                closestDistance = playerDistance;
                closestPlayer = player.GetComponent<NavMeshAgent>(); ;
            }
        }
        //esto tengo q pulirlo, aveces desaparecen los personajes xD
        if (!closestPlayer.enabled || !closestPlayer.isOnNavMesh) return;
        else if (Vector3.Distance(targetPosition, ballTarget.position) > 1.0f)
        {

            targetPosition = ballTarget.position;
            closestPlayer.SetDestination(targetPosition);
        }
    }

}