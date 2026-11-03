using System;
using UnityEngine;

/// <summary>
/// Energía (batería) del jugador. Va en el prefab del Player (Micro_Prefab).
/// Otros scripts la usan así: PlayerEnergy.Instance.TakeDamage(20);
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerEnergy : MonoBehaviour
{
    public static PlayerEnergy Instance { get; private set; }

    [Header("Energía")]
    [SerializeField] private int maxEnergy = 100;

    [Header("Daño por caída")]
    [Tooltip("Layers que cuentan como suelo (Ground y Pushable).")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Si cae desde más de X veces su propia altura, recibe daño.")]
    [SerializeField] private float fallHeightMultiplier = 3f;
    [SerializeField] private int fallDamage = 25;
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Header("Muerte")]
    [Tooltip("Llama a PlayerController.PlayDeath() para la animación de muerte.")]
    [SerializeField] private bool playDeathAnimation = true;

    [Header("Debug (solo Editor y Development Build)")]
    [SerializeField] private bool enableDebugKeys = true;
    [SerializeField] private KeyCode damageKey = KeyCode.J; // K ya la usa PlayerController
    [SerializeField] private KeyCode healKey = KeyCode.H;
    [SerializeField] private int debugAmount = 20;

    public int CurrentEnergy { get; private set; }
    public int MaxEnergy => maxEnergy;
    public bool IsDead { get; private set; }

    /// <summary>(actual, máximo)</summary>
    public event Action<int, int> OnEnergyChanged;
    public event Action OnPlayerDied;

    private Collider2D col;
    private PlayerController controller;
    private bool wasGrounded;
    private float peakY;

    private void Awake()
    {
        Instance = this;
        col = GetComponent<Collider2D>();
        controller = GetComponent<PlayerController>();
        CurrentEnergy = maxEnergy;
    }

    private void Start()
    {
        wasGrounded = IsGrounded();
        peakY = col.bounds.min.y;
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (IsDead) return;

        CheckFallDamage();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (enableDebugKeys)
        {
            if (Input.GetKeyDown(damageKey)) TakeDamage(debugAmount);
            if (Input.GetKeyDown(healKey)) Heal(debugAmount);
        }
#endif
    }

    private void CheckFallDamage()
    {
        bool grounded = IsGrounded();
        float feetY = col.bounds.min.y;

        if (!grounded)
        {
            if (wasGrounded) peakY = feetY;              // acaba de despegar
            else peakY = Mathf.Max(peakY, feetY);        // sigue en el aire
        }
        else if (!wasGrounded)                           // acaba de aterrizar
        {
            float fallDistance = peakY - feetY;
            float limit = col.bounds.size.y * fallHeightMultiplier;
            if (fallDistance > limit) TakeDamage(fallDamage);
        }

        wasGrounded = grounded;
    }

    // Raycast propio (BoxCast hacia abajo): no depende del PlayerController.
    private bool IsGrounded()
    {
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x * 0.9f, b.size.y);
        RaycastHit2D hit = Physics2D.BoxCast(b.center, size, 0f, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentEnergy = Mathf.Max(0, CurrentEnergy - amount);
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);

        if (CurrentEnergy == 0) Die();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentEnergy = Mathf.Min(maxEnergy, CurrentEnergy + amount);
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    private void Die()
    {
        IsDead = true;

        if (playDeathAnimation && controller != null) controller.PlayDeath();

        OnPlayerDied?.Invoke();

        // TODO (Persona 1): cuando exista el GameManager, descomentar esta línea:
        // GameManager.Instance.Lose();
        // Mientras tanto, UIManager muestra la pantalla de Pérdida al escuchar OnPlayerDied.
    }
}
