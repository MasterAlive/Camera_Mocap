
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

    public bool Camera_x_Rotation = true;       // Enable Rotate the Camera From x Axis. Default : true
    public bool Camera_y_Rotation = true;       // Enable Rotate the Camera From y Axis. Default : true
    public bool Camera_z_Rotation = false;      // Enable Rotate the Camera From z Axis. Default : false

    public bool Rotate_when_touch = true;       // Only rotate the Camera when phone is touched

    Vector3 locker;

    private GameObjectRecorder m_Recorder;
    void Start()
    {
        Input.gyro.enabled = true;

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
            locker = (-Input.gyro.rotationRate) / gyro_sensitivity;
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

            gameObject.transform.eulerAngles += locker;
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
        }
    }




    [InspectorButton("start_capture_f")]
    public bool start_capture;

    [InspectorButton("save")]
    public bool stop_capture;

}
