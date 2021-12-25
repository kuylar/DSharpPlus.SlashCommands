using System;
using System.Linq;
using System.Reflection;

using DSharpPlus.SlashCommands.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.SlashCommands
{
    internal static class InstanceCreator
    {
        internal static object CreateInstance(Type t, IServiceProvider services)
        {
            TypeInfo ti = t.GetTypeInfo();

            // constructor DI

            ConstructorInfo[] constructors = ti.DeclaredConstructors
                .Where(xm => xm.IsPublic)
                .ToArray();

            if(constructors.Length != 1)
            {
                constructors = constructors
                    .Where(xm => xm.GetCustomAttribute<ModuleConstructorAttribute>() != null)
                    .ToArray();

                if(constructors.Length != 1)
                {
                    throw new ArgumentException("Unable to select a constructor for the specific constructor.");
                }
            }

            ParameterInfo[] constructorArguments = constructors[0].GetParameters();

            if(constructorArguments.Length != 0 && services == null)
            {
                throw new InvalidOperationException("Service collection needs to contain all necessary values for constructor injection.");
            }

            Object[] arguments = new Object[constructorArguments.Length];

            for(int i = 0; i < arguments.Length; i++)
            {
                arguments[i] = services.GetRequiredService(constructorArguments[i].ParameterType);
            }

            Object moduleInstance = Activator.CreateInstance(t, arguments);

            // property DI

            PropertyInfo[] properties = t.GetRuntimeProperties().Where(xm => xm.CanWrite && xm.SetMethod != null
                && !xm.SetMethod.IsStatic && xm.SetMethod.IsPublic).ToArray();

            foreach(PropertyInfo property in properties)
            {
                if(property.GetCustomAttribute<DontInjectAttribute>() != null)
                {
                    continue;
                }

                Object service = services.GetService(property.PropertyType);

                if(service == null)
                {
                    continue;
                }

                property.SetValue(moduleInstance, service);
            }

            // field DI

            FieldInfo[] fields = t.GetRuntimeFields().Where(xm => !xm.IsInitOnly && !xm.IsStatic && xm.IsPublic).ToArray();

            foreach(FieldInfo field in fields)
            {
                if(field.GetCustomAttribute<DontInjectAttribute>() != null)
                {
                    continue;
                }

                Object service = services.GetService(field.FieldType);

                if(service == null)
                {
                    continue;
                }

                field.SetValue(moduleInstance, service);
            }

            return moduleInstance;
        }
    }
}
