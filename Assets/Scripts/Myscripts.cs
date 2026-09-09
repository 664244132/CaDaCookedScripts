using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// สคริปต์ทดลองเก็บไอเทมและตรวจจับโซนอันตราย (Item Pickup & Danger Zone Prototype)
/// </summary>
public class Myscripts : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI message;
    [FormerlySerializedAs("display_count")]
    [SerializeField] private TextMeshProUGUI displayCount;

    [Header("Item Tracking")]
    private int allItems = 0;
    private int currentItems = 0;

    // Properties สำหรับความเข้ากันได้ย้อนหลัง (Backward Compatibility)
    public GameObject Panel => panel;
    public TextMeshProUGUI Message => message;
    public TextMeshProUGUI DisplayCount => displayCount;
    public TextMeshProUGUI display_count => displayCount;
    public int AllItems => allItems;
    public int CurrentItems => currentItems;
    public int all_items => allItems;
    public int current_items => currentItems;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (message != null) message.gameObject.SetActive(false);
        allItems = GameObject.FindGameObjectsWithTag("Items").Length;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (displayCount != null)
        {
            displayCount.text = $"Find your crew and take supplies : {currentItems}/{allItems}";
        }

        if (allItems > 0 && currentItems >= allItems)
        {
            if (message != null)
            {
                message.gameObject.SetActive(true);
                message.text = "Mission DONE!";
            }
        }
    }

    private void OnCollisionEnter(Collision obj)
    {
        if (obj == null || obj.gameObject == null) return;

        if (obj.gameObject.CompareTag("Electric"))
        {
            Debug.Log("Player Touched The DANGER!");
            if (panel != null) panel.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision obj)
    {
        if (obj == null || obj.gameObject == null) return;

        if (obj.gameObject.CompareTag("Electric"))
        {
            Debug.Log("Player Leave from The DANGER!");
            if (panel != null) panel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider obj)
    {
        if (obj == null || obj.gameObject == null) return;

        if (obj.gameObject.CompareTag("Items"))
        {
            Debug.Log("Trigger Item");
            if (message != null) message.gameObject.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider obj)
    {
        // Early Return ป้องกันการซ้อนเงื่อนไขลึก (De Morgan's Laws & Early Return Pattern)
        if (obj == null || obj.gameObject == null) return;
        if (!obj.gameObject.CompareTag("Items")) return;

        // กด E เก็บของ พร้อมเช็ค Null Safety
        if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
        {
            Destroy(obj.gameObject); // ทำลายกล่องที่เก็บไป
            if (message != null) message.gameObject.SetActive(false); // ลบข้อความไปด้วย
            currentItems++;
            UpdateDisplay();
        }
    }

    private void OnTriggerExit(Collider obj)
    {
        if (obj == null || obj.gameObject == null) return;

        if (obj.gameObject.CompareTag("Items"))
        {
            Debug.Log("Leave Item");
            if (message != null) message.gameObject.SetActive(false);
        }
    }
}
