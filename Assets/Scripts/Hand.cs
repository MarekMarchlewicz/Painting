using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedController))]
public class Hand : MonoBehaviour
{
    [SerializeField]
    private float minDistanceToGrab = 0.2f;

    private DraggableObject[] draggables;

    private DraggableObject draggedObject = null;

    private SteamVR_RenderModel renderModel;

    private void Awake()
    {
        SteamVR_TrackedController controller = GetComponent< SteamVR_TrackedController>();

        controller.TriggerClicked += OnTriggerCliked;
        controller.TriggerUnclicked += OnTriggerUncliked;

        renderModel = GetComponentInChildren<SteamVR_RenderModel>();

        draggables = FindObjectsOfType<DraggableObject>();
    }

    private void OnTriggerCliked(object sender, ClickedEventArgs e)
    {
        float closestDistance = float.MaxValue;
        DraggableObject closestObject = null;

        foreach(DraggableObject draggable in draggables)
        {
            if (!draggable.IsDragged)
            {
                float distance = Vector3.Distance(draggable.transform.position, transform.position);

                if (distance < minDistanceToGrab && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = draggable;
                }
            }
        }

        if(closestObject != null)
        {
            draggedObject = closestObject;

            draggedObject.StartDragging(transform);

            renderModel.gameObject.SetActive(false);
        }
    }

    private void OnTriggerUncliked(object sender, ClickedEventArgs e)
    {
        if(draggedObject != null)
        {
            draggedObject.StopDragging();

            draggedObject = null;

            renderModel.gameObject.SetActive(true);
        }
    }
}
