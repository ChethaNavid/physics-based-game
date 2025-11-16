using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;

        [Header("Camera")]
        [SerializeField] private float cameraSensitivity = 300f;
        [SerializeField] private float cameraFollowSpeed = 10f;

        [Header("Rotation Settings")]
        [SerializeField] private float RotationSpeed = 200f; // degrees per second


        // Player
        private float _speed;
        private float _animationBlend;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // Camera rotation
        private float _cinemachineTargetYaw;
        private float _currentYaw;
        private float _rotationVelocity;

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private bool _hasAnimator;

        private const float _threshold = 0.01f;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            AssignAnimationIDs();
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // Initialize camera rotation
            _cinemachineTargetYaw = transform.eulerAngles.y;
            _currentYaw = _cinemachineTargetYaw;
        }

        private void Update()
        {
            GroundedCheck();
            JumpAndGravity();
            Move();
        }

        private void LateUpdate()
        {
            if (CinemachineCameraTarget != null)
            {
                Vector3 targetPosition = transform.position + Vector3.up * 1.5f;
                CinemachineCameraTarget.transform.position =
                    Vector3.Lerp(CinemachineCameraTarget.transform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);
            }
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
            if (_hasAnimator)
                _animator.SetBool(_animIDGrounded, Grounded);
        }

        public Vector2 look { get; private set; }

        public void OnLook(InputValue value)
        {
            look = value.Get<Vector2>();
        }

        private void CameraRotation()
        {
            if (_input == null || CinemachineCameraTarget == null) return;

            // Only horizontal rotation (yaw)
            _cinemachineTargetYaw += _input.look.x * Time.deltaTime * cameraSensitivity;
            _currentYaw = Mathf.SmoothDampAngle(_currentYaw, _cinemachineTargetYaw, ref _rotationVelocity, RotationSmoothTime);

            // Apply rotation
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
        }

private void Move()
{
    // --- INPUTS ---
    float moveForward = _input.move.y; // W/S
    float moveRight = _input.move.x;   // A/D

    // --- ROTATION ---
    if (Mathf.Abs(moveRight) > 0.01f)
    {
        float rotationAmount = moveRight * RotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotationAmount, 0f);
    }

    // --- SPEED ---
    Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
    float targetSpeed = (_input.sprint ? SprintSpeed : MoveSpeed) * Mathf.Clamp01(new Vector2(moveForward, moveRight).magnitude);
    _speed = Mathf.Lerp(horizontalVelocity.magnitude, targetSpeed, Time.deltaTime * SpeedChangeRate);

    // --- MOVE ---
    Vector3 moveDir = transform.forward * moveForward + transform.right * moveRight;
    moveDir.Normalize();

    Vector3 velocity = moveDir * _speed + new Vector3(0f, _verticalVelocity, 0f);
    _controller.Move(velocity * Time.deltaTime);

    // --- ANIMATIONS ---
    if (_hasAnimator)
    {
        _animator.SetFloat(_animIDSpeed, _speed);
        _animator.SetFloat(_animIDMotionSpeed, new Vector2(moveRight, moveForward).magnitude);
    }
}




        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }
                if (_verticalVelocity < 0f) _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator) _animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;
                if (_fallTimeoutDelta >= 0f) _fallTimeoutDelta -= Time.deltaTime;
                else if (_hasAnimator) _animator.SetBool(_animIDFreeFall, true);
                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0, 1, 0, 0.35f);
            Color transparentRed = new Color(1, 0, 0, 0.35f);
            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}
