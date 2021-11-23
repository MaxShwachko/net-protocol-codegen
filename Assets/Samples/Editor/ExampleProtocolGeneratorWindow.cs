using System.Collections.Generic;
using System.IO;
using Generator.Common;
using NetProtocolCodeGen.Editor.Generator;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Generator.Utils.Directives;
using NetProtocolCodeGen.Editor.Scheme;
using UnityEditor;
using UnityEngine;

namespace Samples.Editor
{
    public class ExampleProtocolGeneratorWindow : EditorWindow
    {
        private const string DefaultPath = "RoomGenerated";
        private const string GenerationPathKey = "RoomGenerationPath";

        private static string _generationPathKey;
        private static string _defaultPath;
        private static string _generationPath;

        private static ProtocolGenerator _protocolGenerator;
        private static EnumsGenerator _enumsGenerator;

        private static void InitGenerators()
        {
            if (_protocolGenerator != null) 
                return;
            
            if (_enumsGenerator != null) 
                return;
                
            _protocolGenerator = new ProtocolGenerator();
            _enumsGenerator = new EnumsGenerator();
            _generationPathKey = GenerationPathKey;
            _defaultPath = DefaultPath;
        }

        [MenuItem("Tools/Example Protocol Generator/Settings")]
        public static void MiOpenWindow()
        {
            var window = GetWindowWithRect<ExampleProtocolGeneratorWindow>(new Rect(0, 0, 600, 100), false, "Room Protocol Generator");
            window.Show();
        }

        [MenuItem("Tools/Example Protocol Generator/Generate")]
        public static void GenerateProperties()
        {
            InitGenerators();
            _generationPath = EditorPrefs.GetString(_generationPathKey, _defaultPath);
            if (string.IsNullOrEmpty(_generationPath))
            {
                Debug.LogError($"[{nameof(ProtocolGenerator)}] Generation path can't be empty!");
                return;
            }

            var directoryInfo = new DirectoryInfo(Path.Combine(Application.dataPath, _generationPath));
            var baseNamespace = _generationPath.Replace("/", ".");
            
            var parser = new ProtocolSchemeParser();
            var json = Resources.Load<TextAsset>("json_scheme_example");
            var methods = parser.ParseMethods(json.text);
            
            UsingDirectivesHolder.Instance.SetUsingDirectives(new UsingDirectives());

            var files = new List<GeneratedFile>();
            var protocolFiles = _protocolGenerator.GenerateProtocols(directoryInfo, baseNamespace, methods, Lang.Javascript);
            var enumFiles = _enumsGenerator.GenerateAllEnums(directoryInfo, baseNamespace, methods);
            files.AddRange(protocolFiles);
            files.AddRange(enumFiles);
            
            if (directoryInfo.Exists)
            {
                Directory.Delete(directoryInfo.FullName, true);
            }
            Save(files);
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Example Protocol Generator/Parse Scheme")]
        public static void ParseScheme()
        {
            var parser = new ProtocolSchemeParser();
            var json = Resources.Load<TextAsset>("json_scheme_example");
            var methods = parser.ParseMethods(json.text);
            Debug.Log("methods: " + methods.Count);
        }

        private void OnEnable()
        {
            InitGenerators();
            _generationPath = EditorPrefs.GetString(_generationPathKey, _defaultPath);
        }

        private void OnGUI()
        {
            DrawGenerationPath();
            DrawGenerateButton();
        }

        private static void DrawGenerationPath()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("GenerationPath");
            _generationPath = GUILayout.TextField(_generationPath);
            GUILayout.EndHorizontal();
        }

        private static void DrawGenerateButton()
        {
            if (GUILayout.Button("Save path"))
            {
                EditorPrefs.SetString(_generationPathKey, _generationPath);
            }
            
            if (GUILayout.Button("Generate Protocol"))
            {
                GenerateProperties();   
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(_generationPathKey, _generationPath);
        }

        private static void Save(IEnumerable<GeneratedFile> files)
        {
            foreach (var file in files)
            {
                var fileInfo = file.FileInfo;
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();
                if (fileInfo.Exists)
                    fileInfo.Delete();
                File.WriteAllText(fileInfo.FullName, file.FileData);
            }
        }
    }
}