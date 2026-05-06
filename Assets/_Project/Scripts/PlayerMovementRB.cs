using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo sistema de entradas de Unity

[RequireComponent(typeof(Rigidbody))] // Asegura que el objeto tenga un Rigidbody
public class PlayerMovementRB : MonoBehaviour
{
    // --- VARIABLES CONFIGURABLES (Exponemos al Inspector para balanceo) ---

    [Header("Movimiento Físico")]
    [Tooltip("Fuerza constante aplicada al movernos")]
    public float moveForce = 50f;
    [Tooltip("Velocidad máxima permitida por el motor físico")]
    public float maxSpeed = 5f;
    public float mouseSensitivity = 0.2f;

    [Header("Salto y Agachado")]
    public float jumpForce = 5f;
    public float crouchHeight = 1f;
    public float standingHeight = 2f;

    [Header("Sprint & Zoom")]
    public float sprintMultiplier = 2f;
    public float zoomFOV = 40f;
    public float normalFOV = 60f;
    public float zoomSpeed = 10f;

    [Header("Head Bobbing (Efecto de pasos)")]
    public float bobFrequency = 5f;
    public float bobAmount = 0.1f;
    private float _bobTimer;

    // --- ESTADOS INTERNOS (Privados para no ensuciar el Inspector) ---
    private bool _isSprinting;
    private bool _isZooming;
    private bool _isCrouching;

    // --- REFERENCIAS DE COMPONENTES ---
    private Rigidbody _rb;
    private CapsuleCollider _col;
    private Transform _cameraTransform;

    private Vector2 _moveInput; // Almacena el valor de WASD o Joystick
    private float _xRotation = 0f; // Controla el ángulo vertical de la cámara

    void Start()
    {
        // Cacheamos las referencias para ahorrar rendimiento
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();
        // Buscamos la cámara en los hijos del jugador
        _cameraTransform = GetComponentInChildren<Camera>().transform;

        // Evitamos que el motor físico haga que el jugador "ruede" como un cilindro
        _rb.freezeRotation = true;

        // Bloqueamos el cursor en el centro para una experiencia FPS real
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- MÉTODOS DE ENTRADA (Llamados por Player Input Component) ---
    // El prefijo "On" es la convención del sistema de mensajes del Input System
    void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    void OnLook(InputValue value) => ProcessRotation(value.Get<Vector2>());

    void OnJump(InputValue value)
    {
        // Verificación simple de suelo: si la velocidad vertical es casi 0, permitimos saltar
        if (Mathf.Abs(_rb.linearVelocity.y) < 0.05f)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnSprint(InputValue value) => _isSprinting = value.isPressed;
    void OnZoom(InputValue value) => _isZooming = value.isPressed;
    void OnCrouch(InputValue value) => _isCrouching = value.isPressed;

    void Update()
    {
        // 1. ZOOM SUAVE: Usamos Lerp (Interpolación lineal) para una transición fluida del FOV
        float targetFOV = _isZooming ? zoomFOV : normalFOV;
        Camera cam = _cameraTransform.GetComponent<Camera>();
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);

        // 2. RAYCAST VISUAL: Una ayuda visual en el editor para saber hacia dónde miramos
        Debug.DrawRay(_cameraTransform.position, _cameraTransform.forward * 2f, Color.red);

        // 3. HEAD BOB: Se procesa en Update para que el movimiento sea suave a la vista
        HandleHeadBob();
    }

    void FixedUpdate()
    {
        // Todo lo que use fuerzas físicas DEBE ir en FixedUpdate (sincronizado con el motor físico)
        ApplyMovement();
        ApplyCrouch();
    }

    void ApplyMovement()
    {
        // Calculamos la dirección relativa a hacia dónde mira el jugador
        Vector3 moveDir = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        // Solo permitimos sprint si nos movemos hacia adelante
        float multiplier = (_isSprinting && _moveInput.y > 0) ? sprintMultiplier : 1f;

        // Aplicamos la fuerza de aceleración
        _rb.AddForce(moveDir * moveForce * multiplier, ForceMode.Acceleration);

        // LIMITADOR DE VELOCIDAD: La física de Unity puede acumular fuerza infinita. Debemos caparla.
        Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z); // Velocidad en el suelo
        if (flatVel.magnitude > maxSpeed * multiplier)
        {
            Vector3 limitedVel = flatVel.normalized * (maxSpeed * multiplier);
            // Aplicamos el límite manteniendo la velocidad vertical (caída/salto)
            _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
        }
    }

    void ApplyCrouch()
    {
        // Cambiamos la altura del colisionador de forma gradual
        float targetHeight = _isCrouching ? crouchHeight : standingHeight;
        _col.height = Mathf.MoveTowards(_col.height, targetHeight, Time.fixedDeltaTime * 10f);

        // Ajustamos el centro del colisionador para que el "suelo" del objeto no cambie
        _col.center = new Vector3(0, _col.height / 2f, 0);
    }

    void HandleHeadBob()
    {
        float horizontalSpeed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
        float baseHeight = _isCrouching ? 0.8f : 1.6f; // Altura de ojos según el estado

        // Si nos movemos y estamos en el suelo...
        if (horizontalSpeed > 0.1f && Mathf.Abs(_rb.linearVelocity.y) < 0.1f)
        {
            // El timer avanza según la velocidad (más rápido corres, más rápido oscila)
            _bobTimer += Time.deltaTime * (horizontalSpeed * bobFrequency);

            // Usamos Sinusoidal para crear el movimiento de subida y bajada
            float offset = Mathf.Sin(_bobTimer) * bobAmount;
            _cameraTransform.localPosition = new Vector3(0, baseHeight + offset, 0);
        }
        else
        {
            // Si nos detenemos, reseteamos el timer y devolvemos la cámara a su altura base suavemente
            _bobTimer = 0;
            float smoothY = Mathf.Lerp(_cameraTransform.localPosition.y, baseHeight, Time.deltaTime * 10f);
            _cameraTransform.localPosition = new Vector3(0, smoothY, 0);
        }
    }

    void ProcessRotation(Vector2 look)
    {
        // Rotación Horizontal (Eje Y): Giramos todo el cuerpo del jugador
        transform.Rotate(Vector3.up * look.x * mouseSensitivity);

        // Rotación Vertical (Eje X): Giramos solo la cámara
        _xRotation -= look.y * mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); // Evitamos que el jugador dé una voltereta hacia atrás
        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
    }
}