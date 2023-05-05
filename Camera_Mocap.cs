
#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEditor.Animations;

public class Camera_Mocap : MonoBehaviour
{
    string location = "Assets/Camera_Mocap/";         // export location
    string folder = "Mocap_Export";                   // export folder
    string filename = "Mocap_";                       // export name

    // defult location : Assets/camera_mocap/mocap_1 

    int extra = 0;
    bool start_record;
    AnimationClip clip;

    [Range(0.5f, 10f)]
    public float gyro_sensitivity = 3f;

    Vector3 final_rotation;

    public bool Camera_x_Rotation = true;       // Enable Rotate the Camera From x Axis. Default : true
    public bool Camera_y_Rotation = true;       // Enable Rotate the Camera From y Axis. Default : true
    public bool Camera_z_Rotation = false;      // Enable Rotate the Camera From z Axis. Default : false

    public bool Rotate_when_touch = true;       // Only rotate the Camera when phone is touched

    Vector3 locker;

    private GameObjectRecorder m_Recorder;
    void Start()
    {
        Input.gyro.enabled = true;

        final_rotation = gameObject.transform.eulerAngles;

        for (int i = 0; i < 50; i++)
       {
            if (File.Exists(location + folder + "/" + filename + extra + ".anim"))
            {
                extra++;
            }
            else
            {                
                break;
            }       
            if(i == 50)
            {
                Debug.LogError("Camera_Mocap_error. there is to many files. your last file will be replaced. edit this script pr delete some ( mocap_export ) files.");
            }
       }          
       
        m_Recorder = new GameObjectRecorder(gameObject);
        m_Recorder.BindComponentsOfType<Transform>(gameObject, true);

    }

    private void LateUpdate()
    {
        if (Input.touchCount > 0 || Rotate_when_touch == false)
        {          
            locker = (-Input.gyro.rotationRateUnbiased) / gyro_sensitivity;
            if (Camera_z_Rotation == false)
            {
                locker.z = 0;
            }

            if (Camera_y_Rotation == false)
            {
                locker.y = 0;
            }

            if (Camera_x_Rotation == false)
            {
                locker.x = 0;
            }

            final_rotation.x += locker.x;
            final_rotation.y += locker.y;
            final_rotation.z += locker.z;
            transform.eulerAngles = final_rotation;
        }
        

        if (start_record)
        {           
            m_Recorder.TakeSnapshot(Time.deltaTime);
        }
    }
 

    public void start_capture_f()
    {
        clip = new AnimationClip();
        clip.name = filename + extra;

        if (Directory.Exists(location + folder + "/"))
        {
            AssetDatabase.CreateAsset(clip, location + folder + "/" + clip.name + ".anim");
        }
        else
        {
            Directory.CreateDirectory(location + folder + "/");
            AssetDatabase.CreateAsset(clip, location + folder + "/" + clip.name + ".anim");
        }
        start_record = true;
    }


    public void save()
    {
        start_record = false;
        
        if (m_Recorder.isRecording)
        {            
            m_Recorder.SaveToClip(clip);
            Debug.Log("saved :" + clip.name);
        }
    }




    [InspectorButton("start_capture_f")]
    public bool start_capture;

    [InspectorButton("save")]
    public bool stop_capture;

}


[System.AttributeUsage(System.AttributeTargets.Field)]
public class InspectorButtonAttribute : PropertyAttribute
{
    public static float kDefaultButtonWidth = 300;

    public readonly string MethodName;

    private float _buttonWidth = kDefaultButtonWidth;
    public float ButtonWidth
    {
        get { return _buttonWidth; }
        set { _buttonWidth = value; }
    }

    public InspectorButtonAttribute(string MethodName)
    {
        this.MethodName = MethodName;
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
public class InspectorButtonPropertyDrawer : PropertyDrawer
{
    private MethodInfo _eventMethodInfo = null;

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        InspectorButtonAttribute inspectorButtonAttribute = (InspectorButtonAttribute)attribute;
        Rect buttonRect = new Rect(position.x + (position.width - inspectorButtonAttribute.ButtonWidth) * 0.5f, position.y, inspectorButtonAttribute.ButtonWidth, position.height);
        if (GUI.Button(buttonRect, label.text))
        {
            System.Type eventOwnerType = prop.serializedObject.targetObject.GetType();
            string eventName = inspectorButtonAttribute.MethodName;

            if (_eventMethodInfo == null)
                _eventMethodInfo = eventOwnerType.GetMethod(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (_eventMethodInfo != null)
                _eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
            else
                Debug.LogWarning(string.Format("InspectorButton: Unable to find method {0} in {1}", eventName, eventOwnerType));
        }
    }
}
#endif
#endif
