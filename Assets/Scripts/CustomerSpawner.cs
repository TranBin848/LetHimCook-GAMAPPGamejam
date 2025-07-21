using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public List<GameObject> customerPrefabs;
    public Transform spawnPoint;
    public MenuList menuList;

    public List<Chair> chairs = new List<Chair>();

    public float spawnInterval = 5f;

    void Start()
    {
        InvokeRepeating("SpawnCustomer", 1f, spawnInterval);
    }

    void SpawnCustomer()
    {
        Chair targetChair = FindEmptyChair();
        if (targetChair != null)
        {
            if (customerPrefabs.Count == 0)
            {
                Debug.LogWarning("Không có prefab nào trong customerPrefabs!");
                return;
            }

            // Random prefab
            int index = Random.Range(0, customerPrefabs.Count);
            GameObject prefab = customerPrefabs[index];

            GameObject customer = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            Customer customerScript = customer.GetComponent<Customer>();

            customerScript.menuList = menuList;
            customerScript.chairTarget = targetChair.transform;
            customerScript.chairScript = targetChair;
            customerScript.spawnPoint = spawnPoint;

            targetChair.isOccupied = true;
            AudioManager.Instance.playitemSFX("DoorBell");
        }
        else
        {
            Debug.Log("Hết ghế trống!");
        }
    }

    Chair FindEmptyChair()
    {
        foreach (Chair chair in chairs)
        {
            if (!chair.isOccupied)
                return chair;
        }
        return null;
    }
}
