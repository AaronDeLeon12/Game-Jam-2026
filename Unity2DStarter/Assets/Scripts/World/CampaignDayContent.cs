using UnityEngine;
// Put this on an always-active parent; drag day-specific scene objects into Content.
[DefaultExecutionOrder(-500)]
public class CampaignDayContent : MonoBehaviour
{
    [Min(1)] public int firstDay = 1;
    [Min(1)] public int lastDay = 99;
    public GameObject[] content;
    private void Awake() { DayManager.EnsureExists(); Apply(DayManager.Instance.CurrentDay); }
    private void OnEnable() { DayManager.DayChanged += Apply; }
    private void Start() { DayManager.EnsureExists(); Apply(DayManager.Instance.CurrentDay); }
    private void OnDisable() { DayManager.DayChanged -= Apply; }
    private void Apply(int day)
    {
        foreach (var item in content) if (item != null) item.SetActive(day >= firstDay && day <= lastDay);
    }
}
