using UnityEngine;
using UnityEngine.UIElements;

namespace JowseyRTSCamera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("Toggle using keyboard controls to move the camera")]
        public bool enableKeyboardControls = true;
        
        [Tooltip("Toggle using mouse controls to move the camera")]
        public bool enableMouseControls = true;
        
        [Tooltip("Toggle panning the camera along the world")]
        public bool enablePanning = true;

        [Tooltip("Toggle rotating the camera horizontally")]
        public bool enableHorizontalRotation = true;

        [Tooltip("Toggle rotating the camera vertically")]
        public bool enableVerticalRotation = true;

        [Tooltip("Toggle zooming in and out")]
        public bool enableZooming = true;

        [Tooltip("Toggle holding the boost key to multiply panning speed")]
        public bool enableBoosting = true;
        
        [Tooltip("Speed at which panning, zooming, and rotating will stop when input is released. Higher stops quicker.")]
        public float easeSpeed = 8;
        
        private Transform xRotRig;
        private Camera cam;
        
        [Header("Panning")]
        [Tooltip("Base panning speed in units/s")]
        public float movementSpeed = 20;

        [Tooltip("Multiplier applied to panning speed when boost key is held")]
        public float boostSpeedMultiplier = 2;

        [Tooltip("When this key is held, panning speed is multiplied by the multiplier")]
        public KeyCode boostKey = KeyCode.LeftShift;
        
        private Vector3 intendedPosition;
        private Vector3 dragStartPosition;
        private Vector3 dragCurrentPosition;

        [Header("Rotation")]
        [Tooltip("How fast, in degrees/s, the camera will rotate when using keyboard controls")]
        public float rotationSpeed = 180;
        
        [Tooltip("Minimum rotation angle in degrees")]
        public float minVerticalRotation = -35;

        [Tooltip("Maximum rotation angle in degrees")]
        public float maxVerticalRotation = 40;
        
        [Tooltip("Allows camera to rotate 360 degrees vertically. Disabling ignores min/max vertical rotation")]
        public bool clampVerticalRotation = true;

        private Vector3 mouseRotateStartPosition;
        private Vector3 mouseRotateCurrentPosition;

        private float xRotation;
        private float yRotation;

        [Header("Zooming")]
        [Tooltip("Each time the scroll wheel is used, the camera will zoom in/out by this many units")]
        public float zoomSpeed = 6;

        [Tooltip("Minimum units from the camera's ground point")]
        public float minZoom = 4;

        [Tooltip("Maximum units from the camera's ground point")]
        public float maxZoom = 32;

        private Vector3 intendedZoom;

        private void Awake()
        {
            xRotRig = transform.GetChild(0);
            cam = xRotRig.GetChild(0).GetComponent<Camera>();
        }
        
        private void Start()
        {
            intendedPosition = transform.position;
            intendedZoom = cam.transform.localPosition;
        }

        private void LateUpdate()
        {
            if(enableMouseControls) HandleMouseInput();
            if(enableKeyboardControls) HandleKeyboardInput();

            HandleMovement();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(intendedPosition, 0.1f);
        }

        private void HandleMouseInput()
        {
            // Mouse drag panning
            if(enablePanning)
            {
                if(Input.GetMouseButtonDown((int) MouseButton.MiddleMouse))
                {
                    var plane = new Plane(Vector3.up, Vector3.zero);
                    var ray = cam.ScreenPointToRay(Input.mousePosition);

                    if(plane.Raycast(ray, out var hit)) dragStartPosition = ray.GetPoint(hit);
                }

                if(Input.GetMouseButton((int) MouseButton.MiddleMouse))
                {
                    var plane = new Plane(Vector3.up, Vector3.zero);
                    var ray = cam.ScreenPointToRay(Input.mousePosition);

                    if(!plane.Raycast(ray, out var hit)) return;

                    dragCurrentPosition = ray.GetPoint(hit);
                    intendedPosition = transform.position + (dragStartPosition - dragCurrentPosition);
                }
            }

            // Mouse scroll
            if(enableZooming && Input.mouseScrollDelta.y != 0) intendedZoom += new Vector3(0, -1, 1) * (Input.mouseScrollDelta.y * zoomSpeed);

            // Mouse rotation
            if(Input.GetMouseButtonDown((int) MouseButton.RightMouse)) mouseRotateStartPosition = Input.mousePosition;

            if(Input.GetMouseButton((int) MouseButton.RightMouse))
            {
                mouseRotateCurrentPosition = Input.mousePosition;
                var delta = mouseRotateStartPosition - mouseRotateCurrentPosition;

                if(enableVerticalRotation) xRotation += delta.y / 4f;
                if(enableHorizontalRotation) yRotation -= delta.x / 4f;

                mouseRotateStartPosition = mouseRotateCurrentPosition;
            }
        }

        private void HandleKeyboardInput()
        {
            // Panning
            if(enablePanning)
            {
                var hor = Input.GetAxisRaw("Horizontal");
                var ver = Input.GetAxisRaw("Vertical");

                var speed = movementSpeed;
                if(enableBoosting && Input.GetKey(boostKey)) speed *= boostSpeedMultiplier;

                if(ver != 0) intendedPosition += transform.forward * (Mathf.Sign(ver) * speed * Time.deltaTime);
                if(hor != 0) intendedPosition += transform.right * (Mathf.Sign(hor) * speed * Time.deltaTime);
            }

            // Rotation
            if(enableHorizontalRotation && Input.GetKey(KeyCode.Q)) yRotation -= rotationSpeed * Time.deltaTime;
            if(enableHorizontalRotation && Input.GetKey(KeyCode.E)) yRotation += rotationSpeed * Time.deltaTime;

            if(enableVerticalRotation && Input.GetKey(KeyCode.R)) xRotation += rotationSpeed * Time.deltaTime;
            if(enableVerticalRotation && Input.GetKey(KeyCode.F)) xRotation -= rotationSpeed * Time.deltaTime;

        }

        private void HandleMovement()
        {
            // Panning
            transform.position = Vector3.Lerp(transform.position, intendedPosition, Time.deltaTime * easeSpeed);

            // Rotation
            if(clampVerticalRotation) xRotation = Mathf.Clamp(xRotation, minVerticalRotation, maxVerticalRotation);
            xRotRig.localRotation = Quaternion.Slerp(xRotRig.localRotation, Quaternion.Euler(xRotation, 0, 0), Time.deltaTime * easeSpeed * 5f);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yRotation, 0), Time.deltaTime * easeSpeed * 5f);

            // Zooming
            intendedZoom.z = -(intendedZoom.y = Mathf.Clamp(intendedZoom.y, minZoom, maxZoom));
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, intendedZoom, Time.deltaTime * easeSpeed);
        }
    }
}
