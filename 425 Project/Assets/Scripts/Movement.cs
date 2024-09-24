using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public MouseLook cameraController;
    public float walkSpeed = 8f;
    public float sprintSpeed = 14f;
    public float airControl = 0.5f;
    public float maxVelocityChange = 10f;

    public float jumpHeight = 5f;

    public float floorStick = 3;

    private Vector2 input; 
    private Rigidbody rb;

    private bool sprinting;

    private bool jumping;

    private bool recentJump;
    private bool grounded;

    private bool inPortal1 = false;
    private bool inPortal2 = false;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        input.Normalize();
        sprinting = Input.GetButton("Sprint");
        jumping = Input.GetButton("Jump");
    }

    /*
    private void OnTriggerStay(Collider other){
        grounded = true; 
    }
    */
    private bool IsGrounded() {
        RaycastHit hit;
        float rayLength = 1.2f; // Adjust based on your character's size
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayLength)) {
            return true;
        }
        return false;
    }

    private bool IsHovering() {
        RaycastHit hit;
        float rayLength = 1.3f; // Adjust based on your character's size
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayLength)) {
            return true;
        }
        return false;
    }

    void FixedUpdate(){
        if(IsGrounded()){
            if(jumping && !recentJump){
                rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
                StartCoroutine(RecentJump());
            }
            
        }else if(IsHovering() && !recentJump){
            rb.velocity = new Vector3(rb.velocity.x, -floorStick, rb.velocity.z);
            transform.position -= new Vector3(0, 0.1f, 0);
        }
            
        if(input.magnitude > 0.5f){
            rb.AddForce(CalculateMovement(sprinting ? (grounded ? sprintSpeed : (sprintSpeed * airControl)) : (grounded ? walkSpeed : (walkSpeed * airControl))), ForceMode.VelocityChange);
        }else{
            var velocity1 = rb.velocity;
            velocity1 = new Vector3(velocity1.x * 0.2f * Time.fixedDeltaTime, velocity1.y, velocity1.z * 0.2f * Time.fixedDeltaTime);
            rb.velocity = velocity1;
        }
        

        /*grounded = false;*/
        
    }

    private Vector3 CalculateMovement(float speed){
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity);  

        targetVelocity*= speed;

        Vector3 velocity = rb.velocity; 

        if(input.magnitude > 0.5f){
            Vector3 velocityChange = targetVelocity - velocity;

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

            velocityChange.y = 0;

            return velocityChange;
        }else{
            return new Vector3();
        }
    }

    private IEnumerator RecentJump(){
        recentJump = true;
        yield return new WaitForSeconds(0.5f);
        recentJump = false;
        yield break;
    }

}