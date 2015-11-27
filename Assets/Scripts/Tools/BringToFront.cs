using UnityEngine;
using System.Collections;

public class BringToFront : MonoBehaviour {

	void OnEnable()
    {
        transform.SetAsLastSibling(); // this will make the object draw last
    }
}
