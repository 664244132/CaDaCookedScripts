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
    [SerializeField] private float shooDistance = 2.0f;     // ระยะที่ผู้เล่นเข้าใกล้แล้วแมวตกใจ
    [SerializeField] private float respawnCooldown = 4.5f;  // กลับมาใหม่ทุกๆ 4.5 วินาที
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private Transform exitPoint;

    private State state;
    private BaseCounter targetCounter;
    private KitchenObject carriedKitchenObject;
    private Vector3 targetPosition;
    private Vector3 spawnPosition;
    private float respawnTimer;
    private float fleeTimer;
    private BaseCounter[] cachedCounters;
    private float searchCooldownTimer;

    private void Awake()
    {
        spawnPosition = transform.position;

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
        // Cache รายการเคาน์เตอร์ทั้งหมดในฉากไว้ล่วงหน้า เพื่อป้องกันการ FindObjectsByType ทุกเฟรมใน Update (Rule 5 & 6)
        cachedCounters = FindObjectsByType<BaseCounter>(FindObjectsSortMode.None);
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

    private void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 moveDir = (destination - transform.position);
        moveDir.y = 0;
        moveDir = moveDir.normalized;

        transform.position += moveDir * speed * Time.deltaTime;
        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * 15f);
        }
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

        // หากยังไม่มีของบนเคาน์เตอร์ ให้เดินวนเวียนแถวทางเข้า
        Vector3 wanderPos = spawnPosition + new Vector3(Mathf.Sin(Time.time * 2f) * 2f, 0, Mathf.Cos(Time.time * 2f) * 2f);
        MoveTowards(wanderPos, walkSpeed * 0.6f);
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
        state = State.Idle;
        fleeTimer = 0f;
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
