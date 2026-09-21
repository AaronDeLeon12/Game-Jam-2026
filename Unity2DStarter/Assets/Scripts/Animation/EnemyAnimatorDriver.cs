using UnityEngine;
public class EnemyAnimatorDriver : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer visual;
    private MantisEnemy mantis;
    private FinalBoss boss;
    private string current;
    private void Awake() { mantis = GetComponent<MantisEnemy>(); boss = GetComponent<FinalBoss>(); }
    private void Update()
    {
        bool attack = mantis != null ? mantis.IsAttacking : boss != null && (boss.IsAttacking || boss.IsShooting);
        bool moving = mantis != null ? mantis.IsMoving : boss != null && boss.IsMoving;
        int direction = mantis != null ? mantis.FacingDirection : boss != null ? boss.FacingDirection : 1;
        visual.flipX = direction < 0;
        string state = attack ? "Attack" : moving ? "Walk" : "Idle";
        if (current != state) { animator.Play(state, 0, 0); current = state; }
    }
}
