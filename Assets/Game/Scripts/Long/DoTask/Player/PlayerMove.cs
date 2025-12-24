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
    public class PlayerMove : MonoBehaviour
    {
        // ===================== STATE =====================
        private enum MovementState
        {
            Grounded,
            Air,
            Climb
        }

        // ===================== CONFIG =====================
        [Header("Move")]
        public float MoveSpeed = 4f;
        public float SpeedChangeRate = 10f;
        public float RotationSmoothTime = 0.12f;

        [Header("Jump")]
        public float JumpHeight = 1.2f;
        public bool EnableAirJump = true;
        public int MaxAirJump = 1;

        [Header("Gravity")]
        public float Gravity = -15f;
        public float TerminalVelocity = 53f;

        [Header("Climb")]
        public bool ClimbActive = true;
        public float ClimbSpeed = 2.5f;
        public float WallCheckDistance = 0.6f;

        [Header("Camera")]
        public GameObject CinemachineCameraTarget;
        public float GroundTopClamp = 70f;
        public float GroundBottomClamp = -70f;
        public float ClimbTopClamp = 30f;
        public float ClimbBottomClamp = -30f;

        // ===================== INTERNAL =====================
        private MovementState _state = MovementState.Air;

        private float _speed;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;

        private int _airJumpLeft;
        private Vector3 _wallNormal;
        private Vector3 _externalVelocity;

        private float _cinemachineYaw;
        private float _cinemachinePitch;

        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private const float _threshold = 0.01f;

        // ===================== UNITY =====================
        private void Awake()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif

            _airJumpLeft = MaxAirJump;
            _cinemachineYaw = CinemachineCameraTarget.transform.eulerAngles.y;
        }

        private void Update()
        {
            CheckGrounded();
            CheckClimb();
            HandleJump();
            ApplyGravity();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        // ===================== STATE CHECK =====================
        private void CheckGrounded()
        {
            bool grounded = (_controller.collisionFlags & CollisionFlags.Below) != 0;

            if (grounded)
            {
                _state = MovementState.Grounded;
                _airJumpLeft = MaxAirJump;

                if (_verticalVelocity < 0f)
                    _verticalVelocity = -2f;
            }
            else if (_state != MovementState.Climb)
            {
                _state = MovementState.Air;
            }
        }

        private void CheckClimb()
        {
            if (_state == MovementState.Grounded)
                return;

            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 1.0f;

            if (Physics.Raycast(origin, transform.forward, out hit, WallCheckDistance))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (angle > 80f && angle < 100f)
                {
                    if (_state != MovementState.Climb)
                        EnterClimb(hit);
                    return;
                }
            }

            if (_state == MovementState.Climb)
                ExitClimb();
        }

        private void EnterClimb(RaycastHit hit)
        {
            _state = MovementState.Climb;
            _wallNormal = hit.normal;
            _verticalVelocity = 0f;
        }

        private void ExitClimb()
        {
            _state = MovementState.Air;
        }

        // ===================== MOVE =====================
        private void Move()
        {
            if (_state == MovementState.Climb && ClimbActive)
            {
                MoveClimb();
                return;
            }

            float targetSpeed = _input.move == Vector2.zero ? 0f : MoveSpeed;
            float currentSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

            _speed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);

            Vector3 inputDir = new Vector3(_input.move.x, 0, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0, rotation, 0);
            }

            Vector3 moveDir = Quaternion.Euler(0, _targetRotation, 0) * Vector3.forward;
            Vector3 velocity = moveDir.normalized * _speed + Vector3.up * _verticalVelocity + _externalVelocity;

            _controller.Move(velocity * Time.deltaTime);
            _externalVelocity = Vector3.Lerp(_externalVelocity, Vector3.zero, Time.deltaTime * 6f);
        }

        private void MoveClimb()
        {
            Vector3 climbUp = Vector3.ProjectOnPlane(Vector3.up, _wallNormal).normalized;
            Vector3 climbRight = Vector3.Cross(_wallNormal, climbUp);

            Vector3 move =
                climbUp * _input.move.y +
                climbRight * _input.move.x;

            _controller.Move(move * ClimbSpeed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(-_wallNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // ===================== JUMP =====================
        private void HandleJump()
        {
            if (!_input.jump) return;

            if (_state == MovementState.Grounded)
            {
                DoJump();
            }
            else if (_state == MovementState.Climb)
            {
                DoClimbJump();
            }
            else if (_state == MovementState.Air && EnableAirJump && _airJumpLeft > 0)
            {
                DoJump();
                _airJumpLeft--;
            }

            _input.jump = false;
        }

        private void DoJump()
        {
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }

        private void DoClimbJump()
        {
            // Vector3 jumpDir = (_wallNormal + Vector3.up).normalized;
            // _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

            // _controller.Move(jumpDir * 2f);
            // nhảy ra khỏi tường, không teleport
            Vector3 jumpOut = _wallNormal * 3f;

            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

            // lưu lực đẩy ngang (xử lý ở Move)
            _externalVelocity = jumpOut;
            ExitClimb();
        }

        // ===================== GRAVITY =====================
        private void ApplyGravity()
        {
            if (_state == MovementState.Climb)
                return;

            if (_verticalVelocity < TerminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        // ===================== CAMERA =====================
        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold)
            {
                float delta = IsMouse ? 1f : Time.deltaTime;
                _cinemachineYaw += _input.look.x * delta;
                _cinemachinePitch += _input.look.y * delta;
            }

            float top = _state == MovementState.Climb ? ClimbTopClamp : GroundTopClamp;
            float bottom = _state == MovementState.Climb ? ClimbBottomClamp : GroundBottomClamp;

            _cinemachinePitch = ClampAngle(_cinemachinePitch, bottom, top);

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(_cinemachinePitch, _cinemachineYaw, 0);
        }

        private bool IsMouse =>
#if ENABLE_INPUT_SYSTEM
            _playerInput.currentControlScheme == "KeyboardMouse";
#else
            false;
#endif

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
