using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    Animator animator;
    public bool isDead;
    float hp;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        hp = 10;
	}
	
	// Update is called once per frame
	void Update () {

	}
    
    void DestroyThis()
    {
        Destroy(gameObject);
    }

    public void Damage(float damage, GameObject attacker)
    {
        hp -= damage;        
        animator.SetBool("Hit", true);
        Invoke("AfterHit", 0.5f);
        if (hp <= 0)
        {
            Kill(attacker);
        }
    }

    private void Kill(GameObject attacker)
    {
        isDead = true;
        Destroy(GetComponent<Collider>());
        animator.SetBool("Dead", true);
        Invoke("DestroyThis", 10f);
    }

    private void AfterHit()
    {
        animator.SetBool("Hit", false);
    }
}
