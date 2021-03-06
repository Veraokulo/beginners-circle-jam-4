﻿using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Player : MonoBehaviour
{
    public const string Tag = "Player";
    public float Health = 100f;
    public float Oxygen = 200f;
    private Rigidbody _rb;
    public GameObject graphics;
    public Animator animator;
    public HealthBar HealthBar;
    public OxygenBar OxygenBar;
    [Range(0, 10)] public float rotationSpeed = 5f;

    private bool _jump;
    [Min(0)] public float jumpForce = 50f;
    [Min(0)] public float speed = 30f;

    private float _distanceToTheGround;
    private Collider _collider;

    private bool IsGrounded =>
        Physics.Raycast(transform.position, -transform.up, out var hit,
            _distanceToTheGround + 0.05f);

    private bool _isResized;
    public float resizingScale = 2f;
    private static readonly int Speed = Animator.StringToHash("speed");
    private static readonly int IsZeroGravity = Animator.StringToHash("isZeroGravity");
    private static readonly int Grounded = Animator.StringToHash("isGrounded");
    private static readonly int Jump = Animator.StringToHash("jump");
    public List<int> Keys;

    private void Start()
    {
        GameManager.Instance.Player = this;
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _distanceToTheGround = _collider.bounds.extents.y;
        HealthBar.SetMaxHealth(Health);
        HealthBar.SetHealth(Health);
        OxygenBar.SetOxygen(Oxygen);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            switch (GameManager.Instance.Gravity.Bodies[_rb])
            {
                case Gravity.GravityMode.FromCenter:
                    GameManager.Instance.Gravity.Bodies[_rb] = Gravity.GravityMode.ToCenter;
                    break;
                case Gravity.GravityMode.ToCenter:
                    GameManager.Instance.Gravity.Bodies[_rb] = Gravity.GravityMode.FromCenter;
                    break;
            }
        }

        if (Input.GetButtonDown("Jump") && IsGrounded) _jump = true;
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            graphics.transform.localRotation = Quaternion.Euler(0, Input.GetAxisRaw("Horizontal") < 0 ? -90 : 90, 0);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.localScale *= _isResized ? resizingScale : 1 / resizingScale;
            _distanceToTheGround *= _isResized ? resizingScale : 1 / resizingScale;
            _isResized = !_isResized;
        }
    }


    private void FixedUpdate()
    {
        Vector3 upDirection;
        var gravity = GameManager.Instance.Gravity.Bodies[_rb];
        switch (gravity)
        {
            case Gravity.GravityMode.FromCenter:
                upDirection = -transform.position.normalized;
                break;
            case Gravity.GravityMode.ToCenter:
                upDirection = transform.position.normalized;
                break;
            default:
                upDirection = transform.up;
                break;
        }

        var leftDirection = (Vector3) Vector2.Perpendicular(upDirection);

        #region Rotation

        if (gravity != Gravity.GravityMode.None)
        {
            var newRotation = Quaternion.Euler(0, 0,
                Vector3.SignedAngle(Vector3.up, upDirection, Vector3.forward));
            transform.rotation = Mathf.Abs(newRotation.eulerAngles.z - transform.rotation.eulerAngles.z) > 10f
                ? Quaternion.Slerp(transform.rotation, newRotation, Time.fixedDeltaTime * rotationSpeed)
                : newRotation;
        }

        #endregion

        #region Velocity change

        var vertical = gravity == Gravity.GravityMode.None
            ? transform.up * (Input.GetAxis("Vertical") * speed)
            : Vector3.Project(_rb.velocity, upDirection);

        if (_jump && gravity != Gravity.GravityMode.None)
        {
            vertical += upDirection * jumpForce;
            _jump = false;
            animator.SetTrigger(Jump);
        }

        var horizontal = leftDirection * -(Input.GetAxis("Horizontal") * speed);
        animator.SetFloat(Speed, horizontal.magnitude);
        animator.SetBool(IsZeroGravity, gravity == Gravity.GravityMode.None);
        animator.SetBool(Grounded, IsGrounded);

        _rb.velocity = horizontal + vertical;

        #endregion
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        HealthBar.SetHealth(Health);
        GameManager.Instance.SetColorFilter(Color.red);
        if (Health <= 0)
        {
            Die();
        }
    }

    public void AddOxygen(float oxygen)
    {
        Oxygen += oxygen;
        OxygenBar.SetOxygen(Oxygen);
    }

    public void TakeOxygen(float oxygen)
    {
        Oxygen -= oxygen;
        OxygenBar.SetOxygen(Oxygen);
        GameManager.Instance.SetColorFilter(Color.gray);
        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.ShowMessage("YOU DIED!");
        GameManager.Instance.GameOver();
    }
}