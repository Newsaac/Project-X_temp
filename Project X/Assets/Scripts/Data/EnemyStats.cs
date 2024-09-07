using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyStats
{
    public int maxHp = 1;
    public int collideDamage = 10;
    public float collideDmgCooldown = 0.5f;
    public float speed = 1f;
    public float detectRange = 20f;
    public float attackRange = 10f;
    public int attackDamage = 10;
    public float attackCooldown = 0.5f;
}
