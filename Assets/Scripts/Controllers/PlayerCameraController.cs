using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
    [RequireComponent(typeof(PlayerInput))]
    [DisallowMultipleComponent]
    public class PlayerCameraController : MonoBehaviour
    {
        // Public Fields

        [Header("Components")]
        public Transform cameraArm;

        public Transform cameraObject;


        [Header("Position")]
        [Range(0.0f, 10.0f)]
        public float minDistance = 0.5f;

        [Range(0.0f, 50.0f)]
        public float maxDistance = 10.0f;

        [Range(0.0f, 90.0f)]
        public float minPitch = 10.0f;

        [Range(0.0f, 90.0f)]
        public float maxPitch = 80.0f;

        public LayerMask cameraCollisionMask;


        [Header("Sensitivity")]
        [Range(0.0f, 5.0f)]
        public float horizontalMovementSensitivity = 1.0f;

        [Range(0.0f, 5.0f)]
        public float verticalMovementSensitivity = 1.0f;

        [Range(0.0f, 5.0f)]
        public float zoomSpeed = 1.0f;


        // Serialized Private Fields

        [Header("States")]
        [SerializeField]
        private float currentDistance = 5.0f;


        // Private Fields

        private Transform player;

        private float pitch;


        // Event Functions

        private void Update()
        {
            // TODO: Change to a more performance-friendly method for tracking the player game object
            player = GameObject.FindWithTag("Player")?.transform;
        }

        private void LateUpdate()
        {
            Vector3 position = player.position;
            transform.position = position;

            Vector3 direction = -cameraObject.forward;
            Vector3 startPosition = position + minDistance * direction;
            Vector3 endPosition = position + maxDistance * direction;

            if (Physics.Linecast(startPosition, endPosition, out RaycastHit raycastResult, cameraCollisionMask))
            {

                float distance = Mathf.Min(currentDistance, minDistance + raycastResult.distance);
                SetCameraDistance(distance);
            }
            else
            {
                SetCameraDistance(currentDistance);
            }
        }


        // PlayerInput Callbacks

        public void OnLook(InputValue value)
        {
            if (!player)
            {
                return;
            }

            Vector2 value2D = value.Get<Vector2>() * Time.deltaTime;

            transform.Rotate(Vector3.up, value2D.x * horizontalMovementSensitivity);

            pitch -= value2D.y * verticalMovementSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraArm.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        }

        public void OnZoom(InputValue value)
        {
            if (!player)
            {
                return;
            }

            float zoom = value.Get<float>() * Time.deltaTime;
            currentDistance -= zoom * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }


        // Private Methods

        private void SetCameraDistance(float distance)
        {
            cameraObject.localPosition = Vector3.back * distance;
        }
    }
}
