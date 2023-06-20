using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public struct VehicleStats
{
    [Range(1,15)]
    public int speed;
    [Range(1,25)]
    public int weight;

    [Range(1, 10)] public int rotationSpeed;
    public float loopCap;

    [Range(60, 100)] public int maxHealth;

    [Range(-1,1)]
    public int isImmune;
}

[Serializable]
public struct VehicleComponents
{
    
    [SerializeField] public Transform vehicle;
    [SerializeField] public Rigidbody body;
    [SerializeField] public PlayerInput input;
    [SerializeField] public Player player;
}

public class PhysicsVehicle : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private VehicleComponents components;

    public VehicleComponents Components => components;

    [SerializeField] private VehicleStats stats;
    
    private Vector2 _defaultAxis;
    private Vector2 _axis;
    private bool _isDrifting;
    private bool _isGrounded;
    private RaycastHit _groundInfo;
    private bool _isBoosting;
    private bool _usingBonus;
    
    private int _currentHealth;
    private const int MAX_BOOST = 100;
    [SerializeField] private float boostAmount = 0;

    private const float TIME_TO_BOOST = 3;
    private float boostTimer = 0;

    public Action<PhysicsVehicle> bonus;

    private PhysicCharacter character;

    public Checkpoint lastCheckpoint;
    public PhysicCharacter Character
    {
        get => character;
        set => character = value;
    }
    private void Awake()
    {
        _currentHealth = stats.maxHealth;
        boostAmount = 20;
    }

    void Update()
    {
        CheckHealth();
     
        if (components.input.actions["Respawn"].IsPressed()) Respawn();
        _defaultAxis = components.input.actions["Movement"].ReadValue<Vector2>();
        _axis = components.input.actions["Movement"].ReadValue<Vector2>() * (Time.deltaTime * 50000f);
        _isDrifting = components.input.actions["Drift"].IsPressed();
        _isBoosting = components.input.actions["Booster"].IsPressed();
        _usingBonus = components.input.actions["Bonus"].IsPressed();
    }

    private void FixedUpdate()
    {
        if (_usingBonus && bonus != null)
        {
            bonus(this);
            bonus = null;
        }
        CheckConstraints();
        Gravity();
        ManageBoost();
        if (_isDrifting)
        {
            Drift();
        }
        else
        {
            Move();
        }
    }

    private void Move()
    {
        if (!_isGrounded) return;
        Vector3 force = (components.vehicle.forward * (_defaultAxis.y * 45 * stats.speed * (_isBoosting? 1.5f : 1f)));
        if (_axis != Vector2.zero)
        {
            components.vehicle.rotation *= Quaternion.Euler(new Vector3(0,stats.rotationSpeed * 0.025f,0) * (_defaultAxis.x * 45));
        }
        var rotation = components.vehicle.rotation;
        transform.rotation = Quaternion.Lerp(rotation,
            Quaternion.FromToRotation(components.vehicle.up, _groundInfo.normal) * rotation, 0.2f);
        components.body.AddForce(force, ForceMode.Acceleration);
    }

    private void Drift()
    {
        if (!_isGrounded) return;
        var rotation = components.vehicle.rotation;
        rotation *= Quaternion.Euler(new Vector3(0,stats.rotationSpeed * 0.05f,0) * (_defaultAxis.x * 15));
        rotation = Quaternion.Lerp(rotation,
            Quaternion.FromToRotation(components.vehicle.up, _groundInfo.normal) * rotation, 0.5f);
        components.vehicle.rotation = rotation;
        var force = (components.vehicle.forward * (stats.speed * 25 * (_isBoosting? 1.5f : 1f))) + (components.vehicle.right * ((Math.Abs(_defaultAxis.x - (-1)) < 0.4f ? _defaultAxis.y : -_defaultAxis.y) * stats.speed * 40));
        components.body.AddForce(force);
    }

    private void CheckConstraints()
    {
        _isGrounded = Physics.Raycast(new Ray(components.vehicle.position, -components.vehicle.up), out _groundInfo, 1.5f);
        if (Mathf.Abs(components.body.velocity.magnitude) > stats.loopCap && _isGrounded)
        {
            components.body.useGravity = false;
        }
        else
        {
            components.body.useGravity = true;
        }
    }

    private void Gravity()
    {
        if (!_isGrounded)
        {
            components.body.AddForce(0,-stats.weight * Time.deltaTime * 1000f,0);
        }
    }

    private void Respawn()
    {
        components.vehicle.position = components.player.LastCheckpoint.transform.position + Vector3.up;
        components.vehicle.rotation = Quaternion.identity;
    }

    private void CheckHealth()
    {
        if (_currentHealth <= 0)
        {
            character.DestroyVehicle();
            
        }
        if (_currentHealth > stats.maxHealth) _currentHealth = stats.maxHealth;
    }

    private void ManageBoost()
    {
        if (_isBoosting && boostAmount > 0)
        {
            boostAmount -= 0.1f;
        }

        if (_isDrifting && !_isBoosting && _axis.x != 0)
        {
            boostTimer += Time.deltaTime * 4;
        }
        else
        {
            boostTimer = 0;
        }

        if (boostTimer > TIME_TO_BOOST && boostAmount <= MAX_BOOST)
        {
            boostAmount += 0.1f;
        }
    }

    public void AddHealth(int amount)
    {
        _currentHealth += amount;
        if (_currentHealth > stats.maxHealth) _currentHealth = stats.maxHealth;
        if (_currentHealth < 0) _currentHealth = 0;
    }

    public void SetResistance(int amount)
    {
        if (amount < -1) amount = -1;
        if (amount > 1) amount = 1;
        stats.isImmune = amount;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            _currentHealth -= 50;
        }
        if (collision.gameObject.TryGetComponent<PhysicsVehicle>(out PhysicsVehicle vehicle))
        {
            _currentHealth -= 10;
            vehicle.AddHealth(-10);
            Debug.Log("Losing health");
        }
    }

    public void LinkPlayer(Player player)
    {
        components.player = player;
    }
}
