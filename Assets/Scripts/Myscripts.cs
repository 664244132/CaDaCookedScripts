using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Myscripts : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI message;
    public TextMeshProUGUI display_count;

    int all_items = 0;
    int current_items = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panel.gameObject.SetActive(false);
        message.gameObject.SetActive(false);
        all_items = GameObject.FindGameObjectsWithTag("Items").Length;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("UPDATE STARTING.....");
        display_count.text = "Find your crew and take supplie : "+current_items+"/"+all_items; //นับจำนวนไอเท็มที่เก็บได้
        if (current_items == all_items)
        {
            message.gameObject.SetActive (true);
            message.text = "Misson DONE!";
        }
    }

    private void OnCollisionEnter(Collision obj)
    {
        if(obj.gameObject.CompareTag("Electric"))
        {
            Debug.Log("Player Touched The DANGER!");
            panel.SetActive(true);
        }
    }

    //private void OnCollisionStay(Collision obj)
    //{
    //    if (obj.gameObject.CompareTag("Electric"))
    //    {
    //        Debug.Log("Player Stay The DANGER!");
    //    }
    //}

    private void OnCollisionExit(Collision obj)
    {
        if (obj.gameObject.CompareTag("Electric"))
        {
            Debug.Log("Player Leave from The DANGER!");
            panel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider obj)
    {
        if(obj.gameObject.CompareTag("Items"))
        {
            Debug.Log("Trigger Item");
            message.gameObject.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider obj)
    {
        if (obj.gameObject.CompareTag("Items"))
        {
            Debug.Log("Stay Item");
            if (Keyboard.current.eKey.isPressed) //กด E เก็บของ
            {
                Destroy(obj.gameObject); //ทำลายกล่องที่เก็บไป
                message.gameObject.SetActive(false); //ลบข้อความไปด้วย
                current_items++;
            }
        }
    }

    private void OnTriggerExit(Collider obj)
    {
        if (obj.gameObject.CompareTag("Items"))
        {
            Debug.Log("Leave Item");
            message.gameObject.SetActive(false);
        }
    }
}
