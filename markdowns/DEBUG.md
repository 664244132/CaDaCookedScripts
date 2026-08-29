# 🛠️ คู่มือการดีแบ๊กและแนวทางการพัฒนาเกมสำหรับ AI Agents (CaDaCook)

คู่มือนี้มีไว้เพื่อให้ AI Agents ทุกตัวที่เข้ามาร่วมพัฒนาโปรเจกต์ **CaDaCook** (เกม Unity 6 C#) ปฏิบัติตามมาตรฐานการทำงาน ความปลอดภัย แนวทางการเขียนโค้ด และขั้นตอนการดีแบ๊กที่ถูกต้องของโปรเจกต์นี้

---

## 🚨 กฎเหล็กด้านความปลอดภัยและข้อจำกัด (Hard Constraints)

1. **ห้ามใช้คำสั่ง Git Commit หรือ Git Push (No Auto-Commits)**
   - ห้ามรันคำสั่ง `git push` หรือ `git commit` ด้วยตนเองเด็ดขาด ผู้ใช้จะเป็นคนคุมการ Commit และ Push ขึ้น GitHub เองเสมอ
2. **ห้ามแก้ไขฐานข้อมูลด้วยตัวเองเด็ดขาด (No Direct DB Writes)**
   - หากมีการเชื่อมต่อระบบ Backend Database ในอนาคต ให้เตรียมโค้ด SQL หรือสคริปต์ให้ผู้ใช้นำไปรันเองเท่านั้น
3. **ห้ามดึงแพ็กเกจใหม่โดยไม่ได้รับอนุญาต (Ask Before Adding Dependencies)**
   - หากจำเป็นต้องติดตั้ง Unity Package ใหม่ผ่าน UPM หรือ NuGet ต้องขออนุญาตผู้ใช้ก่อนเสมอ
4. **เขียนโค้ดที่เป็นมิตรกับผู้เริ่มต้น (Beginner-friendly Code)**
   - เขียนโค้ดให้อ่านง่าย มีโครงสร้างชัดเจน และอธิบายรายละเอียดการเปลี่ยนแปลงเป็นภาษาไทยเสมอ
5. **ปฏิบัติตามกฎ De Morgan's Laws & Early Return**
   - ห้ามเขียน `!(A && B)` หรือ `!(A || B)` ให้แปลงเป็น `!A || !B` หรือ `!A && !B` และใช้ Guard Clauses เพื่อลดระดับความลึกของ if-else
6. **อัปเดตไฟล์ประวัติ `markdowns/LOG.md` ทุกครั้งหลังแก้ไขโค้ด (Mandatory Log Updates)**
   - เมื่อมีการเพิ่มฟีเจอร์หรือแก้ไขบัค ต้องมาบันทึกสรุปใน `LOG.md` เสมอ

---

## 🔄 ขั้นตอนการสืบค้นและแก้ไขบัคใน Unity (Unity Troubleshooting Workflow)

1. **Locate (ค้นหาสคริปต์และคอมโพเนนต์):**
   - ตรวจสอบโฟลเดอร์ให้ถูกจุด (เช่น เคาน์เตอร์อยู่ใน `Assets/Scripts/Counters/`, ระบบ UI อยู่ใน `Assets/Scripts/UI/`, ScriptableObjects อยู่ใน `Assets/Scripts/ScriptableObjects/`)
2. **Analyze (วิเคราะห์หาสาเหตุ):**
   - ตรวจสอบ Unity Console Error Log, Callstack, ค่าใน Inspector, และสถานะของ GameObjects
3. **Propose (เสนอแนวทางแก้ไข):**
   - อธิบายสาเหตุของบัค และเสนอแผนการแก้ไข (Implementation Plan) ให้ผู้ใช้พิจารณาเป็นภาษาไทย
4. **Implement & Edit (ลงมือแก้ไข):**
   - แก้ไขโค้ดเฉพาะจุดที่จำเป็น ไม่ลบหรือแก้ไขฟังก์ชันเดิมที่ไม่เกี่ยวข้อง
5. **Verify (ตรวจสอบความถูกต้อง):**
   - ตรวจสอบว่าโค้ดไม่มี Syntax Error, ตัวแปรและ Event ถูกเรียกใช้อย่างถูกต้อง

---

## ⚡ สรุปวิธีแก้ไขปัญหายอดนิยมในเกม Unity (Common Troubleshooting Matrix)

| ปัญหาที่พบ (Issue) | สาเหตุที่พบบ่อย (Root Cause) | แนวทางแก้ไข (Solution) |
|---|---|---|
| **NullReferenceException ตอนเรียกใช้ Instance** | เรียกใช้ Singleton ก่อนที่ฟังก์ชัน `Awake()` ของคลาสนั้นจะทำงาน | ตรวจสอบ Script Execution Order หรือเปลี่ยนการเรียกใช้ไปไว้ใน `Start()` แทน `Awake()` |
| **กดปุ่ม E หรือ F แล้วตัวละครไม่มีปฏิสัมพันธ์กับเคาน์เตอร์** | 1. เคาน์เตอร์ไม่ได้อยู่บน Layer ที่ระบุใน `countersLayerMask`<br>2. ตัวเคาน์เตอร์ไม่มี Collider<br>3. ทิศทาง Raycast ไม่โดน | ตรวจสอบ Layer ของ Counter Prefab ให้ตรงกับ `countersLayerMask` และตรวจสอบว่ามี BoxCollider / MeshCollider ครบถ้วน |
| **หั่นอาหารไม่ได้ / วัตถุดิบไม่ยอมเปลี่ยนรูป** | ไม่มีสูตร `CuttingRecipeSO` ของวัตถุดิบชิ้นนั้นอยู่ใน `cuttingRecipeSOArray` | สร้าง `CuttingRecipeSO` ใน Project Assets และลากใส่ใน Array ของ `CuttingCounter` ใน Inspector |
| **ทอดเนื้อแล้วเตาไม่ทำงาน / ไม่เปลี่ยนเป็นสุก** | ไม่มี `FryingRecipeSO` สำหรับวัตถุดิบนั้นอยู่ใน `fryingRecipeSOArray` | ตรวจสอบว่าวัตถุดิบมี `FryingRecipeSO` และตั้งค่าเวลา `fryingTimerMax` ถูกต้อง |
| **นำวัตถุดิบใส่จานไม่ได้** | วัตถุดิบไม่ได้ถูกเพิ่มลงใน `validKitchenObjectSOList` ของ `PlateKitchenObject` | เพิ่ม `KitchenObjectSO` ของวัตถุดิบนั้นลงในรายการที่อนุญาตของ Prefab จาน |
| **เสียง Sound Effect ไม่ดัง** | 1. ลืมแนบ `AudioClipRefsSO` ใน Inspector<br>2. Volume ถูกตั้งเป็น 0<br>3. Main Camera ไม่มี AudioListener | ตรวจสอบการลาก ScriptableObject เสียงใส่ `SoundManager` และตรวจสอบ AudioListener บนกล้อง |
| **หลอด Progress Bar หมุนกลับด้านหรือไม่หันหากล้อง** | โหมดใน `LookAtCamera.cs` ไม่ตรงกับมุมกล้อง | ปรับโหมดใน `LookAtCamera` ระหว่าง `CameraForward` หรือ `LookAtInverted` ให้ตรงกับมุมมองกล้อง |
| **ส่งอาหารแล้วขึ้น Failed ตลอดเวลา** | จำนวนหรือชนิดของ `KitchenObjectSO` บนจานไม่ตรงกับสูตรใน `RecipeSO` ครบทุกชิ้น | ตรวจสอบรายการวัตถุดิบใน `RecipeSO` ให้ตรงกับสิ่งที่อยู่บนจานแบบ 1:1 |
| **เกมไม่รับ Input จากคีย์บอร์ดหรือคอนโทรลเลอร์** | `playerInputActions.Player.Enable()` ไม่ได้ถูกเรียกใช้งาน | ตรวจสอบฟังก์ชัน `Awake()` ใน `GameInput.cs` ว่ามีการเรียก `.Enable()` แล้ว |
| **Event ถูกเรียกซ้ำซ้อน 2 ครั้ง** | มีการ Subscribe Event ซ้ำในการเปลี่ยน Scene หรือไม่ได้ Unsubscribe ตอน Object ถูกทำลาย | ตรวจสอบการเพิ่ม `-=` ใน `OnDestroy()` เพื่อปลด Event เสมอ |