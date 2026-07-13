using System.Collections.Generic;
using UnityEngine;

public sealed class DamageArea : MonoBehaviour
{
    [SerializeField] private BoxCollider2D areaCollider;
    [SerializeField] private SpriteRenderer areaRenderer;
    [SerializeField] private float visualDuration = 0.3f;

    private static int enemyTriggerLayer = -1;

    private readonly HashSet<Enemy_Base> damagedEnemies = new HashSet<Enemy_Base>(32);

    private AreaEffectSystem owner;
    private DamageArea prefabKey;
    private CombatPipeline combatPipeline;
    private BallType sourceBallType;
    private DamageType damageType;
    private int damage;
    private int fixedStepCount;
    private float totalVisualTime;
    private float remainingVisualTime;
    private bool isDetecting;
    private Color initialRendererColor;

    private void Awake()
    {
        if (enemyTriggerLayer == -1)
        {
            enemyTriggerLayer = LayerMask.NameToLayer("Enemy Trigger");
        }

        if (areaRenderer != null)
        {
            initialRendererColor = areaRenderer.color;
        }
    }

    private void Update()
    {
        remainingVisualTime = Mathf.Max(0f, remainingVisualTime - Time.deltaTime);
        UpdateVisualAlpha();

        if (remainingVisualTime <= 0f)
        {
            Release();
        }
    }

    private void FixedUpdate()
    {
        if (!isDetecting)
        {
            return;
        }

        if (fixedStepCount <= 0)
        {
            fixedStepCount++;
            return;
        }

        isDetecting = false;

        if (areaCollider != null)
        {
            areaCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    public void Activate(
        AreaEffectSystem poolOwner,
        DamageArea sourcePrefab,
        CombatPipeline pipeline,
        Vector2 center,
        Vector2 size,
        int damageValue,
        BallType ballType,
        DamageType type)
    {
        owner = poolOwner;
        prefabKey = sourcePrefab;
        combatPipeline = pipeline;
        sourceBallType = ballType;
        damageType = type;
        damage = Mathf.Max(0, damageValue);
        fixedStepCount = 0;
        isDetecting = true;
        damagedEnemies.Clear();

        transform.position = center;

        Vector2 safeSize = new Vector2(
            Mathf.Max(0.01f, size.x),
            Mathf.Max(0.01f, size.y));

        if (areaCollider != null)
        {
            areaCollider.size = safeSize;
            areaCollider.enabled = true;
        }

        if (areaRenderer != null)
        {
            areaRenderer.color = initialRendererColor;
            areaRenderer.size = safeSize;
        }

        totalVisualTime = Mathf.Max(visualDuration, Time.fixedDeltaTime * 2f);
        remainingVisualTime = totalVisualTime;
        gameObject.SetActive(true);
    }

    private void TryDamage(Collider2D other)
    {
        if (!isDetecting
            || other == null
            || other.gameObject.layer != enemyTriggerLayer
            || combatPipeline == null
            || damage <= 0)
        {
            return;
        }

        Enemy_Base enemy = other.GetComponent<Enemy_Base>();

        if (enemy == null
            || !enemy.CanReceiveDamage
            || !damagedEnemies.Add(enemy))
        {
            return;
        }

        DamageRequest request = new DamageRequest(
            damage,
            sourceBallType,
            damageType,
            DamageSourceType.Effect,
            false,
            false,
            enemy.Center,
            null,
            null,
            enemy);

        combatPipeline.ResolveDamage(request);
    }

    private void UpdateVisualAlpha()
    {
        if (areaRenderer == null || totalVisualTime <= 0f)
        {
            return;
        }

        Color color = initialRendererColor;
        color.a *= remainingVisualTime / totalVisualTime;
        areaRenderer.color = color;
    }

    private void Release()
    {
        if (owner == null)
        {
            gameObject.SetActive(false);
            return;
        }

        AreaEffectSystem poolOwner = owner;
        DamageArea sourcePrefab = prefabKey;

        owner = null;
        prefabKey = null;
        combatPipeline = null;
        damage = 0;
        totalVisualTime = 0f;
        remainingVisualTime = 0f;
        isDetecting = false;
        damagedEnemies.Clear();

        if (areaCollider != null)
        {
            areaCollider.enabled = false;
        }

        if (areaRenderer != null)
        {
            areaRenderer.color = initialRendererColor;
        }

        poolOwner.Release(this, sourcePrefab);
    }
}
