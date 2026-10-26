using UnityEngine;
using UnityEngine.InputSystem;

namespace Namsan
{
    [RequireComponent(typeof(CharacterController))]
    public class B1Explorer : MonoBehaviour
    {
        public Camera eye;
        [Header("Movement")]
        [Min(.1f)] public float walkSpeed=2.8f;
        [Min(.1f)] public float runSpeed=5f;
        [Min(.1f)] public float crouchSpeed=1.4f;
        [Min(.6f)] public float crouchHeight=1.1f;
        [Header("Running camera motion (set amplitude to 0 to disable)")]
        [Range(0,.1f)] public float runBobAmplitude=.035f;
        [Range(0,3)] public float runRollDegrees=.65f;
        [Min(.1f)] public float runBobFrequency=2.8f;

        CharacterController controller;
        B1Lighter lighter;
        readonly Collider[] standingHits=new Collider[32];
        float pitch, fall, standingHeight, standingStep, bobPhase;
        Vector3 standingCenter, eyeOrigin;
        bool crouched;
        string prompt="";

        void Start()
        {
            controller=GetComponent<CharacterController>();
            standingHeight=controller.height; standingCenter=controller.center;
            standingStep=controller.stepOffset;
            if(eye!=null)
            {
                eyeOrigin=eye.transform.localPosition;
                lighter=GetComponent<B1Lighter>();
                if(lighter==null) lighter=gameObject.AddComponent<B1Lighter>();
                lighter.Initialize(eye);
            }
            var finish=GetComponent<B1WallFinish>();
            if(finish==null) finish=gameObject.AddComponent<B1WallFinish>();
            finish.Apply();
            var referenceArt=GetComponent<B1ReferenceArt>();
            if(referenceArt==null) referenceArt=gameObject.AddComponent<B1ReferenceArt>();
            referenceArt.Apply();
            Capture(true);
        }
        void Capture(bool value) { Cursor.lockState=value?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=!value; }
        void Update()
        {
            var k=Keyboard.current; var m=Mouse.current;
            if(k==null || m==null || eye==null) return;
            if(k.escapeKey.wasPressedThisFrame) Capture(false);
            if(Cursor.lockState!=CursorLockMode.Locked)
            {
                prompt="";
                AnimateEye(false);
                if(lighter!=null) lighter.Tick(false);
                if(m.leftButton.wasPressedThisFrame) Capture(true);
                return;
            }
            var look=m.delta.ReadValue()*.085f;
            transform.Rotate(0,look.x,0); pitch=Mathf.Clamp(pitch-look.y,-80,80);
            if(k.cKey.wasPressedThisFrame)
            {
                if(!crouched) SetCrouched(true);
                else if(CanStand()) SetCrouched(false);
            }
            Vector2 input=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
            input=Vector2.ClampMagnitude(input,1);
            bool sprint=!crouched && (k.leftShiftKey.isPressed || k.rightShiftKey.isPressed) && input.sqrMagnitude>.01f;
            float speed=crouched?crouchSpeed:(sprint?runSpeed:walkSpeed);
            if(controller.isGrounded && fall<0) fall=-2;
            fall+=Physics.gravity.y*Time.deltaTime;
            Vector3 before=transform.position;
            controller.Move((transform.right*input.x*speed+transform.forward*input.y*speed+Vector3.up*fall)*Time.deltaTime);
            Vector3 moved=transform.position-before; moved.y=0;
            // Head motion follows real movement: pushing into a wall or falling doesn't shake the camera.
            bool running=sprint && controller.isGrounded && moved.sqrMagnitude>Mathf.Pow(.2f*Time.deltaTime,2);
            AnimateEye(running);
            prompt="";
            if(lighter!=null && lighter.Tick(true,running)) { prompt=lighter.Prompt; return; }
            if(Physics.Raycast(eye.transform.position,eye.transform.forward,out var hit,2.6f))
            {
                var door=hit.collider.GetComponentInParent<B1Door>();
                if(door!=null) { prompt="E : Open / close door"; if(k.eKey.wasPressedThisFrame) door.Toggle(); }
            }
        }
        void SetCrouched(bool value)
        {
            crouched=value;
            float height=value?Mathf.Clamp(crouchHeight,controller.radius*2,standingHeight):standingHeight;
            // Keep the bottom of the capsule fixed so crouching never lifts the feet.
            controller.stepOffset=Mathf.Min(standingStep,height*.45f);
            controller.height=height;
            controller.center=standingCenter+Vector3.down*((standingHeight-height)*.5f);
        }
        bool CanStand()
        {
            Vector3 scale=transform.lossyScale;
            float radius=controller.radius*Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.z));
            float height=Mathf.Max(standingHeight*Mathf.Abs(scale.y),radius*2);
            Vector3 center=transform.TransformPoint(standingCenter);
            Vector3 offset=transform.up*(height*.5f-radius);
            // Slightly shrink the test to avoid treating normal floor contact as a ceiling.
            int count=Physics.OverlapCapsuleNonAlloc(center-offset,center+offset,Mathf.Max(.01f,radius-.02f),standingHits,~0,QueryTriggerInteraction.Ignore);
            if(count==standingHits.Length) return false;
            for(int i=0;i<count;i++)
            {
                var collider=standingHits[i];
                if(collider!=controller && !collider.transform.IsChildOf(transform)) return false;
            }
            return true;
        }
        void AnimateEye(bool running)
        {
            if(running) bobPhase+=Time.deltaTime*runBobFrequency*Mathf.PI*2;
            else bobPhase=0;
            float amplitude=runBobAmplitude;
            Vector3 target=eyeOrigin+Vector3.down*(standingHeight-controller.height);
            float roll=0;
            if(running && amplitude>0)
            {
                target+=new Vector3(Mathf.Sin(bobPhase*.5f)*amplitude*.45f,Mathf.Sin(bobPhase)*amplitude,0);
                roll=Mathf.Sin(bobPhase*.5f)*runRollDegrees;
            }
            float blend=1-Mathf.Exp(-14*Time.deltaTime);
            eye.transform.localPosition=Vector3.Lerp(eye.transform.localPosition,target,blend);
            eye.transform.localRotation=Quaternion.Slerp(eye.transform.localRotation,Quaternion.Euler(pitch,0,roll),blend);
        }
        void OnGUI()
        {
            GUI.Label(new Rect(18,16,1100,30),"NAMSAN B1 | WASD: Move   Mouse: Look   E: Interact   F: Lighter   C: Crouch   Hold Shift: Run   Esc: Release cursor");
            GUI.Label(new Rect(18,44,300,30),crouched?"CROUCHED | C: Stand (needs clearance)":"");
            GUI.Label(new Rect(Screen.width/2-5,Screen.height/2-12,20,24),"+");
            GUI.Label(new Rect(Screen.width/2-110,Screen.height/2+30,300,30),prompt);
        }
        void OnDisable() { Capture(false); }
    }
}
