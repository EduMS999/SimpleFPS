using UnityEngine;
using UnityEngine.InputSystem; // Importante: Usa el nuevo sistema de Input de Unity

public class PlayerMovementCC : MonoBehaviour
{
    // --- VARIABLES DE CONFIGURACIÓN (Editables en el Inspector) ---
    [Header("Ajustes de Movimiento")]
    public float speed = 5f;
    public float crouchSpeed = 2.5f;
    public float mouseSensitivity = 0.2f;

    [Header("Ajustes de Agachado")]
    public float crouchHeight = 1f;   // Altura al estar agachado
    public float standingHeight = 2f; // Altura normal
    public float timeToCrouch = 10f;  // Velocidad de la transición

    [Header("Física Manual")]
    public float gravity = -9.81f;    // Fuerza de gravedad constante
    public float jumpHeight = 1.5f;   // Altura máxima del salto

    [Header("Sprint & Zoom")]
    public float sprintMultiplier = 2f; // Cuánto aumenta la velocidad al correr
    public float zoomFOV = 40f;         // Campo de visión al apuntar/hacer zoom
    public float normalFOV = 60f;
    public float zoomSpeed = 10f;       // Suavidad del zoom

    [Header("Head Bobbing (Efecto de caminar)")]
    public float bobFrequency = 5f; // Qué tan rápido oscila la cámara
    public float bobAmount = 0.1f;    // Qué tan amplia es la oscilación
    private float _bobTimer;          // Rastreador de tiempo interno para la onda

    [Header("Interacción Física")]
    public float pushPower = 2.0f; // Fuerza con la que el jugador empuja los objetos

    // --- VARIABLES PRIVADAS (Estado interno) ---
    private bool _isSprinting;
    private bool _isZooming;
    private bool _isGrounded;
    private bool _isCrouching;

    private CharacterController _controller; // Referencia al componente físico
    private Transform _cameraTransform;      // Referencia a la cámara (hija del jugador)
    private Vector2 _moveInput;              // Captura de las teclas WASD
    private Vector3 _velocity;               // Vector para la gravedad y saltos
    private float _xRotation = 0f;           // Rotación vertical acumulada (mirar arriba/abajo)

    void Start()
    {
        // Inicializamos las referencias
        _controller = GetComponent<CharacterController>();
        _cameraTransform = GetComponentInChildren<Camera>().transform;

        // Bloqueamos el ratón en el centro de la pantalla y lo ocultamos
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- MÉTODOS DE INPUT (Eventos del Player Input Component) ---
    void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    void OnLook(InputValue value) => ProcessRotation(value.Get<Vector2>());
    void OnJump(InputValue value)
    {
        if (_isGrounded && !_isCrouching) // Solo saltamos si tocamos suelo y no estamos agachados
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    void OnCrouch(InputValue value) => _isCrouching = value.isPressed;
    void OnSprint(InputValue value) => _isSprinting = value.isPressed;
    void OnZoom(InputValue value) => _isZooming = value.isPressed;

    void Update()
    {
        // 1. GESTIÓN DE ALTURA: Cambia el tamaño del colisionador suavemente
        HandleHeight();

        // 2. EFECTO ZOOM: Cambia el FOV de la cámara usando Interpolación Lineal (Lerp)
        float targetFOV = _isZooming ? zoomFOV : normalFOV;
        Camera cam = _cameraTransform.GetComponent<Camera>();
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);

        // 3. FÍSICA Y GRAVEDAD:
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0) _velocity.y = -2f; // Mantiene al jugador pegado al suelo

        // 4. CÁLCULO DE VELOCIDAD:
        float currentSpeed = _isCrouching ? crouchSpeed : speed;
        // Solo corre si se mueve hacia adelante y no está agachado
        if (_isSprinting && !_isCrouching && _moveInput.y > 0) currentSpeed *= sprintMultiplier;

        // 5. MOVIMIENTO FINAL:
        // Calculamos la dirección relativa a hacia dónde mira el jugador
        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _controller.Move(move * currentSpeed * Time.deltaTime);

        // Aplicamos gravedad acumulada
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);

        // 6. BALANCEO DE CABEZA:
        HandleHeadBob();
    }

    void HandleHeight()
    {
        // Ajusta la altura del CharacterController de forma fluida
        float targetHeight = _isCrouching ? crouchHeight : standingHeight;
        _controller.height = Mathf.Lerp(_controller.height, targetHeight, timeToCrouch * Time.deltaTime);

        // Ajusta el centro del colisionador para que no "flote" al encogerse
        _controller.center = new Vector3(0, _controller.height / 2f, 0);
    }

    void HandleHeadBob()
    {
        // ¿El jugador se está moviendo con las teclas?
        float inputMagnitude = _moveInput.magnitude;

        // Calculamos dónde debería estar la cámara según la altura actual
        float baseHeight = _isCrouching ? (crouchHeight * 0.8f) : (standingHeight * 0.8f);

        if (inputMagnitude > 0.1f && _isGrounded)
        {
            // El Timer avanza según el tiempo y la velocidad
            _bobTimer += Time.deltaTime * (speed * bobFrequency);

            // Función Seno: Crea un valor que oscila entre -1 y 1
            float offset = Mathf.Sin(_bobTimer) * bobAmount;

            // Aplicamos la oscilación a la posición Y local de la cámara
            _cameraTransform.localPosition = new Vector3(0, baseHeight + offset, 0);
        }
        else
        {
            // Si se detiene, reseteamos el timer y devolvemos la cámara a su posición base suavemente
            _bobTimer = 0;
            float smoothY = Mathf.Lerp(_cameraTransform.localPosition.y, baseHeight, Time.deltaTime * 10f);
            _cameraTransform.localPosition = new Vector3(0, smoothY, 0);
        }
    }

    void ProcessRotation(Vector2 look)
    {
        // Rotación horizontal (Eje Y): Gira todo el cuerpo del jugador
        transform.Rotate(Vector3.up * look.x * mouseSensitivity);

        // Rotación vertical (Eje X): Solo gira la cámara
        _xRotation -= look.y * mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); // Evita que el jugador de una voltereta hacia atrás
        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // 1. ¿El objeto tiene Rigidbody y no es estático?
        if (body == null || body.isKinematic) return;

        // 2. No empujar objetos que estén por debajo de nuestros pies (el suelo)
        if (hit.moveDirection.y < -0.3f) return;

        // 3. Calcular la dirección del empuje basándonos en el movimiento del jugador
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // 4. Aplicar la fuerza en el punto de impacto
        body.AddForceAtPosition(pushDir * pushPower, hit.point, ForceMode.Impulse);
    }
}