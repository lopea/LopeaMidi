using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class moveForward : MonoBehaviour
{
    [SerializeField] private float speed;
    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime);

    }
}
