using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
/*
————————————————
版权声明：本文为CSDN博主「aﻬ半世癫」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
原文链接：https://blog.csdn.net/sp2673071736/article/details/112094327
 */
namespace SkyFrameWork
{

#if UNITY_EDITOR
    [AttributeUsage(AttributeTargets.Field)]
#endif
    public class RenameAttribute : PropertyAttribute
    {

        ///  枚举名称 
        public readonly string Name = "";

        ///  文本颜色 
        public readonly string HtmlColor = "#ffffff";
        
        ///  多行文本
        public int mutiline = 1;

        ///  重命名属性 
        /// 新名称
        public RenameAttribute(string name)
        {
            Name = name;
        }

        ///  重命名属性 
        /// 新名称
        /// 文本颜色 例如："#FFFFFF" 或 "black"
        public RenameAttribute(string name, string color)
        {
            Name = name;
            HtmlColor = color;
        }

        public RenameAttribute(string name, Color htmlColor)
        {
            Name = name;
            HtmlColor = htmlColor.ToString();
        }
        
        ///  Html颜色转换为Color 
        /// 字符串颜色
        /// 返回Unity的Color类
        public static Color htmlToColor(string hex)
        {
            hex = hex.ToLower();
            if (!String.IsNullOrEmpty(hex))
            {
                ColorUtility.TryParseHtmlString(hex, out Color color);
                return color;
            }

            return new Color(0.705f, 0.705f, 0.705f);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RenameAttribute))]
    public class RenameDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RenameAttribute rename = (RenameAttribute) attribute;
            float baseHeight = base.GetPropertyHeight(property, label);
            if (property.propertyType == SerializedPropertyType.String && rename.mutiline > 1)
            {
                return baseHeight + EditorGUIUtility.singleLineHeight * rename.mutiline;
            }
            if (property.isExpanded)
            {
                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    return baseHeight + EditorGUIUtility.singleLineHeight * (property.CountInProperty());
                }
            }

            return baseHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                RenameAttribute rename = (RenameAttribute) attribute;
                label.text = rename.Name;
                // 重绘GUI
                Color defaultColor = EditorStyles.label.normal.textColor;
                EditorStyles.label.normal.textColor = RenameAttribute.htmlToColor(rename.HtmlColor);
                bool isElement = Regex.IsMatch(property.displayName, "Element \\d+");
                if (isElement)
                    if (base.attribute != rename) label.text = property.displayName;
                    else label.text += property.displayName.Substring(7);
                if (property.propertyType == SerializedPropertyType.Enum)
                {
                    DrawEnum(position, property, label);
                }
                else if (property.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
                else if (property.propertyType == SerializedPropertyType.String)
                {
                    if (rename.mutiline > 1)
                    {
                        Rect labelRect = new Rect(position)
                        {
                            height = EditorGUIUtility.singleLineHeight
                        };
                        Rect textAreaRect = new Rect(position)
                        {
                            height = position.height - EditorGUIUtility.singleLineHeight,
                            y = position.y + EditorGUIUtility.singleLineHeight
                        };
                        EditorGUI.LabelField(labelRect,label);
                        property.stringValue = EditorGUI.TextArea(textAreaRect, property.stringValue);
                    }
                    else
                    {
                        EditorGUI.PropertyField(position, property, label);
                    }
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label);
                }

                EditorStyles.label.normal.textColor = defaultColor;
            }
            // 替换属性名称

        }

        // 绘制枚举类型
        private void DrawEnum(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            // 获取枚举相关属性
            Type type = fieldInfo.FieldType;
            string[] names = property.enumNames;
            string[] values = new string[names.Length];
            Array.Copy(names, values, names.Length);
            while (type.IsArray) type = type.GetElementType();

            // 获取枚举所对应的RenameAttribute
            for (int i = 0; i < names.Length; i++)
            {
                FieldInfo info = type.GetField(names[i]);
                RenameAttribute[] atts = (RenameAttribute[]) info.GetCustomAttributes(typeof(RenameAttribute), true);
                if (atts.Length != 0) values[i] = atts[0].Name;
            }

            // 重绘GUI
            int index = EditorGUI.Popup(position, label.text, property.enumValueIndex, values);
            if (EditorGUI.EndChangeCheck() && index != -1) property.enumValueIndex = index;
        }

        

    }
#endif
}