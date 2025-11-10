using UnityEngine;
using StarterAssets;

public class TennisBallController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the Net GameObject here to set the target destination.")]
    public Transform targetNet;

    [Tooltip("Drag the Player GameObject (with StarterAssetsInputs) here.")]
    public StarterAssetsInputs playerInput;

    [Header("Ball Settings")]
    [Tooltip("The force applied when the ball is hit (Impulse).")]
    public float hitForce = 15f;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Immediately stop physics to prevent movement on start
        rb.isKinematic = true;
    }

    void Start()
    {
        // Save the ball's starting position and rotation
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Safety check for StarterAssetsInputs
        if (playerInput == null)
        {
            playerInput = FindObjectOfType<StarterAssetsInputs>();
            if (playerInput == null)
                Debug.LogError("StarterAssetsInputs not found in the scene!");
        }

        // Make sure shoot input is false at start
        if (playerInput != null) playerInput.shoot = false;

        // Reset ball to initial state
        ResetBall();
    }

    void Update()
    {
        // Hit the ball using StarterAssetsInputs
        if (playerInput != null && rb.isKinematic && playerInput.shoot)
        {
            playerInput.shoot = false;
            HitBall();
        }

        // Optional: allow mouse click for testing
        if (rb.isKinematic && Input.GetMouseButtonDown(0))
        {
            HitBall();
        }
    }

    public void HitBall()
    {
        // Enable physics so the ball can move
        rb.isKinematic = false;

        // Calculate direction to the net and apply impulse
        if (targetNet != null)
        {
            Vector3 direction = (targetNet.position - transform.position).normalized;
            rb.AddForce(direction * hitForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Target Net not assigned!");
        }
    }

    public void ResetBall()
    {

        // Make kinematic to prevent physics from moving it
        rb.isKinematic = true;

        // Reset position and rotation
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Reset input
        if (playerInput != null) playerInput.shoot = false;
    }
}
