using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DeliveryManager: ตัวจัดการคิวออเดอร์, ลูกค้า VIP, ระบบคอมโบ Tip & Combo Streak, และคำนวณคะแนนดาว
/// </summary>
public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    // Events
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    public event EventHandler<OnComboChangedEventArgs> OnComboStreakChanged;
    public event EventHandler<OnVIPOrderEventArgs> OnVIPOrderSpawned;
    public event EventHandler<OnVIPOrderEventArgs> OnVIPOrderSuccess;
    public event EventHandler<OnVIPOrderEventArgs> OnVIPOrderExpired;

    public class OnComboChangedEventArgs : EventArgs
    {
        public int comboStreak;
        public float comboMultiplier;
    }

    public class OnVIPOrderEventArgs : EventArgs
    {
        public OrderData orderData;
    }

    /// <summary>
    /// คลาสข้อมูลออเดอร์ พร้อมระบบ VIP และเวลานับถอยหลัง
    /// </summary>
    [Serializable]
    public class OrderData
    {
        public RecipeSO recipeSO;
        public bool isVIP;
        public float orderTimer;
        public float orderTimerMax;

        public OrderData(RecipeSO recipeSO, bool isVIP, float duration)
        {
            this.recipeSO = recipeSO;
            this.isVIP = isVIP;
            this.orderTimerMax = duration;
            this.orderTimer = duration;
        }

        public float GetTimerNormalized()
        {
            if (orderTimerMax <= 0f) return 1f;
            return Mathf.Clamp01(orderTimer / orderTimerMax);
        }
    }

    [SerializeField] private RecipeListSO recipeListSO;
    private List<OrderData> waitingOrdersList;
    private List<RecipeSO> waitingrecipeSOList; // สำหรับ backward compatibility

    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;

    // ระบบคะแนนและคอมโบ (Scoring & Combo Streak)
    private int successfulRecipesAmount;
    private int scoreMultiplier = 1;
    private int totalScore = 0;
    private int comboStreak = 0;
    private int maxComboStreak = 0;

    // ระบบลูกค้า VIP
    private float vipSpawnCooldown = 30f;
    private float vipSpawnTimer = 25f; // โอกาสเกิดครั้งแรกหลังจากเริ่มเล่นไปสักพัก

    private void Awake()
    {
        Instance = this;
        waitingOrdersList = new List<OrderData>();
        waitingrecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        // ทำงานเฉพาะตอนที่เกมกำลังเล่นอยู่เท่านั้น
        if (KitchenGameManager.Instance != null && !KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }

        // นับเวลาคูลดาวน์สำหรับสุ่มออเดอร์ VIP
        vipSpawnTimer -= Time.deltaTime;

        // นับเวลาเกิดออเดอร์ใหม่
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingOrdersList.Count < waitingRecipesMax && recipeListSO.recipeSOLsit.Count > 0)
            {
                SpawnNewOrder();
            }
        }

        // อัปเดตเวลานับถอยหลังของออเดอร์ (โดยเฉพาะออเดอร์ VIP)
        for (int i = waitingOrdersList.Count - 1; i >= 0; i--)
        {
            OrderData order = waitingOrdersList[i];
            if (order.isVIP)
            {
                order.orderTimer -= Time.deltaTime;
                if (order.orderTimer <= 0f)
                {
                    // ออเดอร์ VIP หมดเวลา!
                    Debug.Log($"[DeliveryManager] VIP Order Expired: {order.recipeSO.recipeName}");
                    waitingOrdersList.RemoveAt(i);
                    SyncBackwardCompatibilityList();

                    // รีเซ็ตคอมโบเมื่อปล่อยให้ออเดอร์ VIP หมดเวลา
                    ResetComboStreak();

                    OnVIPOrderExpired?.Invoke(this, new OnVIPOrderEventArgs { orderData = order });
                    OnRecipeFailed?.Invoke(this, EventArgs.Empty);
                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    /// <summary>
    /// สุ่มสร้างออเดอร์ใหม่ (มีโอกาสเป็นออเดอร์ VIP สีทอง)
    /// </summary>
    private void SpawnNewOrder()
    {
        RecipeSO randomRecipe = recipeListSO.recipeSOLsit[UnityEngine.Random.Range(0, recipeListSO.recipeSOLsit.Count)];

        // ตรวจสอบเงื่อนไขว่าออเดอร์นี้จะเป็น VIP หรือไม่ (เกิดได้เมื่อมีคูลดาวน์พร้อม และยังไม่มี VIP ในคิว)
        bool spawnAsVIP = false;
        bool alreadyHasVIP = waitingOrdersList.Exists(o => o.isVIP);

        if (!alreadyHasVIP && vipSpawnTimer <= 0f)
        {
            // สุ่มโอกาส 50% เมื่อคูลดาวน์พร้อม
            if (UnityEngine.Random.value < 0.5f)
            {
                spawnAsVIP = true;
                vipSpawnTimer = vipSpawnCooldown; // รีเซ็ตคูลดาวน์ VIP
            }
        }

        float orderDuration = spawnAsVIP ? 25.0f : 999f; // VIP มีเวลา 25 วินาที
        OrderData newOrder = new OrderData(randomRecipe, spawnAsVIP, orderDuration);
        waitingOrdersList.Add(newOrder);

        SyncBackwardCompatibilityList();

        if (spawnAsVIP)
        {
            Debug.Log($"[DeliveryManager] 👑 VIP CRITIC ORDER SPAWNED: {randomRecipe.recipeName} (25s Limit)");
            OnVIPOrderSpawned?.Invoke(this, new OnVIPOrderEventArgs { orderData = newOrder });
        }

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void SyncBackwardCompatibilityList()
    {
        waitingrecipeSOList.Clear();
        foreach (var order in waitingOrdersList)
        {
            waitingrecipeSOList.Add(order.recipeSO);
        }
    }

    /// <summary>
    /// ตรวจสอบการส่งอาหารจากจาน
    /// </summary>
    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingOrdersList.Count; i++)
        {
            OrderData waitingOrder = waitingOrdersList[i];
            RecipeSO waitingRecipeSO = waitingOrder.recipeSO;

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO platekitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (platekitchenObjectSO == recipeKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        plateContentsMatchesRecipe = false;
                        break;
                    }
                }

                if (plateContentsMatchesRecipe)
                {
                    // ส่งอาหารได้ถูกต้อง!
                    successfulRecipesAmount++;
                    comboStreak++;
                    maxComboStreak = Mathf.Max(maxComboStreak, comboStreak);

                    // คำนวณตัวคูณคอมโบ (1.0x -> 1.5x -> 2.0x)
                    float currentComboMultiplier = GetComboMultiplier();

                    // คำนวณคะแนนพื้นฐาน (จานปกติ 100 แต้ม, VIP 300 แต้ม 3 เท่า)
                    int basePoints = waitingOrder.isVIP ? 300 : 100;
                    int finalPoints = Mathf.RoundToInt(basePoints * currentComboMultiplier * scoreMultiplier);
                    totalScore += finalPoints;

                    // มอบโบนัสเวลาพิเศษกรณีส่งออเดอร์ VIP สำเร็จ
                    if (waitingOrder.isVIP)
                    {
                        if (KitchenGameManager.Instance != null)
                        {
                            KitchenGameManager.Instance.AddGamePlayingTime(12.0f); // เพิ่มเวลา +12 วินาที
                        }
                        OnVIPOrderSuccess?.Invoke(this, new OnVIPOrderEventArgs { orderData = waitingOrder });
                    }

                    // แจ้งเตือนการเปลี่ยนแปลงของคอมโบ
                    OnComboStreakChanged?.Invoke(this, new OnComboChangedEventArgs
                    {
                        comboStreak = comboStreak,
                        comboMultiplier = currentComboMultiplier
                    });

                    waitingOrdersList.RemoveAt(i);
                    SyncBackwardCompatibilityList();

                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }

        // ส่งผิดสูตร -> รีเซ็ตคอมโบ
        ResetComboStreak();
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverIncorrectRecipe()
    {
        ResetComboStreak();
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    private void ResetComboStreak()
    {
        if (comboStreak > 0)
        {
            comboStreak = 0;
            OnComboStreakChanged?.Invoke(this, new OnComboChangedEventArgs
            {
                comboStreak = 0,
                comboMultiplier = 1.0f
            });
        }
    }

    /// <summary>
    /// ตัวคูณคะแนนตามระดับ Combo Streak:
    /// - Streak 1-2: 1.0x
    /// - Streak 3-4: 1.5x
    /// - Streak 5+: 2.0x
    /// </summary>
    public float GetComboMultiplier()
    {
        if (comboStreak >= 5) return 2.0f;
        if (comboStreak >= 3) return 1.5f;
        return 1.0f;
    }

    /// <summary>
    /// คำนวณระดับดาว 0 - 3 ดาวตามคะแนนรวม:
    /// - 3 ดาว (Gold): 1,200+ คะแนน
    /// - 2 ดาว (Silver): 700+ คะแนน
    /// - 1 ดาว (Bronze): 300+ คะแนน
    /// - 0 ดาว: ต่ำกว่า 300 คะแนน
    /// </summary>
    public int GetStarRating()
    {
        if (totalScore >= 1200) return 3;
        if (totalScore >= 700) return 2;
        if (totalScore >= 300) return 1;
        return 0;
    }

    public string GetChefRankTitle()
    {
        int stars = GetStarRating();
        switch (stars)
        {
            case 3: return "*** [ 3 STARS ] MASTER CHEF (GOLD) ***";
            case 2: return "** [ 2 STARS ] HEAD CHEF (SILVER) **";
            case 1: return "* [ 1 STAR ] LINE COOK (BRONZE) *";
            default: return "[ APPRENTICE CHEF ] TRY AGAIN!";
        }
    }

    public void SetScoreMultiplier(int multiplier)
    {
        this.scoreMultiplier = Mathf.Max(1, multiplier);
    }

    public int GetScoreMultiplier()
    {
        return scoreMultiplier;
    }

    public void SetSpawnRecipeTimerMax(float timerMax)
    {
        this.spawnRecipeTimerMax = Mathf.Max(1f, timerMax);
    }

    public float GetSpawnRecipeTimerMax()
    {
        return spawnRecipeTimerMax;
    }

    public List<OrderData> GetWaitingOrdersList()
    {
        return waitingOrdersList;
    }

    public List<RecipeSO> GetWaitingRecipeSPList()
    {
        return waitingrecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipesAmount;
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public int GetComboStreak()
    {
        return comboStreak;
    }

    public int GetMaxComboStreak()
    {
        return maxComboStreak;
    }
}
