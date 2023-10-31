using UnityEngine;

public class CarSpawner : MonoBehaviourPlus
{
	public GameObject prefab;
	public int amount;
	public float distance, delay, speed;

	void Start()
	{
		Position position = GetPosition();
		for (int i = 0; i < amount; i++)
		{
			Lerp(Instantiate(prefab, transform.position + (delay * i * speed * transform.forward), transform.rotation, transform), position);
		}
	}

	void Lerp(GameObject g, Position position)
	{
		StartCoroutine(MoveToPos(position, speed, g.transform, () => { g.transform.position = transform.position; Lerp(g, position); }));
	}

	Position GetPosition()
	{
		return new Position(transform.position + (transform.forward * distance), transform.eulerAngles);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, 1);
		Gizmos.color = Color.yellow;
		for (int i = 0; i < amount; i++) Gizmos.DrawCube(transform.position + (delay * i * speed * transform.forward), Vector3.one / 2);
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(GetPosition(), 1);
	}
}
