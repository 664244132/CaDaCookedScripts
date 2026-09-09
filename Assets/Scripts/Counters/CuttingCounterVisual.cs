using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    private const string CUT = "Cut";

    [SerializeField] private CuttingCounter cuttingCounter;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (cuttingCounter != null)
        {
            cuttingCounter.OnCut += CuttingCounter_OnCut;
        }
    }

    private void OnDestroy()
    {
        if (cuttingCounter != null)
        {
            cuttingCounter.OnCut -= CuttingCounter_OnCut;
        }
    }

    private void CuttingCounter_OnCut(object sender, System.EventArgs e)
    {
        animator.SetTrigger(CUT);
    }
}
