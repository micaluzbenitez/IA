using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [Header("HP")]
    [SerializeField] private float hp;

    [Header("Damage")]
    [SerializeField] private int damage;

    public void TakeDamage(float damage)
    {       
        if (hp <= 0) return;

        hp -= damage;
        if (hp < 0) hp = 0;
    }
}
