using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private const string BaseLayerPrefix = "Base Layer.";
    private const string VisualStatusParameterName = "VisualStatus";

    [Header("References")]
    [SerializeField] private Enemy_Base enemy;
    [SerializeField] private StatusController statusController;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private SpriteRenderer blockSprite;

    [Header("Animation State Names")]
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string hitStateName = "Hit";
    [SerializeField] private string deathStateName = "Death";

    [Header("Attack Motion")]
    [SerializeField] private float attackDuration = 0.35f;

    private int visualStatusHash;
    private int idleStateHash;
    private int hitStateHash;
    private int deathStateHash;
    private bool isAttackPlaying;
    private float attackElapsed;
    private Vector3 bodyStartPosition;
    private Vector3 bodyDefaultLocalPosition;
    private Color blockDefaultColor;

    private void Awake()
    {
        if (bodyTransform != null)
        {
            bodyDefaultLocalPosition = bodyTransform.localPosition;
        }

        if (blockSprite != null)
        {
            blockDefaultColor = blockSprite.color;
        }

        visualStatusHash = Animator.StringToHash(VisualStatusParameterName);
        idleStateHash = GetStateHash(idleStateName);
        hitStateHash = GetStateHash(hitStateName);
        deathStateHash = GetStateHash(deathStateName);
    }

    private void OnEnable()
    {
        if (enemy == null)
        {
            return;
        }

        ResetVisualState();
        enemy.EnemyDamaged += OnEnemyDamaged;
        enemy.EndlineReached += OnEndlineReached;
        enemy.EnemyDied += OnEnemyDied;

        if (statusController != null)
        {
            statusController.VisualStatusChanged += OnVisualStatusChanged;
            OnVisualStatusChanged(statusController.VisualStatus);
        }
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.EnemyDamaged -= OnEnemyDamaged;
            enemy.EndlineReached -= OnEndlineReached;
            enemy.EnemyDied -= OnEnemyDied;
        }

        if (statusController != null)
        {
            statusController.VisualStatusChanged -= OnVisualStatusChanged;
        }
    }

    private void Update()
    {
        if (!isAttackPlaying || enemy == null)
        {
            return;
        }

        attackElapsed += Time.deltaTime;

        float duration = Mathf.Max(0.0001f, attackDuration);
        float ratio = Mathf.Clamp01(attackElapsed / duration);

        if (bodyTransform != null && enemy.AttackTarget != null)
        {
            bodyTransform.position = Vector3.Lerp(bodyStartPosition, enemy.AttackTarget.position, ratio);
        }

        SetBlockAlpha(1f - ratio);

        if (ratio >= 1f)
        {
            isAttackPlaying = false;
            enemy.CompleteEndlineAttack();
        }
    }

    public void OnDeathAnimationFinished()
    {
        if (enemy != null)
        {
            enemy.Despawn();
        }
    }

    private void OnEnemyDamaged(Enemy_Base target)
    {
        PlayState(hitStateHash);
    }

    private void OnEndlineReached(Enemy_Base target)
    {
        StartAttackMotion(target);
    }

    private void OnEnemyDied(Enemy_Base target)
    {
        PlayState(deathStateHash);
    }

    private void OnVisualStatusChanged(EnemyVisualStatus visualStatus)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetInteger(visualStatusHash, (int)visualStatus);
    }

    private void StartAttackMotion(Enemy_Base target)
    {
        if (target == null)
        {
            return;
        }

        if (animator != null)
        {
            PlayState(idleStateHash);
            animator.Update(0f);

            animator.enabled = false;
        }

        if (bodyTransform != null)
        {
            bodyStartPosition = bodyTransform.position;
        }

        attackElapsed = 0f;
        isAttackPlaying = true;

        if (target.AttackTarget == null || bodyTransform == null)
        {
            isAttackPlaying = false;
            target.CompleteEndlineAttack();
        }
    }

    private void ResetVisualState()
    {
        isAttackPlaying = false;
        attackElapsed = 0f;

        if (bodyTransform != null)
        {
            bodyTransform.localPosition = bodyDefaultLocalPosition;
        }

        if (blockSprite != null)
        {
            blockSprite.color = blockDefaultColor;
        }

        if (animator == null)
        {
            return;
        }

        animator.enabled = true;
        animator.SetInteger(visualStatusHash, (int)EnemyVisualStatus.Idle);

        PlayState(idleStateHash);
        animator.Update(0f);
    }

    private void SetBlockAlpha(float value)
    {
        if (blockSprite == null)
        {
            return;
        }

        Color color = blockSprite.color;
        color.a = Mathf.Clamp01(value);
        blockSprite.color = color;
    }

    private void PlayState(int stateHash)
    {
        if (animator == null
            || stateHash == 0
            || !animator.HasState(0, stateHash))
        {
            return;
        }

        animator.Play(stateHash, 0, 0f);
    }

    private static int GetStateHash(string stateName)
    {
        if (string.IsNullOrEmpty(stateName))
        {
            return 0;
        }

        string fullStateName = stateName.IndexOf('.') >= 0
            ? stateName
            : BaseLayerPrefix + stateName;

        return Animator.StringToHash(fullStateName);
    }
}
