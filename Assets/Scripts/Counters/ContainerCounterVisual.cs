using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour
{
    private const string OPEN_CLOSE = "OpenClose";

    [SerializeField] private ContainerCounter containerCounter;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (containerCounter != null)
        {
            containerCounter.OnPlayerGrabbedObject += ContainerCounter_OnPlayerGrabbedObject;
        }
    }

    private void OnDestroy()
    {
        if (containerCounter != null)
        {
            containerCounter.OnPlayerGrabbedObject -= ContainerCounter_OnPlayerGrabbedObject;
        }
    }

    private void ContainerCounter_OnPlayerGrabbedObject(object sender, System.EventArgs e)
    {
        animator.SetTrigger(OPEN_CLOSE);
    }
}
