using UnityEngine;
using UnityEngine.InputSystem;
using PokerWar.Interfaces;
using PokerWar.Managers;
using PokerWar.World;

namespace PokerWar.Player
{
    /// <summary>
    /// First-person player controller with movement, camera, and interaction.
    /// Uses Unity's new Input System for cross-platform input handling.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float jumpHeight = 1.5f;

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float lookXLimit = 80f;

        [Header("References")]
        [SerializeField] private Camera playerCamera;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool isGrounded;
        private float cameraRotationX = 0f;
        private bool isSprinting = false;

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction interactAction;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            // Create camera if not assigned
            if (playerCamera == null)
            {
                GameObject cameraObj = new GameObject("Player Camera");
                cameraObj.transform.SetParent(transform);
                cameraObj.transform.localPosition = new Vector3(0, 0.6f, 0);
                playerCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Set player tag
            gameObject.tag = "Player";

            // Set up input actions
            SetupInputActions();
        }

        private void SetupInputActions()
        {
            // Try to load InputSystem_Actions asset
            var inputAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");

            if (inputAsset == null)
            {
                CreateFallbackActions();
                return;
            }

            var playerMap = inputAsset.FindActionMap("Player");
            if (playerMap != null)
            {
                moveAction = playerMap.FindAction("Move");
                lookAction = playerMap.FindAction("Look");
                jumpAction = playerMap.FindAction("Jump");
                sprintAction = playerMap.FindAction("Sprint");
                interactAction = playerMap.FindAction("Interact");

                moveAction?.Enable();
                lookAction?.Enable();
                jumpAction?.Enable();
                sprintAction?.Enable();
                interactAction?.Enable();

                if (interactAction != null)
                    interactAction.performed += OnInteract;
            }
            else
            {
                CreateFallbackActions();
            }
        }

        private void CreateFallbackActions()
        {
            Debug.LogWarning("Using fallback input actions. Assign InputSystem_Actions asset for full functionality.");

            moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            lookAction = new InputAction("Look", binding: "<Mouse>/delta");
            jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
            sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");
            interactAction = new InputAction("Interact", binding: "<Keyboard>/e");

            interactAction.performed += OnInteract;

            moveAction.Enable();
            lookAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
            interactAction.Enable();
        }

        private void Update()
        {
            HandleMovement();
            HandleLook();
        }

        private void HandleMovement()
        {
            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (moveAction != null)
            {
                moveInput = moveAction.ReadValue<Vector2>();
            }

            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

            isSprinting = sprintAction != null && sprintAction.ReadValue<float>() > 0;
            float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

            controller.Move(move * currentSpeed * Time.deltaTime);

            if (jumpAction != null && jumpAction.WasPressedThisFrame() && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleLook()
        {
            if (lookAction == null) return;

            lookInput = lookAction.ReadValue<Vector2>();

            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

            cameraRotationX -= lookInput.y * mouseSensitivity;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -lookXLimit, lookXLimit);

            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            // Try to interact with nearby interactables
            IInteractable nearestInteractable = FindNearestInteractable();

            if (nearestInteractable != null && nearestInteractable.IsInRange())
            {
                Debug.Log($"Interacting with: {nearestInteractable.GetInteractionPrompt()}");
                nearestInteractable.Interact();
            }
            else
            {
                Debug.Log("No interactable objects in range");
            }
        }

        private IInteractable FindNearestInteractable()
        {
            // For now, just check for the chest
            if (GameManager.Instance == null) return null;

            var worldGen = GameManager.Instance.WorldGenerator as PokerWar.World.ProceduralWorldGenerator;
            if (worldGen == null) return null;

            GameObject chest = worldGen.GetCurrentChest();
            if (chest != null)
            {
                // Unity's GetComponent doesn't work reliably with interfaces
                // Always use concrete class types, then return as interface
                return chest.GetComponent<ChestInteractable>();
            }

            return null;
        }

        private void OnEnable()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            jumpAction?.Enable();
            sprintAction?.Enable();
            interactAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            jumpAction?.Disable();
            sprintAction?.Disable();
            interactAction?.Disable();
        }

        private void OnDestroy()
        {
            if (interactAction != null)
                interactAction.performed -= OnInteract;

            // Dispose fallback actions if created
            if (Resources.Load<InputActionAsset>("InputSystem_Actions") == null)
            {
                moveAction?.Dispose();
                lookAction?.Dispose();
                jumpAction?.Dispose();
                sprintAction?.Dispose();
                interactAction?.Dispose();
            }
        }
    }
}
