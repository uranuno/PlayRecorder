using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayRecorder : MonoBehaviour {

	public enum StatusType {
		None,
		Info,
		Warning
	}

	public string directory = "";
	public string nameFormat = "yyyyMMdd-HHmmss-fff";
	public string extension = "png";

	public float delay = 10f;
	public bool oneShot = false;

	public int framerate {
		get { return (int)(1f / (delay*0.01f)); }
	}

	[SerializeField] bool isRecording = false;

	public string status = "";
	public StatusType statusType = StatusType.None;

	void Start () {
		StopRecording ();
	}

	public void StopRecording () {
		isRecording = false;
		Time.captureFramerate = 0;
		
		status = "Idle...";
		statusType = StatusType.None;
	}

	public void StartRecording () {
		isRecording = true;

		if (oneShot) {
			Capture ();
			isRecording = false;
		}
		else {
			if (!Application.isPlaying || EditorApplication.isPaused) {
				status = "Only 'One Shot' is enabled when not in play mode!";
				statusType = StatusType.Warning;
				isRecording = false;
			} else {
				Time.captureFramerate = framerate;
			}
		}
	}

	void Update () {
		if (isRecording) {
			Capture ();
		}
	}

	public void Capture () {
		string path = directory + System.DateTime.Now.ToString (nameFormat) + "." + extension;
		Application.CaptureScreenshot (path);
		
		status = "Captured!\n" + path;
		statusType = StatusType.Info;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayRecorder))]
public class PlayRecorderEditor : Editor {

	SerializedProperty directoryProp;
	SerializedProperty nameFormatProp;
	SerializedProperty extensionProp;

	SerializedProperty oneShotProp;
	SerializedProperty delayProp;

	SerializedProperty isRecordingProp;

	SerializedProperty statusProp;
	SerializedProperty statusTypeProp;

	new PlayRecorder target {
		get {
			return (PlayRecorder)(base.target);
		}
	}

	void OnEnable () {
		directoryProp = serializedObject.FindProperty("directory");
		nameFormatProp = serializedObject.FindProperty("nameFormat");
		extensionProp = serializedObject.FindProperty("extension");

		oneShotProp = serializedObject.FindProperty("oneShot");
		delayProp = serializedObject.FindProperty("delay");

		isRecordingProp = serializedObject.FindProperty("isRecording");

		statusProp = serializedObject.FindProperty("status");
		statusTypeProp = serializedObject.FindProperty("statusType");
	}

	override public void OnInspectorGUI () {
		serializedObject.Update ();

		DrawPathEditor ();
		DrawRecorder ();

		serializedObject.ApplyModifiedProperties ();
		if (GUI.changed) EditorUtility.SetDirty (target);
	}

	void DrawPathEditor () {
		GUILayout.Label ("File Path", EditorStyles.boldLabel);

		string directory = directoryProp.stringValue;
		string nameFormat = nameFormatProp.stringValue;
		string extension = extensionProp.stringValue;

		EditorGUILayout.LabelField ("Directory :", directory);
		EditorGUILayout.LabelField ("Name Formtat :", nameFormat);
		EditorGUILayout.LabelField ("Extension :", extension);
		
		if (GUILayout.Button ("Edit")) {
			string fullPath = EditorUtility.SaveFilePanel ("Save Capture", directory, nameFormat, extension);
			UpdatePathInfo (fullPath);
		}
	}
	
	void UpdatePathInfo (string fullPath) {
		if (string.IsNullOrEmpty (fullPath)) return;
		
		string dataPath = Application.dataPath.Split (new string[]{"Assets"}, System.StringSplitOptions.None)[0];
		string path = fullPath.Split(new string[]{dataPath}, System.StringSplitOptions.None)[1];
		
		string[] splitedPath = path.Split('/');
		
		directoryProp.stringValue = "";
		for(int i = 0, len = splitedPath.Length; i < len; i++) {
			if(i < len-1) {
				directoryProp.stringValue += splitedPath[i] + "/";
			} else {
				string[] splitedName = splitedPath[i].Split('.');
				nameFormatProp.stringValue = splitedName[0];
				extensionProp.stringValue = splitedName[1];
			}
		}
	}

	void DrawRecorder () {
		GUILayout.Label ("Record Settings", EditorStyles.boldLabel);

		delayProp.floatValue = EditorGUILayout.FloatField ("Delay (FR:" + target.framerate + ") :", delayProp.floatValue);
		oneShotProp.boolValue = EditorGUILayout.Toggle ("One Shot : ", oneShotProp.boolValue);

		if (isRecordingProp.boolValue) {
			if (GUILayout.Button ("Stop")) {
				target.StopRecording ();
			}
		} else {
			string btnStr = oneShotProp.boolValue ? "One Shot" : "Start Recording";
			if (GUILayout.Button (btnStr)) {
				target.StartRecording ();
			}
		}
		
		EditorGUILayout.HelpBox ("Status : " + statusProp.stringValue,
		                         (MessageType)statusTypeProp.enumValueIndex, true);
	}
}
#endif
