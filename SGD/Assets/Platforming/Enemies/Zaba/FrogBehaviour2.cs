﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogBehaviour2 : Enemy
{
    public Transform Target;
    public float firingAngle = 45.0f;
    public float gravity = 9.8f;
    public float rotSpeed=1f;
    public float afkTime = 3f;

    public float buffedAfk;
    public float buffedGravity;
    public float buffedRotSpeed;
    private Transform myTransform;
    public LayerMask lm;

    public AudioSource deathSound;
    public AudioSource jumpSound;
    public AudioSource landSound;
    public AudioSource nomNomSound;
    public GameObject frontGO;
    public GameObject rightGO;
    public GameObject Crown;
    public GameObject Gem;
    Collider frontColl;
    Collider rightColl;
    bool isDead = false;
    private bool hasCollide = false;
    public override void Awake()
    {
        base.Awake();
        frontColl = frontGO.GetComponent<Collider>();
        rightColl = rightGO.GetComponent<Collider>();
        myTransform = transform;
    }
    private void LateUpdate()
    {
        hasCollide = false;
    }
    void Start()
    {
        Activate();
    }  
    IEnumerator BrainScope()
    {
        anim.speed = 1f;
        while (true)
        {
            if (isOnGround)
            {
                FindPlace(12f);
                yield return new WaitForFixedUpdate();
                if (Target != null)
                {
                    yield return RotateTowardsPosition();
                    yield return JumpToPosition();
                }
                yield return new WaitForSeconds(afkTime-Random.Range(-afkTime/4,afkTime/3));

            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForFixedUpdate();
        }
    }
    public void FindPlace(float MaxDist)
    {
        List<GameObject> grounds = new List<GameObject>();
        if (!Crown.activeSelf)
        {
            lm = LayerMask.GetMask("Gem", "Ground", "Player");
        }
        else
        {
            lm = LayerMask.GetMask("Ground");
        }
        Collider[] colls=Physics.OverlapBox(frontColl.bounds.center, frontColl.bounds.size / 2, transform.rotation, lm);
        foreach (Collider c in colls)
        {
            Vector3 cp = new Vector3(c.transform.position.x, transform.position.y, c.transform.position.z);
            if (Vector3.Distance(transform.position, cp) > 1.5f && Vector3.Distance(transform.position, cp) < MaxDist)
            {
                grounds.Add(c.gameObject);
            }
        }

        colls = Physics.OverlapBox(rightColl.bounds.center, rightColl.bounds.size / 2, transform.rotation, lm);
        foreach (Collider c in colls)
        {
            Vector3 cp = new Vector3(c.transform.position.x, transform.position.y, c.transform.position.z);
            if (Vector3.Distance(transform.position, cp) > 1.5f && Vector3.Distance(transform.position, cp)<MaxDist)
            {
                grounds.Add(c.gameObject);
            }
        }
        if (grounds.Count >= 1)
        {
            Target = grounds[Random.Range(0, grounds.Count)].transform;
        }


    }

    IEnumerator RotateTowardsPosition()
    {
        Vector3 tarPos = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        Vector3 targetDirection = tarPos - transform.position;
        Quaternion targetRotaion = Quaternion.LookRotation(tarPos - transform.position);
        while (Quaternion.Angle(transform.rotation, targetRotaion)>=1f)
        {
            float singleStep = rotSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator JumpToPosition()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        lm = LayerMask.GetMask("Wall");
        float maxRayDistance = 12.5f;
        if (!Physics.Raycast(ray, maxRayDistance, lm))
        {
            anim.SetBool("isJumping", true);
            // Short delay added before frog jumps
            Vector3 tarPos = new Vector3(Target.position.x, transform.position.y, Target.position.z);
            yield return new WaitForSeconds(0.5f);
            jumpSound.Play();
            rb.useGravity = false;

            // Calculate distance to target
            float target_Distance = Vector3.Distance(transform.position, tarPos);

            // Calculate the velocity needed to move the target at specified angle.
            float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

            // Extract the X  Y componenent of the velocity
            float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
            float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

            // Calculate flight time.
            float flightDuration = target_Distance / Vx;

            float elapse_time = 0;

            while (elapse_time < flightDuration)
            {
                transform.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);

                elapse_time += Time.deltaTime;
                if (elapse_time + 0.35f > flightDuration)
                    anim.SetBool("isJumping", false);

                yield return new WaitForFixedUpdate();
            }
            landSound.Play();
            rb.useGravity = true;
        }
        else
        {
            Debug.Log("There is a wall i cant jump");
        }
        
        Target = null;
    }
    public override void Die()
    {
        if (!isDead)
        {
            StopAllCoroutines();
            LevelManager.Instance.StartCoroutine(LevelManager.Instance.SpawnMonster("f", startPos, startRot));
            base.Die();

            if (Crown.activeSelf)
            {
                GameObject g = Instantiate(Gem);
                g.transform.position = transform.position + Vector3.up * 0.3f;
                Crown.GetComponent<DissolveEffect>().StartDissolve();
            }
            deathSound.Play();
            isDead = true;
        }
    }
    public void DieWithCrown()
    {
        if (!isDead)
        {
            LevelManager.Instance.StartCoroutine(LevelManager.Instance.SpawnMonster("ff", startPos, startRot));
            base.Die();
            deathSound.Play();
            isDead = true;
        }
    }
    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.gameObject.CompareTag("Enemy")&&!hasCollide)
        {
            if (transform.position.y - 0.15f > other.gameObject.transform.position.y)
            {
                other.GetComponent<Enemy>().Die();
            }
            else
            {
                Die();
            }
            hasCollide = true;
        }
        if (other.gameObject.CompareTag("Gem") && !Crown.activeSelf && !hasCollide)
        {
            afkTime = buffedAfk;
            gravity = buffedGravity;
            rotSpeed = buffedRotSpeed;
            Crown.SetActive(true);
            nomNomSound.Play();
            Destroy(other.gameObject);
            hasCollide = true;
        }
        if (other.gameObject.CompareTag("Void") && !hasCollide)
        {
            if (Crown.activeSelf)
            {
                DieWithCrown();
            }
            else
            {
                Die();
            }
            hasCollide = true;
        }
    }
}
