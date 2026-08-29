# 🛠️ CaDaCook - Tech Stack & Version Specifications

เอกสารสรุปสถาปัตยกรรม เทคโนโลยี เอนจิน เครื่องมือ แพ็กเกจ และเวอร์ชันที่ใช้งานภายในโปรเจกต์เกม **CaDaCook**

---

## 📊 Summary Table (ตารางสรุปเทคโนโลยีหลัก)

| หมวดหมู่ (Category) | เทคโนโลยี / แพ็กเกจ (Technology / Package) | เวอร์ชั่น (Version) | หน้าที่และความรับผิดชอบ (Role & Description) |
| :--- | :--- | :--- | :--- |
| **Game Engine** | Unity Engine 6 | `6000.3.6f1` | Game Engine หลัก ขับเคลื่อน Physics, Rendering และ Runtime |
| **Render Pipeline** | Universal Render Pipeline (URP) | `17.3.0` | กราฟิกไปป์ไลน์แบบ URP ปรับแต่ง Shaders และแสงเงาอย่างมีประสิทธิภาพ |
| **Programming Language** | C# | `.NET / Unity 6 API` | ภาษาโปรแกรมมิ่งหลักในการพัฒนาระบบเกมเพลย์ทั้งหมด |
| **Input System** | Unity New Input System | `1.18.0` | จัดการ Input Actions, Gamepad/Keyboard bindings (`PlayerInputActions`) |
| **Camera System** | Cinemachine | `2.10.5` | จัดการมุมกล้องติดตามตัวละคร (Follow / Virtual Camera) |
| **UI Framework** | uGUI & TextMesh Pro | `2.0.0` | ระบบอินเทอร์เฟซผู้ใช้ ทั้ง World-Space HUD และ Screen-Space Canvas |
| **Level Design Tool** | ProBuilder | `6.0.8` | เครื่องมือปั้นโมเดลฉากและโครงสร้างครัวเบื้องต้นภายใน Editor |
| **Navigation & AI** | Unity AI Navigation | `2.0.9` | ระบบ NavMesh และการคำนวณเส้นทาง (รองรับระบบ NPC แมว/หนูในครัว) |
| **Performance Tools** | Burst Compiler & Mathematics | `1.8.27` / `1.3.3` | เพิ่มประสิทธิภาพการประมวลผลและการคำนวณเวกเตอร์ |
| **Testing Suite** | Unity Test Framework & NUnit | `1.6.0` / `2.0.5` | เฟรมเวิร์กสำหรับเขียน Unit Test และ Integration Test ระบบเกม |

---

## 🎮 1. Unity Engine & Graphic Pipeline

### Unity 6 Specifications
- **Editor Version:** `6000.3.6f1 (bbb010bdb8a3)`
- **Target Platform:** PC Standalone (Windows x64 / macOS), Gamepad Support
- **Physics Engine:** Unity Built-in 3D Physics (`PhysX`) สำหรับตรวจจับ Raycast, CapsuleCast, Trigger Colliders (คราบน้ำมัน, หลุมดัก, ไฟไหม้)
- **Modern API Compliance:**
  - ใช้ `Object.FindFirstObjectByType<T>()` และ `Object.FindAnyObjectByType<T>()` ทดแทน `FindObjectOfType` ที่ล้าสมัย
  - ใช้ `textWrappingMode = TextWrappingModes.NoWrap` ทดแทน `enableWordWrapping`
  - ใช้ระบบ `GetSafeMaterial()` ดึง Scene Shaders อัตโนมัติ ป้องกัน Shaders หายใน Standalone Builds

---

## 🕹️ 2. Input System & Controls

ใช้ **Unity New Input System (`com.unity.inputsystem` v1.18.0)**
- **Action Map (`Player`):**
  - `Move` (`Vector2`): การเคลื่อนที่ (`WASD` หรือ ลูกศร หรือ Left Stick บน Gamepad)
  - `Interact` (`Button`): การกดปุ่มมีปฏิสัมพันธ์หลัก (`E` หรือปุ่มบน Controller) และการทิ้งถังดับเพลิงลงพื้น
  - `InteractAlternate` (`Button`): การกดปุ่มทำงานพิเศษ เช่น หั่นอาหาร หรือกดค้างเพื่อฉีดถังดับเพลิง (`F` หรือปุ่มบน Controller)
  - `Pause` (`Button`): ปุ่มหยุดเกมชั่วคราว (`Escape` หรือปุ่ม Menu บน Controller)

---

## 🧩 3. สถาปัตยกรรมและดีไซน์แพทเทิร์น (Architecture & Design Patterns)

1. **Singleton Pattern:**
   - `KitchenGameManager.Instance`, `DeliveryManager.Instance`, `RushHourManager.Instance`, `GameInput.Instance`, `SoundManager.Instance`, `Player.Instance`
2. **Observer Pattern (C# Events):**
   - แยก Logic ออกจาก Presentation อย่างเด็ดขาด:
     - `OnStateChanged`, `OnGamePaused`, `OnGameUnpaused`
     - `OnRecipeSpawned`, `OnRecipeCompleted`, `OnRecipeSuccess`, `OnRecipeFailed`
     - `OnRushHourStarted`, `OnRushHourEnded`
     - `OnAnyFireStarted`, `OnAnyFireExtinguished`
     - `OnCatMeow`, `OnCatScared`, `OnPlayerTripped`
3. **Data-Driven Architecture (ScriptableObjects):**
   - `KitchenObjectSO`, `CuttingRecipeSO`, `FryingRecipeSO`, `BurningRecipeSO`, `RecipeSO`, `RecipeListSO`, `AudioClipRefsSO`
4. **Interface Segregation:**
   - `IKitchenObjectParent`: ใช้งานโดย `Player`, `BaseCounter`, `KitchenCatNPC`
   - `IHasProgress`: ใช้งานโดย `CuttingCounter`, `StoveCounter`
5. **Finite State Machine (FSM):**
   - `KitchenGameManager` (โฟลว์เกม)
   - `StoveCounter` (การทอดและไฟไหม้)
   - `KitchenCatNPC` (พฤติกรรมแมว: เดินเล่น, ขโมยของ, วิ่งหนี)
