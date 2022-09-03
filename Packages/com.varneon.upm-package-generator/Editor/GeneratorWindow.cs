using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using InputType = Varneon.UPMPackageGenerator.Editor.InputValidationUtility.InputType;

namespace Varneon.UPMPackageGenerator.Editor
{
    public class GeneratorWindow : EditorWindow
    {
        /// <summary>
        /// Root VisualTreeAsset for the window's UI
        /// </summary>
        [SerializeField]
        private VisualTreeAsset windowUxml;

        /// <summary>
        /// Field valid icon
        /// </summary>
        [SerializeField]
        private Texture2D iconValid;

        /// <summary>
        /// Field invalid icon
        /// </summary>
        [SerializeField]
        private Texture2D iconInvalid;

        /// <summary>
        /// The name of the UPM package
        /// </summary>
        public string PackageName;

        /// <summary>
        /// Button for generating the package
        /// </summary>
        private Button generateButton;

        /// <summary>
        /// Documentation URL for naming a UPM package
        /// </summary>
        private const string UPM_PACKAGE_NAMING_DOCUMENTATION_URL = "https://docs.unity3d.com/Manual/cus-naming.html";

        [MenuItem("Varneon/UPM Package Generator")]
        public static GeneratorWindow OpenWindow()
        {
            GeneratorWindow window = GetWindow<GeneratorWindow>("UPM Package Generator");
            window.minSize = new Vector2(384, 192);
            return window;
        }

        private void OnEnable()
        {
            windowUxml.CloneTree(rootVisualElement);

            SerializedObject so = new SerializedObject(this);

            VisualElement packageNameValidationIcon = rootVisualElement.Q("ValidationIcon_PackageName");

            TextField nameField = rootVisualElement.Q<TextField>("TextField_PackageName");
            nameField.Bind(so);
            nameField.RegisterValueChangedCallback(a => {
                bool isValidInput = InputValidationUtility.ValidateInput(InputType.UPMPackageName, a.newValue);
                SetFieldValidationIconState(packageNameValidationIcon, isValidInput);
                generateButton.SetEnabled(isValidInput);
                });

            (generateButton = rootVisualElement.Q<Button>("Button_Generate")).clicked += () => GeneratePackage();

            rootVisualElement.Q<Button>("Button_NamingHelpURL").clicked += () => Application.OpenURL(UPM_PACKAGE_NAMING_DOCUMENTATION_URL);

            generateButton.SetEnabled(false);
        }

        /// <summary>
        /// Generates the UPM package folder based on the provided options
        /// </summary>
        private void GeneratePackage()
        {
            string packageFolderPath = Path.Combine("Packages", PackageName);

            string manifestPath = Path.Combine(packageFolderPath, "package.json");

            if (!Directory.Exists(packageFolderPath))
            {
                Directory.CreateDirectory(packageFolderPath);
            }

            using (StreamWriter writer = new StreamWriter(manifestPath))
            {
                writer.Write(string.Join("\n", new string[] { "{", string.Format("\t\"name\": \"{0}\",\"version\": \"0.0.1\"", PackageName), "}" }));
            }

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            EditorUtility.RevealInFinder(packageFolderPath);

            Close();
        }

        private void SetFieldValidationIconState(VisualElement element, bool valid)
        {
            element.style.backgroundImage = valid ? iconValid : iconInvalid;
        }
    }
}
