# 🛡️ คู่มือมาตรฐานความปลอดภัยและความถูกต้องของโค้ด (CaDaCook Security & Integrity Policy)

เอกสารนี้ระบุมาตรฐานความปลอดภัย ความสมบูรณ์ของข้อมูล และแนวทางการตรวจสอบโค้ด (Audit Guide) ของโปรเจกต์เกม **CaDaCook** เพื่อให้ **AI Agents** และนักพัฒนาใช้เป็นแนวทางปฏิบัติอย่างเคร่งครัด

---

## 🔒 1. นโยบายความปลอดภัยของเกม (Game Client & Data Security)

1. **Save Data & Score Integrity:**
   - การบันทึกข้อมูลคะแนนสถิติของผู้เล่น (เช่น `successfulRecipesAmount`, การปลดล็อกด่าน) หากบันทึกลงเครื่อง (Local Storage / PlayerPrefs) จะต้องมีการตรวจสอบความสมเหตุสมผลของค่าตัวเลข ไม่ปล่อยให้แก้ไขตัวเลขติดลบหรือค่าเกินจริง
   - หากมีการเชื่อมต่อไปยังระบบ Cloud (เช่น Unity Cloud Save หรือ REST API หลังบ้าน) ข้อมูลคะแนนจะต้องผ่านการตรวจสอบความถูกต้องก่อนบันทึกเสมอ
2. **API Keys & Secret Credentials Safety:**
   - **ห้าม** ฝัง API Key, Secret Tokens หรือ Private Key ลงในซอร์สโค้ด C# หรือคอมมิตขึ้น Version Control เด็ดขาด
   - ข้อมูลที่เป็นความลับต้องถูกจัดเก็บผ่าน Environment Variables หรือการตั้งค่าบน Unity Gaming Services Dashboard
3. **Safe Asset & Prefab Loading:**
   - การ Spawn Prefab หรือโหลด ScriptableObjects ต้องมีการตรวจสอบค่า Null เสมอ เพื่อป้องกันข้อผิดพลาดที่ทำให้ตัวเกมค้างหรือ Crash

---

## 🚨 2. ข้อจำกัดและกฎความปลอดภัยที่ต้องปฏิบัติตาม (Hard Constraints)

1. **ห้ามทำการแก้ไขฐานข้อมูลด้วยตนเอง (No Direct DB Writes):**
   - หากในอนาคตมีระบบจัดเก็บข้อมูลบนฐานข้อมูล (เช่น SQL Database / Supabase) **ห้าม** AI Agent รันคำสั่ง SQL แก้ไขตารางเองเด็ดขาด ให้เตรียมสคริปต์ SQL ให้ผู้ใช้เป็นผู้นำไปรันเองเท่านั้น
2. **ห้ามใช้คำสั่ง Git Commit หรือ Git Push (No Auto-Commits):**
   - **ห้าม** รันคำสั่ง `git commit` หรือ `git push` ใน Terminal หรือผ่านเครื่องมือใดๆ เด็ดขาด ผู้ใช้จะเป็นคนตรวจสอบและ Commit ขึ้น GitHub เองเสมอ
3. **ห้ามติดตั้งหรือดึงไลบรารีใหม่โดยไม่ได้รับอนุญาต:**
   - การเพิ่ม Dependencies หรือ Assets ใหม่ ต้องได้รับความยินยอมจากผู้ใช้ก่อนเสมอ

---

## 📋 3. AI Agent Automated Code & Security Audit Checklist

ทุกครั้งที่ AI Agent เข้ามาแก้ไข เพิ่มเติม หรือตรวจสอบซอร์สโค้ดในโปรเจกต์ **บังคับต้องผ่านการตรวจสอบตาม Checklist 10 ข้อนี้:**

- [ ] **1. Safe Event Invocation:** มีการเรียกใช้ C# Events ผ่าน `?.Invoke(this, ...)` เพื่อป้องกัน `NullReferenceException` หรือไม่?
- [ ] **2. Memory Leak Prevention:** มีการ Unsubscribe Event (`-=`) ใน `OnDestroy()` หรือ `OnDisable()` เมื่อคอมโพเนนต์ถูกทำลายหรือไม่?
- [ ] **3. Zero GC Alloc in `Update()`:** ปราศจากการใช้คำสั่ง `new` (เช่น `new List()`, `new string()`) ภายในลูป `Update()` หรือไม่?
- [ ] **4. LayerMask Optimization:** ทุกคำสั่ง `Physics.Raycast` หรือ `Physics.CapsuleCast` มีการระบุ `LayerMask` เฉพาะเจาะจงหรือไม่?
- [ ] **5. De Morgan's Laws & Early Return:** ตรวจสอบโครงสร้างเงื่อนไขว่าไม่มี `!(A && B)` หรือ `!(A || B)` และใช้ Guard Clauses เพื่อลด Nesting แล้วหรือไม่?
- [ ] **6. ScriptableObject Integrity:** ข้อมูลสูตรอาหารและวัตถุดิบถูกเก็บใน `ScriptableObject` และไม่มีการ Hardcode ค่าลงในคลาส Logic ใช่หรือไม่?
- [ ] **7. No Hardcoded Credentials:** ตรวจสอบว่าไม่มี API Key หรือ Token ลับถูกเขียนลงใน C# Script?
- [ ] **8. Decoupled Presentation:** โค้ด Game Logic ไม่ได้ผูกติดโดยตรงกับ Animator หรือ AudioSource แต่สื่อสารผ่าน C# Events ใช่หรือไม่?
- [ ] **9. No Auto Git Commands:** ไม่มีการเรียกใช้ `git push` หรือ `git commit` ในการทำงาน?
- [ ] **10. No Direct Database Modification:** ไม่มีการรันคำสั่งแก้ไขฐานข้อมูลโดยตรงโดยไม่ได้รับอนุญาตจากผู้ใช้?
