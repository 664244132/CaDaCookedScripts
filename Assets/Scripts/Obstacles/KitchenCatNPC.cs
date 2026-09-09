using System;
using UnityEngine;

/// <summary>
/// แมวป่วนครัว (Neko Cat NPC)
/// เมื่อแอบขโมยวัตถุดิบได้ จะวิ่งสปีดเต็มฝีเท้า (Super Sprint 9.5f) หนีให้พ้นรัศมีกล้องของผู้เล่น
/// ก่อนจะแวบหายไป และกลับมาป่วนใหม่ทุกๆ 4.5 วินาที
/// </summary>
public class KitchenCatNPC : MonoBehaviour, IKitchenObjectParent
{
    public event EventHandler OnCatMeow;
    public event EventHandler OnCatScared;

    private enum State
    {
        Idle,
        WalkingToCounter,
        Fleeing,
        WaitingToRespawn
    }

    [Header("Cat Speed Settings")]
    [SerializeField] private float walkSpeed = 3.8f;
    [SerializeField] private float fleeSpeed = 9.5f;        // วิ่งเร็วมากๆ เพื่อหนีพ้นกล้อง
    [SerializeField] private float shooDistance = 1.7f;     // ระยะที่ผู้เล่นเข้าใกล้แล้วแมวตกใจ (1.7 เมตร)
    [SerializeField] private float respawnCooldown = 7.0f;  // กลับมาเกิดใหม่หลังขโมยของสำเร็จ 7 วินาที
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Pass-Through Settings")]
    [SerializeField] private bool canPassThroughObjects = true; // อนุญาตให้แมวเดินทะลุเคาน์เตอร์และวัตถุต่างๆ ได้ ไม่ติดขัด

    [Header("Patrol & Idle Pause Settings")]
    [SerializeField] private float arrivalThreshold = 0.25f;       // ระยะที่ถือว่าเดินถึงจุดหมาย
    [SerializeField] private float minIdlePauseDuration = 1.5f;     // เวลาหยุดยืนดมกลิ่นตรวจตราขั้นต่ำ
    [SerializeField] private float maxIdlePauseDuration = 3.5f;     // เวลาหยุดยืนดมกลิ่นตรวจตราสูงสุด
    [SerializeField] private float patrolRadius = 0.85f;           // รัศมีการเดินลาดตระเวนรอบจุดเกิดบนพื้นทางเดินโล่ง

    private State state;
    private BaseCounter targetCounter;
    private KitchenObject carriedKitchenObject;
    private Vector3 targetPosition;
    private Vector3 spawnPosition;
    private Vector3 currentPatrolWaypoint;
    private float idlePauseTimer;
    private bool isWaitingAtWaypoint;
    private float respawnTimer;
    private float fleeTimer;
    private BaseCounter[] cachedCounters;
    private float searchCooldownTimer;

    private void Awake()
    {
        spawnPosition = transform.position;

        // ปรับแต่ง Collider และ Rigidbody ให้เป็น Trigger ทันทีที่โหลดออบเจกต์
        EnsurePassThroughColliders();

        // ค้นหาหรือสร้าง HoldPoint สำหรับคาบวัตถุดิบ
        if (kitchenObjectHoldPoint == null)
        {
            Transform foundHold = transform.Find("HoldPoint");
            if (foundHold != null)
            {
                kitchenObjectHoldPoint = foundHold;
            }
            else
            {
                GameObject holdObj = new GameObject("HoldPoint");
                holdObj.transform.SetParent(transform, false);
                holdObj.transform.localPosition = new Vector3(0, 0.65f, 0.45f);
                kitchenObjectHoldPoint = holdObj.transform;
            }
        }
    }

    private void Start()
    {
        state = State.Idle;
        if (spawnPosition == Vector3.zero)
        {
            spawnPosition = transform.position;
        }

        EnsurePassThroughColliders();

        // สุ่ม Waypoint แรกและหยุดยืนดูลาดเลาก่อนเริ่มเดิน
        currentPatrolWaypoint = spawnPosition;
        isWaitingAtWaypoint = true;
        idlePauseTimer = UnityEngine.Random.Range(minIdlePauseDuration, maxIdlePauseDuration);

        // Cache รายการเคาน์เตอร์ทั้งหมดในฉากไว้ล่วงหน้า เพื่อป้องกันการ FindObjectsByType ทุกเฟรมใน Update (Rule 5 & 6)
        cachedCounters = FindObjectsByType<BaseCounter>(FindObjectsSortMode.None);
    }

    /// <summary>
    /// สุ่มเลือกจุดตรวจตราแห่งใหม่บนพื้นทางเดินโล่งรอบจุดเกิด พร้อมระบบ Counter Clearance ป้องกันไม่ให้จุดหมายตกไปอยู่ในเคาน์เตอร์
    /// </summary>
    private void PickNewPatrolWaypoint()
    {
        float randomOffsetX = UnityEngine.Random.Range(-patrolRadius, patrolRadius);
        float randomOffsetZ = UnityEngine.Random.Range(-patrolRadius, patrolRadius);
        Vector3 candidatePos = new Vector3(spawnPosition.x + randomOffsetX, spawnPosition.y, spawnPosition.z + randomOffsetZ);

        // ตรวจเช็ค Counter Clearance: หากจุดหมายอยู่ใกล้เคาน์เตอร์ใดๆ เกิน 1.15 เมตร ให้ปรับจุดหมายถอยห่างจากเคาน์เตอร์ออกมายังพื้นโล่ง
        if (cachedCounters != null)
        {
            foreach (BaseCounter counter in cachedCounters)
            {
                if (counter != null && Vector3.Distance(candidatePos, counter.transform.position) < 1.15f)
                {
                    Vector3 awayFromCounter = (candidatePos - counter.transform.position);
                    awayFromCounter.y = 0;
                    if (awayFromCounter.sqrMagnitude < 0.01f)
                    {
                        awayFromCounter = (spawnPosition - counter.transform.position);
                        awayFromCounter.y = 0;
                    }
                    candidatePos = counter.transform.position + awayFromCounter.normalized * 1.35f;
                    candidatePos.y = spawnPosition.y;
                    break;
                }
            }
        }

        currentPatrolWaypoint = candidatePos;
        isWaitingAtWaypoint = false;
    }

    /// <summary>
    /// ปรับแต่ง Collider ทั้งหมดบนตัวแมวให้เป็น Trigger (isTrigger = true) และติดตั้ง Rigidbody ให้เป็น Kinematic
    /// เพื่อให้แมวสามารถเดินทะลุผ่านเคาน์เตอร์ กำแพง และวัตถุต่างๆ ได้อย่างราบรื่น ไม่ติดขัด 100%
    /// </summary>
    private void EnsurePassThroughColliders()
    {
        if (!canPassThroughObjects) return;

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.isTrigger = true;
        }

        if (!TryGetComponent(out Rigidbody rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    /// <summary>
    /// กำหนดพิกัดจุดเกิดและจุดเดินลาดตระเวนริมขอบจอ
    /// </summary>
    public void SetSpawnPosition(Vector3 pos)
    {
        spawnPosition = pos;
        transform.position = pos;
        currentPatrolWaypoint = pos;
        isWaitingAtWaypoint = true;
        idlePauseTimer = 1.0f;
    }

    /// <summary>
    /// กำหนดระยะห่างที่ผู้เล่นเข้าใกล้แล้วแมวจะตกใจวิ่งหนี (ค่าเริ่มต้น 1.7 เมตร)
    /// </summary>
    public void SetShooDistance(float distance)
    {
        shooDistance = distance;
    }

    public float GetShooDistance()
    {
        return shooDistance;
    }

    /// <summary>
    /// กำหนดเวลาคูลดาวน์ก่อนที่แมวจะกลับมาเกิดใหม่หลังขโมยของสำเร็จ (ค่าเริ่มต้น 7 วินาที)
    /// </summary>
    public void SetRespawnCooldown(float cooldown)
    {
        respawnCooldown = cooldown;
    }

    public float GetRespawnCooldown()
    {
        return respawnCooldown;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                FindTargetCounter();
                break;

            case State.WalkingToCounter:
                if (targetCounter == null || !targetCounter.HasKitchenObject())
                {
                    state = State.Idle;
                    break;
                }

                MoveTowards(targetCounter.transform.position, walkSpeed);

                if (Vector3.Distance(transform.position, targetCounter.transform.position) < 1.3f)
                {
                    StealFromCounter();
                }
                break;

            case State.Fleeing:
                fleeTimer += Time.deltaTime;
                MoveTowards(targetPosition, fleeSpeed);

                // ตรวจสอบว่าวิ่งหนีพ้นกล้องแล้วหรือยัง (วิ่งเกิน 2.2 วินาที หรือวิ่งออกไปไกลกว่า 13 เมตร)
                float distanceFromCenter = Vector3.Distance(transform.position, Vector3.zero);
                if (fleeTimer >= 2.2f || distanceFromCenter >= 13f)
                {
                    // หนีพ้นกล้องสำเร็จ ตรวจสอบวัตถุดิบที่ขโมยมา
                    if (HasKitchenObject())
                    {
                        KitchenObject stolenObject = GetKitchenObject();
                        if (stolenObject is FireExtinguisher extinguisher)
                        {
                            // หากเป็นถังดับเพลิง ห้าม DestroySelf เด็ดขาด! ให้เริ่มคูลดาวน์ Respawn 15 วิ กลับจุดเดิม
                            ClearKitchenObject();
                            extinguisher.ScheduleRespawn(15.0f);
                            Debug.Log("😼 KitchenCat: Dropped FireExtinguisher off-camera. It will respawn in 15 seconds!");
                        }
                        else
                        {
                            stolenObject.DestroySelf();
                        }
                    }
                    
                    HideForRespawn();
                }
                break;

            case State.WaitingToRespawn:
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    RespawnCat();
                }
                break;
        }

        if (state != State.WaitingToRespawn && state != State.Fleeing)
        {
            CheckPlayerShoo();
        }
    }

    /// <summary>
    /// เคลื่อนที่เข้าหาจุดหมายอย่างนุ่มนวล โดยไม่ Overshoot หรือสั่นกระตุก
    /// คืนค่า true เมื่อเดินถึงจุดหมายแล้ว (ระยะห่างน้อยกว่า arrivalThreshold)
    /// </summary>
    private bool MoveTowards(Vector3 destination, float speed)
    {
        Vector3 toDest = destination - transform.position;
        toDest.y = 0;
        float dist = toDest.magnitude;

        if (dist <= arrivalThreshold)
        {
            // ถึงจุดหมายแล้ว หยุดเดินสนิท
            return true;
        }

        Vector3 moveDir = toDest / dist;
        float step = Mathf.Min(speed * Time.deltaTime, dist);

        Vector3 nextPos = transform.position + moveDir * step;
        if (canPassThroughObjects)
        {
            // ล็อคระดับความสูงแกน Y ให้ตรงกับระดับพื้นเดิมเสมอ ป้องกันการลอยขึ้นไปบนหลังเคาน์เตอร์ขณะเดินทะลุ
            nextPos.y = spawnPosition.y;
        }

        transform.position = nextPos;
        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, moveDir, 10f * Time.deltaTime, 0f);
        }

        return false;
    }

    private void FindTargetCounter()
    {
        if (cachedCounters == null || cachedCounters.Length == 0)
        {
            cachedCounters = FindObjectsByType<BaseCounter>(FindObjectsSortMode.None);
        }

        // ค้นหาเป้าหมายเป็นช่วงๆ ทุก 0.25 วินาที เพื่อประหยัด CPU และไม่สร้าง GC ใน Update (Rule 5 & 6)
        searchCooldownTimer -= Time.deltaTime;
        if (searchCooldownTimer <= 0f)
        {
            searchCooldownTimer = 0.25f;
            foreach (BaseCounter counter in cachedCounters)
            {
                if (counter != null && counter.HasKitchenObject())
                {
                    KitchenObject targetObj = counter.GetKitchenObject();
                    // ตาม Q3 ตัวเลือก A: แมวไม่แตะต้องจานอาหาร (Blacklist Plates) ขโมยเฉพาะวัตถุดิบหรือถังดับเพลิง
                    if (targetObj is PlateKitchenObject || targetObj is DirtyPlateKitchenObject)
                    {
                        continue;
                    }

                    targetCounter = counter;
                    state = State.WalkingToCounter;
                    OnCatMeow?.Invoke(this, EventArgs.Empty);
                    Debug.Log($"🐱 KitchenCat: Targeted food on [{counter.name}]! Sneaking in...");
                    return;
                }
            }
        }

        // หากยังไม่มีของบนเคาน์เตอร์ ให้เดินตรวจตราตามจุด Waypoint หรือยืนดมกลิ่นพักตรวจตราอย่างเป็นธรรมชาติ
        if (isWaitingAtWaypoint)
        {
            idlePauseTimer -= Time.deltaTime;

            // ขณะหยุดยืนพัก ให้หันหน้ามองเข้าหาห้องครัวอย่างเป็นธรรมชาติ (เสมือนคอยสอดส่องว่ามีอะไรให้ขโมยไหม)
            Vector3 kitchenDirection = (Vector3.zero - transform.position);
            kitchenDirection.y = 0;
            if (kitchenDirection != Vector3.zero)
            {
                transform.forward = Vector3.RotateTowards(transform.forward, kitchenDirection.normalized, 2f * Time.deltaTime, 0f);
            }

            if (idlePauseTimer <= 0f)
            {
                PickNewPatrolWaypoint();
            }
        }
        else
        {
            // ค่อยๆ เดินไปยัง Waypoint ที่สุ่มไว้
            bool arrived = MoveTowards(currentPatrolWaypoint, walkSpeed * 0.6f);
            if (arrived)
            {
                isWaitingAtWaypoint = true;
                idlePauseTimer = UnityEngine.Random.Range(minIdlePauseDuration, maxIdlePauseDuration);
            }
        }
    }

    private void StealFromCounter()
    {
        if (targetCounter != null && targetCounter.HasKitchenObject() && !HasKitchenObject())
        {
            KitchenObject kitchenObject = targetCounter.GetKitchenObject();
            if (kitchenObject is PlateKitchenObject || kitchenObject is DirtyPlateKitchenObject)
            {
                // หากเป็นจานอาหาร ไม่ขโมย
                state = State.Idle;
                return;
            }

            kitchenObject.SetKitchenObjectParent(this);

            StartSprintingAway();
            Debug.Log("😼 KitchenCat: STOLE INGREDIENT! SPRINTING AWAY FAST!");
        }
        else
        {
            state = State.Idle;
        }
    }

    private void StartSprintingAway()
    {
        state = State.Fleeing;
        fleeTimer = 0f;

        // คำนวณทิศทางวิ่งหนีออกจากครัว / ห่างจากผู้เล่นแบบความเร็วสูง
        Vector3 escapeDir = transform.position - (Player.Instance != null ? Player.Instance.transform.position : Vector3.zero);
        escapeDir.y = 0;
        if (escapeDir.sqrMagnitude < 0.1f)
        {
            escapeDir = transform.forward;
        }
        escapeDir = escapeDir.normalized;

        // จุดหมายการวิ่งหนีออกนอกฉาก (ไกล 20 เมตร)
        targetPosition = transform.position + escapeDir * 20f;
    }

    private void CheckPlayerShoo()
    {
        if (Player.Instance == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        if (distanceToPlayer <= shooDistance)
        {
            ScareCat();
        }
    }

    public void ScareCat()
    {
        if (state == State.Fleeing || state == State.WaitingToRespawn) return;

        OnCatScared?.Invoke(this, EventArgs.Empty);
        Debug.Log("🙀 KitchenCat: Scared by Player! Dropping food and sprinting away!");

        if (HasKitchenObject())
        {
            KitchenObject droppedObj = GetKitchenObject();
            if (droppedObj is FireExtinguisher extinguisher)
            {
                // หากเป็นถังดับเพลิง ให้หล่นลงพื้น ณ ตำแหน่งแมวทันที ผู้เล่นจะได้หยิบกลับไปใช้ได้
                ClearKitchenObject();
                extinguisher.DropToFloor(transform.position);
                Debug.Log("🙀 KitchenCat: Dropped FireExtinguisher onto floor while scared!");
            }
            else
            {
                droppedObj.DestroySelf();
            }
        }

        StartSprintingAway();
    }

    private void HideForRespawn()
    {
        state = State.WaitingToRespawn;
        respawnTimer = respawnCooldown;
        transform.position = spawnPosition + new Vector3(0, -100f, 0); // ซ่อนตัวใต้ฉากชั่วคราว
        Debug.Log("💨 KitchenCat: Escaped camera view! Waiting to respawn...");
    }

    private void RespawnCat()
    {
        transform.position = spawnPosition;
        EnsurePassThroughColliders();
        state = State.Idle;
        fleeTimer = 0f;
        isWaitingAtWaypoint = true;
        idlePauseTimer = UnityEngine.Random.Range(minIdlePauseDuration, maxIdlePauseDuration);
        PickNewPatrolWaypoint();
        Debug.Log("🐱 KitchenCat: Respawned back at the kitchen entrance!");
    }

    // --- IKitchenObjectParent Implementation ---
    public Transform GetKitchenObjectFollowTranform()
    {
        return kitchenObjectHoldPoint != null ? kitchenObjectHoldPoint : transform;
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return GetKitchenObjectFollowTranform();
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.carriedKitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject()
    {
        return carriedKitchenObject;
    }

    public void ClearKitchenObject()
    {
        this.carriedKitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return carriedKitchenObject != null;
    }
}
