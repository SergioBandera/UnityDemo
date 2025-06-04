using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private Rigidbody charRb;
    private Vector2 inputVector;
    private Vector3 moveDirection;
    private PlayerInputActions inputActions;
    private AttachBall attachBallScript;
    private Collider myCol = null;
    public PlayerManager playerManager; // Asigna en el inspector o desde código

    void Awake()
    {
        inputActions = new PlayerInputActions();
        attachBallScript = GetComponent<AttachBall>();
    }
    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Kick.performed += OnKick;
        inputActions.Player.Pass.performed += OnPass;
    }

    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Kick.performed -= OnKick;
        inputActions.Player.Pass.performed -= OnPass;
        inputActions.Player.Disable();
    }

    void Start()
    {

        charRb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();
    }

    void OnMove(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }

    void OnKick(InputAction.CallbackContext context)
    {
        GameObject ball = attachBallScript.GetAttachedBall();

        if (ball != null)
        {
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            Collider ballCol = ball.GetComponent<Collider>();
            Collider myCol = GetComponent<Collider>();

            if (ballRb != null && ballCol != null && myCol != null)
            {
                ballCol.enabled = true;
                Physics.IgnoreCollision(ballCol, myCol, true);

                ballRb.isKinematic = false;

                // Levanta la pelota al chutar
                Vector3 kickDirection = (transform.forward + Vector3.up * 0.3f).normalized;
                float kickForce = 20f;
                ballRb.AddForce(kickDirection * kickForce, ForceMode.Impulse);

                attachBallScript.ClearAttachedBall();
            }
        }
    }

    public void OnPass(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // Solo ejecuta cuando la acción se realiza (no mientras está mantenida)

        if (attachBallScript == null) return;
        GameObject ball = attachBallScript.GetAttachedBall();
        if (ball == null) return;

        // Encuentra el compañero más cercano en la dirección de visión
        PlayerController teammate = FindTeammateInView();
        if (teammate == null) return;

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb == null) return;
        Collider ballCol = ball.GetComponent<Collider>(); if (ballCol == null) return;
        Physics.IgnoreCollision(ballCol, myCol, true);

        // Calcula la dirección y fuerza hacia el pie del compañero
        Vector3 targetPos = teammate.transform.position; // Puedes ajustar con offset al pie si quieres
        Vector3 passDirection = (targetPos - ball.transform.position).normalized;
        float passForce = 12f; // Ajusta la fuerza según distancia

        // Opcional: añade un poco de altura para que no vaya raso
        passDirection += Vector3.up * 0.1f;

        ballRb.isKinematic = false;
        ballRb.AddForce(passDirection.normalized * passForce, ForceMode.Impulse);

        attachBallScript.ClearAttachedBall();
    }

    private PlayerController FindTeammateInView()
    {
        float maxDot = 0.7f; // Ajusta el ángulo de visión (1 = recto, 0 = 90º)
        PlayerController best = null;
        foreach (var player in playerManager.players)
        {
            if (player == this) continue; // No a sí mismo
            Vector3 toPlayer = (player.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, toPlayer);
            if (dot > maxDot)
            {
                maxDot = dot;
                best = player;
            }
        }
        return best;
    }

    void FixedUpdate()
    {
        moveDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        Vector3 moveVelocity = moveDirection * moveSpeed;
        Vector3 newPosition = charRb.position + moveVelocity * Time.fixedDeltaTime;
        charRb.MovePosition(newPosition);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            Quaternion smoothedRotation = Quaternion.Slerp(charRb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            charRb.MoveRotation(smoothedRotation);
        }
    }

    public bool HasBallAttached()
    {
        return attachBallScript != null && attachBallScript.GetAttachedBall() != null;
    }

}
