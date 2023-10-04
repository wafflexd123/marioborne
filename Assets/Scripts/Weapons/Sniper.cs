using UnityEngine;

public class Sniper : Gun
{
    protected LineRenderer line;

    protected override void Start()
    {
        base.Start();
        line = GetComponent<LineRenderer>();
        line.widthMultiplier = 0.05f;
        line.enabled = false;
    }

    protected override void OnPickup()
    {
        base.OnPickup();
        if(wielder is AIController)
        {
            wielder.model.sniperLayer = true;
            wielder.model.holdingSniper = true;
            line.enabled = true;
        }
    }

    void FixedUpdate()
    {
        if (wielder is AIController controller)
        {
            Physics.Raycast(firePosition.position, controller.transform.forward, out RaycastHit hit);
            line.SetPositions(new Vector3[] { firePosition.position, hit.point });
        }
    }

    protected override void OnDrop()
    {
        base.OnDrop();
        wielder.model.holdingSniper = false;
        wielder.model.sniperLayer = false;
        line.enabled = false;
    }
}
