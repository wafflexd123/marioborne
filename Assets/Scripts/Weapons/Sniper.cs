using UnityEngine;

public class Sniper : Gun
{
    protected LineRenderer line;
    private Color color = Color.red;

    protected override void Start()
    {
        base.Start();
        line = GetComponent<LineRenderer>();
        line.widthMultiplier = 0.05f;
        line.startColor = color;
        line.endColor = color;
        line.enabled = false;
    }

    protected override void OnPickup()
    {
        base.OnPickup();
        if(wielder is AIController)
        {
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
        line.enabled = false;
    }

    protected override void Shoot()
    {
        Instantiate(bulletPrefab, firePosition.position, Quaternion.identity).Initialise(bulletSpeed, DirectionWithSpread(ammo.maxSpread), wielder, ammo.color, true);
    }
}
