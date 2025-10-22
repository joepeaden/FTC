using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptableObjectComparer : EditorWindow
{
    [SerializeField]
    bool rowsEstablished;
    [SerializeField]
    private List<ScriptableObject> comparisonScriptables = new List<ScriptableObject>();

    [MenuItem("Window/Custom Windows/ScriptableObjectComparer")]
    public static void ShowObjectComparer()
    {
        ScriptableObjectComparer wnd = CreateWindow<ScriptableObjectComparer>("ScriptableObjectComparer");
        wnd.titleContent = new GUIContent("ScriptableObjectComparer");
        wnd.Show();
    }

    public void CreateGUI()
    {
        rowsEstablished = false;

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Drop zone for multiple ScriptableObjects
        Box dropZone = new Box();
        dropZone.style.borderBottomWidth = 1;
        dropZone.style.borderLeftWidth = 1;
        dropZone.style.borderRightWidth = 1;
        dropZone.style.borderTopWidth = 1;
        dropZone.style.borderTopColor = Color.grey;
        dropZone.style.borderBottomColor = Color.grey;
        dropZone.style.borderLeftColor = Color.grey;
        dropZone.style.borderRightColor = Color.grey;
        dropZone.style.paddingBottom = 10;
        dropZone.style.paddingTop = 10;
        dropZone.style.paddingLeft = 10;
        dropZone.style.paddingRight = 10;
        dropZone.style.marginBottom = 10;

        Label dropLabel = new Label("Drag and Drop ScriptableObjects here to add them for comparison");
        dropZone.Add(dropLabel);

        root.Add(dropZone);

        // Register drag and drop callbacks on the drop zone
        dropZone.RegisterCallback<DragEnterEvent>(OnDragEnter);
        dropZone.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
        dropZone.RegisterCallback<DragPerformEvent>(OnDragPerform);

        Button c = new Button();
        c.text = "Clear";
        c.clicked += Clear;
        root.Add(c);

        Box gridContainer = new Box();
        gridContainer.style.flexDirection = FlexDirection.Row;
        gridContainer.name = "GridContainer";
        root.Add(gridContainer);

        foreach (ScriptableObject so in comparisonScriptables)
        {
            AddScriptable(so);
        }
    }

    private void Clear()
    {
        comparisonScriptables.Clear();
        rootVisualElement.Q("GridContainer").Clear();
        rowsEstablished = false;
    }

    private void OnDragEnter(DragEnterEvent evt)
    {
        if (DragAndDrop.objectReferences.Any(obj => obj is ScriptableObject))
        {
            evt.StopPropagation();
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
    }

    private void OnDragUpdate(DragUpdatedEvent evt)
    {
        if (DragAndDrop.objectReferences.Any(obj => obj is ScriptableObject))
        {
            evt.StopPropagation();
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        if (DragAndDrop.objectReferences.Any(obj => obj is ScriptableObject))
        {
            evt.StopPropagation();
            DragAndDrop.AcceptDrag();
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                if (obj is ScriptableObject so)
                {
                    comparisonScriptables.Add(so);
                    AddScriptable(so);
                }
            }
        }
    }

    private void AddScriptable(ScriptableObject scriptableToCompare)
    {
        VisualElement root = rootVisualElement;

        var fieldValues = scriptableToCompare.GetType().GetFields();

        // Set up rows
        Box parentContainer = new Box();
        parentContainer.style.flexDirection = FlexDirection.Column;

        Box titleContainer = new Box();
        titleContainer.style.flexDirection = FlexDirection.Row;
        if (!rowsEstablished)
        {
            Label spacer = new Label();
            spacer.style.width = 160;
            titleContainer.Add(spacer);
        }
        Label scriptableName = new Label(scriptableToCompare.name);
        scriptableName.style.width = 100;
        titleContainer.Add(scriptableName);
        parentContainer.Add(titleContainer);


        foreach (FieldInfo field in fieldValues)
        {
            Box theContainer = new Box();
            theContainer.style.flexDirection = FlexDirection.Row;

            if (!rowsEstablished)
            {
                VisualElement label = new Label(field.Name);
                label.style.width = 160;
                label.style.borderRightColor = Color.black;
                label.style.borderRightWidth = 2;
                label.style.borderBottomColor = Color.black;
                label.style.borderBottomWidth = 2;

                theContainer.Add(label);
            }

            VisualElement inputField;
            var value = field.GetValue(scriptableToCompare);

            if (value is string strVal)
            {
                inputField = new TextField();
                (inputField as TextField).value = strVal;
                inputField.AddToClassList(".unity-base-field__aligned");
                inputField.RegisterCallback<ChangeEvent<string>>((evt) =>
                {
                    field.SetValue(scriptableToCompare, evt.newValue);
                    EditorUtility.SetDirty(scriptableToCompare);
                    AssetDatabase.SaveAssets();
                });
            }
            else if (value is float floatVal)
            {
                inputField = new TextField();
                (inputField as TextField).value = floatVal.ToString();
                inputField.AddToClassList(".unity-base-field__aligned");
                inputField.RegisterCallback<ChangeEvent<string>>((evt) =>
                {
                    field.SetValue(scriptableToCompare, float.Parse(evt.newValue));
                    EditorUtility.SetDirty(scriptableToCompare);
                    AssetDatabase.SaveAssets();
                });
            }
            else if (value is int intVal)
            {
                inputField = new TextField();
                (inputField as TextField).value = intVal.ToString();
                inputField.AddToClassList(".unity-base-field__aligned");
                inputField.RegisterCallback<ChangeEvent<string>>((evt) =>
                {
                    field.SetValue(scriptableToCompare, int.Parse(evt.newValue));
                    EditorUtility.SetDirty(scriptableToCompare);
                    AssetDatabase.SaveAssets();
                });
            }
            else if (value is bool boolVal)
            {
                inputField = new Toggle();
                (inputField as Toggle).value = boolVal;

                inputField.RegisterCallback<ChangeEvent<bool>>((evt) =>
                {
                    field.SetValue(scriptableToCompare, evt.newValue);
                    EditorUtility.SetDirty(scriptableToCompare);
                    AssetDatabase.SaveAssets();
                });

                //newTextField.AddToClassList(".unity-base-field__aligned");
            }
            else if (value is ScriptableObject so)
            {
                inputField = new ObjectField();
                (inputField as ObjectField).value = so;
                inputField.RegisterCallback<ChangeEvent<ScriptableObject>>((evt) =>
                {
                    field.SetValue(scriptableToCompare, evt.newValue);
                    EditorUtility.SetDirty(scriptableToCompare);
                    AssetDatabase.SaveAssets();
                });
            }
            else if (value is Sprite sprite)
            {
                inputField = new ObjectField();
                (inputField as ObjectField).value = sprite;
                inputField.RegisterCallback<ChangeEvent<Sprite>>((evt) =>
                {
                    field.SetValue(scriptableToCompare, evt.newValue);
                    EditorUtility.SetDirty(scriptableToCompare);
                    AssetDatabase.SaveAssets();
                });
            }
            else if (value is AudioClip audioClip)
            {
                inputField = new ObjectField();
                (inputField as ObjectField).value = audioClip;
                inputField.RegisterCallback<ChangeEvent<AudioClip>>((evt) =>
                {
                    field.SetValue(scriptableToCompare, evt.newValue);
                    EditorUtility.SetDirty(scriptableToCompare);
                    AssetDatabase.SaveAssets();
                });
            }
            else
            {
                inputField = new TextField("NotValid");
            }

            inputField.style.width = 100;
            inputField.style.borderRightColor = Color.black;
            inputField.style.borderRightWidth = 2;
            //newTextField.style.borderBottomColor = Color.black;
            //newTextField.style.borderBottomWidth = 2;



            theContainer.Add(inputField);

            parentContainer.Add(theContainer);

            //VisualElement horizLine = new Label();
            //horizLine.style.height = 2;
            //horizLine.style.backgroundColor = Color.black;

            //root.Add(horizLine);

            //root.Add(horizLine);
        }

        rowsEstablished = true;

        Box gridParent = (Box)root.Q("GridContainer");
        gridParent.Add(parentContainer);
        //root.Add(theContainer);

        //root.Add(labelFromUXML);

    }


}
