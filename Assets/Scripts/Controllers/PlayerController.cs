using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        // Public Fields

        [Header("Movement Factors")]
        [Range(0.0f, 10000.0f)]
        public float maxSpeed = 10.0f;

        [Range(0.0f, 1000.0f)]
        public float turnSpeed = 50.0f;

        [Range(0.0f, 10.0f)]
        public float jumpHeight = 2.0f;

        [Range(0.0f, 10.0f)]
        public float gravityMultiplier = 3.0f;

        [Range(0.0f, 10.0f)]
        public float groundStickAcceleration = 2.0f;


        [Header("Ground Check")]
        public Transform groundCheck;

        [Range(0.0f, 1.0f)]
        public float groundCheckRadius = 0.3f;

        public LayerMask groundMask;

        public Color groundCheckGizmoColor = Color.cyan;


        // Private Serialized Fields (for showing the state in the inspector)

        [Header("States")]
        [SerializeField]
        private bool isOnGround;

        [SerializeField]
        private Vector3 forward;

        [SerializeField]
        private Vector3 velocity;

        [SerializeField]
        private Vector3 movement;

        [SerializeField]
        private float turnAngle;


        // Public Properties

        public float Gravity => Physics.gravity.y * gravityMultiplier;


        // Private Fields

        private CharacterController controller;

        private Camera mainCamera;


        // Event Functions

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            isOnGround = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

            if (isOnGround && velocity.y < 0.0f)
            {
                velocity.y = -groundStickAcceleration;
            }

            Vector3 cameraForward = mainCamera.transform.forward;
            forward = new Vector3(cameraForward.x, 0.0f, cameraForward.z).normalized;
        }

        private void LateUpdate()
        {
            if (movement != Vector3.zero)
            {
                Vector3 crossOfForwardAndMove = Vector3.Cross(transform.forward, movement.normalized);
                float angle = Mathf.Asin(crossOfForwardAndMove.magnitude) * Mathf.Rad2Deg;

                if (crossOfForwardAndMove.magnitude <= float.Epsilon)
                {
                    angle = 180.0f;
                }

                turnAngle = Mathf.Min(angle, turnSpeed * 180.0f * Time.deltaTime);

                // This should be on the same side of forward if turn left, or on the opposite side if turn right
                Vector3 crossOfMoveAndAngle = Vector3.Cross(movement.normalized, crossOfForwardAndMove);

                // Make the angle negative if the character has to turn right
                if (Vector3.Dot(crossOfMoveAndAngle, transform.forward) < 0.0f)
                {
                    turnAngle -= 1.0f;
                }

                transform.Rotate(Vector3.up, turnAngle);
            }

            velocity.y += Gravity * Time.deltaTime;
            // Vector3 horizontalVelocity = maxSpeed * movement.magnitude * transform.forward;
            Vector3 totalVelocity = movement * maxSpeed + velocity;
            controller.Move(totalVelocity * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = groundCheckGizmoColor;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }


        // PlayerInput Callbacks

        public void OnMove(InputValue value)
        {
            Vector2 value2D = value.Get<Vector2>();

            if (value2D.magnitude <= float.Epsilon)
            {
                movement = Vector3.zero;
                return;
            }

            Vector3 right = Vector3.Cross(Vector3.up, forward);

            value2D = value2D.magnitude * value2D.normalized;
            movement = value2D.x * right + value2D.y * forward;
        }

        public void OnFire(InputValue value)
        {
            Debug.Log("OnFire");
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed && isOnGround)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -groundStickAcceleration * Gravity);
            }
        }
    }
}
