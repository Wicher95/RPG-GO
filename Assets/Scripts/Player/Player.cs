﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject enemiesTrigger;
    public GameObject enemiesTriggerPivot;
    public GameObject weaponHolder;
    public Animator animator;
    public float moveSpeed;
    public float rotateSpeed;
    public float gravity = 0.2f;
    private float Gravity = -9.81f;
    private Vector3 _moveDir = Vector3.zero;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    private bool controlsDisabled;
    private float vertical;
    private float horizontal;
    private float targetDistance;
    private bool isAttacking;
    public List<GameObject> weaponBounds;
    public List<GameObject> weaponBoundsPrevious;

    public List<Transform> enemies;
    private List<string> enemiesHit;
    private Damage damage;
    bool jump;
    public bool grounded;
    bool diveAvaible;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        controlsDisabled = false;
        enemies = new List<Transform>();
        enemiesHit = new List<string>();
        damage = FindObjectOfType<Damage>();
        diveAvaible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (!controlsDisabled)
        {

            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");

            if (Input.GetButton("Fire1"))
            {
                controlsDisabled = true;
                GetClosestEnemy(enemies, gameObject.transform);
            }
            else
            {
                targetDistance = 0.0f;
            }

            Vector3 camForward_Dir = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 move = vertical * camForward_Dir + horizontal * Camera.main.transform.right;

            if (move.magnitude > 1f) move.Normalize();

            // Calculate the rotation for the player
            move = transform.InverseTransformDirection(move);

            // Get Euler angles
            float turnAmount = Mathf.Atan2(move.x, move.z);

            transform.Rotate(0, turnAmount * rotateSpeed * Time.deltaTime, 0);

            animator.SetFloat("Vertical", move.magnitude);

            //Vector3 _moveDir = transform.worldToLocalMatrix.MultiplyVector(transform.forward) * move.magnitude;
            if (controller.isGrounded)
            {
                _moveDir = transform.forward * move.magnitude;

                _moveDir *= moveSpeed;
            }

            _moveDir.y += Gravity * Time.deltaTime;

            controller.Move(_moveDir * Time.deltaTime);

            float hitRange = 2f * (move.magnitude + 1.0f);
            enemiesTrigger.GetComponent<CapsuleCollider>().height = hitRange;
            enemiesTrigger.transform.localPosition = new Vector3(0.0f, 0.0f, (hitRange / 2) + 0.5f);
            enemiesTriggerPivot.transform.localRotation = Quaternion.Euler(0.0f, 20f * turnAmount, 0.0f);

            bool dive = Input.GetKey(KeyCode.Space);            
            if (dive && diveAvaible)
            {
                animator.SetBool("Dive", true);
                animator.SetFloat("Vertical", 0.0f);
                diveAvaible = false;
                controlsDisabled = true;
                StartCoroutine(Dive());
                return;
            }
        }
        else
        {
            _moveDir.x = 0.0f;
            _moveDir.z = 0.0f;
            _moveDir.y += Gravity * Time.deltaTime;

            controller.Move(_moveDir * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (controlsDisabled) return;     
    }

    IEnumerator Dive()
    {
        float v = 0;
        float h = 0;
        if (vertical > 0.1f)
            v = 1.0f;
        else if (vertical < -0.1f)
            v = -1.0f;
        if (horizontal > 0.1f)
            h = 1.0f;
        else if (horizontal < -0.1f)
            h = -1.0f;
        Vector3 camForward_Dir = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 move = v * camForward_Dir + h * Camera.main.transform.right;
        float turnAmount = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, turnAmount, 0));

        float elapse_time = 0;

        while (elapse_time < 1.55f)
        {
            //Move
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            elapse_time += Time.deltaTime;

            yield return null;
        }
        animator.SetBool("Dive", false);
        Invoke("DiveRecharge", 0.2f);
        controlsDisabled = false;
    } 

    void DiveRecharge()
    {
        diveAvaible = true;
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y + 0.05f);
    }

    void disableControls()
    {
        isAttacking = true;
        controlsDisabled = true;
        Invoke("enableWeapon", 0.3f);
    }

    void enableWeapon()
    {        
        damage.enableWeaponCollider();
    }

    void enableControls()
    {
        isAttacking = false;
        animator.SetBool("Fire1", false);
        controlsDisabled = false;
        damage.disableWeaponCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            if (!other.gameObject.GetComponent<Enemy>().isDead)
            {
                enemies.Add(other.gameObject.transform);
                Debug.Log("Enemy in range");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 9 && !isAttacking)
        {
            enemies.RemoveAll(name => name == other.gameObject.transform);
            Debug.Log("Enemy out of range");
        }
    }

    void GetClosestEnemy(List<Transform> enemies, Transform fromThis)
    {
        Transform bestTarget = null;
        if (enemies.Count > 0)
        {
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = fromThis.position;
            try
            {
                foreach (Transform potentialTarget in enemies.ToArray())
                {
                    if(potentialTarget == null) { enemies.Remove(potentialTarget); }
                    else if (potentialTarget.GetComponent<Enemy>().isDead) { enemies.Remove(potentialTarget); }
                    else
                    {
                        Vector3 directionToTarget = potentialTarget.position - currentPosition;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if (dSqrToTarget < closestDistanceSqr)
                        {
                            closestDistanceSqr = dSqrToTarget;
                            bestTarget = potentialTarget;
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { Debug.LogError(ex.Message); }
            catch (MissingReferenceException ex) { Debug.LogError(ex.Message); }
        }
        if(bestTarget != null)
        {
            StartCoroutine(moveToClosestEnemy(bestTarget, true));
        } else
        {
            StartCoroutine(moveToClosestEnemy(new GameObject().transform, false));
        }
    }

    IEnumerator moveToClosestEnemy(Transform target, bool haveTarget)
    {
        if(!haveTarget)
        {
            target.position = transform.position + transform.forward*1.95f;
        }
        float dragDown = gravity;
        float angle = 10f;

        // Calculate distance to target
        targetDistance = Vector3.Distance(transform.position, target.position) - 1f;
        animator.SetFloat("TargetDistance", targetDistance);
        animator.SetBool("Fire1", true);
        dragDown *= Mathf.Abs(targetDistance);

        // Calculate the velocity needed to throw the object to the target at specified angle.
        float projectile_Velocity = targetDistance / (Mathf.Sin(2 * angle * Mathf.Deg2Rad) / dragDown);
        if (projectile_Velocity <= 0f)
            projectile_Velocity = -projectile_Velocity;

        // Extract the X  Y componenent of the velocity
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(angle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(angle * Mathf.Deg2Rad);

        if (targetDistance <= 0f)
        {
            Vx = -Vx;
        }

        // Calculate flight time.
        float flightDuration = targetDistance / Vx;

        float elapse_time = 0;

        while (elapse_time < flightDuration)
        {
            transform.Translate(0, (Vy - (dragDown * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);

            // Rotate projectile to face the target.
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((target.position - transform.position).normalized), Time.deltaTime * 4.0f);

            elapse_time += Time.deltaTime;

            yield return null;
        }
    }
}