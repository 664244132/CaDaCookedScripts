using TMPro;
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
        if (panel != null) panel.SetActive(false);
        if (message != null) message.gameObject.SetActive(false);
        all_items = GameObject.FindGameObjectsWithTag("Items").Length;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (display_count != null)
        {
            display_count.text = "Find your crew and take supplie : " + current_items + "/" + all_items;
        }

        if (all_items > 0 && current_items >= all_items)
        {
            if (message != null)
            {
                message.gameObject.SetActive(true);
                message.text = "Misson DONE!";
            }
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
            if (Keyboard.current != null && Keyboard.current.eKey.isPressed) //กด E เก็บของ พร้อมเช็ค Null Safety
            {
                Destroy(obj.gameObject); //ทำลายกล่องที่เก็บไป
                if (message != null) message.gameObject.SetActive(false); //ลบข้อความไปด้วย
                current_items++;
                UpdateDisplay();
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
