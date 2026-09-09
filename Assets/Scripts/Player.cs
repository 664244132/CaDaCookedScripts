using System;
using UnityEngine;

/// <summary>
/// ตัวควบคุมตัวละครเชฟหลัก (Player Controller)
/// จัดการการเคลื่อนที่ 3 มิติ, การหมุนตัว, การตรวจจับเคาน์เตอร์ตรงหน้า, การหยิบจับวัตถุดิบ
/// และการตอบสนองต่ออุปสรรคในฉาก (พื้นลื่นคราบน้ำมัน, หลุมดักสะดุด, การฉีดถังดับเพลิง)
/// </summary>
public class Player : MonoBehaviour, IKitchenObjectParent
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [Header("Movement & Interaction Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask = ~0; // ปรับแต่ง Layer การตรวจจับการชนตามกฎ Rule 10
    [SerializeField] private Transform kitchenObjectHoldPoint;

    [Header("Dash Mechanic Settings")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.16f;
    [SerializeField] private float dashCooldownDuration = 1.0f;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    // --- Dash Variables ---
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;
    private ParticleSystem dashParticleSystem;

    // --- Obstacle & Effect Variables ---
    private bool isSlipping;
    private float slipTimer;
    private float slipCooldownTimer;
    private float slipSpeedMultiplier = 1f;
    private float spinSpeed;
    private Vector3 slipMomentum;

    private float slowTimer;
    private float slowMultiplier = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one player instance!");
        }
        Instance = this;
    }

    private void Start()
    {
        if (gameInput == null)
        {
            gameInput = GameInput.Instance;
        }

        if (gameInput != null)
        {
            gameInput.OnInteractAction += GameInput_OnInteractAction;
            gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
            gameInput.OnDashAction += GameInput_OnDashAction;
        }

        CreateDashDustEffect();
    }

    private void OnDestroy()
    {
        if (gameInput != null)
        {
            gameInput.OnInteractAction -= GameInput_OnInteractAction;
            gameInput.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
            gameInput.OnDashAction -= GameInput_OnDashAction;
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        // 1. หากผู้เล่นกำลังถือถังดับเพลิงอยู่ และกด E -> ทำการทิ้งถังดับเพลิงลงพื้น (หรือวางบนเคาน์เตอร์ถ้าว่าง)
        if (HasKitchenObject() && GetKitchenObject() is FireExtinguisher heldExtinguisher)
        {
            if (selectedCounter != null && !selectedCounter.HasKitchenObject())
            {
                selectedCounter.Interact(this);
            }
            else
            {
                Vector3 dropPos = transform.position + transform.forward * 0.85f;
                heldExtinguisher.DropToFloor(dropPos);
            }
            return;
        }

        // 2. ปฏิสัมพันธ์กับเคาน์เตอร์ที่เลือก
        if (selectedCounter != null)
        {
            if (selectedCounter.TryGetComponent(out FireHazard fireHazard) && (fireHazard.IsBurning() || fireHazard.IsLockedOut()))
            {
                Debug.Log("🚫 Counter is Burning or Locked Out! Cannot interact!");
                return;
            }

            selectedCounter.Interact(this);
            return;
        }

        // 3. ตรวจจับการหยิบถังดับเพลิงที่วางอยู่บนพื้นด้วยปุ่ม E
        if (!HasKitchenObject())
        {
            FireExtinguisher[] allExts = FindObjectsByType<FireExtinguisher>(FindObjectsSortMode.None);
            FireExtinguisher closestExt = null;
            float closestDist = 2.5f;

            foreach (FireExtinguisher ext in allExts)
            {
                if (ext.GetKitchenObjectParent() == null)
                {
                    float dist = Vector3.Distance(transform.position, ext.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestExt = ext;
                    }
                }
            }

            if (closestExt != null)
            {
                closestExt.SetKitchenObjectParent(this);
                Debug.Log("🧯 Player: Picked up FireExtinguisher with [E]!");
                return;
            }
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        // หากผู้เล่นถือถังดับเพลิงอยู่ และกด F -> พ่นละอองขาวดับเพลิง
        if (HasKitchenObject() && GetKitchenObject() is FireExtinguisher fireExtinguisher)
        {
            fireExtinguisher.TriggerSprayPulse(transform.forward);
            return;
        }

        if (selectedCounter != null)
        {
            if (selectedCounter.TryGetComponent(out FireHazard fireHazard) && (fireHazard.IsBurning() || fireHazard.IsLockedOut()))
            {
                return;
            }

            selectedCounter.InteractAlternate(this);
        }
    }

    /// <summary>
    /// ทำงานเมื่อผู้เล่นกดปุ่ม Dash (Spacebar บน Keyboard หรือ South/Shoulder Button บน Gamepad)
    /// </summary>
    private void GameInput_OnDashAction(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance == null || !KitchenGameManager.Instance.IsGamePlaying()) return;
        if (dashCooldownTimer > 0f || isDashing) return;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // หากไม่ได้กดปุ่มทิศทาง ให้ Dash ไปข้างหน้าตามที่ตัวละครหันหน้าอยู่
        if (moveDir == Vector3.zero)
        {
            dashDirection = transform.forward;
        }
        else
        {
            dashDirection = moveDir;
            transform.forward = moveDir;
        }

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldownDuration;

        // หากกด Dash บนพื้นลื่น SlipperyFloor (หรือขณะกำลังลื่น) ให้เพิ่มแรงส่งสไลด์พุ่งตัวไกลขึ้น!
        if (isSlipping || slipTimer > 0f)
        {
            slipMomentum += dashDirection * 8f;
            slipTimer += 0.25f;
            Debug.Log("⚡ SUPER SLIDE BOOST! Player dashed on slippery floor!");
        }

        if (dashParticleSystem != null)
        {
            dashParticleSystem.Play();
        }
    }

    private void Update()
    {
        UpdateObstacleTimers();
        HandleMovement();
        HandleInteractions();
        HandleExtinguisherContinuousSpray();
    }

    /// <summary>
    /// ตรวจจับการกดค้างปุ่ม Alternate เพื่อพ่นละอองดับเพลิงต่อเนื่อง (ผ่าน New Input System รองรับ Keyboard & Gamepad)
    /// </summary>
    private void HandleExtinguisherContinuousSpray()
    {
        if (HasKitchenObject() && GetKitchenObject() is FireExtinguisher fireExt)
        {
            if (gameInput != null && gameInput.IsInteractAlternatePressed())
            {
                fireExt.StartSpraying(transform.forward);
            }
            else
            {
                fireExt.StopSpraying();
            }
        }
    }

    private void UpdateObstacleTimers()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (slipTimer > 0f)
        {
            slipTimer -= Time.deltaTime;
            if (slipTimer <= 0f)
            {
                isSlipping = false;
                slipSpeedMultiplier = 1f;
                slipMomentum = Vector3.zero;
            }
        }

        if (slipCooldownTimer > 0f)
        {
            slipCooldownTimer -= Time.deltaTime;
        }

        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                slowMultiplier = 1f;
            }
        }
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        // 1. หากอยู่ในสถานะ Dash พุ่งตัว
        if (isDashing && dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            float dashMoveDistance = dashSpeed * Time.deltaTime;
            float playerR = 0.5f;
            float playerH = 2.0f;

            bool canDash = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerH, playerR, dashDirection, dashMoveDistance, collisionsLayerMask, QueryTriggerInteraction.Ignore);
            if (canDash)
            {
                transform.position += dashDirection * dashMoveDistance;
            }
            else
            {
                // ชนเคาน์เตอร์หรือสิ่งกีดขวาง ให้หยุดแดชทันที
                isDashing = false;
                dashTimer = 0f;
            }

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }

            isWalking = true;
            return;
        }

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // คำนวณความเร็วที่ได้รับผลกระทบจากอุปสรรค
        float currentSpeed = moveSpeed * slowMultiplier;

        // หากกำลังลื่นไถล
        if (isSlipping || slipTimer > 0f)
        {
            currentSpeed = moveSpeed * slipSpeedMultiplier;

            if (moveDir != Vector3.zero)
            {
                slipMomentum = Vector3.Lerp(slipMomentum, moveDir, Time.deltaTime * 2.5f);
            }
            else if (slipMomentum == Vector3.zero)
            {
                slipMomentum = transform.forward;
            }

            moveDir = slipMomentum.normalized;

            // หมุนตัวเชฟตอนลื่นไถล
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        float moveDistance = currentSpeed * Time.deltaTime;
        float playerRadius = 0.5f;
        float playerHeight = 2.0f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance, collisionsLayerMask, QueryTriggerInteraction.Ignore);

        if (!canMove && moveDir != Vector3.zero)
        {
            // ตรวจจับการเดินสไลด์ตามแนวแกน X
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance, collisionsLayerMask, QueryTriggerInteraction.Ignore);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                // ตรวจจับการเดินสไลด์ตามแนวแกน Z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance, collisionsLayerMask, QueryTriggerInteraction.Ignore);
                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove && moveDir != Vector3.zero)
        {
            transform.position += moveDir * currentSpeed * Time.deltaTime;
        }

        isWalking = inputVector != Vector2.zero || isSlipping || slipTimer > 0f;

        // หมุนตัวตามทิศทางปกติถ้าไม่ได้ลื่นไถล
        if (!isSlipping && slipTimer <= 0f && moveDir != Vector3.zero)
        {
            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }
    }

    // --- Obstacle Public API ---
    public void TriggerSlipImpulse(float speedMultiplier = 1.5f, float duration = 0.45f, float spin = 360f)
    {
        if (slipCooldownTimer > 0f || slipTimer > 0f) return; // ป้องกันการลื่นซ้ำซ้อนไม่หยุด

        Debug.Log("🛢️ Player: Slipped on Oil briefly!");
        this.slipCooldownTimer = 1.0f; // Cooldown 1 วินาทีก่อนที่จะลื่นรอบใหม่
        this.slipTimer = duration;     // ลื่นแค่ 0.45 วินาทีพอดีๆ
        this.slipSpeedMultiplier = speedMultiplier;
        this.spinSpeed = spin;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        if (inputVector != Vector2.zero)
        {
            this.slipMomentum = new Vector3(inputVector.x, 0, inputVector.y).normalized;
        }
        else
        {
            this.slipMomentum = transform.forward;
        }
    }

    public void SetSlipping(bool slipping, float multiplier, float duration, float spin)
    {
        if (slipping)
        {
            TriggerSlipImpulse(multiplier, duration, spin);
        }
    }

    public void TriggerSlipDecay(float duration)
    {
        if (slipTimer > 0.25f)
        {
            slipTimer = 0.25f;
        }
    }

    public void ApplySlowEffect(float multiplier, float duration)
    {
        Debug.Log("🕳️ Player: Tripped on Pothole! Speed reduced!");
        this.slowMultiplier = multiplier;
        this.slowTimer = duration;
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    public BaseCounter GetSelectedCounter()
    {
        return selectedCounter;
    }

    // --- IKitchenObjectParent Implementation ---
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    // รองรับชื่อเดิมเพื่อความเข้ากันได้
    public Transform GetKitchenObjectFollowTranform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    /// <summary>
    /// สร้างเอฟเฟกต์กลุ่มฝุ่น/ควันขาวกระจายออกด้านหลังตอนพุ่งตัว Dash
    /// </summary>
    private void CreateDashDustEffect()
    {
        GameObject dustObj = new GameObject("DashDustEffect");
        dustObj.transform.SetParent(transform, false);
        dustObj.transform.localPosition = new Vector3(0, 0.15f, -0.35f);

        dashParticleSystem = dustObj.AddComponent<ParticleSystem>();
        dashParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = dashParticleSystem.main;
        main.playOnAwake = false;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = 0.3f;
        main.startSpeed = 2.5f;
        main.startSize = 0.35f;
        main.startColor = new Color(1f, 1f, 1f, 0.65f);

        var emission = dashParticleSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });

        var shape = dashParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.25f;

        ParticleSystemRenderer renderer = dustObj.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.material = FireExtinguisher.GetSafeMaterial(new Color(0.92f, 0.92f, 0.95f, 0.6f));
        }
    }
}

