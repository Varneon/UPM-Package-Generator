using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Varneon.UPMPackageGenerator.Editor.InputValidationUtility;
using FieldValidityState = Varneon.UPMPackageGenerator.Editor.InputValidationUtility.FieldValidityState;
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
        /// Field unused icon
        /// </summary>
        [SerializeField]
        private Texture2D iconUnused;

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
        public string PackageName = string.Empty;

        /// <summary>
        /// The display name of the UPM package
        /// </summary>
        public string PackageDisplayName;

        public string AuthorName;

        public string AuthorEmail;

        public string AuthorURL;

        public string AssemblyName;

        public bool
            CreateEditorFolder,
            CreateRuntimeFolder;

        public PackageType PackageType;

        private string customPackageDirectory = DEFAULT_PACKAGE_DIRECTORY;

        /// <summary>
        /// Button for generating the package
        /// </summary>
        private Button generateButton;

        private VisualElement[] validationIcons;

        private bool isPackageInfoValid;

        /// <summary>
        /// Documentation URL for naming a UPM package
        /// </summary>
        private const string UPM_PACKAGE_NAMING_DOCUMENTATION_URL = "https://docs.unity3d.com/Manual/cus-naming.html";

        private const string DEFAULT_PACKAGE_DIRECTORY = "Packages";

        [MenuItem("Varneon/UPM Package Generator")]
        public static GeneratorWindow OpenWindow()
        {
            GeneratorWindow window = GetWindow<GeneratorWindow>("UPM Package Generator");
            window.minSize = new Vector2(384, 384);
            return window;
        }

        private void OnEnable()
        {
            windowUxml.CloneTree(rootVisualElement);

            SerializedObject so = new SerializedObject(this);

            Label packageDirectoryLabel = rootVisualElement.Q<Label>("Label_PackageDirectory");

            void RefreshPackageDirectoryLabelAction() { packageDirectoryLabel.text = GetPackageDirectory(); }

            TextField assemblyNameField = rootVisualElement.Q<TextField>("TextField_AssemblyName");
            assemblyNameField.Bind(so);

            rootVisualElement.Q<Toggle>("Toggle_CreateEditorFolder").Bind(so);
            rootVisualElement.Q<Toggle>("Toggle_CreateRuntimeFolder").Bind(so);

            TextField packageNameField = rootVisualElement.Q<TextField>("TextField_PackageName");
            packageNameField.Bind(so);
            packageNameField.RegisterValueChangedCallback(a => {
                SetFieldValidationIconState(rootVisualElement.Q("ValidationIcon_PackageName"), ValidateInput(InputType.UPMPackageName, a.newValue));
                UpdateGenerateButtonEnabledState();
                RefreshPackageDirectoryLabelAction();
                RefreshAssemblyNameField();
                });

            TextField packageDisplayNameField = rootVisualElement.Q<TextField>("TextField_PackageDisplayName");
            packageDisplayNameField.Bind(so);
            packageDisplayNameField.RegisterValueChangedCallback(a => {
                SetFieldValidationIconState(rootVisualElement.Q("ValidationIcon_PackageDisplayName"), ValidateGenericTextInput(a.newValue));
                UpdateGenerateButtonEnabledState();
                RefreshAssemblyNameField();
            });

            TextField authorNameField = rootVisualElement.Q<TextField>("TextField_AuthorName");
            authorNameField.Bind(so);
            authorNameField.RegisterValueChangedCallback(a => {
                SetFieldValidationIconState(rootVisualElement.Q("ValidationIcon_AuthorName"), ValidateGenericTextInput(a.newValue));
                UpdateGenerateButtonEnabledState();
                RefreshAssemblyNameField();
            });

            TextField authorEmailField = rootVisualElement.Q<TextField>("TextField_AuthorEmail");
            authorEmailField.Bind(so);
            authorEmailField.RegisterValueChangedCallback(a => {
                SetFieldValidationIconState(rootVisualElement.Q("ValidationIcon_AuthorEmail"), ValidateInput(InputType.Email, a.newValue));
                UpdateGenerateButtonEnabledState();
            });

            TextField authorURLField = rootVisualElement.Q<TextField>("TextField_AuthorURL");
            authorURLField.Bind(so);
            authorURLField.RegisterValueChangedCallback(a => {
                SetFieldValidationIconState(rootVisualElement.Q("ValidationIcon_AuthorURL"), ValidateInput(InputType.URL, a.newValue));
                UpdateGenerateButtonEnabledState();
            });

            void RefreshAssemblyNameField()
            {
                bool validAuthor = !string.IsNullOrWhiteSpace(AuthorName);

                bool validPackageName = !string.IsNullOrWhiteSpace(PackageDisplayName);

                assemblyNameField.value = validAuthor && validPackageName ? string.Join(".", AuthorName.Replace('-', '.').Replace(" ", string.Empty), PackageDisplayName.Replace('-', '.').Replace(" ", string.Empty)) : PackageName;
            }

            Button browseCustomDirectoryButton = rootVisualElement.Q<Button>("Button_BrowseCustomDirectory");
            browseCustomDirectoryButton.clicked += () => {
                string newCustomDirectory = EditorUtility.SaveFolderPanel("Choose package directory", "Packages", PackageName);
                customPackageDirectory = string.IsNullOrWhiteSpace(newCustomDirectory) ? DEFAULT_PACKAGE_DIRECTORY : newCustomDirectory;
                RefreshPackageDirectoryLabelAction();
            };

            EnumField packageTypeField = rootVisualElement.Q<EnumField>("EnumField_PackageType");
            packageTypeField.Bind(so);
            packageTypeField.RegisterValueChangedCallback(a =>
            {
                SetElementVisibleState(browseCustomDirectoryButton, ((PackageType)a.newValue) == PackageType.Local);
                RefreshPackageDirectoryLabelAction();
            });

            (generateButton = rootVisualElement.Q<Button>("Button_Generate")).clicked += () => GeneratePackage();

            rootVisualElement.Q<Button>("Button_NamingHelpURL").clicked += () => Application.OpenURL(UPM_PACKAGE_NAMING_DOCUMENTATION_URL);

            validationIcons = rootVisualElement.Query(className: "validationIcon").ToList().ToArray();

            generateButton.SetEnabled(false);
        }

        private void UpdateGenerateButtonEnabledState()
        {
            foreach(VisualElement validationIcon in validationIcons)
            {
                if(validationIcon.style.backgroundImage == iconInvalid)
                {
                    generateButton.SetEnabled(false);

                    return;
                }
            }

            generateButton.SetEnabled(true);
        }

        /// <summary>
        /// Generates the UPM package folder based on the provided options
        /// </summary>
        private void GeneratePackage()
        {
            string packageFolderPath = GetPackageDirectory();

            string manifestPath = Path.Combine(packageFolderPath, "package.json");

            if (!Directory.Exists(packageFolderPath))
            {
                Directory.CreateDirectory(packageFolderPath);
            }

            PackageManifestGenerator.GenerateManifest(
                manifestPath,
                PackageName,
                PackageDisplayName,
                AuthorName,
                AuthorEmail,
                AuthorURL
                );

            if(PackageType == PackageType.Local)
            {
                try
                {
                    UPMUtility.AddLocalPackage(packageFolderPath);
                }
                catch
                {
                    Debug.LogWarning($"Couldn't add local package ({packageFolderPath}) via UPM! Please add the package manually from Package Manager.");
                }
            }

            AssetDatabase.SaveAssets();

            GenerateAssemblyDefinitions(packageFolderPath);

            AssetDatabase.Refresh();

            EditorUtility.RevealInFinder(packageFolderPath);

            Close();
        }

        private void GenerateAssemblyDefinitions(string directory)
        {
            string runtimeName = AssemblyName + ".Runtime";
            string editorName = AssemblyName + ".Editor";

            if (CreateRuntimeFolder)
            {
                AssemblyDefinitionGenerator.CreateAssemblyDefinition(Path.Combine(directory, "Runtime", runtimeName + ".asmdef"), runtimeName, false);
            }

            if (CreateEditorFolder)
            {
                AssemblyDefinitionGenerator.CreateAssemblyDefinition(Path.Combine(directory, "Editor", editorName + ".asmdef"), editorName, true, CreateRuntimeFolder ? runtimeName : null);
            }
        }

        private void SetFieldValidationIconState(VisualElement element, FieldValidityState state)
        {
            element.style.backgroundImage = state == FieldValidityState.None ? iconUnused : state == FieldValidityState.Valid ? iconValid : iconInvalid;
        }

        private void SetElementVisibleState(VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private string GetPackageDirectory()
        {
            switch (PackageType)
            {
                case PackageType.Local:
                    return Path.Combine(customPackageDirectory, PackageName);
                case PackageType.Embedded:
                default:
                    return Path.Combine(DEFAULT_PACKAGE_DIRECTORY, PackageName);
            }
        }
    }
}
