using UnityEngine;

public class AttachBall : MonoBehaviour
{
    public Transform rightFoot;           // Asignar en inspector el transform del pie derecho
    private GameObject ball = null;
    private Rigidbody ballRb = null;
    private Collider ballCollider = null;
    private float attachCooldown = 0.2f; // Tiempo de espera tras chutar (en segundos)
    private float cooldownTimer = 0f;
    public PlayerManager playerManager; // Asigna en el inspector

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Verifica que colisiona con objeto etiquetado "Pelota" y que el cooldown ha terminado
        if (collision.gameObject.CompareTag("Pelota") && ball == null && cooldownTimer <= 0f)
        {
            ball = collision.gameObject;

            ballRb = ball.GetComponent<Rigidbody>();
            ballCollider = ball.GetComponent<Collider>();

            if (ballRb != null && ballCollider != null)
            {
                ballRb.isKinematic = true;   // Desactiva física para poder moverla manualmente
                ballCollider.enabled = false; // Desactiva colisión para evitar problemas al moverla
            }

            // Notifica al PlayerManager para seleccionar este jugador
            if (playerManager != null)
            {
                playerManager.SelectPlayerWithBall();
            }
        }
    }

    private void FixedUpdate()
    {
        // Si hay pelota atada, mueve la pelota a la posición del pie derecho con un offset si quieres
        if (ball != null && rightFoot != null && ballRb.isKinematic)
        {
            ball.transform.position = rightFoot.position + rightFoot.forward * 0.4f + Vector3.up * 0.17f;
            ball.transform.rotation = rightFoot.rotation;
        }

    }
    public GameObject GetAttachedBall()
    {
        return ball;
    }

    public void ClearAttachedBall()
    {
        if (ball != null)
        {
            // Reactiva la colisión y la física antes de soltar la referencia
            if (ballCollider != null)
                ballCollider.enabled = true;
            if (ballRb != null)
                ballRb.isKinematic = false;

            ball = null;
            ballRb = null;
            ballCollider = null;

            // Activa el cooldown para evitar re-ataque inmediato
            cooldownTimer = attachCooldown;
        }
    }

}
