﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public bool isOnGround = false;
    protected Animator anim;
    protected Rigidbody rb;
    // Ked umiera enemak
    public Collider[] colliders;
    protected DissolveEffect[] de;
    public GameObject summonerEffect;
    protected bool summoningComplete = false;
    public virtual void Die()
    {
        if (colliders != null)
        {
            foreach(Collider c in colliders)
            {
                c.enabled = false;
            }
        }
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        foreach(DissolveEffect d in de)
        {
            if(d.gameObject.activeSelf)
                d.startDissolve();
        }
    }
    //Aktivovanie nepriatela, aby sa zacal spravat ako je definovane v GDD
    public virtual void Activate()
    {
        StartCoroutine(Summoning());
    }
    public virtual void Awake()
    {
        if((rb= GetComponent<Rigidbody>())== null)
        {
            Debug.Log("Rigidbody not found");
        }
        anim = GetComponent<Animator>();
        if ((anim = GetComponent<Animator>()) == null)
        {
            Debug.Log("Animator not found");
        }
        de = GetComponentsInChildren<DissolveEffect>();
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")|| collision.gameObject.CompareTag("LivingGround"))
        {
            isOnGround = true;
        }
        if (collision.gameObject.CompareTag("Void"))
        {
            Die();
        }
    }
    protected virtual void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("LivingGround"))
        {
            isOnGround = true;
        }
    }
    protected virtual void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("LivingGround"))
        {
            isOnGround = false;
        }
    }
    public virtual void FixedUpdate()
    {

    }
    public virtual void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Spike"))
        {
            Die();
        }
    }
    IEnumerator Summoning()
    {
        anim.speed = 0f;
        foreach (DissolveEffect d in de)
        {
            if (d.gameObject.activeSelf)
            {
                summonerEffect.SetActive(true);
                yield return StartCoroutine(d.Summon());
            }
            summonerEffect.SetActive(false);
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine("BrainScope");
        summoningComplete = true;
    }
}
