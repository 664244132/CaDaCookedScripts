using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ถังดับเพลิง (Fire Extinguisher)
/// 1. ดับไฟที่เกิดบนเคาน์เตอร์ได้จริงและรวดเร็ว (Extinguishes Fire Hazards effectively)
/// 2. เมื่อวางลงพื้น สามารถกด [E] เพื่อหยิบกลับขึ้นมาใช้ใหม่ได้เสมอ
/// 3. ละอองขาว (White Mist Spray) หันและพุ่งไปทางด้านหน้าของผู้เล่นเสมอ
/// 4. ป้ายบอกปุ่มกดเด่นชัด (High-Contrast World Space Canvas Banner) มองเห็นง่ายทั้งขณะวางบนพื้นและขณะถือ
/// </summary>
public class FireExtinguisher : KitchenObject
{
    [Header("Extinguisher Settings")]
    [SerializeField] private float extinguishRate = 180f; // อัตราการดับไฟต่อวินาที (ดับไฟสนิทได้ใน ~0.55 วินาที)
    [SerializeField] private float extinguishRange = 3.8f; // ระยะการฉีด
    [SerializeField] private ParticleSystem sprayParticleSystem;
    [SerializeField] private AudioSource sprayAudioSource;

    private bool isSpraying;
    private float sprayCooldownTimer;

    // UI ป้ายบอกปุ่มกดแบบ World Space Canvas
    private GameObject promptCanvasObject;
    private RectTransform promptRectTransform;
    private Image promptBackground;
    private TextMeshProUGUI promptText;
    private Camera targetCamera;

    // ระบบคืนชีพถังดับเพลิงเมื่อถูกแมวขโมย (Anti Soft-Lock Respawn System)
    private Vector3 initialSpawnPosition;
    private bool isRespawning;
    private float respawnTimer;
    private Player cachedPlayer;

    private static Shader cachedSafeShader;

    private Player GetPlayer()
    {
        if (cachedPlayer == null)
        {
            cachedPlayer = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();
        }
        return cachedPlayer;
    }

    private void Awake()
    {
        EnsureVisuals();
        EnsureCollider();
        InitializeWhiteMistSpray();
        InitializePromptUI();
    }

    private void Start()
    {
        EnsureVisuals();
        initialSpawnPosition = transform.position;
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }

        if (sprayParticleSystem != null)
        {
            sprayParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    /// <summary>
    /// รับประกันว่าโมเดล 3D ของถังดับเพลิง (ตัวถังสีแดง + หัวฉีดทรงสี่เหลี่ยมสีเทา + ท่อปลายหัวฉีดสีดำ)
    /// ถูกสร้างและลง Material ครบถ้วน ไม่หายไปเมื่อ Build เกม Standalone
    /// </summary>
    public void EnsureVisuals()
    {
        // 1. ตัวถังทรงกระบอกสีแดง (Red Cylinder Body)
        MeshRenderer bodyRenderer = GetComponent<MeshRenderer>();
        if (bodyRenderer != null)
        {
            bodyRenderer.material = GetSafeMaterial(new Color(0.9f, 0.05f, 0.05f), 0.5f, 0.6f);
        }

        // 2. หัวฉีดทรงสี่เหลี่ยมสีเทา (Grey Top Nozzle)
        Transform nozzleTransform = transform.Find("TopNozzle");
        if (nozzleTransform == null)
        {
            // ตรวจสอบชื่อเดิมถ้ามี
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Nozzle") || child.name.Contains("Cube"))
                {
                    nozzleTransform = child;
                    nozzleTransform.name = "TopNozzle";
                    break;
                }
            }
        }

        if (nozzleTransform == null)
        {
            GameObject nozzleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nozzleObj.name = "TopNozzle";
            nozzleObj.transform.SetParent(transform, false);
            nozzleObj.transform.localPosition = new Vector3(0, 1.05f, 0.2f);
            nozzleObj.transform.localScale = new Vector3(0.32f, 0.28f, 0.55f);
            if (nozzleObj.TryGetComponent(out Collider c)) Destroy(c);
            nozzleTransform = nozzleObj.transform;
        }

        MeshRenderer nozzleRenderer = nozzleTransform.GetComponent<MeshRenderer>();
        if (nozzleRenderer != null)
        {
            nozzleRenderer.material = GetSafeMaterial(new Color(0.68f, 0.70f, 0.74f), 0.85f, 0.85f);
        }

        // 3. ท่อปลายหัวฉีดสีดำ (Black Nozzle Tip)
        Transform tipTransform = transform.Find("NozzleTip");
        if (tipTransform == null)
        {
            GameObject tipObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tipObj.name = "NozzleTip";
            tipObj.transform.SetParent(transform, false);
            tipObj.transform.localPosition = new Vector3(0, 1.05f, 0.5f);
            tipObj.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            tipObj.transform.localScale = new Vector3(0.12f, 0.15f, 0.12f);
            if (tipObj.TryGetComponent(out Collider tc)) Destroy(tc);
            tipTransform = tipObj.transform;
        }

        MeshRenderer tipRenderer = tipTransform.GetComponent<MeshRenderer>();
        if (tipRenderer != null)
        {
            tipRenderer.material = GetSafeMaterial(new Color(0.12f, 0.12f, 0.15f), 0.5f, 0.5f);
        }
    }

    /// <summary>
    /// สร้าง Material ที่ปลอดภัยสำหรับทั้ง Editor และ Standalone Build โดยดึง Shader จาก Scene Object หาก Shader.Find คืนค่า null
    /// </summary>
    public static Material GetSafeMaterial(Color color, float metallic = 0.5f, float smoothness = 0.5f)
    {
        if (cachedSafeShader == null)
        {
            // 1. ค้นหา URP Lit หรือ Standard
            cachedSafeShader = Shader.Find("Universal Render Pipeline/Lit");
            if (cachedSafeShader == null) cachedSafeShader = Shader.Find("Standard");

            // 2. หากรันใน Build แล้ว Shader.Find คืนค่า null ให้ยืม Shader จาก MeshRenderer ที่มีอยู่ใน Scene
            if (cachedSafeShader == null)
            {
                MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
                foreach (var r in renderers)
                {
                    if (r.sharedMaterial != null && r.sharedMaterial.shader != null)
                    {
                        cachedSafeShader = r.sharedMaterial.shader;
                        break;
                    }
                }
            }
        }

        Material mat = new Material(cachedSafeShader != null ? cachedSafeShader : Shader.Find("Sprites/Default"));
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }

    /// <summary>
    /// รับประกันว่ามี Collider สำหรับการตรวจจับและหยิบขึ้นมา
    /// </summary>
    public void EnsureCollider()
    {
        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
        }
        else
        {
            CapsuleCollider newCol = gameObject.AddComponent<CapsuleCollider>();
            newCol.center = new Vector3(0, 0.35f, 0);
            newCol.radius = 0.35f;
            newCol.height = 0.8f;
        }
    }

    /// <summary>
    /// สร้างระบบ Particle ละอองสีขาว (White Mist Foam Spray)
    /// </summary>
    private void InitializeWhiteMistSpray()
    {
        if (sprayParticleSystem != null) return;

        Transform existingSpray = transform.Find("WhiteMistSpray");
        if (existingSpray != null)
        {
            sprayParticleSystem = existingSpray.GetComponent<ParticleSystem>();
            return;
        }

        GameObject sprayObj = new GameObject("WhiteMistSpray");
        sprayObj.transform.SetParent(transform, false);
        sprayObj.transform.localPosition = new Vector3(0, 0.45f, 0.3f);
        sprayObj.transform.localRotation = Quaternion.identity;

        sprayParticleSystem = sprayObj.AddComponent<ParticleSystem>();
        sprayParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = sprayParticleSystem.main;
        main.playOnAwake = false;
        main.duration = 1.0f;
        main.loop = true;
        main.startLifetime = 0.5f;
        main.startSpeed = 8.5f;
        main.startSize = 0.4f;
        main.startColor = new Color(1f, 1f, 1f, 0.85f); // ละอองสีขาวชัดเจน
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = sprayParticleSystem.emission;
        emission.rateOverTime = 100f;

        var shape = sprayParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;

        var sizeOverLifetime = sprayParticleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.4f);
        curve.AddKey(1.0f, 1.6f); // ขยายตัวออกเมื่อพุ่งไปด้านหน้า
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

        var colorOverLifetime = sprayParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = grad;

        ParticleSystemRenderer renderer = sprayObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default"));
        renderer.material.color = Color.white;

        sprayParticleSystem.Stop();
    }

    /// <summary>
    /// สร้างป้ายบอกปุ่มกดแบบ World Space Canvas Banner สีสันเด่นชัด ขนาดใหญ่ ลอยเหนือถังดับเพลิง
    /// </summary>
    private void InitializePromptUI()
    {
        if (promptCanvasObject != null) return;

        promptCanvasObject = new GameObject("ExtinguisherPromptCanvas");
        promptCanvasObject.transform.SetParent(transform, false);
        promptCanvasObject.transform.localPosition = new Vector3(0, 1.4f, 0);
        promptCanvasObject.transform.localScale = Vector3.one * 0.016f; // ขยายขนาดสเกลใหญ่ขึ้น มองเห็นได้ชัดเจนจากมุมกล้องสูง

        Canvas canvas = promptCanvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;

        promptRectTransform = promptCanvasObject.GetComponent<RectTransform>();
        promptRectTransform.sizeDelta = new Vector2(320f, 100f);

        // พื้นหลังป้ายสีเข้มตัดขอบ
        promptBackground = promptCanvasObject.AddComponent<Image>();
        promptBackground.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

        // กล่องข้อความ
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptCanvasObject.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.fontSize = 30;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        promptText.fontStyle = FontStyles.Bold;
        promptText.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private void Update()
    {
        // หากอยู่ในช่วงคูลดาวน์รอ Respawn หลังถูกแมวขโมย
        if (isRespawning)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                RespawnAtInitialPosition();
            }
            return;
        }

        UpdatePromptBillboard();

        Player player = GetPlayer();
        bool isHeldByPlayer = (player != null && GetKitchenObjectParent() as Player == player);

        // ปรับทิศทางละอองขาวให้ตรงกับทิศที่ผู้เล่นหันหน้าตลอดเวลาแบบ Realtime
        if (isSpraying && isHeldByPlayer && sprayParticleSystem != null)
        {
            sprayParticleSystem.transform.position = player.transform.position + player.transform.forward * 0.45f + Vector3.up * 0.4f;
            sprayParticleSystem.transform.forward = player.transform.forward;
        }

        if (sprayCooldownTimer > 0f)
        {
            sprayCooldownTimer -= Time.deltaTime;
            if (sprayCooldownTimer <= 0f && isSpraying)
            {
                StopSpraying();
            }
        }

        // หากไม่ได้ถูกถือ ให้หยุดฉีดทันที
        if (isSpraying && !isHeldByPlayer)
        {
            StopSpraying();
        }
    }

    /// <summary>
    /// ควบคุมการแสดงผล ตำแหน่งลอย และหันหน้าเข้าหากล้องของป้ายข้อความ
    /// </summary>
    private void UpdatePromptBillboard()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        if (promptCanvasObject == null || promptText == null) return;

        // หันหน้าตามกล้องเสมอ
        promptCanvasObject.transform.forward = targetCamera.transform.forward;

        Player player = GetPlayer();
        if (player == null) return;

        bool isHeldByPlayer = (GetKitchenObjectParent() as Player == player);

        if (isHeldByPlayer)
        {
            // กำลังถืออยู่ -> แสดงป้ายแนะนำการใช้งานขนาดใหญ่พิเศษ ลอยเหนือศีรษะผู้เล่น
            promptCanvasObject.SetActive(true);
            promptRectTransform.sizeDelta = new Vector2(360f, 110f);
            promptBackground.color = new Color(0.04f, 0.04f, 0.07f, 0.95f);
            promptText.text = UITheme.FormatExtinguisherHoldPrompt();
            
            // ลอยอยู่เหนือศีรษะผู้เล่นอย่างชัดเจน
            promptCanvasObject.transform.position = player.transform.position + Vector3.up * 2.35f;
        }
        else
        {
            // วางอยู่บนพื้น -> แสดงป้ายสีทองลอยพร้อมแอนิเมชันโยกขึ้นลง (Bobbing) เหนือถังดับเพลิง
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= 4.5f)
            {
                promptCanvasObject.SetActive(true);
                promptRectTransform.sizeDelta = new Vector2(310f, 75f);
                promptBackground.color = new Color(0.12f, 0.10f, 0.02f, 0.96f);
                promptText.text = UITheme.FormatExtinguisherPickupPrompt();
                
                // ลอยและโยกขึ้นลงเล็กน้อยเหนือถังดับเพลิง
                float bobOffset = Mathf.Sin(Time.time * 4f) * 0.1f;
                promptCanvasObject.transform.position = transform.position + Vector3.up * (1.4f + bobOffset);
            }
            else
            {
                promptCanvasObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// พ่นละอองดับไฟ 1 จังหวะ (เมื่อกด F)
    /// </summary>
    public void TriggerSprayPulse(Vector3 forwardDirection)
    {
        StartSpraying(forwardDirection);
        sprayCooldownTimer = 0.4f;
    }

    /// <summary>
    /// เริ่มพ่นละอองขาวดับเพลิง และค้นหาดับไฟเคาน์เตอร์ด้านหน้า
    /// </summary>
    public void StartSpraying(Vector3 forwardDirection)
    {
        isSpraying = true;

        Player player = GetPlayer();
        Vector3 originPos = (player != null) ? player.transform.position : transform.position;
        Vector3 forward = (player != null) ? player.transform.forward : forwardDirection;

        if (sprayParticleSystem != null)
        {
            sprayParticleSystem.transform.position = originPos + forward * 0.45f + Vector3.up * 0.4f;
            sprayParticleSystem.transform.forward = forward;
            if (!sprayParticleSystem.isPlaying)
            {
                sprayParticleSystem.Play();
            }
        }

        if (sprayAudioSource != null && !sprayAudioSource.isPlaying)
        {
            sprayAudioSource.Play();
        }

        // ค้นหาและดับไฟที่เกิดขึ้นบนเคาน์เตอร์ด้านหน้าผู้เล่นอย่างแม่นยำและครอบคลุม
        FireHazard[] allHazards = FindObjectsByType<FireHazard>(FindObjectsSortMode.None);
        foreach (FireHazard hazard in allHazards)
        {
            if (hazard.IsBurning())
            {
                Vector3 toHazard = hazard.transform.position - originPos;
                toHazard.y = 0;
                float dist = toHazard.magnitude;

                // ตรวจสอบระยะและมุมด้านหน้า (ครอบคลุมระยะ 4.5 เมตร และมุมมอง 85 องศา หรืออยู่ในระยะประชิด 2.0 เมตร)
                if (dist <= extinguishRange + 1.2f)
                {
                    float angle = Vector3.Angle(forward, toHazard.normalized);
                    if (angle <= 85f || dist <= 2.0f)
                    {
                        hazard.Extinguish(extinguishRate * Time.deltaTime);
                    }
                }
            }
        }
    }

    /// <summary>
    /// หยุดพ่นละอองขาวทันที
    /// </summary>
    public void StopSpraying()
    {
        isSpraying = false;
        sprayCooldownTimer = 0f;

        if (sprayParticleSystem != null && sprayParticleSystem.isPlaying)
        {
            sprayParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (sprayAudioSource != null && sprayAudioSource.isPlaying)
        {
            sprayAudioSource.Stop();
        }
    }

    /// <summary>
    /// วาง/ทิ้งถังดับเพลิงลงบนพื้น (เมื่อกด E ขณะถือ)
    /// </summary>
    public void DropToFloor(Vector3 dropPosition)
    {
        if (GetKitchenObjectParent() != null)
        {
            GetKitchenObjectParent().ClearKitchenObject();
        }

        SetKitchenObjectParent(null);
        transform.SetParent(null);
        transform.position = GameplayEventsBootstrap.ClampToPlayableBounds(dropPosition, 0.35f);
        transform.rotation = Quaternion.identity;
        EnsureCollider();
        StopSpraying();

        Debug.Log("🧯 FireExtinguisher: Dropped to floor safely! Can be picked up with [E] again.");
    }

    public bool IsSpraying() => isSpraying;

    /// <summary>
    /// สั่งให้ถังดับเพลิงซ่อนตัวและเริ่มนับคูลดาวน์ 15 วินาทีเพื่อ Respawn กลับมายังจุดวางเดิม (ป้องกัน Soft-lock เมื่อแมวขโมย)
    /// </summary>
    public void ScheduleRespawn(float delay = 15.0f)
    {
        isRespawning = true;
        respawnTimer = delay;

        if (GetKitchenObjectParent() != null)
        {
            GetKitchenObjectParent().ClearKitchenObject();
        }
        SetKitchenObjectParent(null);
        transform.SetParent(null);
        StopSpraying();

        // ปิดการแสดงผลและ Collider ชั่วคราวเพื่อให้ Update() ยังคงนับเวลาถอยหลังได้
        SetVisibility(false);
        Debug.Log($"🧯 FireExtinguisher: Cat stole extinguisher! Respawing at {initialSpawnPosition} in {delay:F1}s...");
    }

    private void RespawnAtInitialPosition()
    {
        isRespawning = false;
        transform.position = initialSpawnPosition;
        transform.rotation = Quaternion.identity;
        EnsureCollider();
        SetVisibility(true);
        Debug.Log($"🧯 FireExtinguisher: RESPAWNED at initial position ({initialSpawnPosition}) successfully!");
    }

    private void SetVisibility(bool visible)
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in renderers)
        {
            r.enabled = visible;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
        {
            c.enabled = visible;
        }

        if (promptCanvasObject != null)
        {
            promptCanvasObject.SetActive(visible);
        }
    }
}

