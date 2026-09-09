using System;
using System.Collections;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;
    [SerializeField] private FireHazard fireHazard;

    private State state;
    private float fryingTimer;
    private FryingRecipeSO fryingRecipeSO;
    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    // Cache event args เพื่อลดขยะ GC (0 GC Allocation) ตามกฎ REFACTORCODE.md ข้อ 5
    private readonly IHasProgress.OnProgressChangedEventArgs progressChangedEventArgs = new IHasProgress.OnProgressChangedEventArgs();
    private readonly OnStateChangedEventArgs stateChangedEventArgs = new OnStateChangedEventArgs();

    private void Awake()
    {
        if (fireHazard == null)
        {
            fireHazard = GetComponent<FireHazard>();
        }
    }

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer += Time.deltaTime * 1.3f;

                    NotifyProgressChanged(fryingTimer / fryingRecipeSO.fryingTimerMax);

                    if (fryingTimer > fryingRecipeSO.fryingTimerMax)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        state = State.Fried;
                        burningTimer = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                        NotifyStateChanged();
                    }
                    break;
                case State.Fried:
                    burningTimer += Time.deltaTime;

                    NotifyProgressChanged(burningTimer / burningRecipeSO.burningTimerMax);

                    if (burningTimer > burningRecipeSO.burningTimerMax)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state = State.Burned;

                        if (fireHazard != null)
                        {
                            fireHazard.Ignite();
                        }

                        NotifyStateChanged();
                        NotifyProgressChanged(0f);
                    }
                    break;
                case State.Burned:
                    break;
            }
        }
        else
        {
            // Defensive Guard: หากไม่มีวัตถุอยู่บนเตา แต่สถานะเตายังค้างอยู่ (เช่น แมวขโมยไป) ให้รีเซ็ตกลับเป็น State.Idle ทันที
            if (state != State.Idle)
            {
                ResetStoveState();
            }
        }
    }

    public override void ClearKitchenObject()
    {
        base.ClearKitchenObject();

        // เมื่อวัตถุดิบถูกนำออกจากเตา (เช่น แมวขโมย หรือผู้เล่นหยิบออก) ให้รีเซ็ตสถานะเตากลับเป็น Idle ทันที
        if (state != State.Idle)
        {
            ResetStoveState();
        }
    }

    /// <summary>
    /// ล้างสถานะการทำงานของเตาแก๊ส ดับไฟ ปิดควัน หยุดเสียงฉ่า และรีเซ็ตหลอด Progress
    /// </summary>
    private void ResetStoveState()
    {
        state = State.Idle;
        fryingTimer = 0f;
        burningTimer = 0f;
        NotifyStateChanged();
        NotifyProgressChanged(0f);
    }

    private void NotifyStateChanged()
    {
        stateChangedEventArgs.state = state;
        OnStateChanged?.Invoke(this, stateChangedEventArgs);
    }

    private void NotifyProgressChanged(float progressNormalized)
    {
        progressChangedEventArgs.progressNormalized = progressNormalized;
        OnProgressChanged?.Invoke(this, progressChangedEventArgs);
    }

    public override void Interact(Player player)
    {
        if (fireHazard != null && fireHazard.IsBurning())
        {
            // ไม่สามารถหยิบของได้ขณะที่ไฟกำลังลุกไหม้ ต้องดับไฟก่อน
            return;
        }
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    
                    state = State.Frying;
                    fryingTimer = 0f;

                    NotifyStateChanged();
                    NotifyProgressChanged(fryingTimer / fryingRecipeSO.fryingTimerMax);
                }
            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();

                        state = State.Idle;

                        NotifyStateChanged();
                        NotifyProgressChanged(0f);
                    }
                }
            }
            else
            {
                // player grabs the item with empty hands
                GetKitchenObject().SetKitchenObjectParent(player);

                // **[BUG FIX]: Reset state and UI when picking up item from stove without a plate**
                state = State.Idle;

                NotifyStateChanged();
                NotifyProgressChanged(0f);
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputkitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputkitchenObjectSO);
        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }
        else
        {
            return null;
        }
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputkitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputkitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputkitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputkitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }
}
