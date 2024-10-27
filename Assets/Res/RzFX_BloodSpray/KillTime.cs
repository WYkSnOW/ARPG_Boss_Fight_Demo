using UnityEngine;

public class KillTime : MonoBehaviour {
	public float KillDelayTime = 1;

	private void Awake () {
		Destroy (gameObject, KillDelayTime);
	}
}