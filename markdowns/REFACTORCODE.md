# 📜 กฎเหล็กการพัฒนาและ Refactor โค้ด C# ใน Unity (CaDaCook)

**คุณคือ Senior Software Engineer**
**บังคับต้องอ่านและปฏิบัติตามกฎ 15 ข้อนี้อย่างเคร่งครัดเมื่อพัฒนาหรือปรับปรุงโปรเจกต์ CaDaCook**

---

## 🚨 กฎเหล็ก 15 ข้อสำหรับการเขียน C# ใน Unity

### 1. **Decoupled Architecture & Observer Pattern (กฎแยก Logic ออกจาก Visual/Audio/UI):**
   - ห้ามเขียนตรรกะเกม (Game Logic) ผูกติดกับ Visual Effects, Animator หรือ AudioSource โดยตรง
   - **แนวทางปฏิบัติ:** ให้ Game Logic ส่งสัญญาณผ่าน **C# Events (`event EventHandler`)** และสร้างคลาสแยกสำหรับ Visual/Audio มารับฟังอีเวนต์ (เช่น `CuttingCounter` ส่ง `OnCut` ➔ `CuttingCounterVisual` เล่นแอนิเมชัน และ `SoundManager` เล่นเสียงมีด)

### 2. **Data-Driven Architecture with ScriptableObjects:**
   - ห้าม Hardcode ค่าสูตรอาหาร, เวลาในการปรุง, หรือข้อมูลวัตถุดิบลงใน MonoBehaviour เด็ดขาด
   - **แนวทางปฏิบัติ:** สร้างและใช้งาน `ScriptableObject` เสมอ (เช่น `KitchenObjectSO`, `RecipeSO`, `CuttingRecipeSO`, `FryingRecipeSO`, `BurningRecipeSO`) เพื่อให้ Game Designer ปรับแต่งค่าผ่าน Inspector ได้อย่างอิสระ

### 3. **Interface-Based Programming (ความยืดหยุ่นของระบบ):**
   - ใช้ Interfaces ในการกำหนดความสามารถของวัตถุแทนการอ้างอิงคลาสแบบเจาะจง
   - ใช้งาน `IKitchenObjectParent` สำหรับทุกสิ่งที่มีหรือถือวัตถุดิบได้ (`Player`, `BaseCounter`)
   - ใช้งาน `IHasProgress` สำหรับทุกเคาน์เตอร์ที่มีการนับความคืบหน้า เพื่อให้ `ProgressBarUI` ทำงานร่วมกันได้ทุกตัว

### 4. **De Morgan's Laws & Early Return (โครงสร้างโค้ดแบบ Flat):**
   - **ห้าม** เขียนเงื่อนไข `!(A && B)` หรือ `!(A || B)` ให้แปลงเป็น `!A || !B` หรือ `!A && !B` ตามกฎ De Morgan เสมอ
   - ใช้ **Early Return (Guard Clauses)** ออกจากฟังก์ชันทันทีเมื่อเงื่อนไขไม่ตรง เพื่อลดระดับความลึกของการซ้อน `if-else`

### 5. **หลีกเลี่ยงการสร้างขยะหน่วยความจำ (Garbage Collection & GC Alloc Zero in Update):**
   - **ห้าม** ใช้คำสั่ง `new` (เช่น `new List<>()`, `new Vector3()`, `new string()`) ภายในฟังก์ชัน `Update()` หรือ `FixedUpdate()`
   - ให้สร้างและจัดสรรหน่วยความจำใน `Awake()` หรือ `Start()` แล้วนำกลับมาใช้ซ้ำ (Cache & Reuse)

### 6. **ห้ามใช้ `FindGameObjectsWithTag` หรือ `GetComponent` ซ้ำๆ ใน `Update()`:**
   - **ห้าม** เรียกใช้ `GameObject.Find`, `GameObject.FindGameObjectsWithTag`, หรือ `GetComponent<T>()` ใน `Update()` เพราะกิน CPU มหาศาล
   - ให้ Cache Reference ไว้ในตัวแปร Member ตอน `Awake()` หรือ `Start()` เสมอ

### 7. **Event Subscription & Memory Leak Prevention:**
   - ทุกครั้งที่มีการ Subscribe Event (เช่น `gameInput.OnInteractAction += ...`) ต้องตรวจสอบการ Unsubscribe ใน `OnDestroy()` หรือ `OnDisable()` เมื่อจำเป็น เพื่อป้องกันการเกิด Memory Leak หรือการเรียก Callback บน Object ที่ถูกทำลายไปแล้ว

### 8. **Safe C# Event Invocation (`?.Invoke`):**
   - ทุกครั้งที่มีการเรียกใช้อีเวนต์ ต้องใช้ Safe Null Navigation `?.Invoke()` เสมอ เพื่อป้องกัน `NullReferenceException` เมื่อไม่มีคลาสใดมาดักฟัง:
     ```csharp
     OnStateChanged?.Invoke(this, EventArgs.Empty);
     ```

### 9. **C# Naming Conventions มาตรฐาน Unity:**
   - **PascalCase:** ชื่อคลาส, อินเทอร์เฟซ (`I...`), ฟังก์ชัน, Property, และ C# Events (เช่น `KitchenGameManager`, `GetMovementVectorNormalized`, `OnRecipeSpawned`)
   - **camelCase:** ตัวแปร Local และพารามิเตอร์ของฟังก์ชัน (เช่น `inputVector`, `plateKitchenObject`)
   - **camelCase พร้อม `[SerializeField] private`:** ตัวแปรที่เปิดให้ตั้งค่าผ่าน Inspector (เช่น `[SerializeField] private float moveSpeed = 8f;`)
   - **UPPER_CASE / PascalCase:** สำหรับค่าคงที่ (`const`)

### 10. **Physics & Raycast LayerMask Optimization:**
   - เมื่อทำการยิง Raycast, CapsuleCast หรือ BoxCast ต้องระบุ `LayerMask` เสมอ (เช่น `countersLayerMask`) เพื่อป้องกันไม่ให้ Physics Engine ตรวจจับ Colliders ที่ไม่เกี่ยวข้อง ซึ่งทำให้เกมกระตุก

### 11. **Unity New Input System Best Practices:**
   - จัดการอินพุตของผู้เล่นผ่านคลาสส่วนกลาง `GameInput.cs` และส่งออกเป็น C# Events ห้ามเขียนดักจับปุ่มกระจัดกระจายในหลายๆ คลาส

### 12. **Beginner-Friendly & Clean Code:**
   - เขียนโค้ดให้อ่านง่าย มีโครงสร้างชัดเจน คอมเมนต์อธิบายตรรกะที่สำคัญเป็นภาษาไทย และหลีกเลี่ยงการเขียนโค้ดที่ซับซ้อนเกินความจำเป็น

### 13. **Do Not Delete or Modify Existing Code Unnecessarily:**
   - ห้ามลบหรือแก้ไขฟังก์ชันการทำงานเดิมที่มีอยู่แล้วในโปรเจกต์โดยไม่ได้รับการร้องขอจากผู้ใช้

### 14. **ห้ามแก้ไขฐานข้อมูลด้วยตนเอง (No Direct DB Writes):**
   - หากในอนาคตมีการเชื่อมต่อระบบ Backend / Database ห้ามรันสคริปต์แก้ไขฐานข้อมูลเองเด็ดขาด ให้เตรียมโค้ด SQL หรือ API Specs ให้ผู้ใช้เป็นคนดำเนินการเท่านั้น

### 15. **ห้ามใช้คำสั่ง Git Commit หรือ Git Push (No Auto-Commits):**
   - ห้ามรันคำสั่ง `git push` หรือ `git commit` ด้วยตนเองเด็ดขาด ผู้ใช้จะเป็นคนควบคุมและตรวจสอบโค้ดก่อน Commit ขึ้น GitHub เองเสมอ