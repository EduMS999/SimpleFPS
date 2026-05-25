using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class SimpleEnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Attacking }

    [Header("Setup")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTransform;
    private Rigidbody rb; // Referencia para la física de muerte
    private HealthSystem healthSystem;

    [Header("Patrol Settings")]
    [SerializeField] private Transform patrolPointsParent;   // El objeto padre que contiene todos los puntos (hijos) en la jerarquía
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitAtPoint = 1.5f;
    private int currentPointIndex;
    private bool isWaiting;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;

    [Header("Configuración de Ataque")]
    [SerializeField] private float attackDamage = 15f; // Daño por cada golpe
    [SerializeField] private float attackRate = 1.5f;   // Segundos entre ataques
    private float nextAttackTime;                      // Control del tiempo de ataque

    [Header("Efecto de Muerte Física")]
    [SerializeField] private float deathForce = 2f; // Fuerza hacia atrás al morir
    [SerializeField] private float deathRotationTorque = 15f; // Fuerza de giro al caer
    [SerializeField] private float timeBeforeDisappear = 5f; // Tiempo antes de borrar el cuerpo

    [Header("Current Status")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrolling;

    
    private void Start()
    {
        // Si no asignamos el agente en el Inspector, lo buscamos en este objeto
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // Forzamos a buscar el Rigidbody en el objeto padre
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("¡Falta el componente Rigidbody en el objeto " + gameObject.name + "!");
        }

        // Si no asignamos el jugador a mano, lo buscamos por Tag
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }


        // Buscamos el componente HealthSystem en este objeto
        healthSystem = GetComponent<HealthSystem>();

        if (healthSystem != null)
        {
            // Nos suscribimos al evento de muerte
            healthSystem.OnDeath += HandleDeath;
        }

        /*
                // 1. Creamos el array con el tamaño exacto según cuántos hijos tenga el 'waypointParent'
                patrolPoints = new Transform[patrolPointsParent.childCount];
                // 2. Llenamos el array guardando la posición (Transform) de cada hijo
                for (int i = 0; i < patrolPointsParent.childCount; i++)
                    {
                    patrolPoints[i] = patrolPointsParent.GetChild(i);
                    }
                GoToNextPatrolPoint();
        */


        // Ya no inicializamos patrolPoints aquí. Esperamos a que nos los asignen.
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
    }

    // 1. En el Start(), eliminamos o comentamos la parte donde se llenaba el array desde patrolPointsParent.
    // 2. Creamos este método público para que el Spawner nos dé los puntos
    public void SetPatrolPoints(Transform[] newPoints)
    {
        patrolPoints = newPoints;
        currentPointIndex = 0; // Reiniciamos el índice de puntos
        isWaiting = false;     // Por si acaso estaba esperando

        // Forzamos a que el NavMeshAgent esté activo y configurado
        if (agent == null) agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null && patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.isStopped = false; // Nos aseguramos de que no esté pausado
            GoToNextPatrolPoint();
        }
        else
        {
            Debug.LogWarning($"[SimpleEnemyAI] {gameObject.name} recibió puntos de patrulla vacíos o nulos.");
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Medimos la distancia actual entre la IA y el jugador
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // --- MÁQUINA DE ESTADOS (FSM) ---
        switch (currentState)
        {
            case EnemyState.Patrolling:
                PatrolBehavior(distanceToPlayer);
                break;

            case EnemyState.Chasing:
                ChaseBehavior(distanceToPlayer);
                break;

            case EnemyState.Attacking:
                AttackBehavior(distanceToPlayer);
                break;
        }
    }

    // ================= COMPORTAMIENTOS (ESTADOS) =================

    private void PatrolBehavior(float distanceToPlayer)
    {
        // Si el jugador entra en el rango de detección, cambiamos a perseguir
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chasing;
            isWaiting = false;
            StopAllCoroutines(); // Detenemos la espera en el punto de patrulla
            return;
        }

        // Si ya llegamos al punto de patrulla actual (con un pequeño margen de error)
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                StartCoroutine(WaitAndMoveRoutine());
            }
        }
    }

    private void ChaseBehavior(float distanceToPlayer)
    {
        // Si el jugador se aleja demasiado, volvemos a patrullar
        if (distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Patrolling;
            GoToNextPatrolPoint();
            return;
        }

        // Si estamos lo suficientemente cerca, cambiamos a atacar
        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attacking;
            return;
        }

        // Actualizamos el destino del NavMeshAgent hacia el jugador en tiempo real
        agent.SetDestination(playerTransform.position);
    }

    private void AttackBehavior(float distanceToPlayer)
    {
        // Si el jugador se sale del rango de ataque, volvemos a perseguirlo
        if (distanceToPlayer > attackRange)
        {
            currentState = EnemyState.Chasing;
            return;
        }

        // Detenemos al enemigo en el sitio para que no empuje al jugador
        agent.SetDestination(transform.position);

        // Hacemos que rote suavemente hacia el jugador para mirarlo de frente
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Evitamos que la IA se incline si el jugador salta
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

        // --- LÓGICA DE ATAQUE POR TIEMPO ---
        //Debug.Log("¡La IA te está atacando!");
        if (Time.time >= nextAttackTime)
        {
            AttackPlayer();
            nextAttackTime = Time.time + attackRate; // Bloqueamos el ataque hasta que pase el tiempo
        }
        
    }


    // ================= MÉTODOS AUXILIARES =================

    private void GoToNextPatrolPoint()
    {
        // Si no hay puntos asignados en el inspector, no hacemos nada
        if (patrolPoints.Length == 0) return;

        // Enviamos al agente al punto actual
        agent.destination = patrolPoints[currentPointIndex].position;

        // Avanzamos al siguiente punto (y vuelve al inicio al llegar al final del array)
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    private IEnumerator WaitAndMoveRoutine()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitAtPoint);
        GoToNextPatrolPoint();
        isWaiting = false;
    }

    
    private void OnDestroy()
    {
        // Buena práctica: Cancelamos la suscripción al destruir el objeto
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
        }
    }

    private void AttackPlayer()
    {
        if (playerTransform == null) return;

        // Buscamos el componente HealthSystem en el jugador
        HealthSystem playerHealth = playerTransform.GetComponent<HealthSystem>();

        if (playerHealth != null)
        {
            Debug.Log($"¡La IA te ha atacado y te ha hecho {attackDamage} de daño!");
            playerHealth.TakeDamage(attackDamage);
        }
    }

    // ================= EFECTO DE MUERTE FÍSICA =================
    private void HandleDeath()
    {
        //Debug.Log("La IA ha muerto. Aplicando físicas de caída.");

        // 1. Desactivamos este componente para que no siga evaluando el Update
        this.enabled = false;

        CambiarLayerJerarquia(gameObject, LayerMask.NameToLayer("Ignore Raycast")) ; // Para no bloquear balas cuando esta muerto

        // 2. Apagamos por completo el NavMeshAgent para que no interfiera con el Rigidbody
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 3. Activamos las físicas en el Rigidbody
        if (rb != null)
        {
            rb.isKinematic = false; // Deja de ser kinemático para que le afecte la gravedad
            rb.detectCollisions = true; // Nos aseguramos de que siga chocando contra el suelo

            // Forzamos a que el colisionador de este objeto no sea Trigger
            CapsuleCollider col = GetComponent<CapsuleCollider>();
            if (col != null)
            {
                col.isTrigger = false;
            }

            // 4. Calculamos una dirección hacia atrás basándonos en hacia dónde miraba el enemigo
            Vector3 pushDir = -transform.forward;
            pushDir.y = 0.8f; // Un empujón diagonal hacia arriba y atrás para una caída más limpia

            // Aplicamos la fuerza de impacto de muerte
            rb.AddForce(pushDir * deathForce, ForceMode.Impulse);

            // Aplicamos un torque (giro) aleatorio para que rote al caer al suelo
            Vector3 torque = new Vector3(
                Random.Range(-deathRotationTorque, deathRotationTorque),
                Random.Range(-deathRotationTorque, deathRotationTorque),
                Random.Range(-deathRotationTorque, deathRotationTorque)
            );
            rb.AddTorque(torque, ForceMode.Impulse);
        }

        // 5. Destruimos el objeto del enemigo de la escena tras unos segundos
        Destroy(gameObject, timeBeforeDisappear);
    }

    // Dibuja los radios de detección y ataque en el Editor para que sea fácil ajustarlos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // Función auxiliar para recorrer todos los hijos y ignorar los disparos una vez muerto
    private void CambiarLayerJerarquia(GameObject objetoPadre, int nuevoLayer)
    {
        objetoPadre.layer = nuevoLayer;
        foreach (Transform hijo in objetoPadre.transform)
        {
            CambiarLayerJerarquia(hijo.gameObject, nuevoLayer);
        }
    }

}
