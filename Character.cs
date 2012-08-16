using UnityEngine;
using System.Collections;

[Binarizable]
public class Character : MonoBehaviour {
	
	[Binarizable(1)]
	public Vector3 position;
	[Binarizable(1)]
	public Vector3 rotation;

	// Use this for initialization
	void Start () {
		position = gameObject.transform.position;
		rotation = gameObject.transform.localEulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
