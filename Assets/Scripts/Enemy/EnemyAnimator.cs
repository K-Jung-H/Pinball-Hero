using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private Enemy_Base enemy;
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTriggerName = "Hit";
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string deathTriggerName = "Death";

    private int hitTriggerHash;
    private int attackTriggerHash;
    private int deathTriggerHash;

    private void Awake()
    {
        hitTriggerHash = Animator.StringToHash(hitTriggerName);
        attackTriggerHash = Animator.StringToHash(attackTriggerName);
        deathTriggerHash = Animator.StringToHash(deathTriggerName);
    }

    private void OnEnable()
    {
        if (enemy == null)
        {
            return;
        }

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

    public void OnAttackAnimationFinished()
    {
        if (enemy != null)
        {
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
        SetTrigger(attackTriggerName, attackTriggerHash);
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
}
