using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private Enemy_Base enemy;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private SpriteRenderer blockSprite;
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string hitTriggerName = "Hit";
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private float attackDuration = 0.35f;

    private int hitTriggerHash;
    private int deathTriggerHash;
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

        hitTriggerHash = Animator.StringToHash(hitTriggerName);
        deathTriggerHash = Animator.StringToHash(deathTriggerName);
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
    }

    private void OnDisable()
    {
        if (enemy == null)
        {
            return;
        }

        enemy.EnemyDamaged -= OnEnemyDamaged;
        enemy.EndlineReached -= OnEndlineReached;
        enemy.EnemyDied -= OnEnemyDied;
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
        SetTrigger(hitTriggerName, hitTriggerHash);
    }

    private void OnEndlineReached(Enemy_Base target)
    {
        StartAttackMotion(target);
    }

    private void OnEnemyDied(Enemy_Base target)
    {
        SetTrigger(deathTriggerName, deathTriggerHash);
    }

    private void SetTrigger(string triggerName, int triggerHash)
    {
        if (animator == null || string.IsNullOrEmpty(triggerName))
        {
            return;
        }

        animator.SetTrigger(triggerHash);
    }

    private void StartAttackMotion(Enemy_Base target)
    {
        if (target == null)
        {
            return;
        }

        if (animator != null)
        {
            if (!string.IsNullOrEmpty(idleStateName))
            {
                animator.Play(idleStateName, 0, 0f);
                animator.Update(0f);
            }

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

        if (!string.IsNullOrEmpty(idleStateName))
        {
            animator.Play(idleStateName, 0, 0f);
            animator.Update(0f);
        }
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
}
