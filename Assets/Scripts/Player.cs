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
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    // --- Obstacle & Effect Variables ---
    private bool isSlipping;
    private float slipTimer;
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
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void OnDestroy()
    {
        if (gameInput != null)
        {
            gameInput.OnInteractAction -= GameInput_OnInteractAction;
            gameInput.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
            return;
        }

        // ตรวจจับการหยิบถังดับเพลิงที่วางอยู่บนพื้น
        if (!HasKitchenObject())
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 2.5f))
            {
                if (hit.collider.TryGetComponent(out FireExtinguisher ext) && ext.GetKitchenObjectParent() == null)
                {
                    ext.SetKitchenObjectParent(this);
                }
            }
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        // หากผู้เล่นถือถังดับเพลิงอยู่ ให้กดฉีดพ่นสารดับเพลิง
        if (HasKitchenObject() && GetKitchenObject() is FireExtinguisher fireExtinguisher)
        {
            fireExtinguisher.StartSpraying(transform.forward);
            return;
        }

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void Update()
    {
        UpdateObstacleTimers();
        HandleMovement();
        HandleInteractions();
    }

    private void UpdateObstacleTimers()
    {
        if (slipTimer > 0f)
        {
            slipTimer -= Time.deltaTime;
            if (slipTimer <= 0f && !isSlipping)
            {
                slipSpeedMultiplier = 1f;
                slipMomentum = Vector3.zero;
            }
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
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // คำนวณความเร็วที่ได้รับผลกระทบจากอุปสรรค
        float currentSpeed = moveSpeed * slowMultiplier;

        // หากกำลังลื่นไถล
        if (isSlipping || slipTimer > 0f)
        {
            currentSpeed *= slipSpeedMultiplier;
            if (moveDir != Vector3.zero)
            {
                slipMomentum = Vector3.Lerp(slipMomentum, moveDir, Time.deltaTime * 3f);
            }
            moveDir = slipMomentum.normalized;

            // หมุนตัวเชฟตอนลื่นไถล
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }

        float moveDistance = currentSpeed * Time.deltaTime;
        float playerRadius = 0.5f;
        float playerHeight = 2.0f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove && moveDir != Vector3.zero)
        {
            // ตรวจจับการเดินสไลด์ตามแนวแกน X
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                // ตรวจจับการเดินสไลด์ตามแนวแกน Z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
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

        isWalking = inputVector != Vector2.zero;

        // หมุนตัวตามทิศทางปกติถ้าไม่ได้ลื่นไถล
        if (!isSlipping && slipTimer <= 0f && moveDir != Vector3.zero)
        {
            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }
    }

    // --- Obstacle Public API ---
    public void SetSlipping(bool slipping, float multiplier, float duration, float spin)
    {
        if (!this.isSlipping && slipping)
        {
            Debug.Log("🛢️ Player: Stepped on Oil! Slipping and spinning!");
        }

        this.isSlipping = slipping;
        this.slipSpeedMultiplier = multiplier;
        this.spinSpeed = spin;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        if (inputVector != Vector2.zero)
        {
            this.slipMomentum = new Vector3(inputVector.x, 0, inputVector.y).normalized;
        }
        else if (slipMomentum == Vector3.zero)
        {
            this.slipMomentum = transform.forward;
        }
    }

    public void TriggerSlipDecay(float duration)
    {
        this.isSlipping = false;
        this.slipTimer = duration;
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
}
