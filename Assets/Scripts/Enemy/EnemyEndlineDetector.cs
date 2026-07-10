using System;
using UnityEngine;

public class EnemyEndlineDetector : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayerMask;

    public event Action<Enemy_Base, int> EnemyReachedEndline;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!ContainsLayer(enemyLayerMask, other.gameObject.layer))
        {
            return;
        }

        if (!other.TryGetComponent(out Enemy_Base enemy))
        {
            return;
        }

        if (enemy.ReachEndline())
        {
            EnemyReachedEndline?.Invoke(enemy, enemy.Damage);
        }
    }

    private static bool ContainsLayer(LayerMask layerMask, int layer)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
