using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private Vector2 inputVector;
    private Vector3 moveDirection;
    private PlayerInputActions inputActions;
    private AttachBall attachBallScript;
    public PlayerManager playerManager; // Asigna en el inspector o desde código
    private GameObject _ball;
    private Rigidbody _charRb;
    private Rigidbody _ballRb;
    private Collider _ballCol;
    private Collider _myCol;


    void Awake()
    {
        inputActions = new PlayerInputActions();
        attachBallScript = GetComponent<AttachBall>();
        _ball = attachBallScript.GetAttachedBall();
        _myCol = GetComponent<Collider>();
        _charRb = GetComponent<Rigidbody>();
        if (_ball != null)
        {
            _ballRb = _ball.GetComponent<Rigidbody>();
            _ballCol = _ball.GetComponent<Collider>();
        }
    }
    // on enable on disable y on move son keybind del input system, se ejecutan cuando el jugador presiona o suelta las teclas asignadas
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

    void OnMove(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }

    void OnKick(InputAction.CallbackContext context)
    {

        if (_ball != null)
        {
            if (_ballRb != null && _ballCol != null && _myCol != null)
            {
                _ballCol.enabled = true;
                Physics.IgnoreCollision(_ballCol, _myCol, true);
                _ballRb.isKinematic = false;

                // Levanta la pelota al chutar
                Vector3 kickDirection = (transform.forward + Vector3.up * 0.3f).normalized;
                float kickForce = 20f;
                _ballRb.AddForce(kickDirection * kickForce, ForceMode.Impulse);
                attachBallScript.ClearAttachedBall();
                UpdateBallReferences();
                SetControlledByAI();
            }
        }
    }

    public void OnPass(InputAction.CallbackContext context)
    {
        Debug.Log("Intentando pasar la pelota");
        if (!context.performed) return; // Solo ejecuta cuando la acción se realiza (no mientras está mantenida)
        if (attachBallScript == null) return;
        if (_ball == null) return;

        // Encuentra el compañero más cercano en la dirección de visión, si no hay compañero, no hay pase
        PlayerController teammate = FindTeammateInView();
        if (teammate == null) return;
        if (_ballRb == null) return;
        if (_ballCol == null) return;
        Physics.IgnoreCollision(_ballCol, _myCol, true);

        // Calcula la dirección y fuerza hacia el pie del compañero
        Vector3 targetPos = teammate.transform.position; // Puedes ajustar con offset al pie si quieres
        Vector3 passDirection = (targetPos - _ball.transform.position).normalized;
        float passForce = 12f; // Ajusta la fuerza según distancia
        // Opcional: añade un poco de altura para que no vaya raso
        passDirection += Vector3.up * 0.1f;
        _ballRb.isKinematic = false;
        _ballRb.AddForce(passDirection.normalized * passForce, ForceMode.Impulse);
        attachBallScript.ClearAttachedBall();
        UpdateBallReferences();
        SetControlledByAI();
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
        Vector3 newPosition = _charRb.position + moveVelocity * Time.fixedDeltaTime;
        _charRb.MovePosition(newPosition);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            Quaternion smoothedRotation = Quaternion.Slerp(_charRb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            _charRb.MoveRotation(smoothedRotation);
        }
    }

    public bool HasBallAttached()
    {
        return attachBallScript != null && attachBallScript.GetAttachedBall() != null;
    }

    public void UpdateBallReferences()
    {
        _ball = attachBallScript.GetAttachedBall();
        if (_ball != null)
        {
            _ballRb = _ball.GetComponent<Rigidbody>();
            _ballCol = _ball.GetComponent<Collider>();
        }
        else
        {
            _ballRb = null;
            _ballCol = null;
        }
    }

    public void SetControlledByPlayer(bool controlled)
    {
        var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
            navAgent.enabled = !controlled; // Desactiva NavMeshAgent si es controlado por el jugador

        if (_charRb != null)
        {
            _charRb.isKinematic = false; // Activa física
            _charRb.useGravity = true;
        }
    }

    public void SetControlledByAI()
    {
        var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            // Comprobar si la posición está en el NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                navAgent.enabled = true;
                transform.position = hit.position; // Asegura que está sobre el NavMesh
            }
            else
            {
                Debug.LogWarning("Jugador fuera del NavMesh, no se activa NavMeshAgent.");
                navAgent.enabled = false;
            }
        }

        if (_charRb != null)
            _charRb.isKinematic = true; // Desactiva física si lo mueve la IA
    }
}
