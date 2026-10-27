using System;
using System.Linq;
using System.Reflection;
using Game.Data.Effects;
using UnityEditor;
using UnityEngine;

namespace Game.Data.Editor
{
    /// <summary>
    /// Inspector for [SerializeReference] effect, condition and requirement lists: a dropdown picks the
    /// type, then its fields are drawn below. New data classes appear in the menu automatically.
    /// </summary>
    [CustomPropertyDrawer(typeof(AbilityEffectData), true)]
    [CustomPropertyDrawer(typeof(ConditionData), true)]
    [CustomPropertyDrawer(typeof(RequirementData), true)]
    public sealed class PolymorphicDataDrawer : PropertyDrawer
    {
        private const string EmptyLabel = "— выбрать —";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            foreach (var child in Children(property))
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(child, true);
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SplitSharedReference(property);

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var buttonRect = EditorGUI.PrefixLabel(line, label);
            var current = property.managedReferenceValue?.GetType();
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(current == null ? EmptyLabel : LabelOf(current)), FocusType.Keyboard))
                ShowTypeMenu(property);

            var y = line.yMax;
            EditorGUI.indentLevel++;
            foreach (var child in Children(property))
            {
                y += EditorGUIUtility.standardVerticalSpacing;
                var height = EditorGUI.GetPropertyHeight(child, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child, true);
                y += height;
            }

            EditorGUI.indentLevel--;
        }

        private void ShowTypeMenu(SerializedProperty property)
        {
            var baseType = ElementType(fieldInfo.FieldType);
            var menu = new GenericMenu();
            var types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(type => !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(LabelOf);

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(LabelOf(type)), property.managedReferenceValue?.GetType() == type, () =>
                {
                    property.serializedObject.Update();
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        private static System.Collections.Generic.IEnumerable<SerializedProperty> Children(SerializedProperty property)
        {
            if (property.managedReferenceValue == null)
                yield break;

            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                yield return iterator.Copy();
            }
        }

        /// <summary>
        /// The list "+" button can copy the previous element's reference, so two entries would share one
        /// object. Give the copy its own fresh instance of the same type.
        /// </summary>
        private static void SplitSharedReference(SerializedProperty property)
        {
            var value = property.managedReferenceValue;
            const string marker = ".Array.data[";
            var markerIndex = property.propertyPath.LastIndexOf(marker, StringComparison.Ordinal);
            if (value == null || markerIndex < 0)
                return;

            var array = property.serializedObject.FindProperty(property.propertyPath.Substring(0, markerIndex));
            var index = int.Parse(property.propertyPath.Substring(markerIndex + marker.Length).TrimEnd(']'));
            for (var i = 0; i < index; i++)
            {
                if (array.GetArrayElementAtIndex(i).managedReferenceId != property.managedReferenceId)
                    continue;

                property.managedReferenceValue = Activator.CreateInstance(value.GetType());
                property.serializedObject.ApplyModifiedProperties();
                return;
            }
        }

        private static Type ElementType(Type fieldType)
        {
            if (fieldType.IsArray)
                return fieldType.GetElementType();
            return fieldType.IsGenericType ? fieldType.GetGenericArguments()[0] : fieldType;
        }

        private static string LabelOf(Type type) =>
            type.GetCustomAttribute<DataLabelAttribute>()?.Label ?? ObjectNames.NicifyVariableName(type.Name);
    }
}
