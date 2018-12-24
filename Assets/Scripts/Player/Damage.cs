using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour {

    public GameObject player;
    Collider weaponCollider;
    List<string> enemiesHit;

	// Use this for initialization
	void Start () {
        weaponCollider = GetComponent<Collider>();
        weaponCollider.enabled = false;
        enemiesHit = new List<string>();        
	}

    public void enableWeaponCollider()
    {
        weaponCollider.enabled = true;
    }

    public void disableWeaponCollider()
    {
        weaponCollider.enabled = false;
        Invoke("clearEnemiesHit", 0.2f);
    }

    void clearEnemiesHit()
    {
        enemiesHit.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enemiesHit.Contains(other.name) && other.gameObject.layer == 9)
        {
            enemiesHit.Add(other.name);
            other.gameObject.GetComponent<Enemy>().Damage(5.5f, player);
            Debug.Log(other.name + " Hit");
        }
    }
}
