﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Graphing;

namespace UnityEditor.ShaderGraph
{
    internal abstract class ShaderNode : AbstractMaterialNode
    {      
        internal ShaderNode()
        {
        }

        [SerializeField]
        private List<ShaderParameter> m_Parameters = new List<ShaderParameter>();

        internal List<ShaderParameter> parameters
        {
            get { return m_Parameters; }
        }

        internal void DefineNode(NodeTypeDescriptor descriptor)
        {
            name = descriptor.name;
            AddShaderValuesFromTypeDescriptor(descriptor);
        }

        internal void AddShaderValuesFromTypeDescriptor(NodeTypeDescriptor descriptor)
        {
            var validSlotIds = new List<int>();
            if(descriptor.inPorts != null)
            {
                foreach (InputDescriptor input in descriptor.inPorts)
                {
                    AddSlot(new ShaderPort(input));
                    validSlotIds.Add(input.id);
                }
            }
            if(descriptor.outPorts != null)
            {
                foreach (OutputDescriptor output in descriptor.outPorts)
                {
                    AddSlot(new ShaderPort(output));
                    validSlotIds.Add(output.id);
                }
            }
            RemoveSlotsNameNotMatching(validSlotIds);
            
            var validParameters = new List<int>();
            if(descriptor.parameters != null)
            {
                foreach (InputDescriptor parameter in descriptor.parameters)
                {
                    AddParameter(new ShaderParameter(parameter));
                    validParameters.Add(parameter.id);
                }
            }
            RemoveParametersNameNotMatching(validParameters);
        }

        internal void AddParameter(ShaderParameter parameter)
        {
            var addingParameter = parameter;
            var foundParameter = FindParameter(parameter.id);

            m_Parameters.RemoveAll(x => x.id == parameter.id);
            m_Parameters.Add(parameter);
            parameter.owner = this;

            Dirty(ModificationScope.Topological);

            if (foundParameter == null)
                return;

            addingParameter.CopyValuesFrom(foundParameter);
        }

        internal void RemoveParameter(int parameterId)
        {
            m_Parameters.RemoveAll(x => x.id == parameterId);
            Dirty(ModificationScope.Topological);
        }

        internal ShaderParameter FindParameter(int id)
        {
            foreach (var parameter in m_Parameters)
            {
                if (parameter.id == id)
                    return parameter;
            }
            return null;
        }

        internal void RemoveParametersNameNotMatching(IEnumerable<int> parameterIds, bool supressWarnings = false)
        {
            var invalidParameters = m_Parameters.Select(x => x.id).Except(parameterIds);

            foreach (var invalidParameter in invalidParameters.ToArray())
            {
                if (!supressWarnings)
                    Debug.LogWarningFormat("Removing Invalid Parameter: {0}", invalidParameter);
                RemoveSlot(invalidParameter);
            }
        }

        internal IShaderValue GetShaderValue(IShaderValueDescriptor descriptor)
        {
            var parameter = FindParameter(descriptor.id);
            if(parameter != null)
                return parameter;

            var port = FindSlot<ShaderPort>(descriptor.id);
            if(port != null)
                return port;

            return null;
        }

        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            s_TempSlots.Clear();
            GetInputSlots(s_TempSlots);
            foreach (var slot in s_TempSlots)
            {
                ShaderPort port = slot as ShaderPort;
                if(port.HasEdges())
                    continue;
                
                properties.Add(port.ToPreviewProperty(port.ToShaderVariableName()));
            }

            foreach(ShaderParameter parameter in m_Parameters)
                properties.Add(parameter.ToPreviewProperty(parameter.ToShaderVariableName()));
        }

        public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        {
            if (!generationMode.IsPreview())
                return;

            foreach (var port in this.GetInputSlots<ShaderPort>())
            {
                if(!port.HasEdges())
                {
                    string overrideReferenceName = port.ToShaderVariableName();
                    IShaderProperty[] defaultProperties = port.ToDefaultPropertyArray(overrideReferenceName);
                    foreach(IShaderProperty property in defaultProperties)
                        properties.AddShaderProperty(property);
                }
            }

            foreach (var parameter in m_Parameters)
            {
                string overrideReferenceName = parameter.ToShaderVariableName();
                IShaderProperty[] defaultProperties = parameter.ToDefaultPropertyArray(overrideReferenceName);
                foreach(IShaderProperty property in defaultProperties)
                        properties.AddShaderProperty(property);
            }
        }
    }
}
