using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using TypeConverter.Utils;

namespace TypeConverter.Attempts
{
    internal class MapAttempt : IConversionAttempt
    {
        private readonly Dictionary<Tuple<Type, Type>, List<Tuple<PropertyInfo, PropertyInfo>>> mapping;

        public MapAttempt()
        {
            this.mapping = new Dictionary<Tuple<Type, Type>, List<Tuple<PropertyInfo, PropertyInfo>>>();
        }

        public ConversionResult TryConvert(object value, Type sourceType, Type targetType)
        {
            var target = Activator.CreateInstance(targetType);
            var map = this.GetMappingForTypes(sourceType, targetType);
            foreach (var m in map)
            {
                var sourceValue = m.Item1.GetValue(value);
                m.Item2.SetValue(target, sourceValue);
            }
        

            return new ConversionResult(target);
        }

        private List<Tuple<PropertyInfo, PropertyInfo>> GetMappingForTypes(Type sourceType, Type targetType)
        {
            lock (this.mapping)
            {
                var key = new Tuple<Type, Type>(sourceType, targetType);
                if (this.mapping.ContainsKey(key))
                {
                    return this.mapping[key];
                }

                return null;
            }
        }

        public void RegisterMapping<TTarget, TSource>(Expression<Func<TTarget, object>> destinationMember, Expression<Func<TSource, object>> sourceMember)
        {
            var sourcePropertyInfo = ReflectionHelper.GetPropertyInfo(sourceMember);
            var destinationPropertyInfo = ReflectionHelper.GetPropertyInfo(destinationMember);

            lock (this.mapping)
            {
                var map = this.GetMappingForTypes(typeof(TSource), typeof(TTarget));
                if (map == null)
                {
                    this.mapping.Add(new Tuple<Type, Type>(typeof(TSource), typeof(TTarget)), new List<Tuple<PropertyInfo, PropertyInfo>> { new Tuple<PropertyInfo, PropertyInfo>(sourcePropertyInfo, destinationPropertyInfo) });
                }
                else
                {
                    map.Add(new Tuple<PropertyInfo, PropertyInfo>(sourcePropertyInfo, destinationPropertyInfo));
                }
            }
        }

        internal void Reset()
        {
            lock (this.mapping)
            {
                this.mapping.Clear();
            }
        }
    }
}