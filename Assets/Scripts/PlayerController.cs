using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float walkSpeed = 10;
	
	public CircleCollider2D footCollider;
	
	public AudioSource footStep1;
	public AudioSource footStep2;
	public AudioSource thud1;
	public AudioSource thud2;
	
	// Footstep generator (hacky!)	
	public GameObject leftFoot;
	public GameObject rightFoot;
	Vector3 lastLeftFootPos = Vector3.zero;
	Vector3 lastRightFootPos = Vector3.zero;
	Vector3 lastLeftFootVel = Vector3.zero;
	Vector3 lastRightFootVel = Vector3.zero;
	
	Vector3 leftFootOffset = Vector3.zero;
	Vector3 rightFootOffset = Vector3.zero;
	bool leftTriggered = false;
	bool rightTriggered = false;
	
	bool firstFallHit = true;
	

	
	GameObject model;
	public bool isGrounded = false;
	
	bool tryJump = false;
	
	float horizontalSpeed;

	// Use this for initialization
	void Start () {
		model = transform.FindChild("Animation_firstHuman_02").gameObject;
	
	}
	
	// Update is called once per frame
	void Update () {
		if (GameMode.singleton.isEndOfLevel){
			horizontalSpeed = 0;
			
			return;	
		}
		horizontalSpeed = walkSpeed * Input.GetAxis("Horizontal");
		if (Mathf.Abs (horizontalSpeed) > 0.01f && !GameMode.singleton.isEditingCircuit){
			if (horizontalSpeed > 0){
				model.transform.rotation = Quaternion.Euler(0, 90, 0);
			}
			else{
				model.transform.rotation = Quaternion.Euler(0, -90, 0);
			}
		}
		
		tryJump = Input.GetKey(KeyCode.Space);
		
		
		ProcessFootsteps();
		
		
	}
	
	void FixedUpdate () {
//		Debug.Log("FixedUpdate: " + Time.fixedTime);
		model.GetComponent<Animator>().SetFloat("speed", Mathf.Abs (GetComponent<Rigidbody2D>().velocity.x));
		
		if (!GameMode.singleton.isEditingCircuit){
			Vector3 newVel = GetComponent<Rigidbody2D>().velocity;
			newVel.x = horizontalSpeed;
			GetComponent<Rigidbody2D>().velocity = newVel;
			
		}
		model.GetComponent<Animator>().SetBool ("isGrounded", isGrounded);
		
		
		if (tryJump && isGrounded && !GameMode.singleton.isEditingCircuit){
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 2.5f), ForceMode2D.Impulse);
		}
		
			
		model.GetComponent<Animator>().SetBool("isActioning", GameMode.singleton.isEditingCircuit);

		
		// Reset (so it can be set asgain by any collision messages)
		isGrounded = false;
	}
	
	bool TestForGround(Collision2D collision){
	
		Vector2 upDir = new Vector3(0, 	1);
		foreach (ContactPoint2D contactPoint in collision.contacts){
			if (contactPoint.otherCollider != footCollider) continue;
			
			Vector2 collisionNormal = contactPoint.normal;
//			Vector2 startPos = contactPoint.point;
//			Vector2 endPos =  startPos + collisionNormal;

			float dotResult = Vector2.Dot(collisionNormal.normalized, upDir);
//			Debug.Log("Dot = " + dotResult);
			if (dotResult > 0.8f){
				return true;
//				Debug.DrawLine(startPos, endPos, Color.green);
			}
//			else{
//				Debug.DrawLine(startPos, endPos, Color.red);
//				
//			}
			
		}
		return false;
		
		
	}
	
	bool TestForTerminal(Collision2D collision){
		if (collision.gameObject.GetComponent<Terminal>() != null){
			foreach (ContactPoint2D contactPoint in collision.contacts){
				if (contactPoint.otherCollider != footCollider) continue;
				
				Vector2 collisionNormal = contactPoint.normal;
				if (Mathf.Abs (collisionNormal.y) < 0.1f){
					return true;
				}
			}
		
		}
		return false;
	}
	
	void OnCollisionEnter2D(Collision2D collision){
		//Debug.Log("OnCollisionEnter2D: " + col.collider.gameObject.name);
		if (TestForGround(collision)){
			isGrounded = true;
			if (!firstFallHit){
				thud1.Play ();
			}
			firstFallHit = false;
		}
		
	}
	
	void OnCollisionStay2D(Collision2D collision){
		//Debug.Log("OnCollisionStay2D: " + col.collider.gameObject.name);
		if (TestForGround(collision)){
			isGrounded = true;
		}
		
	}
	
	void OnGUI2(){

		float lineHeight = 20.0f;
		int lineNum = 0;
		if (isGrounded){
			if (rightFootOffset.y < 0.1f){
				GUI.Label(new Rect(10, lineHeight * lineNum++, Screen.width, lineHeight), "rightFootOffset.y: " + rightFootOffset.y);
				GUI.Label(new Rect(10, lineHeight * lineNum++, Screen.width, lineHeight), "lastRightFootVel.y: " + lastRightFootVel.y);
			}
			else{
				lineNum += 2;
			}
			
			
			if (leftFootOffset.y < 0.1f){
				GUI.Label(new Rect(10, lineHeight * lineNum++, Screen.width, lineHeight), "lastLeftFootVel.y: " + lastLeftFootVel.y);
				GUI.Label(new Rect(10, lineHeight * lineNum++, Screen.width, lineHeight), "leftFootOffset.y: " + leftFootOffset.y);
			}
		}
		

	}
	
	public void PlayFootSound(){
		footStep1.Play();
		footStep1.pitch = Random.Range(0.75f, 1.5f);
		
	}
	
	// If the feet were going down and are now going up and we are walking, then do a footstep sound
	void ProcessFootsteps(){
	
		// Get current position and instantanouse velocity
		Vector3 leftFootPos = leftFoot.transform.position;
		Vector3 rightFootPos = rightFoot.transform.position;
		Vector3 leftFootVel = leftFootPos - lastLeftFootPos;
		Vector3 rightFootVel = rightFootPos - lastRightFootPos;
				
		lastLeftFootPos = leftFootPos;
		lastRightFootPos = rightFootPos;
		lastLeftFootVel = leftFootVel;
		lastRightFootVel = rightFootVel;
		
		leftFootOffset = leftFootPos - transform.position;
		rightFootOffset = rightFootPos - transform.position;
		
		if (isGrounded && model.GetComponent<Animator>().GetFloat("speed") > 0.1f){
			if (rightFootOffset.y < 0.1f){
				if (!rightTriggered){
					PlayFootSound ();
					
				}
				rightTriggered = true;
			}
			else{
				rightTriggered = false;
			}

			if (leftFootOffset.y < 0.1f){

				if (!leftTriggered){
					PlayFootSound ();
					
				}
				leftTriggered = true;

				
			}
			else{
				leftTriggered = false;
			}
		}
	}
	
//	void OnTriggerEnter2D(Collider collider){
//		if (collider.gameObject.tag == "Player"){
//			Debug.Log ("Exit Level");
//		}
//	}
}
