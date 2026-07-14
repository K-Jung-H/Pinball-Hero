using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnedEnemyRoot;
    [SerializeField] private Collider2D spawnAreaCollider;
    [SerializeField] private Vector2 cellSize = Vector2.one;
    [SerializeField] private float rowSpawnInterval = 1f;
    [Min(0)]
    [SerializeField] private int initialPoolSizePerEnemyType = 8;

    private readonly List<SpawnedEnemyState> activeEnemies = new List<SpawnedEnemyState>();
    private readonly List<Enemy_Base> spawnedEnemies = new List<Enemy_Base>();

    private Transform attackTarget;
    private StageDefinitionSO stageDefinition;
    private WaveDefinitionSO waveDefinition;
    private bool[] spawnedEntries;
    private int initialSpawnRows;
    private int currentWaveIndex;
    private int nextSpawnLine;
    private int pendingAttackCount;
    private float spawnTimer;
    private bool isRunning;
    private bool hasFinishedAllWaves;
    private bool hasRaisedStageCompleted;
    private EnemyPool enemyPool;

    public Vector2 CellSize => cellSize;
    public Bounds BoardBounds => spawnAreaCollider != null
        ? spawnAreaCollider.bounds
        : default;

    public event Action<Enemy_Base> EnemyDefeated;
    public event Action<int> ExperienceGained;
    public event Action<int> PlayerDamageRequested;
    public event Action<int> WaveStarted;
    public event Action StageCompleted;

    private void Awake()
    {
        EnsureEnemyPool();
    }

    private void EnsureEnemyPool()
    {
        if (enemyPool != null)
        {
            return;
        }

        Transform poolParent = spawnedEnemyRoot != null
            ? spawnedEnemyRoot
            : transform;

        enemyPool = new EnemyPool(poolParent);
    }

    public void SetAttackTarget(Transform target)
    {
        attackTarget = target;
    }

    public void ResetRun()
    {
        EnsureEnemyPool();
        isRunning = false;

        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            Enemy_Base enemy = spawnedEnemies[i];

            if (enemy == null)
            {
                continue;
            }

            UnsubscribeEnemy(enemy);
        }

        spawnedEnemies.Clear();
        activeEnemies.Clear();
        enemyPool?.ReturnAll();
        stageDefinition = null;
        waveDefinition = null;
        spawnedEntries = null;
        initialSpawnRows = 0;
        currentWaveIndex = 0;
        nextSpawnLine = 0;
        pendingAttackCount = 0;
        spawnTimer = 0f;
        hasFinishedAllWaves = false;
        hasRaisedStageCompleted = false;
    }

    public void StartStage(StageDefinitionSO definition)
    {
        EnsureEnemyPool();

        if (definition == null)
        {
            Debug.LogError("StageDefinition is not assigned.");
            return;
        }

        WaveDefinitionSO[] waves = definition.Waves;

        if (waves == null || waves.Length <= 0)
        {
            Debug.LogError("Stage waves are not assigned.");
            return;
        }

        if (spawnAreaCollider == null)
        {
            Debug.LogError("SpawnArea Collider2D is not assigned.");
            return;
        }

        stageDefinition = definition;
        initialSpawnRows = Mathf.Max(0, stageDefinition.InitialSpawnRows);
        currentWaveIndex = 0;
        pendingAttackCount = 0;
        hasFinishedAllWaves = false;
        hasRaisedStageCompleted = false;
        PrewarmEnemyPool(stageDefinition);
        StartWave(stageDefinition.Waves[currentWaveIndex]);
    }

    private void StartWave(WaveDefinitionSO definition)
    {
        if (definition == null)
        {
            Debug.LogError("WaveDefinition is not assigned.");
            StartNextWave();
            return;
        }

        waveDefinition = definition;
        WaveSpawnEntry[] entries = waveDefinition.SpawnEntries;

        if (entries == null || entries.Length <= 0)
        {
            Debug.LogError("Wave spawn entries are not assigned.");
            StartNextWave();
            return;
        }

        spawnedEntries = new bool[entries.Length];
        nextSpawnLine = Mathf.Clamp(initialSpawnRows, 0, waveDefinition.WaveHeight);
        activeEnemies.Clear();
        spawnTimer = 0f;
        isRunning = true;

        WaveStarted?.Invoke(currentWaveIndex + 1);

        SpawnLines(0, nextSpawnLine - 1, nextSpawnLine - 1);
    }

    private void Update()
    {
        if (!isRunning || waveDefinition == null)
        {
            return;
        }

        if (nextSpawnLine >= waveDefinition.WaveHeight)
        {
            if (activeEnemies.Count <= 0)
            {
                StartNextWave();
            }

            return;
        }

        spawnTimer += Time.deltaTime;

        if (!CanSpawnNextLine())
        {
            return;
        }

        if (spawnTimer < rowSpawnInterval)
        {
            return;
        }

        SpawnNextLine();
    }

    private void SpawnNextLine()
    {
        SpawnLines(nextSpawnLine, nextSpawnLine, nextSpawnLine);
        nextSpawnLine++;
        spawnTimer = 0f;
    }

    private void SpawnLines(int startLine, int endLine, int anchorLine)
    {
        if (startLine > endLine)
        {
            return;
        }

        WaveSpawnEntry[] entries = waveDefinition.SpawnEntries;

        for (int i = 0; i < entries.Length; i++)
        {
            if (spawnedEntries[i])
            {
                continue;
            }

            WaveSpawnEntry entry = entries[i];
            EnemyDefinitionSO enemyDefinition = entry.Enemy;

            if (enemyDefinition == null)
            {
                continue;
            }

            int enemyStartRow = entry.Row;
            int enemyEndRow = entry.Row + enemyDefinition.CellHeight - 1;

            if (!IsRangeOverlapping(startLine, endLine, enemyStartRow, enemyEndRow))
            {
                continue;
            }

            if (SpawnEnemy(entry, enemyDefinition, anchorLine))
            {
                spawnedEntries[i] = true;
            }
        }
    }

    private bool SpawnEnemy(WaveSpawnEntry entry, EnemyDefinitionSO enemyDefinition, int anchorLine)
    {
        if (!IsEntryInsideWave(entry, enemyDefinition))
        {
            Debug.LogWarning("Wave spawn entry is outside the wave bounds.");
            return false;
        }

        Enemy_Base prefab = enemyDefinition.EnemyPrefab;

        if (prefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned.");
            return false;
        }

        Vector3 position = GetSpawnPosition(entry, enemyDefinition, anchorLine);
        Enemy_Base enemy = enemyPool != null
            ? enemyPool.Get(enemyDefinition, position)
            : null;

        if (enemy == null)
        {
            return false;
        }

        enemy.SetAttackTarget(attackTarget);
        SubscribeEnemy(enemy);
        spawnedEnemies.Add(enemy);
        activeEnemies.Add(new SpawnedEnemyState(enemy, enemyDefinition));

        return true;
    }

    private void OnEnemyDied(Enemy_Base enemy)
    {
        EnemyDefinitionSO enemyDefinition = FindEnemyDefinition(enemy);

        if (enemy != null)
        {
            UnsubscribeActiveEvents(enemy);
        }

        EnemyDefeated?.Invoke(enemy);

        if (enemyDefinition != null && enemyDefinition.ExperienceReward > 0)
        {
            ExperienceGained?.Invoke(enemyDefinition.ExperienceReward);
        }

        RemoveActiveEnemy(enemy);
    }

    private void OnEnemyAttackCompleted(Enemy_Base enemy, int damage)
    {
        if (enemy != null)
        {
            enemy.EnemyAttackCompleted -= OnEnemyAttackCompleted;
        }

        if (damage > 0)
        {
            PlayerDamageRequested?.Invoke(damage);
        }

        pendingAttackCount = Mathf.Max(0, pendingAttackCount - 1);
        TryCompleteStage();
    }

    private void OnEnemyEndlineReached(Enemy_Base enemy)
    {
        pendingAttackCount++;

        if (enemy != null)
        {
            enemy.EnemyDied -= OnEnemyDied;
            enemy.EndlineReached -= OnEnemyEndlineReached;
        }

        RemoveActiveEnemy(enemy);
    }

    private void OnEnemyDespawned(Enemy_Base enemy)
    {
        UnsubscribeEnemy(enemy);

        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == enemy)
            {
                spawnedEnemies.RemoveAt(i);
                return;
            }
        }
    }

    private void PrewarmEnemyPool(StageDefinitionSO definition)
    {
        if (enemyPool == null || definition == null)
        {
            return;
        }

        WaveDefinitionSO[] waves = definition.Waves;

        for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
        {
            WaveDefinitionSO wave = waves[waveIndex];

            if (wave == null || wave.SpawnEntries == null)
            {
                continue;
            }

            WaveSpawnEntry[] entries = wave.SpawnEntries;

            for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
            {
                enemyPool.Prewarm(entries[entryIndex].Enemy, initialPoolSizePerEnemyType);
            }
        }
    }

    private void SubscribeEnemy(Enemy_Base enemy)
    {
        if (enemy == null)
        {
            return;
        }

        UnsubscribeEnemy(enemy);
        enemy.EnemyDied += OnEnemyDied;
        enemy.EndlineReached += OnEnemyEndlineReached;
        enemy.EnemyAttackCompleted += OnEnemyAttackCompleted;
        enemy.Despawned += OnEnemyDespawned;
    }

    private void UnsubscribeActiveEvents(Enemy_Base enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.EnemyDied -= OnEnemyDied;
        enemy.EndlineReached -= OnEnemyEndlineReached;
        enemy.EnemyAttackCompleted -= OnEnemyAttackCompleted;
    }

    private void UnsubscribeEnemy(Enemy_Base enemy)
    {
        if (enemy == null)
        {
            return;
        }

        UnsubscribeActiveEvents(enemy);
        enemy.Despawned -= OnEnemyDespawned;
    }

    private Vector3 GetSpawnPosition(WaveSpawnEntry entry, EnemyDefinitionSO enemyDefinition, int anchorLine)
    {
        Bounds spawnAreaBounds = spawnAreaCollider.bounds;
        float firstColumnCenterX = spawnAreaBounds.min.x + cellSize.x * 0.5f;
        float enemyCenterOffsetX = (enemyDefinition.CellWidth - 1) * cellSize.x * 0.5f;
        float enemyCenterOffsetY = (enemyDefinition.CellHeight - 1) * cellSize.y * 0.5f;

        float x = firstColumnCenterX + entry.Column * cellSize.x + enemyCenterOffsetX;
        float y = spawnAreaBounds.center.y + (entry.Row - anchorLine) * cellSize.y + enemyCenterOffsetY;

        return new Vector3(x, y, 0f);
    }

    private bool CanSpawnNextLine()
    {
        float spawnAreaBottomY = spawnAreaCollider.bounds.min.y;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            SpawnedEnemyState state = activeEnemies[i];

            if (state.Enemy == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            float enemyTopY = state.Enemy.transform.position.y + state.Definition.CellHeight * cellSize.y * 0.5f;

            if (enemyTopY > spawnAreaBottomY)
            {
                return false;
            }
        }

        return true;
    }

    private void RemoveActiveEnemy(Enemy_Base enemy)
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i].Enemy == enemy)
            {
                activeEnemies.RemoveAt(i);
                return;
            }
        }
    }

    private EnemyDefinitionSO FindEnemyDefinition(Enemy_Base enemy)
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            SpawnedEnemyState state = activeEnemies[i];

            if (state.Enemy == enemy)
            {
                return state.Definition;
            }
        }

        return null;
    }

    private static bool IsRangeOverlapping(int firstStart, int firstEnd, int secondStart, int secondEnd)
    {
        return firstStart <= secondEnd && secondStart <= firstEnd;
    }

    private bool IsEntryInsideWave(WaveSpawnEntry entry, EnemyDefinitionSO enemyDefinition)
    {
        if (waveDefinition == null || enemyDefinition == null)
        {
            return false;
        }

        if (entry.Column < 0 || entry.Row < 0)
        {
            return false;
        }

        if (enemyDefinition.CellWidth <= 0 || enemyDefinition.CellHeight <= 0)
        {
            return false;
        }

        if (entry.Column + enemyDefinition.CellWidth > waveDefinition.BoardWidth)
        {
            return false;
        }

        return entry.Row + enemyDefinition.CellHeight <= waveDefinition.WaveHeight;
    }

    private void StartNextWave()
    {
        if (stageDefinition == null)
        {
            isRunning = false;
            return;
        }

        currentWaveIndex++;

        if (currentWaveIndex >= stageDefinition.Waves.Length)
        {
            isRunning = false;
            waveDefinition = null;
            hasFinishedAllWaves = true;
            TryCompleteStage();
            return;
        }

        StartWave(stageDefinition.Waves[currentWaveIndex]);
    }

    private void TryCompleteStage()
    {
        if (!hasFinishedAllWaves
            || pendingAttackCount > 0
            || hasRaisedStageCompleted)
        {
            return;
        }

        hasRaisedStageCompleted = true;
        StageCompleted?.Invoke();
    }

    private readonly struct SpawnedEnemyState
    {
        public readonly Enemy_Base Enemy;
        public readonly EnemyDefinitionSO Definition;

        public SpawnedEnemyState(Enemy_Base enemy, EnemyDefinitionSO definition)
        {
            Enemy = enemy;
            Definition = definition;
        }
    }
}
