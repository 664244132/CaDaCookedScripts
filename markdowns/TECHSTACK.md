# 🛠️ CaDaCook - Tech Stack & Version Specifications

เอกสารสรุปสถาปัตยกรรม เทคโนโลยี เอนจิน เครื่องมือ แพ็กเกจ และเวอร์ชันที่ใช้งานภายในโปรเจกต์เกม **CaDaCook**

---

## 📊 Summary Table (ตารางสรุปเทคโนโลยีหลัก)

| หมวดหมู่ (Category) | เทคโนโลยี / แพ็กเกจ (Technology / Package) | เวอร์ชั่น (Version) | หน้าที่และความรับผิดชอบ (Role & Description) |
| :--- | :--- | :--- | :--- |
| **Game Engine** | Unity Engine 6 | `6000.3.6f1` | Game Engine หลัก ขับเคลื่อน Physics, Rendering และ Runtime |
| **Render Pipeline** | Universal Render Pipeline (URP) | `17.3.0` | กราฟิกไปป์ไลน์แบบ URP ปรับแต่ง Shaders และแสงเงาอย่างมีประสิทธิภาพ |
| **Programming Language** | C# | `.NET / Unity API` | ภาษาโปรแกรมมิ่งหลักในการพัฒนาระบบเกมเพลย์ทั้งหมด |
| **Input System** | Unity New Input System | `1.18.0` | จัดการ Input Actions, Gamepad/Keyboard bindings (`PlayerInputActions`) |
| **Camera System** | Cinemachine | `2.10.5` | จัดการมุมกล้องติดตามตัวละคร (Follow / Virtual Camera) |
| **UI Framework** | uGUI & TextMesh Pro | `2.0.0` | ระบบอินเทอร์เฟซผู้ใช้ ทั้ง World-Space HUD และ Screen-Space Canvas |
| **Level Design Tool** | ProBuilder | `6.0.8` | เครื่องมือปั้นโมเดลฉากและโครงสร้างครัวเบื้องต้นภายใน Editor |
| **Navigation & AI** | Unity AI Navigation | `2.0.9` | ระบบ NavMesh และการคำนวณเส้นทาง (รองรับระบบ NPC/Customer ในอนาคต) |
| **Multiplayer Base** | Unity Multiplayer Center | `1.0.1` | ระบบโครงสร้างพื้นฐานเพื่อรองรับการต่อยอดเป็นเกม Multiplayer Co-op |
| **Performance Tools** | Burst Compiler & Mathematics | `1.8.27` / `1.3.3` | เพิ่มประสิทธิภาพการประมวลผลและการคำนวณเวกเตอร์ |
| **Cloud Services** | Unity Cloud Save & Cloud Code | `3.4.0` / `2.10.2` | รองรับการบันทึกข้อมูลคะแนนและฟังก์ชันฝั่ง Cloud ในอนาคต |
| **Testing Suite** | Unity Test Framework & NUnit | `1.6.0` / `2.0.5` | เฟรมเวิร์กสำหรับเขียน Unit Test และ Integration Test ระบบเกม |

---

## 🎮 1. Unity Engine & Graphic Pipeline

### Unity 6 Specifications
- **Editor Version:** `6000.3.6f1 (bbb010bdb8a3)`
- **Target Platform:** PC Standalone (Windows x64 / macOS), Gamepad Support
- **Physics Engine:** Unity Built-in 3D Physics (`PhysX`) สำหรับตรวจจับ Raycast, CapsuleCast และ Collisions

### Universal Render Pipeline (URP)
- **Package:** `com.unity.render-pipelines.universal` (v17.3.0)
- **Shader Graph:** `com.unity.shadergraph` (v17.3.0)
- **Lighting & Post-Processing:** ใช้ URP Post-Processing Volume สำหรับ Color Grading, Bloom และ Depth of Field

---

## 🕹️ 2. Input System & Controls

ใช้ **Unity New Input System (`com.unity.inputsystem` v1.18.0)**
- **Asset Config:** `Assets/InputSystem_Actions.inputactions` และ `PlayerInputActions.cs`
- **Action Map (`Player`):**
  - `Move` (`Vector2`): การเคลื่อนที่ (`WASD` หรือ ลูกศร หรือ Left Stick บน Gamepad)
  - `Interact` (`Button`): การกดปุ่มมีปฏิสัมพันธ์หลัก (`E` หรือปุ่มบน Controller)
  - `InteractAlternate` (`Button`): การกดปุ่มทำงานพิเศษ เช่น หั่นอาหาร (`F` หรือปุ่มบน Controller)
  - `Pause` (`Button`): ปุ่มหยุดเกมชั่วคราว (`Escape` หรือปุ่ม Menu บน Controller)

---

## 🧩 3. สถาปัตยกรรมและดีไซน์แพทเทิร์น (Architecture & Design Patterns)

1. **Singleton Pattern:**
   - นำมาใช้ในคลาสศูนย์กลางของระบบ เช่น:
     - `KitchenGameManager.Instance`
     - `DeliveryManager.Instance`
     - `GameInput.Instance`
     - `SoundManager.Instance`
     - `Player.Instance`
2. **Observer Pattern (C# Events & Action delegates):**
   - แยก Logic ออกจาก Presentation อย่างเด็ดขาด (Decoupling):
     - `OnStateChanged`, `OnGamePaused`, `OnGameUnpaused`
     - `OnRecipeSpawned`, `OnRecipeCompleted`, `OnRecipeSuccess`, `OnRecipeFailed`
     - `OnProgressChanged`
3. **Data-Driven Architecture (ScriptableObjects):**
   - ใช้วัตถุ ScriptableObject สำหรับจัดการข้อมูลและสูตรอาหาร เพื่อความยืดหยุ่นสูง:
     - `KitchenObjectSO`: ข้อมูลวัตถุดิบ (Prefab, Sprite, Name)
     - `CuttingRecipeSO`: ข้อมูลการหั่น (Input, Output, Max Cuts)
     - `FryingRecipeSO`: ข้อมูลการทอด (Input, Output, Frying Timer)
     - `BurningRecipeSO`: ข้อมูลการไหม้ (Input, Output, Burning Timer)
     - `RecipeSO`: สูตรอาหารสำเร็จ (List of Ingredients)
     - `RecipeListSO`: รายการสูตรอาหารทั้งหมดในเกม
     - `AudioClipRefsSO`: รวม Reference คลิปเสียงทั้งหมด
4. **Interface Segregation:**
   - `IKitchenObjectParent`: กำหนดพฤติกรรมการถือ/วาง/ส่งต่อวัตถุดิบ (สืบทอดโดย `Player` และ `BaseCounter`)
   - `IHasProgress`: กำหนด Event และค่า Progress สำหรับอัปเดตแถบหลอดพลัง (`CuttingCounter`, `StoveCounter`)
5. **Finite State Machine (FSM):**
   - ควบคุมโฟลว์การเล่นของเกมใน `KitchenGameManager` (`WaitingToStart` ➔ `CountdownToStart` ➔ `GamePlaying` ➔ `GameOver`)
   - ควบคุมการทอดบนเตาใน `StoveCounter` (`Idle` ➔ `Frying` ➔ `Fried` ➔ `Burned`)

---

## 📁 4. Unity Project Structure & Scenes Configuration

### รายการ Scenes ใน Build Settings:
1. `Assets/Scenes/MainMenuScene.unity` (Index 0) — เมนูหลักของเกม
2. `Assets/Scenes/GameScene.unity` (Index 1) — หน้าห้องครัวสำหรับเล่นเกมหลัก
3. `Assets/Scenes/LoadingScene.unity` (Index 2) — ซีนคั่นระหว่างโหลดฉากผ่าน `Loader.cs`

---

## 🚀 5. Key Configuration Files

- [`Packages/manifest.json`](file:///d:/unity/My%20project/Packages/manifest.json) — รายการแพ็กเกจและ Dependencies ทั้งหมดของ Unity 6
- [`ProjectSettings/ProjectVersion.txt`](file:///d:/unity/My%20project/ProjectSettings/ProjectVersion.txt) — ไฟล์ระบุเวอร์ชัน Unity Editor
- [`ProjectSettings/EditorBuildSettings.asset`](file:///d:/unity/My%20project/ProjectSettings/EditorBuildSettings.asset) — การตั้งค่าลำดับ Scenes ในการ Build
- [`Assets/InputSystem_Actions.inputactions`](file:///d:/unity/My%20project/Assets/InputSystem_Actions.inputactions) — การตั้งค่า Binding ของระบบ Input
