using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Assign the box or object you want the ball to hit.")]
    public Transform target;

    [Header("Force Settings")]
    [Tooltip("How much force is applied toward the target (Impulse).")]
    public float hitForce = 500f;

    [Header("Arc Settings")]
    [Tooltip("Adds upward force to make the shot more realistic.")]
    public float upwardForce = 3f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            Debug.LogError("No Rigidbody found! Please add a Rigidbody to the ball.");
        if (target == null)
            Debug.LogWarning("No target assigned. Please assign a target in the Inspector.");
    }

    void Update()
    {
        // Fire the ball toward the target when pressing Space or clicking
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && target != null)
        {
            ShootAtTarget();
        }
    }

    void ShootAtTarget()
    {
        // Reset any previous movement
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Compute direction to target
        Vector3 direction = (target.position - transform.position).normalized;

        // Add upward arc (optional, for realism)
        direction.y += upwardForce * 0.01f;

        // Apply impulse force toward the target
        rb.AddForce(direction * hitForce, ForceMode.Impulse);
    }
}
