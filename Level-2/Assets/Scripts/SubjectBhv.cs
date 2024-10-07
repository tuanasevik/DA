using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubjectBhv : MonoBehaviour {

    public UnityPharus.UnityPharusManager.Subject Subject;

    void Start () {
        transform.position = Subject.position;
    }

    void Update () {
        transform.position = Subject.position;
        GetComponentInChildren<TMPro.TextMeshPro>().text = "sub" + Subject.id;
    }
    
}
