//Simple Module system using DI by SeleniumSoul 2026
//Adapted from PuppetPlay

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DREditor
{
	public interface IModule
	{
		Task Init();
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public sealed class ModuleAttribute : Attribute { }

	public sealed class DRModuleSystem
	{
		private static readonly Dictionary<Type, object> currentModules = new();

		public static void Inject(object target)
		{
			if (target == null)
				throw new ArgumentException($"No {nameof(target)} found.");

			Type type = target.GetType();

			//TODO: Use loop if you want to support inheritance.
			FieldInfo[] moduleFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

			foreach (FieldInfo field in moduleFields)
			{
				if (!Attribute.IsDefined(field, typeof(ModuleAttribute)))
					continue;

				if (currentModules.TryGetValue(field.FieldType, out object value))
				{
					field.SetValue(target, value);
				}
				else
				{
					throw new Exception($"No dependency found for {field.Name}.");
				}
			}
		}

		public static void RegisterModule<T>(T module) where T : class
		{
			currentModules[typeof(T)] = module ?? throw new ArgumentNullException("module");
		}

		public static void UnregisterModule<T>(T module)
		{
			if (currentModules.ContainsKey(typeof(T)))
				currentModules.Remove(typeof(T));
		}

		public static T GetModule<T>(T module) where T : class => 
			(T)currentModules.GetValueOrDefault(typeof(T));

		public static bool TryGetModule<T>(out T module) where T : class
		{
			if (currentModules.TryGetValue(typeof(T), out var t))
			{
				module = (T)t;
				return true;
			}
			else
			{
				module = default;
				return false;
			}
		}

		public bool IsRegistered<T>() where T : class => 
			currentModules.ContainsKey(typeof(T));
	}
}
