using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    private const string IS_WALKING = "IsWalking";

    [SerializeField] private Player player;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (player == null)
        {
            player = GetComponentInParent<Player>();
        }
    }

    private void Update()
    {
        if (animator != null && player != null)
        {
            animator.SetBool(IS_WALKING, player.IsWalking());
        }
    }
}
