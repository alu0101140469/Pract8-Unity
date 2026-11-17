using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarBehaviour : MonoBehaviour
{
    public ReticleBehaviour Reticle;
    public ScoreManager ScoreManager;
    public float Speed = 1.2f;
    public float TurnSpeed = 720f;

    private Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        // El coche se mueve por código siguiendo la retícula, no por fuerzas físicas.
        body.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (Reticle == null || body == null)
            return;

        Vector3 targetPosition = Reticle.transform.position;
        Vector3 direction = targetPosition - body.position;
        Vector3 flatDirection = Vector3.ProjectOnPlane(direction, Vector3.up);

        if (flatDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
            Quaternion nextRotation = Quaternion.RotateTowards(
                body.rotation,
                targetRotation,
                TurnSpeed * Time.fixedDeltaTime
            );
            body.MoveRotation(nextRotation);
        }

        body.MovePosition(Vector3.MoveTowards(
            body.position,
            targetPosition,
            Speed * Time.fixedDeltaTime
        ));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        PackageBehaviour package = other.GetComponentInParent<PackageBehaviour>();
        if (package != null)
        {
            ChangeScore(1);
            Destroy(package.gameObject);
            return;
        }

        // Busca el tag Spider en el propio collider o en cualquiera de sus padres.
        Transform current = other.transform;
        while (current != null)
        {
            if (current.CompareTag("Spider"))
            {
                ChangeScore(-1);
                Destroy(current.gameObject);
                return;
            }
            current = current.parent;
        }
    }

    private void ChangeScore(int amount)
    {
        if (ScoreManager != null)
        {
            ScoreManager.AddPoints(amount);
        }
        else
        {
            Debug.LogError("CarBehaviour: no se ha asignado ScoreManager al coche.", this);
        }
    }
}