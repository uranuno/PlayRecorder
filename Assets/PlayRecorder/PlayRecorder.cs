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

	public bool continuous = true;
	public float delay = 10f;

	[SerializeField] bool recording = false;

	public string status = "";
	public StatusType statusType = StatusType.None;

	float accum = 0;

	void Start () {
		StartRecording (false);
	}

	public void StartRecording (bool state) {

		recording = state;
		
		if (recording) {
			if (continuous) {
				// Continuous Shot
				if (!Application.isPlaying || EditorApplication.isPaused) {
					status = "Continuous shot is enabled on PlayMode only!";
					statusType = StatusType.Warning;
					recording = false;
				}
			} else {
				// One Shot
				Capture ();
				recording = false;
			}
		} else {
			status = "Idle...";
			statusType = StatusType.None;
		}
	}

	void Update () {
		if (recording) {
			if (accum >= delay * 0.01f) {
				Capture ();
				accum = 0;
			}
			accum += Time.deltaTime;
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

	SerializedProperty continuousProp;
	SerializedProperty delayProp;

	SerializedProperty recordingProp;

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

		continuousProp = serializedObject.FindProperty("continuous");
		delayProp = serializedObject.FindProperty("delay");

		recordingProp = serializedObject.FindProperty("recording");

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
		GUILayout.Label ("Saved Path", EditorStyles.boldLabel);

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
		
		continuousProp.boolValue = EditorGUILayout.BeginToggleGroup ("Continuous Shot :", continuousProp.boolValue);
		delayProp.floatValue = EditorGUILayout.FloatField ("Delay(ms) :", delayProp.floatValue);
		EditorGUILayout.EndToggleGroup ();

		bool recording = recordingProp.boolValue;
		if (GUILayout.Button (recording ? "Stop" : "Record")) {
			target.StartRecording(!recording);
		}
		
		EditorGUILayout.HelpBox ("Status : " + statusProp.stringValue,
		                         (MessageType)statusTypeProp.enumValueIndex, true);
	}
}
#endif
