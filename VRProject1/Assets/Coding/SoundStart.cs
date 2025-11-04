using System;
using UnityEngine;

public class SoundStart : MonoBehaviour
{
    [SerializeField] private GameObject Sound;
    [SerializeField] private GameObject SoundObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Sound.SetActive(true);
        SoundObject.SetActive(false);
    }
}

