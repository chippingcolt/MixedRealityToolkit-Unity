﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Editor
{
    [CustomEditor(typeof(MixedRealityRegisteredServiceProvidersProfile))]
    public class MixedRealityRegisteredServiceProviderProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private static readonly GUIContent MinusButtonContent = new GUIContent("-", "Unregister");
        private static readonly GUIContent AddButtonContent = new GUIContent("+ Register a new Service Provider");
        private SerializedProperty configurations;

        private static bool[] configFoldouts;

        private const string ProfileTitle = "Registered Services Settings";
        private const string ProfileDescription = "This profile defines any additional Services like systems, features, and managers to register with the Mixed Reality Toolkit.";

        protected override void OnEnable()
        {
            base.OnEnable();

            configurations = serializedObject.FindProperty("configurations");
            configFoldouts = new bool[configurations.arraySize];
        }

        public override void OnInspectorGUI()
        {
            RenderProfileHeader(ProfileTitle, ProfileDescription, target);

            using (new GUIEnabledWrapper(!IsProfileLock((BaseMixedRealityProfile)target)))
            {
                serializedObject.Update();

                RenderList(configurations);

                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override bool IsProfileInActiveInstance()
        {
            var profile = target as BaseMixedRealityProfile;

            return MixedRealityToolkit.IsInitialized && profile != null &&
                   MixedRealityToolkit.Instance.HasActiveProfile &&
                   MixedRealityToolkit.Instance.ActiveProfile.RegisteredServiceProvidersProfile == profile;
        }

        private void RenderList(SerializedProperty list)
        {
            bool changed = false;
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope())
            {
                if (GUILayout.Button(AddButtonContent, EditorStyles.miniButton))
                {
                    list.InsertArrayElementAtIndex(list.arraySize);
                    SerializedProperty managerConfig = list.GetArrayElementAtIndex(list.arraySize - 1);
                    var componentName = managerConfig.FindPropertyRelative("componentName");
                    componentName.stringValue = $"New Configuration {list.arraySize - 1}";
                    var priority = managerConfig.FindPropertyRelative("priority");
                    priority.intValue = 10;
                    var runtimePlatform = managerConfig.FindPropertyRelative("runtimePlatform");
                    runtimePlatform.intValue = -1;
                    var configurationProfile = managerConfig.FindPropertyRelative("configurationProfile");
                    configurationProfile.objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                    var componentType = ((MixedRealityRegisteredServiceProvidersProfile)serializedObject.targetObject).Configurations[list.arraySize - 1].ComponentType;
                    componentType.Type = null;
                    configFoldouts = new bool[list.arraySize];
                    return;
                }

                EditorGUILayout.Space();

                if (list == null || list.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("Register a new Service Provider.", MessageType.Warning);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Configurations", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                }
                EditorGUILayout.Space();

                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty managerConfig = list.GetArrayElementAtIndex(i);
                    var componentName = managerConfig.FindPropertyRelative("componentName");
                    var componentType = managerConfig.FindPropertyRelative("componentType");
                    var priority = managerConfig.FindPropertyRelative("priority");
                    var runtimePlatform = managerConfig.FindPropertyRelative("runtimePlatform");
                    var configurationProfile = managerConfig.FindPropertyRelative("configurationProfile");

                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            configFoldouts[i] = EditorGUILayout.Foldout(configFoldouts[i], componentName.stringValue, true);

                            if (GUILayout.Button(MinusButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                            {
                                list.DeleteArrayElementAtIndex(i);
                                serializedObject.ApplyModifiedProperties();
                                changed = true;
                                break;
                            }
                        }

                        SystemType systemType = null;
                        string typeName = null;
                        MixedRealityExtensionServiceAttribute providerAttribute = null;

                        if (configFoldouts[i] || RenderAsSubProfile)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(componentName);
                                changed |= EditorGUI.EndChangeCheck();

                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(componentType);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    // Try to assign default configuration profile when type changes.
                                    serializedObject.ApplyModifiedProperties();
                                    AssignDefaultConfigurationValues(((MixedRealityRegisteredServiceProvidersProfile)serializedObject.targetObject).Configurations[i].ComponentType, configurationProfile, runtimePlatform);
                                    changed = true;
                                    break;
                                }

                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(priority);
                                EditorGUILayout.PropertyField(runtimePlatform);

                                changed |= EditorGUI.EndChangeCheck();

                                Type serviceType = null;
                                if (configurationProfile.objectReferenceValue != null)
                                {
                                    serviceType = (target as MixedRealityRegisteredServiceProvidersProfile).Configurations[i].ComponentType;
                                }

                                changed |= RenderProfile(configurationProfile, null, true, true, serviceType);
                            }

                            serializedObject.ApplyModifiedProperties();
                        }

                        systemType = ((MixedRealityRegisteredServiceProvidersProfile)serializedObject.targetObject).Configurations[i].ComponentType;
                        if (systemType != null)
                        {
                            typeName = systemType.Type?.Name;
                            providerAttribute = typeName != null ? MixedRealityExtensionServiceAttribute.Find(systemType) : null;
                        }

                        if (providerAttribute != null && providerAttribute.RequiredProfileName != null && configurationProfile.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox($"{(string.IsNullOrEmpty(providerAttribute.Name) ? typeName : providerAttribute.Name)} requires a {providerAttribute.RequiredProfileName}", MessageType.Warning);
                        }
                    }
                    EditorGUILayout.Space();
                }
            }

            if (changed && MixedRealityToolkit.IsInitialized)
            {
                EditorApplication.delayCall += () => MixedRealityToolkit.Instance.ResetConfiguration(MixedRealityToolkit.Instance.ActiveProfile);
            }
        }

        private void AssignDefaultConfigurationValues(Type componentType, SerializedProperty configurationProfile, SerializedProperty runtimePlatform)
        {
            configurationProfile.objectReferenceValue = null;
            runtimePlatform.intValue = -1;

            if (componentType != null &&
                MixedRealityExtensionServiceAttribute.Find(componentType) is MixedRealityExtensionServiceAttribute attr)
            {
                configurationProfile.objectReferenceValue = attr.DefaultProfile;
                runtimePlatform.intValue = (int)attr.RuntimePlatforms;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}