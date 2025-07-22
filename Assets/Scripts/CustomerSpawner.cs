using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public List<GameObject> customerPrefabs;
    public Transform spawnPoint;
    public MenuList menuList;

    public List<Chair> chairs = new List<Chair>();

    public float spawnInterval = 5f;

    public Chef chef; // 🔥 Thêm tham chiếu đến Chef

    void Start()
    {
        StartCoroutine(WaitForChefInteraction());
    }

    IEnumerator WaitForChefInteraction()
    {
        // Đợi cho đến khi chef.hasInteracted == true
        yield return new WaitUntil(() => chef.hasInteracted);

        // Đợi thêm 10 giây trước khi spawn
        yield return new WaitForSeconds(8f);

        // Bắt đầu spawn lặp lại
        InvokeRepeating("SpawnCustomer", 0f, spawnInterval);
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
