using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHollow.Generator
{
    /// <summary>
    /// A simple helper for using Cecil to modify an existing assembly.
    /// 
    /// http://www.mono-project.com/Cecil
    /// </summary>
    public class ModGenerator
    {
        private string _assemblyPath;
        private AssemblyDefinition _assemblyDefinition;

        /// <summary>
        /// Modify a given assembly by its path.
        /// 
        /// NOTE: you must call <see cref="Save"/> in order to save your changes.
        /// </summary>
        /// <param name="assemblyPath"></param>
        public ModGenerator(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
            _assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath); 
        }

        #region Batch Operations

        /// <summary>
        /// Transform the list of classes by making all methods public, virtualizing any instance methods
        /// and 
        /// </summary>
        /// <param name="classNames"></param>
        public void BatchMakePublicMethodsAndVirtualize(IEnumerable<string> classNames)
        {
            foreach (string className in classNames)
            {
                if (className.Contains("."))
                {
                    FindChild(className, MakePublic);
                }
                else
                {
                    var types = _assemblyDefinition.MainModule.Types.Where(t => t.Name == className);

                    if (null != types)
                    {
                        foreach (var type in types)
                            MakePublicMethodsAndVirtualize(type);
                    }
                }
            }
        }

        /// <summary>
        /// Transform the list of classes by making them public (but doing nothing else to them).
        /// </summary>
        /// <param name="classNames"></param>
        public void BatchMakePublic(IEnumerable<string> classNames)
        {
            foreach (string className in classNames)
            {
                if (className.Contains("."))
                {
                    FindChild(className, MakePublic);
                }
                else
                {
                    var types = _assemblyDefinition.MainModule.Types.Where(t => t.Name == className);

                    if (null != types)
                    {
                        foreach (var type in types)
                            MakePublic(type);
                    }
                }
            }

            //var inGameScreen.Methods[""];
        }

        #endregion
        
        #region One-Offs

        /// <summary>
        /// Virtualize a given method call instead of calling "this."
        /// </summary>
        /// <param name="className"></param>
        /// <param name="methodToSearch"></param>
        /// <param name="callToVirtualize"></param>
        public void MakeCallVirtual(string className, string methodToSearch, string callToVirtualize)
        {
            var types = _assemblyDefinition.MainModule.Types.Where(t => t.Name == className);
            if (null != types)
            {
                foreach (var type in types)
                {
                    if (methodToSearch == "*compilergenerated*")
                    {
                        // we don't know the name because its in a LINQ call
                        var methods = type.Methods;
                        foreach (var method in methods)
                        {
                            VirtualizeCall(method, callToVirtualize);
                        }
                    }
                    else
                    {
                        // if this is a normal method just look for it
                        var method = type.Methods.Where(m => m.Name == methodToSearch).FirstOrDefault();

                        if (null != method)
                        {
                            VirtualizeCall(method, callToVirtualize);
                        }
                    }
                }
            }
        }

        private void VirtualizeCall(MethodDefinition method, string callToVirtualize)
        {
            var body = method.Body;
            var processor = body.GetILProcessor();
            foreach (var instruction in body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call)
                {
                    var def = instruction.Operand as MethodDefinition;
                    if (null != def && def.Name == callToVirtualize)
                    {
                        // if this is the right method call, make it virtual
                        instruction.OpCode = OpCodes.Callvirt;
                    }
                }
            }
        }

        /// <summary>
        /// Make the given type public and that's it.
        /// </summary>
        /// <param name="type"></param>
        public void MakePublic(TypeDefinition type)
        {
            if (!type.IsPublic)
            {
                if (type.IsNested)
                    type.IsNestedPublic = true;
                else
                    type.IsPublic = true;
            }
        }

        /// <summary>
        /// Make the given type public, make its methods public, virtualize instance methods and make any fields public.
        /// </summary>
        /// <param name="type"></param>
        public void MakePublicMethodsAndVirtualize(TypeDefinition type)
        {
            MakePublic(type);
            if (type.IsInterface)
            {
                return;
            }

            if (type.HasMethods)
            {
                foreach (MethodDefinition method in type.Methods.Where(m => !m.IsConstructor))
                {
                    if (!method.IsPublic)
                        method.IsPublic = true;

                    if (!method.IsVirtual && !method.IsStatic)
                        method.IsVirtual = true;
                }
            }

            if (type.HasProperties)
            {
                // right now we are going to leave properties alone because we don't use any
                //foreach (PropertyDefinition prop in type.Properties.Where(p => !p.IsRuntimeSpecialName))
                //{
                //    if (prop.GetMethod != null && !prop.GetMethod.IsPublic)
                //        prop.GetMethod.IsPublic = true;

                //    if (prop.SetMethod != null && !prop.SetMethod.IsPublic)
                //        prop.SetMethod.IsPublic = true;
                //}
            }

            if (type.HasFields)
            {
                foreach (FieldDefinition field in type.Fields)
                {
                    if (!field.IsPublic)
                        field.IsPublic = true;
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper method for splitting up nested class names. Only works one level deep.
        /// </summary>
        /// <param name="className"></param>
        /// <param name="modifyType"></param>
        private void FindChild(string className, Action<TypeDefinition> modifyType)
        {
            var parent = className.Split(new char[] { '.' })[0];
            var child = className.Split(new char[] { '.' })[1];
            var tx = _assemblyDefinition.MainModule.Types.Where(t => t.Name == parent);

            if (null != tx)
            {
                foreach (var type in tx)
                {
                    if (type.HasNestedTypes)
                    {
                        modifyType(type.NestedTypes.Where(t => t.Name == child).FirstOrDefault());
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Save the assembly being modified to a file.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            _assemblyDefinition.Write(path);
        }
    }
}
