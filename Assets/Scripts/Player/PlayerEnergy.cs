using System;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerEnergy : MonoBehaviour
{
    public static PlayerEnergy Instance { get; private set; }

    [Header("Energía")]
    [SerializeField] private int maxEnergy = 100;

    [Header("Daño por caída")]
    [Tooltip("Desmárcalo en un nivel donde las caídas no deban quitar energía.")]
    [SerializeField] private bool fallDamageEnabled = true;
    [Tooltip("Si cae desde más de X veces su propia altura, recibe daño.")]
    [SerializeField] private float fallHeightMultiplier = 3f;
    [SerializeField] private int fallDamage = 25;


    [Header("Teclas de prueba")]
    [SerializeField] private KeyCode damageKey = KeyCode.J; 
    [SerializeField] private KeyCode healKey = KeyCode.H;
    [SerializeField] private int debugAmount = 20;

    public int CurrentEnergy { get; private set; }
    public int MaxEnergy => maxEnergy;
    public bool IsDead { get; private set; }

    public event Action<int, int> OnEnergyChanged;
    public event Action OnPlayerDied;

    private Collider2D col;
    private PlayerController controller;
    private bool wasGrounded;
    private float peakY; 

    private void Awake()
    {
        Instance = this;
        col = GetComponent<CapsuleCollider2D>(); 
        controller = GetComponent<PlayerController>();
        CurrentEnergy = maxEnergy;

        if (col == null || controller == null)
        {
            Debug.LogError("PlayerEnergy debe ir en el mismo objeto que PlayerController y su collider.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        PlayerController.OnMuerte += HandleExternalDeath;
    }

    private void OnDisable()
    {
        PlayerController.OnMuerte -= HandleExternalDeath;
    }

    private void Start()
    {
        wasGrounded = IsGrounded();
        peakY = col.bounds.min.y;
        Notify();   
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (IsDead) return;

        CheckFallDamage();

        if (controller.debugKeys && controller.ControlActivo)
        {
            if (Input.GetKeyDown(damageKey)) TakeDamage(debugAmount);
            if (Input.GetKeyDown(healKey)) Heal(debugAmount);
        }
    }


    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        if (GameManager.Estado == EstadoJuego.Victoria) return;   // ya ganó: nada le hace daño

        SetEnergy(CurrentEnergy - amount);
        if (CurrentEnergy == 0) Die();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        SetEnergy(CurrentEnergy + amount);
    }


    private void CheckFallDamage()
    {
        bool grounded = IsGrounded();
        float feetY = col.bounds.min.y;

        if (grounded)
        {
            if (!wasGrounded && fallDamageEnabled)  
            {
                float fallDistance = peakY - feetY;
                float limit = col.bounds.size.y * fallHeightMultiplier;
                if (fallDistance > limit) TakeDamage(fallDamage);
            }

            peakY = feetY;  
        }
        else
        {
            peakY = Mathf.Max(peakY, feetY); 
        }

        wasGrounded = grounded;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            controller.groundCheck.position, controller.groundRadius, controller.groundLayer) != null;
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (OnPlayerDied != null) OnPlayerDied();

        controller.PlayDeath();
    }

 
    private void HandleExternalDeath()
    {
        if (IsDead) return; 
        SetEnergy(0);
        IsDead = true;

        if (OnPlayerDied != null) OnPlayerDied();
    }



    private void SetEnergy(int value)
    {
        value = Mathf.Clamp(value, 0, maxEnergy);
        if (value == CurrentEnergy) return;

        CurrentEnergy = value;
        Notify();
    }

    private void Notify()
    {
        if (OnEnergyChanged != null) OnEnergyChanged(CurrentEnergy, maxEnergy);

        if (controller != null && controller.debugKeys);
    }
}