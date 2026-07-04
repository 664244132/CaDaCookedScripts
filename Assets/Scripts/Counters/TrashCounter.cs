using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class TrashCounter : BaseCounter // ชื่อ counter , BaseCounter คือ counter หลักหรือคลาสเเม่ที่มีหน้าที่เหมือนกันที่เป็น counter
{

    public static event EventHandler OnAnyObjectTrashed;

    public override void Interact(Player player) //ฟังก์ชันทำงานเมื่อผู้เล่นกดปุ่ม
    {
        if (player.HasKitchenObject()) //เช็คว่าเล่นถือของอยู่ไหม
        {
            player.GetKitchenObject().DestroySelf(); //ถ้าใช่ก็ลบของที่ถืออยู่ไป

            OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
        }
    }
}