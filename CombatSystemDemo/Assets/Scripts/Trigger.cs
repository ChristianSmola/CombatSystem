using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Trigger : MonoBehaviour
{
    public class Detection
    {
        public GameObject GO;
        public Collider col;

        public Detection(GameObject obj, Collider C)
        {
            GO = obj;
            col = C;
        }
    }

    public List<Detection> DetectedObjects = new List<Detection>();

    private void OnTriggerEnter(Collider other)
    {
        if (DetectedObjects.Where(O => O.col == other).ToList().Count == 0)
        {
            if (other.gameObject != this.gameObject && other.gameObject != this.gameObject.transform.parent.gameObject && other.gameObject.name != this.gameObject.name)
                DetectedObjects.Add(new Detection(other.gameObject, other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (DetectedObjects.Where(O => O.col == other).ToList().Count != 0)
        {
            DetectedObjects.Remove(DetectedObjects.Where(O => O.col == other).ToList()[0]);
        }
    }
}
