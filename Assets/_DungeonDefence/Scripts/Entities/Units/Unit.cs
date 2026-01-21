using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Ally,
    Enemy
}

public abstract class Unit : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected float maxHp = 100f;
    [SerializeField] protected float currentHp;
    [SerializeField] protected Team team;

    protected Node currentNode;

    public Team MyTeam => team;
    public bool IsDead => currentHp <= 0;

    protected virtual void Start()
    {
        currentHp = maxHp;
    }

    public virtual void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHp -= damage;
        
        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} (Team: {team}) Died.");
        Destroy(gameObject);
    }

    public void SetNode(Node newNode)
    {
        if (currentNode != null && currentNode.TileEffect != null)
        {
            currentNode.TileEffect.OnUnitExit(this);
        }

        currentNode = newNode;
        transform.position = newNode.WorldPosition;

        if (currentNode != null && currentNode.TileEffect != null)
        {
            currentNode.TileEffect.OnUnitEnter(this);
        }
    }
}