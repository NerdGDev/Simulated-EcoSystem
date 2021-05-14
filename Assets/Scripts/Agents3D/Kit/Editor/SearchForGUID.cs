using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class SearchForGUID : SearchableEditorWindow
{
	#region Editor
	static SearchForGUID window;
	[MenuItem("Kit/Project Search/Search GUID reference &#g")]
	private static void Init()
	{
		if (window == null)
		{
			window = (SearchForGUID)SearchableEditorWindow.GetWindow(typeof(SearchForGUID));
			window.Show();
		}
		else
			window.SearchGUID();
	}
	#endregion

	#region Getter Setter
	internal class Package
	{
		public Process process;
		public string command = string.Empty;
		public List<string> result = new List<string>();
		public List<string> error = new List<string>();
		public bool HasStarted { get { return process != null && process != default(Process); } }
		public bool HasExited { get { return HasStarted && process.HasExited; } }
		public void Free()
		{
			process = null;
			command = string.Empty;
			result.Clear();
			error.Clear();
		}
	}
	internal Package package;
	internal class Setting
	{
		public Object obj;
		public bool CheckScene = true;
		public bool CheckMaterial = true;
		public bool CheckPrefab = true;
		public override string ToString()
		{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			// *.unity *.prefab *.mat
			return string.Format("{0}{1}{2}",
				(CheckScene ? "*.unity " : ""),
				(CheckPrefab ? "*.prefab " : ""),
				(CheckMaterial ? "*.mat " : "")
			).TrimEnd(' ');
#else
            // --include "*.prefab" --include "*.unity" --include "*.mat"
            return string.Format("{0}{1}{2}",
                (CheckScene ? "--include \"*.unity\" " : ""),
                (CheckPrefab ? "--include \"*.prefab\" " : ""),
                (CheckMaterial ? "--include \"*.mat\" " : "")
            ).TrimEnd(' ');
#endif
		}
	}
	internal Setting setting;
	
	private string selectedGUID = "";
	private string ProjectPath
	{
		get
		{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			return Application.dataPath.Replace("/", @"\"); // WTF ? DataPath, you have one job to do.
#else
            return Application.dataPath;
#endif
		}
	}
	#endregion

	#region System
	public override void OnEnable()
	{
		package = new Package();
		setting = new Setting();
	}
	void OnGUI()
	{
		DisplaySelectedObject();
		DisplaySearchResult();
	}
	void OnApplicationQuit()
	{
		SuspendExistProcess();
	}
	#endregion

	#region Display panel
	private Vector2 scrollResult = Vector2.zero;


	private void DisplaySearchResult()
	{
		if (package.HasStarted && package.HasExited)
		{
			GUILayout.Label("Project folder : " + ProjectPath);
			GUILayout.Label("last command :");
			GUILayout.TextArea(package.command);

			if (package.error.Count > 0)
			{
				GUILayout.Label("Error logged :");
				foreach (string str in package.error)
				{
					GUILayout.Label(str);
				}
			}

			if (package.HasStarted && !package.HasExited)
			{
				GUILayout.Label("Searching ...");
			}
			else if (package.HasExited && package.result.Count > 0)
			{
				scrollResult = GUILayout.BeginScrollView(scrollResult);
				GUILayout.Label("result :");
				foreach (string str in package.result)
				{
					if (GUILayout.Button(str))
					{
						// Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(str);
						Object obj = AssetDatabase.LoadMainAssetAtPath(str);
						if (obj != null)
						{
							EditorApplication.ExecuteMenuItem("Window/Project");
							EditorUtility.FocusProjectWindow();
							EditorGUIUtility.PingObject(obj);
						}
						else
							UnityEngine.Debug.Log("Ping Object fail.\n" + str);
					}
				}
				GUILayout.EndScrollView();
			}
			else if (package.result.Count == 0 && package.HasExited)
			{
				GUILayout.Label("No matching result.");
			}
			else
			{
				GUILayout.Label("What juat happen ?");
			}
		}
	}
	private void DisplaySelectedObject()
	{
		Object obj = Selection.activeObject;
		if (obj == null)
		{
			GUILayout.Label(string.Format("Selected Object : {0}", "None"));
		}
		else
		{
			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
			GUILayout.Label(string.Format("Selected Object : {0}", obj.name));
			selectedGUID = string.Join(string.Empty, Selection.assetGUIDs);
			GUILayout.Label(string.Format("GUID : {0}\nInstance ID : {1}\nType : {2}", selectedGUID, obj.GetInstanceID(), obj.GetType()));
			GUILayout.EndVertical();

			GUILayout.BeginVertical(GUILayout.MaxWidth(300f));
			if (GUILayout.Button("Ping Selected", GUILayout.MaxHeight(80f)))
			{
				Selection.activeObject = (setting.obj == null) ? obj : setting.obj;
				EditorGUIUtility.PingObject(Selection.activeObject);
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			DisplaySearchSetting();

			GUILayout.Space(20f);

		}
	}
	private void DisplaySearchSetting()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Search name code", GUILayout.Height(80f), GUILayout.Width(130f)))
		{
			SearchInCode();
			setting.obj = Selection.activeObject;
		}
		if (Selection.activeObject.GetType() == typeof(Texture2D))
		{
			if (GUILayout.Button("Search NGUI sprite", GUILayout.Height(80f), GUILayout.Width(130f)))
			{
				SearchSpriteReference();
				setting.obj = Selection.activeObject;
			}
		}
		GUILayout.Space(30f);
		if (GUILayout.Button("Search for asset", GUILayout.Height(80f)))
		{
			SearchGUID();
			setting.obj = Selection.activeObject;
		}
		GUILayout.Space(20f);
		GUILayout.BeginVertical(GUILayout.Width(100f));
		setting.CheckScene = GUILayout.Toggle(setting.CheckScene, "Check Scene");
		setting.CheckMaterial = GUILayout.Toggle(setting.CheckMaterial, "Check Material");
		setting.CheckPrefab = GUILayout.Toggle(setting.CheckPrefab, "Check Prefab");
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
	#endregion

	#region Core
	public void SearchGUID()
	{
		if (Selection.activeObject == null)
		{
			UnityEngine.Debug.LogWarning("Please select object to search");
			return;
		}
		string guid = string.Join(string.Empty, Selection.assetGUIDs);

#if UNITY_EDITOR_WIN
		PlatfromCommand(@"findstr.exe", string.Format("/S /M /O /L /C:\"{0}\" {1}", guid, setting.ToString()));
#else
		PlatfromCommand(@"grep", string.Format("-l -R {0} \"{1}\" .", setting.ToString(), guid));
#endif
	}
	public void SearchInCode()
	{
		if (Selection.activeObject == null)
		{
			UnityEngine.Debug.LogWarning("Please select object to search");
			return;
		}
		string fileName = Selection.activeObject.name;
		UnityEngine.Debug.Log(fileName);

#if UNITY_EDITOR_WIN
		PlatfromCommand(@"findstr.exe", string.Format("/S /M /O /L /C:\"{0}\" {1}", fileName, "*.cs *.js"));

#else
		PlatfromCommand(@"grep", string.Format("-l -R {0} \"{1}\"", "--include \"*.cs\" --include \"*.js\"", fileName));
#endif
	}
	public void SearchSpriteReference()
	{
		if (Selection.activeObject == null)
		{
			UnityEngine.Debug.LogWarning("Please select object to search");
			return;
		}
		string assetName = Selection.activeObject.name;

		bool tmp = setting.CheckMaterial;
		setting.CheckMaterial = false;

#if UNITY_EDITOR
		PlatfromCommand(@"findstr.exe", string.Format("/S /M /O /L /C:\"{0}\" {1}", assetName, setting.ToString()));
#else
		PlatfromCommand(@"grep", string.Format("-l -R {0} \"{1}\"", setting.ToString(), assetName));
#endif
		setting.CheckMaterial = tmp;
	}
	/// <summary>Run OS Command</summary>
	/// <param name="shell"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	/// <see cref="http://forum.unity3d.com/threads/start-a-external-package.process.17488/"/>
	/// <seealso cref="http://ss64.com/nt/findstr.html"/>
	private void PlatfromCommand(string shell, string args = "")
	{
		SuspendExistProcess();
		package.command = string.Format("{0} {1}", shell, args);
#if DEBUG
		UnityEngine.Debug.Log(package.command);
#endif
		package.process = new Process();
		package.process.StartInfo.FileName = shell;
		package.process.StartInfo.Arguments = args;
		package.process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
		// package.process.StartInfo.CreateNoWindow = true;
		package.process.StartInfo.UseShellExecute = false;
		package.process.StartInfo.WorkingDirectory = ProjectPath;
		package.process.StartInfo.RedirectStandardOutput = true;
		package.process.StartInfo.RedirectStandardError = true;
		//package.process.StartInfo.RedirectStandardInput = true;

		package.process.EnableRaisingEvents = true;
		package.process.Start();
		using (StreamReader stdout = package.process.StandardOutput)
		{
			string line;
			while ((line = stdout.ReadLine()) != null)
#if UNITY_EDITOR_OSX
				package.result.Add("Assets/" + line.Trim().Substring(2)); // get rid of the "./" at the beginning
#else
				package.result.Add("Assets/" + line.Trim());
#endif
		}
		using (StreamReader stderr = package.process.StandardError)
		{
			string line;
			while ((line = stderr.ReadLine()) != null)
				package.error.Add(line.Trim());
		}
	}
	private void SuspendExistProcess(bool log = true)
	{
		package.result.Clear();
		package.error.Clear();
		if (package.HasExited)
			return;
		try
		{
			if (package.process == null)
				return;
			package.process.CloseMainWindow();
			// package.process.Close();
			package.process.Kill();
			if (log)
				UnityEngine.Debug.Log("Search Process suspend");
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("Fail to suspend OS process \n" + e.Message);
		}
		finally
		{
			package.Free();
		}
	}
	#endregion
}
