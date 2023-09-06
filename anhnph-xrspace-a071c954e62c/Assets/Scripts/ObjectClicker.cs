using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicker : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

            print(UnityEngine.Camera.main);
            print(ray);

            if (Physics.Raycast(ray, out hit, 100000f))
            {
                print("hit");
                GameObject gameObject;
                if (gameObject = hit.transform.GetComponent<GameObject>())
                {
                    print(gameObject.name);
                }
            }
        }
    }
}
