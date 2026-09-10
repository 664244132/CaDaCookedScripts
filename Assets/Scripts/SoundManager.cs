using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance {  get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    [Header("Audio Pooling Optimization (Zero GC Allocations - Rule 5)")]
    [SerializeField] private int audioSourcePoolSize = 20;
    private AudioSource[] audioSourcePool;
    private Transform[] audioSourceTransforms;
    private int poolIndex = 0;
    private Camera mainCameraCached;

    private void Awake()
    {
        Instance = this;
        InitializeAudioSourcePool();
        mainCameraCached = Camera.main;
    }

    /// <summary>
    /// สร้าง AudioSource Pool ล่วงหน้า 20 ช่องสัญญาณ เพื่อนำกลับมาใช้ซ้ำ (Recycle) 
    /// ป้องกันการ Instantiate/Destroy GameObject เสียงทุกครั้งที่เดินหรือทำอาหาร (0 GC Allocations)
    /// </summary>
    private void InitializeAudioSourcePool()
    {
        GameObject poolContainer = new GameObject("--- AudioSourcePool ---");
        poolContainer.transform.SetParent(transform);

        audioSourcePool = new AudioSource[audioSourcePoolSize];
        audioSourceTransforms = new Transform[audioSourcePoolSize];

        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject soundObj = new GameObject($"PooledAudioSource_{i:D2}");
            soundObj.transform.SetParent(poolContainer.transform);

            AudioSource source = soundObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1.0f; // มิติเสียง 3D เหมือน PlayClipAtPoint
            source.minDistance = 2.0f;
            source.maxDistance = 25.0f;
            source.rolloffMode = AudioRolloffMode.Linear;

            audioSourcePool[i] = source;
            audioSourceTransforms[i] = soundObj.transform;
        }
    }

    private void Start()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
            DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
            DeliveryManager.Instance.OnOrderAngry += DeliveryManager_OnOrderAngry;
        }

        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;

        if (Player.Instance != null)
        {
            Player.Instance.OnPickedSomething += Player_OnPickedSomething;
        }

        BaseCounter.OnAnyObjectPlaceHere += BaseCounter_OnAnyObjectPlaceHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
        FireHazard.OnAnyFireStarted += FireHazard_OnAnyFireStarted;
        PotholeTrap.OnPlayerTripped += PotholeTrap_OnPlayerTripped;
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSuccess -= DeliveryManager_OnRecipeSuccess;
            DeliveryManager.Instance.OnRecipeFailed -= DeliveryManager_OnRecipeFailed;
            DeliveryManager.Instance.OnOrderAngry -= DeliveryManager_OnOrderAngry;
        }
        CuttingCounter.OnAnyCut -= CuttingCounter_OnAnyCut;
        if (Player.Instance != null)
        {
            Player.Instance.OnPickedSomething -= Player_OnPickedSomething;
        }
        BaseCounter.OnAnyObjectPlaceHere -= BaseCounter_OnAnyObjectPlaceHere;
        TrashCounter.OnAnyObjectTrashed -= TrashCounter_OnAnyObjectTrashed;
        FireHazard.OnAnyFireStarted -= FireHazard_OnAnyFireStarted;
        PotholeTrap.OnPlayerTripped -= PotholeTrap_OnPlayerTripped;
    }

    private void FireHazard_OnAnyFireStarted(object sender, System.EventArgs e)
    {
        if (audioClipRefsSO.warning != null && audioClipRefsSO.warning.Length > 0)
        {
            FireHazard fireHazard = sender as FireHazard;
            PlaySound(audioClipRefsSO.warning, fireHazard != null ? fireHazard.transform.position : Vector3.zero);
        }
    }

    private void PotholeTrap_OnPlayerTripped(object sender, System.EventArgs e)
    {
        if (Player.Instance != null)
        {
            PlaySound(audioClipRefsSO.objectDrop, Player.Instance.transform.position);
        }
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(audioClipRefsSO.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlaceHere(object sender, System.EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySound(audioClipRefsSO.objectDrop, baseCounter.transform.position);
    }

    private void Player_OnPickedSomething(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.objectPickup, Player.Instance.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipRefsSO.chop, cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(audioClipRefsSO.deliveryFail, deliveryCounter.transform.position);
    }

    private void DeliveryManager_OnOrderAngry(object sender, DeliveryManager.OnVIPOrderEventArgs e)
    {
        if (audioClipRefsSO != null && audioClipRefsSO.warning != null && audioClipRefsSO.warning.Length > 0)
        {
            if (mainCameraCached == null) mainCameraCached = Camera.main;
            Vector3 soundPos = mainCameraCached != null ? mainCameraCached.transform.position : Vector3.zero;
            PlaySound(audioClipRefsSO.warning, soundPos, 1.2f);
        }
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(audioClipRefsSO.deliverySuccess, deliveryCounter.transform.position);
    }

    /// <summary>
    /// เล่นเสียงสัญญาณเตือน (Warning Sound / Beep) เช่น เตือนเมื่อจานหมด
    /// </summary>
    public void PlayWarningSound(Vector3 position)
    {
        if (audioClipRefsSO != null && audioClipRefsSO.warning != null && audioClipRefsSO.warning.Length > 0)
        {
            PlaySound(audioClipRefsSO.warning, position, 1.2f);
        }
    }

    private void PlaySound(AudioClip[] audioClipArray , Vector3 position , float volume = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0,audioClipArray.Length)], position, volume);
    }

    /// <summary>
    /// เล่นเสียง SFX 3D แบบไร้การสร้างขยะ Heap (0 GC Allocation) โดยนำ AudioSource จาก Pool มาใช้ซ้ำ
    /// </summary>
    private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
    {
        if (audioClip == null) return;

        // Fallback กรณี Pool ยังไม่ถูกเตรียมไว้
        if (audioSourcePool == null || audioSourcePool.Length == 0)
        {
            AudioSource.PlayClipAtPoint(audioClip, position, volume);
            return;
        }

        // หมุนเวียนดึง AudioSource จาก Pool แบบ Round-Robin
        AudioSource audioSource = audioSourcePool[poolIndex];
        Transform sourceTransform = audioSourceTransforms[poolIndex];
        poolIndex = (poolIndex + 1) % audioSourcePoolSize;

        sourceTransform.position = position;
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1.0f;
        audioSource.Play();
    }

    public void PlayerFootstepsSound(Vector3 position , float volume)
    {
        PlaySound(audioClipRefsSO.footstep, position, volume);
    }
}
