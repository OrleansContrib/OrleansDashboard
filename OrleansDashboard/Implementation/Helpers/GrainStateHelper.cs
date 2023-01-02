using Orleans;
using Orleans.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard.Implementation.Helpers
{
  internal static class GrainStateHelper
  {
    public static IEnumerable<Type> GetGrainTypes()
    {
      return AppDomain.CurrentDomain.GetAssemblies()
                               .SelectMany(s => s.GetTypes())
                               .Where(w => w.IsAssignableTo(typeof(IGrain))
                                         && !w.Namespace.StartsWith("Orleans")
                                         && w.IsClass
                                         && !w.IsGenericType);
    }

    public static (object, string) GetGrainId(string id, Type implementationType)
    {
      object grainId = null;
      string keyExtension = "";
      var compoundGrainId = id.Split(",");

      try
      {
        if (implementationType.IsAssignableTo(typeof(IGrainWithGuidCompoundKey)))
        {
          if (compoundGrainId.Length != 2)
            throw new InvalidOperationException("Inform grain id in format `{ id},{additionalKey}`");

          grainId = Guid.Parse(compoundGrainId.First());
          keyExtension = compoundGrainId.Last();
        }
        else if (implementationType.IsAssignableTo(typeof(IGrainWithIntegerCompoundKey)))
        {
          if (compoundGrainId.Length != 2)
            throw new InvalidOperationException("Inform grain id in format {id},{additionalKey}");

          grainId = Convert.ToInt64(compoundGrainId.First());
          keyExtension = compoundGrainId.Last();
        }
        else if (implementationType.IsAssignableTo(typeof(IGrainWithIntegerKey)))
        {
          grainId = Convert.ToInt64(id);
        }
        else if (implementationType.IsAssignableTo(typeof(IGrainWithGuidKey)))
        {
          grainId = Guid.Parse(id);
        }
        else if (implementationType.IsAssignableTo(typeof(IGrainWithStringKey)))
        {
          grainId = id;
        }
      }
      catch (Exception ex)
      {
        throw new Exception($"Error when trying to convert grain Id {id}, {implementationType}", ex);
      }

      return (grainId, keyExtension);
    }

    public static IEnumerable<Type> GetPropertiesAndFieldsForGrainState(Type implementationType)
    {
      var impProperties = implementationType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

      var impFields = implementationType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

      var filterProps = impProperties
                          .Where(w => w.PropertyType.IsAssignableTo(typeof(IStorage)))
                          .Select(s => s.PropertyType.GetGenericArguments().First());

      var filterFields = impFields
                          .Where(w => w.FieldType.IsAssignableTo(typeof(IStorage)))
                          .Select(s => s.FieldType.GetGenericArguments().First());

      return filterProps.Union(filterFields);
    }

    public static MethodInfo GenerateGetGrainMethod(IGrainFactory grainFactory, object grainId, string keyExtension, string methodName)
    {
      if (string.IsNullOrWhiteSpace(keyExtension))
      {
        return grainFactory.GetType().GetMethods()
                        .First(w => w.Name == methodName
                              && w.GetParameters().Count() == 2
                              && w.GetParameters()[0].ParameterType == typeof(Type)
                              && w.GetParameters()[1].ParameterType == grainId.GetType());
      }
      else
      {
        return grainFactory.GetType().GetMethods()
                        .First(w => w.Name == methodName
                              && w.GetParameters().Count() == 3
                              && w.GetParameters()[0].ParameterType == typeof(Type)
                              && w.GetParameters()[1].ParameterType == grainId.GetType()
                              && w.GetParameters()[2].ParameterType == typeof(string));
      }
    }

    public static Type GetGrainType(string grainType)
    {
      var typeParts = grainType.Split(",");
      if (typeParts.Length != 2)
        throw new ArgumentException("Grain type must be in format `Namespace, Class, Method`");
      var @namespace = typeParts[0];
      var @class = typeParts[1];

      return AppDomain.CurrentDomain.GetAssemblies()
                          .SelectMany(s => s.GetTypes())
                          .Where(w => w.Name.Equals(@class) && w.Namespace.Equals(@namespace))
                          .FirstOrDefault();
    }

    public static IEnumerable<Type> GetCallableGrainTypes()
    {
      return AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(s => s.GetTypes())
          .Where(w => w.IsAssignableTo(typeof(IGrain))
                      && !w.Namespace.StartsWith("Orleans")
                      && w.IsInterface
                      && !w.IsGenericType);

    }

    public static IEnumerable<MethodInfo> GetCallableGrainMethods()
    {
      return GetCallableGrainTypes()
          .SelectMany(s => s.GetMethods())
          .Where(w => !w.GetParameters().Any());

    }


  }
}