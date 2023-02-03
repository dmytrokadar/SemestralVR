using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletableObject : MonoBehaviour
{
    void Delete()
    {
        Destroy(gameObject);
    }
}
