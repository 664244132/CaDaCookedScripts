using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    
    private Player player;
    private float footstepTimer;
    private float footstepTimerMax = .1f;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        footstepTimer -= Time.deltaTime;
        if (footstepTimer < 0f)
        {
            footstepTimer = footstepTimerMax;


            if (player != null && player.IsWalking())
            {
                float volume = 1f;
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayerFootstepsSound(player.transform.position, volume);
                }
            }
        }
    }
}
