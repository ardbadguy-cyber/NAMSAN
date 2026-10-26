using UnityEngine;
using UnityEngine.InputSystem;

namespace Namsan
{
    [RequireComponent(typeof(CharacterController))]
    public class B1Explorer : MonoBehaviour
    {
        public Camera eye;
        CharacterController controller;
        float pitch, fall;
        string prompt;
        void Start() { controller=GetComponent<CharacterController>(); Capture(true); }
        void Capture(bool value) { Cursor.lockState=value?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=!value; }
        void Update()
        {
            var k=Keyboard.current; var m=Mouse.current;
            if(k==null || m==null || eye==null) return;
            if(k.escapeKey.wasPressedThisFrame) Capture(false);
            if(Cursor.lockState!=CursorLockMode.Locked) { if(m.leftButton.wasPressedThisFrame) Capture(true); return; }
            var look=m.delta.ReadValue()*.085f;
            transform.Rotate(0,look.x,0); pitch=Mathf.Clamp(pitch-look.y,-80,80); eye.transform.localRotation=Quaternion.Euler(pitch,0,0);
            Vector2 input=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
            input=Vector2.ClampMagnitude(input,1);
            if(controller.isGrounded && fall<0) fall=-2;
            fall+=Physics.gravity.y*Time.deltaTime;
            controller.Move((transform.right*input.x*2.8f+transform.forward*input.y*2.8f+Vector3.up*fall)*Time.deltaTime);
            prompt="";
            if(Physics.Raycast(eye.transform.position,eye.transform.forward,out var hit,2.6f))
            {
                var door=hit.collider.GetComponentInParent<B1Door>();
                if(door!=null) { prompt="E : Open / close door"; if(k.eKey.wasPressedThisFrame) door.Toggle(); }
            }
        }
        void OnGUI()
        {
            GUI.Label(new Rect(18,16,900,30),"NAMSAN B1 | WASD: Move   Mouse: Look   E: Door   Esc: Release cursor");
            GUI.Label(new Rect(Screen.width/2-5,Screen.height/2-12,20,24),"+");
            GUI.Label(new Rect(Screen.width/2-110,Screen.height/2+30,300,30),prompt);
        }
        void OnDisable() { Capture(false); }
    }
}
