﻿using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

#if UNITY_2018_1
using UnityEditor.Experimental.UIElements.GraphView;
#endif

namespace UnityEditor.ShaderGraph.Drawing
{
    static class CompatibilityExtensions
    {
#if UNITY_2018_1
        public static void OpenTextEditor(this BlackboardField field)
        {
            field.RenameGo();
        }
#endif

        public static void AppendAction(this ContextualMenu contextualMenu, string actionName, Action action, Func<ContextualMenu.MenuAction.StatusFlags> actionStatusCallback)
        {
            Debug.Assert(action != null);
            Debug.Assert(actionStatusCallback != null);
            contextualMenu.AppendAction(actionName, e => action(), e => actionStatusCallback());
        }

        public static void AppendAction(this ContextualMenu contextualMenu, string actionName, Action action, ContextualMenu.MenuAction.StatusFlags statusFlags)
        {
            Debug.Assert(action != null);
            contextualMenu.AppendAction(actionName, e => action(), e => statusFlags);
        }
    }
}
