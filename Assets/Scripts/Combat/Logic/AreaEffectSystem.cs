using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AreaEffectSystem : MonoBehaviour
{
    [SerializeField] private DamageAreaPoolEntry[] poolEntries;

    public Vector2 CellSize { get; private set; } = Vector2.one;
    public Bounds BoardBounds { get; private set; }

    private void Awake()
    {
        if (poolEntries == null)
        {
            return;
        }

        for (int i = 0; i < poolEntries.Length; i++)
        {
            poolEntries[i]?.Initialize(this);
        }
    }

    public void Initialize(Vector2 cellSize, Bounds boardBounds)
    {
        CellSize = new Vector2(
            Mathf.Max(0.01f, Mathf.Abs(cellSize.x)),
            Mathf.Max(0.01f, Mathf.Abs(cellSize.y)));

        BoardBounds = boardBounds;
    }

    public void ResetRun()
    {
        if (poolEntries == null)
        {
            return;
        }

        for (int i = 0; i < poolEntries.Length; i++)
        {
            poolEntries[i]?.ResetRun();
        }
    }

    public bool Play(
        DamageArea prefab,
        CombatPipeline combatPipeline,
        Vector2 center,
        Vector2 size,
        int damage,
        BallType sourceBallType,
        DamageType damageType)
    {
        DamageAreaPoolEntry entry = FindEntry(prefab);

        if (entry == null || combatPipeline == null || damage <= 0)
        {
            return false;
        }

        DamageArea area = entry.Get(this);

        if (area == null)
        {
            return false;
        }

        area.Activate(
            this,
            prefab,
            combatPipeline,
            center,
            size,
            damage,
            sourceBallType,
            damageType);

        return true;
    }

    internal void Release(DamageArea area, DamageArea prefab)
    {
        DamageAreaPoolEntry entry = FindEntry(prefab);

        if (entry != null)
        {
            entry.Release(area);
            return;
        }

        if (area != null)
        {
            area.ResetState();
        }
    }

    private DamageAreaPoolEntry FindEntry(DamageArea prefab)
    {
        if (prefab == null || poolEntries == null)
        {
            return null;
        }

        for (int i = 0; i < poolEntries.Length; i++)
        {
            DamageAreaPoolEntry entry = poolEntries[i];

            if (entry != null && entry.Prefab == prefab)
            {
                return entry;
            }
        }

        return null;
    }

    [Serializable]
    private sealed class DamageAreaPoolEntry
    {
        [SerializeField] private DamageArea prefab;
        [SerializeField] private int initialPoolSize = 8;

        private Queue<DamageArea> availableAreas;
        private List<DamageArea> allAreas;

        public DamageArea Prefab => prefab;

        public void Initialize(AreaEffectSystem owner)
        {
            int capacity = Mathf.Max(0, initialPoolSize);
            availableAreas = new Queue<DamageArea>(capacity);
            allAreas = new List<DamageArea>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                DamageArea area = Create(owner);

                if (area != null)
                {
                    availableAreas.Enqueue(area);
                }
            }
        }

        public DamageArea Get(AreaEffectSystem owner)
        {
            if (availableAreas == null)
            {
                Initialize(owner);
            }

            return availableAreas.Count > 0
                ? availableAreas.Dequeue()
                : Create(owner);
        }

        public void Release(DamageArea area)
        {
            if (area == null)
            {
                return;
            }

            area.ResetState();
            availableAreas.Enqueue(area);
        }

        public void ResetRun()
        {
            if (availableAreas == null || allAreas == null)
            {
                return;
            }

            availableAreas.Clear();

            for (int i = allAreas.Count - 1; i >= 0; i--)
            {
                DamageArea area = allAreas[i];

                if (area == null)
                {
                    allAreas.RemoveAt(i);
                    continue;
                }

                area.ResetState();
                availableAreas.Enqueue(area);
            }
        }

        private DamageArea Create(AreaEffectSystem owner)
        {
            if (prefab == null)
            {
                return null;
            }

            DamageArea area = Instantiate(prefab, owner.transform);
            area.gameObject.SetActive(false);
            allAreas.Add(area);
            return area;
        }
    }
}
