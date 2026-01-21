using Unity.VisualScripting;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class PlayerMoveManager : MonoBehaviour
    {
        public static PlayerMoveManager Instance { get; set; }
        private enum State
        {
            Normal,
            Climb
        }

        [Header("Move")]
        public float moveSpeed = 4f;
        public float rotationSmoothTime = 0.12f;

        [Header("Jump")]
        public float jumpHeight = 1.2f;
        public float gravity = -15f;
        public bool airJumpActive = false;
        public int maxAirJump = 1;

        [Header("Climb")]
        public bool climbActive = false;
        public float climbSpeed = 3f;
        public float wallCheckDistance = 0.6f;
        public float heightThreshold = 1.1f;
        public LayerMask climbableLayer;

        [Header("Camera")]
        public GameObject CinemachineCameraTarget;
        public float topClamp = 70f;
        public float bottomClamp = -70f;

        // ================= INTERNAL =================
        private State _state = State.Normal;

        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private Animator _animator;
        private GameObject _mainCamera;

        private float _verticalVelocity;
        private float _rotationVelocity;
        private int _airJumpLeft;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private float _cinemachineYaw;
        private float _cinemachinePitch;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            airJumpActive = false;
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _animator = GetComponentInChildren<Animator>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif

            _airJumpLeft = maxAirJump;
            _cinemachineYaw = CinemachineCameraTarget.transform.eulerAngles.y;
        }

        private void Update()
        {
            CheckClimb();
            HandleJump();
            ApplyGravity();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        // ================= MOVE =================
        void Move()
        {
            if (_state == State.Climb)
            {
                MoveClimb();
                return;
            }

            Vector3 inputDir = new Vector3(_input.move.x, 0, _input.move.y).normalized;

            if (inputDir != Vector3.zero)
            {
                float targetRotation =
                    Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg +
                    _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    targetRotation,
                    ref _rotationVelocity,
                    rotationSmoothTime);

                transform.rotation = Quaternion.Euler(0, rotation, 0);
            }

            Vector3 moveDir = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;
            Vector3 velocity =
                moveDir * moveSpeed * inputDir.magnitude +
                Vector3.up * _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);

            if (_animator != null)
                _animator.SetBool("SpeedBool", inputDir != Vector3.zero);

            if (_controller.isGrounded)
            {
                _airJumpLeft = maxAirJump;
                if (_verticalVelocity < 0)
                    _verticalVelocity = -2f;

                if (_animator != null)
                    _animator.SetBool("IsGrounded", true);
            }
            else
            {
                if (_animator != null)
                    _animator.SetBool("IsGrounded", false);
            }
        }

        void MoveClimb()
        {
            _controller.Move(Vector3.up * climbSpeed * Time.deltaTime);

            if (CheckWall(out RaycastHit hit))
            {
                Quaternion rot = Quaternion.LookRotation(-hit.normal);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
            }
            else
            {
                ExitClimb();
            }
        }

        // ================= JUMP =================
        void HandleJump()
        {
            if (!_input.jump) return;

            if (_controller.isGrounded)
            {
                Jump();
            }
            else if (_state == State.Climb)
            {
                ExitClimb();
                Jump();
            }
            else if (airJumpActive && _airJumpLeft > 0)
            {
                Jump();
                _airJumpLeft--;
            }

            _input.jump = false;
        }

        void Jump()
        {
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // ================= CLIMB (LIKE ENEMY) =================
        void CheckClimb()
        {
            if (!climbActive || _controller.isGrounded)
                return;

            if (_state == State.Normal && PlayerIsAbove() && CheckWall())
            {
                _verticalVelocity = 0f;
                _state = State.Climb;
            }
        }

        void ExitClimb()
        {
            _state = State.Normal;
        }

        bool PlayerIsAbove()
        {
            return transform.position.y + heightThreshold <
                   transform.position.y + _controller.height;
        }

        bool CheckWall()
        {
            return Physics.Raycast(
                transform.position,
                transform.forward,
                wallCheckDistance,
                climbableLayer
            );
        }

        bool CheckWall(out RaycastHit hit)
        {
            return Physics.Raycast(
                transform.position,
                transform.forward,
                out hit,
                wallCheckDistance,
                climbableLayer
            );
        }

        // ================= GRAVITY =================
        void ApplyGravity()
        {
            if (_state == State.Climb)
                return;

            _verticalVelocity += gravity * Time.deltaTime;
        }

        // ================= CAMERA =================
        void CameraRotation()
        {
            if (_input.look.sqrMagnitude > 0.01f)
            {
                float delta = IsMouse ? 1f : Time.deltaTime;
                _cinemachineYaw += _input.look.x * delta;
                _cinemachinePitch += _input.look.y * delta;
            }

            _cinemachinePitch = Mathf.Clamp(_cinemachinePitch, bottomClamp, topClamp);

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(_cinemachinePitch, _cinemachineYaw, 0);
        }

        bool IsMouse =>
#if ENABLE_INPUT_SYSTEM
            _playerInput.currentControlScheme == "KeyboardMouse";
#else
            false;
#endif
    }
}
