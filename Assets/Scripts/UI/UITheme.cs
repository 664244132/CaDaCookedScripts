using UnityEngine;

/// <summary>
/// ศูนย์กลางสำหรับจัดการ Theming, Style Tokens, สี และการจัดรูปแบบข้อความ UI (TextMeshPro)
/// ทำหน้าที่เปรียบเสมือน Stylesheet กลาง (CSS) ของเกม CaDaCook เพื่อแก้ปัญหา Inline Styling (DRY Principle)
/// </summary>
public static class UITheme
{
    // =========================================================================
    // 🎨 COLOR PALETTE (Hex Codes สำหรับ TextMeshPro Rich Text Tags)
    // =========================================================================
    public const string COLOR_PRIMARY_CYAN   = "#00E5FF"; // สีฟ้าครามสดใส (การโต้ตอบหลัก, ขัดล้าง, จานสะอาด)
    public const string COLOR_WARNING_ORANGE = "#FF9100"; // สีส้มเตือน (ตะแกรงเต็ม, แจ้งเตือนด่วน)
    public const string COLOR_WARNING_AMBER  = "#FFA500"; // สีส้มอำพัน (คำเตือน, ข้อความสำคัญ)
    public const string COLOR_ALERT_RED      = "#E02020"; // สีแดงแจ้งเตือน (ลูกค้าโกรธ, ไฟไหม้)
    public const string COLOR_SUCCESS_GREEN  = "#76FF03"; // สีเขียวนีออน (งานสำเร็จ, หยิบจานสะอาด)
    public const string COLOR_ACCENT_YELLOW  = "#FFE600"; // สีเหลืองสดใส (ปุ่ม Action, ฉีดถังดับเพลิง)
    public const string COLOR_YELLOW         = "#FFFF00"; // สีเหลืองมาตรฐาน (ปุ่ม Pick up)
    public const string COLOR_GOLD           = "#FFD700"; // สีทองพรีเมียม (คะแนน VIP, Tutorial)
    public const string COLOR_SILVER         = "#E2E8F0"; // สีเงินประกาย (ผลคะแนน 2 ดาว)
    public const string COLOR_BRONZE         = "#CD7F32"; // สีทองแดง (ผลคะแนน 1 ดาว)
    public const string COLOR_INFO_SKY       = "#38BDF8"; // สีฟ้าพาสเทล (สถิติจานที่ส่ง)
    public const string COLOR_MUTED_GRAY     = "#94A3B8"; // สีเทาหม่น (ข้อความรอง, นับถอยหลัง)
    public const string COLOR_WHITE          = "#FFFFFF"; // สีขาวมาตรฐาน

    // =========================================================================
    // 🌈 UNITY COLOR OBJECTS (สำหรับ Image, TextMeshPro.color)
    // =========================================================================
    public static readonly Color ColorPrimaryCyan          = new Color(0.0f, 0.85f, 1.0f);
    public static readonly Color ColorWarningOrange         = new Color(1.0f, 0.4f, 0.15f);
    public static readonly Color ColorSuccessGreen         = new Color(0.46f, 1.0f, 0.01f);
    public static readonly Color ColorAccentYellow         = new Color(1.0f, 0.9f, 0.0f);
    public static readonly Color ColorTitleOrange          = new Color(1.0f, 0.45f, 0.0f);
    public static readonly Color ColorDarkBackground       = new Color(0.12f, 0.12f, 0.15f, 0.92f);
    public static readonly Color ColorOverlayBackground     = new Color(0.02f, 0.02f, 0.04f, 0.97f);
    public static readonly Color ColorWarningBoxBackground = new Color(0.25f, 0.11f, 0.01f, 0.95f);
    public static readonly Color ColorWarningOrangeText    = new Color(1.0f, 0.65f, 0.0f);

    // =========================================================================
    // 📝 TEXT FORMATTING HELPERS (DRY: รวมศูนย์การสร้างข้อความ UI/World-Space)
    // =========================================================================

    /// <summary>
    /// ป้ายบอกจำนวนกองจานสะอาดที่ผู้เล่นถืออยู่
    /// </summary>
    public static string FormatCleanPlateBadge(int count)
    {
        return $"<color={COLOR_PRIMARY_CYAN}><b>CLEAN x{count}</b></color>";
    }

    /// <summary>
    /// ป้ายบอกจำนวนกองจานเปื้อนบน DeliveryCounter พร้อมปุ่มแนะนำ
    /// </summary>
    public static string FormatDirtyPlateBadge(int count)
    {
        return $"DIRTY x{count}\n<color={COLOR_YELLOW}><size=75%>[E] PICK UP</size></color>";
    }

    /// <summary>
    /// ป้ายแจ้งเตือนเมื่อตะแกรงสะเด็ดน้ำเต็ม ต้องยกจานออกก่อน
    /// </summary>
    public static string FormatRackFullPrompt(int cleanPlatesCount, int maxCleanPlates, int dirtyPlatesCount)
    {
        return $"<color={COLOR_WARNING_ORANGE}><b>[ ! ] RACK FULL! PICK UP [E]</b></color>\n" +
               $"<size=75%>Rack Full ({cleanPlatesCount}/{maxCleanPlates}) | Sink: {dirtyPlatesCount}</size>";
    }

    /// <summary>
    /// ป้ายแจ้งการกด [F] เพื่อขัดล้างจานในอ่าง
    /// </summary>
    public static string FormatScrubPrompt(int currentScrub, int maxScrub, int dirtyPlatesCount, int cleanPlatesCount, int maxCleanPlates)
    {
        return $"<color={COLOR_PRIMARY_CYAN}><b>[F] SCRUB ({currentScrub}/{maxScrub})</b></color>\n" +
               $"<size=75%>Plates in Sink: {dirtyPlatesCount} (Rack: {cleanPlatesCount}/{maxCleanPlates})</size>";
    }

    /// <summary>
    /// ป้ายแจ้งการกด [E] เพื่อหยิบจานสะอาดออกจากตะแกรง
    /// </summary>
    public static string FormatPickCleanPlatePrompt(int cleanPlatesCount, int maxCleanPlates)
    {
        return $"<color={COLOR_SUCCESS_GREEN}><b>[E] PICK CLEAN PLATE</b></color>\n" +
               $"<size=75%>Clean Plates: {cleanPlatesCount}/{maxCleanPlates}</size>";
    }

    /// <summary>
    /// ข้อความปุ่มกดเมื่อผู้เล่นกำลังถือถังดับเพลิง (ฉีด / วาง)
    /// </summary>
    public static string FormatExtinguisherHoldPrompt()
    {
        return $"<size=30><color={COLOR_ACCENT_YELLOW}><b>[ F ]  HOLD TO SPRAY</b></color></size>\n" +
               $"<size=26><color={COLOR_WHITE}><b>[ E ]  DROP TO FLOOR</b></color></size>";
    }

    /// <summary>
    /// ข้อความปุ่มกดเมื่อถังดับเพลิงวางอยู่บนพื้น
    /// </summary>
    public static string FormatExtinguisherPickupPrompt()
    {
        return $"<size=34><color={COLOR_ACCENT_YELLOW}><b>[ E ]</b></color></size>  " +
               $"<size=30><color={COLOR_WHITE}><b>PICK UP</b></color></size>";
    }

    /// <summary>
    /// รูปแบบชื่อเมนู VIP 3X
    /// </summary>
    public static string FormatVipRecipe(string recipeName)
    {
        return $"<color={COLOR_GOLD}><b>[VIP 3X]</b></color> <color={COLOR_WHITE}>{recipeName}</color>";
    }

    /// <summary>
    /// รูปแบบชื่อเมนูลูกค้ากำลังโกรธพร้อมเวลานับถอยหลัง
    /// </summary>
    public static string FormatAngryRecipe(string recipeName, int remainingSeconds)
    {
        return $"<color={COLOR_ALERT_RED}><b>[ANGRY {remainingSeconds}s]</b></color> <color=#200000>{recipeName}</color>";
    }

    /// <summary>
    /// รูปแบบข้อความ Super Combo (Streak >= 5)
    /// </summary>
    public static string FormatSuperCombo(int streak, float multiplier)
    {
        return $"<size=26><color=#FF4500><b>>>> SUPER COMBO x{streak} <<<</b></color></size>\n" +
               $"<size=20><color={COLOR_WARNING_AMBER}><b>BONUS x{multiplier:F1} SCORE!</b></color></size>";
    }

    /// <summary>
    /// รูปแบบข้อความ Standard Combo
    /// </summary>
    public static string FormatStandardCombo(int streak, float multiplier)
    {
        return $"<size=24><color=#FF7700><b>[ COMBO x{streak} ]</b></color></size>\n" +
               $"<size=19><color={COLOR_WARNING_AMBER}><b>BONUS x{multiplier:F1} SCORE!</b></color></size>";
    }

    /// <summary>
    /// คืนค่า Hex Color ตามระดับดาว
    /// </summary>
    public static string GetStarRatingColor(int stars)
    {
        return stars switch
        {
            3 => COLOR_GOLD,
            2 => COLOR_SILVER,
            1 => COLOR_BRONZE,
            _ => COLOR_MUTED_GRAY
        };
    }

    /// <summary>
    /// สร้างข้อความแดชบอร์ดสรุปคะแนน GameOverUI เต็มรูปแบบ (DRY Format)
    /// </summary>
    public static string FormatGameOverDashboard(string rankTitle, int stars, int totalScore, int deliveredAmount, int maxCombo, float remainingSeconds)
    {
        string starBadgeColor = GetStarRatingColor(stars);
        return $"<size=44><color={starBadgeColor}><b>{rankTitle}</b></color></size>\n\n" +
               $"<size=62><color={COLOR_GOLD}><b>FINAL SCORE : {totalScore:N0} PTS</b></color></size>\n\n" +
               $"<size=32><color={COLOR_WHITE}>Dishes Delivered : <color={COLOR_INFO_SKY}><b>{deliveredAmount}</b></color>      |      Max Combo : <color={COLOR_WARNING_AMBER}><b>x{maxCombo}</b></color></color></size>\n\n" +
               $"<size=24><color={COLOR_MUTED_GRAY}>Returning to Main Menu in <b>{Mathf.CeilToInt(remainingSeconds)}s</b>...</color></size>";
    }

    /// <summary>
    /// ข้อความหัวเรื่อง Tutorial พร้อมเอฟเฟกต์กระพริบ
    /// </summary>
    public static string FormatTutorialHeader(bool isPulsing)
    {
        string promptColor = isPulsing ? COLOR_GOLD : COLOR_WHITE;
        return $"<b>HOW TO PLAY   -   <color={promptColor}>>>> PRESS ANY BUTTON TO START <<<</color></b>";
    }

    /// <summary>
    /// ข้อความแจ้งเตือนสีส้มเรื่องถังดับเพลิงในหน้า Tutorial
    /// </summary>
    public static string FormatTutorialWarningBox()
    {
        return $"<b><size=28><color={COLOR_WARNING_AMBER}>[ ! ] IMPORTANT WARNING : FIRE EXTINGUISHER</color></size></b>\n" +
               $"<b><color={COLOR_WARNING_AMBER}>* DO NOT place Fire Extinguisher on counters! The CATS will STEAL it!</color></b>\n" +
               $"<b><color={COLOR_GOLD}>* Always press [ E ] to DROP Extinguisher safely on the FLOOR!</color></b>";
    }

    // =========================================================================
    // 📁 LOCAL ASSET CATALOG (การเข้าถึง Fonts และ Icons ภายในเครื่องแบบ Local)
    // =========================================================================
    public static class LocalAssets
    {
        public const string FONT_LIBERATION_SANS = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";
        public const string FONT_LIBERATION_SANS_SDF = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        public const string ICON_BREAD = "Assets/ChaosKitchen/New Unity Project/Assets/_Assets/Textures/Icons/Bread.png";
        public const string ICON_CABBAGE = "Assets/ChaosKitchen/New Unity Project/Assets/_Assets/Textures/Icons/Cabbage.png";
        public const string ICON_CHEESE = "Assets/ChaosKitchen/New Unity Project/Assets/_Assets/Textures/Icons/CheeseSlice.png";
        public const string ICON_MEAT = "Assets/ChaosKitchen/New Unity Project/Assets/_Assets/Textures/Icons/MeatPattyCooked.png";
        public const string ICON_PLATE = "Assets/ChaosKitchen/New Unity Project/Assets/_Assets/Textures/Icons/Plate.png";
        public const string ICON_TOMATO = "Assets/ChaosKitchen/New Unity Project/Assets/_Assets/Textures/Icons/TomatoSlice.png";
        public const string TEXTURE_LOGO_SMALL = "Assets/ChaosKitchen/New Unity Project/Assets/CodeMonkeyFree/Textures/CodeMonkeyLogoSmallHeight.png";
        public const string TEXTURE_LOGO_KITCHEN_CHAOS = "Assets/ChaosKitchen/New Unity Project/Assets/_Assets/Textures/KitchenChaosLogo.png";
    }
}
