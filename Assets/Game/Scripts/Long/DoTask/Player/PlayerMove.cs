using System;
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
        private enum MovementState
        {
            Grounded,
            Air,
            Wall
        }

        [Header("Player")]
        public float MoveSpeed = 2.0f;
        [Range(0f, 0.3f)] public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        [Header("Jump")]
        public bool ActveAirJump = true;
        public float JumpHeight = 1.2f;
        public int MaxAirJump = 1;

        [Header("Gravity")]
        public float Gravity = -15.0f;
        public float TerminalVelocity = 53.0f;

        [Header("Wall")]
        public bool ActiveClimb = true;
        public float WallCheckDistance = 0.6f;
        public float WallSlideSpeed = -2f;
        public float ClimbSpeed = 2.5f;
        public Vector3 WallJumpForce = new Vector3(6f, 8f, 6f);

        [Header("Timeout")]
        public float JumpTimeout = 0.5f;
        public float FallTimeout = 0.15f;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;

        private MovementState _state;

        private float _speed;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private int _airJumpLeft;

        private Vector3 _wallNormal;

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private Animator _animator;
        private GameObject _mainCamera;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private bool _hasAnimator;
        private const float _threshold = 0.01f;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private void Awake()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _hasAnimator = TryGetComponent(out _animator);

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif

            AssignAnimationIDs();

            _cinemachineTargetYaw = CinemachineCameraTarget.transform.eulerAngles.y;
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            CheckGrounded();
            HandleWallCheck();
            HandleJump();
            HandleWallMovement();
            ApplyGravity();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        // ===================== CORE =====================

        private void CheckGrounded()
        {
            bool grounded = (_controller.collisionFlags & CollisionFlags.Below) != 0;

            if (grounded)
            {
                _state = MovementState.Grounded;
                _airJumpLeft = MaxAirJump;
                _fallTimeoutDelta = FallTimeout;

                if (_verticalVelocity < 0f)
                    _verticalVelocity = -2f;
            }
            else if (_state != MovementState.Wall)
            {
                _state = MovementState.Air;
            }

            if (_hasAnimator)
                _animator.SetBool(_animIDGrounded, grounded);
        }

        private void HandleWallCheck()
        {
            if (_state == MovementState.Grounded)
                return;

            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(origin, transform.forward, out hit, WallCheckDistance))
            {
                if (Vector3.Angle(hit.normal, Vector3.up) > 80f)
                {
                    // VÀO WALL STATE
                    if (_state != MovementState.Wall)
                    {
                        _verticalVelocity = 0f; // 🔥 KHÓA RƠI NGAY LẬP TỨC
                    }

                    _state = MovementState.Wall;
                    _wallNormal = hit.normal;
                    return;
                }
            }

            if (_state == MovementState.Wall)
                _state = MovementState.Air;
        }

        private void HandleWallMovement()
        {
            if (_state != MovementState.Wall)
                return;

            if (ActiveClimb)
            {
                float climbInput = _input.move.y;
                _verticalVelocity = climbInput * ClimbSpeed;
            }
            else
            {
                if (_verticalVelocity < 0f)
                    _verticalVelocity = WallSlideSpeed;
            }
        }

        private void HandleJump()
        {
            if (_state == MovementState.Grounded)
            {
                if (_jumpTimeoutDelta > 0)
                    _jumpTimeoutDelta -= Time.deltaTime;

                if (_input.jump && _jumpTimeoutDelta <= 0f)
                    DoJump();
            }
            else
            {
                if (_input.jump)
                {
                    if (_state == MovementState.Wall)
                    {
                        DoWallJump();
                    }
                    else if (ActveAirJump && _airJumpLeft > 0)
                    {
                        DoJump();
                        _airJumpLeft--;
                    }
                }

                if (_fallTimeoutDelta > 0)
                    _fallTimeoutDelta -= Time.deltaTime;
                else if (_hasAnimator)
                    _animator.SetBool(_animIDFreeFall, true);
            }

            _input.jump = false;
        }

        private void DoJump()
        {
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            _jumpTimeoutDelta = JumpTimeout;

            if (_hasAnimator)
                _animator.SetBool(_animIDJump, true);
        }

        private void DoWallJump()
        {
            Vector3 jumpDir = _wallNormal + Vector3.up;

            _verticalVelocity = WallJumpForce.y;

            Vector3 horizontal =
                new Vector3(jumpDir.x * WallJumpForce.x, 0, jumpDir.z * WallJumpForce.z);

            _controller.Move(horizontal * Time.deltaTime);

            _state = MovementState.Air;
        }

        private void ApplyGravity()
        {
            if (_state == MovementState.Wall && ActiveClimb)
                return;

            if (_verticalVelocity < TerminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        private void Move()
        {
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
            Vector3 velocity = moveDir.normalized * _speed + Vector3.up * _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);

            if (_hasAnimator)
            {
                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, 1f);
            }
        }

        // ===================== CAMERA =====================

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float delta = IsCurrentDeviceMouse ? 1f : Time.deltaTime;
                _cinemachineTargetYaw += _input.look.x * delta;
                _cinemachineTargetPitch += _input.look.y * delta;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0);
        }

        private bool IsCurrentDeviceMouse =>
#if ENABLE_INPUT_SYSTEM
            _playerInput.currentControlScheme == "KeyboardMouse";
#else
            false;
#endif

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
