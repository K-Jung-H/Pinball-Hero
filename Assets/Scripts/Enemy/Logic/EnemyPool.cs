using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyPool
{
    private readonly Transform parent;
    private readonly Dictionary<EnemyDefinitionSO, PoolBucket> buckets =
        new Dictionary<EnemyDefinitionSO, PoolBucket>();
    private readonly Dictionary<Enemy_Base, PoolBucket> bucketByEnemy =
        new Dictionary<Enemy_Base, PoolBucket>();
    private readonly List<Enemy_Base> allEnemies = new List<Enemy_Base>();
    private readonly HashSet<Enemy_Base> availableEnemies = new HashSet<Enemy_Base>();

    public EnemyPool(Transform parent)
    {
        this.parent = parent;
    }

    public void Prewarm(EnemyDefinitionSO definition, int count)
    {
        if (definition == null || definition.EnemyPrefab == null)
        {
            return;
        }

        PoolBucket bucket = GetOrCreateBucket(definition);
        int targetCount = Mathf.Max(0, count);

        while (bucket.InstanceCount < targetCount)
        {
            if (CreateEnemy(bucket) == null)
            {
                break;
            }
        }
    }

    public Enemy_Base Get(EnemyDefinitionSO definition, Vector3 position)
    {
        if (definition == null || definition.EnemyPrefab == null)
        {
            return null;
        }

        PoolBucket bucket = GetOrCreateBucket(definition);

        if (bucket.Available.Count <= 0 && CreateEnemy(bucket) == null)
        {
            return null;
        }

        Enemy_Base enemy = null;

        while (bucket.Available.Count > 0)
        {
            Enemy_Base candidate = bucket.Available.Dequeue();

            if (candidate == null || !availableEnemies.Remove(candidate))
            {
                continue;
            }

            enemy = candidate;
            break;
        }

        if (enemy == null)
        {
            return null;
        }

        enemy.transform.SetPositionAndRotation(position, Quaternion.identity);
        enemy.Initialize(definition);
        enemy.gameObject.SetActive(true);

        return enemy;
    }

    public void ReturnAll()
    {
        for (int i = allEnemies.Count - 1; i >= 0; i--)
        {
            Enemy_Base enemy = allEnemies[i];

            if (enemy == null)
            {
                allEnemies.RemoveAt(i);
                continue;
            }

            if (!availableEnemies.Contains(enemy))
            {
                enemy.Despawn();
            }
        }
    }

    private PoolBucket GetOrCreateBucket(EnemyDefinitionSO definition)
    {
        if (!buckets.TryGetValue(definition, out PoolBucket bucket))
        {
            bucket = new PoolBucket(definition);
            buckets.Add(definition, bucket);
        }

        return bucket;
    }

    private Enemy_Base CreateEnemy(PoolBucket bucket)
    {
        Enemy_Base prefab = bucket.Definition.EnemyPrefab;

        if (prefab == null)
        {
            return null;
        }

        Enemy_Base enemy = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
        enemy.Despawned += Release;

        bucket.InstanceCount++;
        bucketByEnemy.Add(enemy, bucket);
        allEnemies.Add(enemy);
        enemy.Despawn();

        return enemy;
    }

    private void Release(Enemy_Base enemy)
    {
        if (enemy == null
            || !bucketByEnemy.TryGetValue(enemy, out PoolBucket bucket)
            || !availableEnemies.Add(enemy))
        {
            return;
        }

        enemy.gameObject.SetActive(false);
        bucket.Available.Enqueue(enemy);
    }

    private sealed class PoolBucket
    {
        public PoolBucket(EnemyDefinitionSO definition)
        {
            Definition = definition;
        }

        public EnemyDefinitionSO Definition { get; }
        public Queue<Enemy_Base> Available { get; } = new Queue<Enemy_Base>();
        public int InstanceCount { get; set; }
    }
}
