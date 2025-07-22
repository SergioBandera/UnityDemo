using UnityEngine;

public class AttachBall : MonoBehaviour
{
    [SerializeField] private Transform rightFoot; // Asignar en inspector
    [SerializeField] private PlayerManager playerManager; // Asignar en inspector

    private GameObject ball = null;
    private Rigidbody ballRb = null;
    private Collider ballCollider = null;
    private float attachCooldown = 0.2f;
    private float cooldownTimer = 0f;
    public static AttachBall Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pelota") && ball == null && cooldownTimer <= 0f)
        {
            ball = collision.gameObject;
            ballRb = ball.GetComponent<Rigidbody>();
            ballCollider = ball.GetComponent<Collider>();

            ballRb.isKinematic = true;
            ballCollider.enabled = false;
            SetTeamHaveBallByTag(gameObject.tag);
            playerManager?.SelectPlayerWithBall();
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.UpdateBallReferences();
                playerController.SetControlledByPlayer(true); // Ahora el jugador es controlado por el usuario
            }
            Debug.Log($"Jugador {gameObject.name} recibe la pelota en {transform.position}");
        }
    }

    private void FixedUpdate()
    {
        if (ball != null && rightFoot != null && ballRb.isKinematic)
        {
            ball.transform.position = rightFoot.position + rightFoot.forward * 0.4f + Vector3.up * 0.17f;
            ball.transform.rotation = rightFoot.rotation;
        }
    }

    public GameObject GetAttachedBall() => ball;
    public PlayerManager.Team GetTeam()
    {
        return playerManager.teamHaveBall;
    }

    public void ClearAttachedBall()
    {
        if (ball == null) return;

        ballCollider.enabled = true;
        ballRb.isKinematic = false;

        ball = null;
        ballRb = null;
        ballCollider = null;
        SetTeamHaveBallByTag();
        cooldownTimer = attachCooldown;
    }

    public void SetTeamHaveBall(PlayerManager.Team team)
    {
        if (playerManager != null)
        {
            playerManager.teamHaveBall = team;
        }
    }
    public void SetTeamHaveBallByTag(string teamTagName = "None")
    {
        if (playerManager != null)
        {
            if (teamTagName == "BlueTeam")
                SetTeamHaveBall(PlayerManager.Team.BlueTeam);
            else if (teamTagName == "RedTeam")
                SetTeamHaveBall(PlayerManager.Team.RedTeam);
            else
                SetTeamHaveBall(PlayerManager.Team.None);
        }
    }


}