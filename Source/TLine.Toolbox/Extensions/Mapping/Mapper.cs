namespace TripLine.Toolbox.Extensions.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public enum IgnoreConfiguration { UseDefaultOnly, UseSpecifiedOnly, UseBoth };

    public class Mapper
    {
        #region Static Fields

        private static readonly Dictionary<TypeKey, List<string>> _ignoredConfig =
            new Dictionary<TypeKey, List<string>>();

        #endregion

        #region Fields

        private readonly object _source;

        private List<string> _ignoreProp;

        private IgnoreConfiguration _ignoredConfiguration = IgnoreConfiguration.UseDefaultOnly;

        #endregion

        #region Constructors and Destructors

        public Mapper(object o)
        {
            _source = o;
            _ignoreProp = new List<string>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Clears the global ignore list
        /// </summary>
        public static void ClearConfiguration()
        {
            _ignoredConfig.Clear();
        }

        /// <summary>
        ///     Configures the ignore list for specific used types (S, D)
        /// </summary>
        /// <typeparam name="S">Source Type</typeparam>
        /// <typeparam name="D">Destination Type</typeparam>
        /// <param name="configure"></param>
        public static void Configure<S, D>(Action<List<string>> configure)
        {
            var key = new TypeKey { Source = typeof(S), Destination = typeof(D) };
            if (!_ignoredConfig.ContainsKey(key))
            {
                _ignoredConfig.Add(key, new List<string>());
            }

            configure.Invoke(_ignoredConfig[key]);
        }

        /// <summary>
        ///     Creates a new instance of the type T making sure that the destination
        ///     object is fully set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>instance of T</returns>
        public T CreateAllDestination<T>()
        {
            object dest = Activator.CreateInstance(typeof(T));
            MapAllDestination(dest);
            return (T)dest;
        }

        /// <summary>
        ///     Creates a new instance of the type T making sure that the source
        ///     object is fully copied
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>instance of T</returns>
        public T CreateAllSource<T>()
        {
            object dest = Activator.CreateInstance(typeof(T));
            MapAllSource(dest);
            return (T)dest;
        }

        /// <summary>
        ///     Adds a property to the Mapper ignore list
        /// </summary>
        /// <param name="propName">The name of the property</param>
        /// <param name="ignoreConfiguration"></param>
        /// <returns></returns>
        public Mapper Ignore(string propName, IgnoreConfiguration ignoreConfiguration)
        {
            _ignoredConfiguration = ignoreConfiguration;
            _ignoreProp.Add(propName);
            return this;
        }

        /// <summary>
        ///     Adds a list of properties to the Mapper ignore list
        /// </summary>
        /// <param name="propNames">List of property names</param>
        /// <param name="ignoreConfiguration"></param>
        /// <returns></returns>
        public Mapper Ignore(List<string> propNames, IgnoreConfiguration ignoreConfiguration)
        {
            _ignoredConfiguration = ignoreConfiguration;
            _ignoreProp.AddRange(propNames);
            return this;
        }

        /// <summary>
        ///     Maps all the properties of source to dest object. While making sure that the destination
        ///     object is fully set
        /// </summary>
        /// <param name="dest"></param>
        public void MapAllDestination(object dest)
        {
            var key = new TypeKey { Source = _source.GetType(), Destination = dest.GetType() };
            var ignoreProp = GetIgnoreProp(key);

            foreach (var destProp in dest.GetType().GetProperties().Where(prop => !ignoreProp.Contains(prop.Name)))
            { 
                var sourceProp = _source.GetType().GetProperty(destProp.Name);
                if (sourceProp != null && sourceProp.GetGetMethod(false) != null)
                {
                    destProp.SetValue(dest, sourceProp.GetValue(_source));
                }
                else
                {
                    throw MapperMissingPropertyException.From(destProp);
                }
            }
        }

        /// <summary>
        ///     Maps all the properties of source to dest object. While making sure that the source
        ///     object is fully copied
        /// </summary>
        /// <param name="dest"></param>
        public void MapAllSource(object dest)
        {
            var key = new TypeKey { Source = _source.GetType(), Destination = dest.GetType() };
            var ignoreProp = GetIgnoreProp(key);

            foreach (var sourceProp in _source.GetType().GetProperties().Where(prop => !ignoreProp.Contains(prop.Name)))
            {
                var destProp = dest.GetType().GetProperty(sourceProp.Name);
                if (destProp != null && destProp.GetSetMethod(false) != null)
                {
                    destProp.SetValue(dest, sourceProp.GetValue(_source));
                }
                else
                {
                    throw MapperMissingPropertyException.From(sourceProp);
                }
            }
        }

        private List<string> GetIgnoreProp(TypeKey key)
        {
            var ignoreProp = new List<string>();
            switch (_ignoredConfiguration)
            {
                case IgnoreConfiguration.UseSpecifiedOnly:
                    ignoreProp = _ignoreProp;
                    break;
                case IgnoreConfiguration.UseDefaultOnly:
                    if (_ignoredConfig.Keys.ToList().Any(tk => tk.Equals(key)))
                        ignoreProp = _ignoredConfig.First(d => d.Key.Equals(key)).Value;
                    break;
                case IgnoreConfiguration.UseBoth:
                    if (_ignoredConfig.Keys.ToList().Any(tk => tk.Equals(key)))
                        ignoreProp = _ignoredConfig.First(d => d.Key.Equals(key)).Value;
                    ignoreProp.AddRange(_ignoreProp);
                    break;
            }
            return ignoreProp;
        }

        #endregion

        private class TypeKey
        {
            #region Public Properties

            public Type Destination { get; set; }

            public Type Source { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object obj)
            {
                if (obj is TypeKey)
                {
                    var t = obj as TypeKey;
                    return Source == t.Source && Destination == t.Destination;
                }

                return false;
            }

            // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            // It's from Jon Skeet, so it should be good enough for us...
            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 23 + (Destination == null ? 0 : Destination.GetHashCode());
                    hash = hash * 23 + (Source == null ? 0 : Source.GetHashCode());
                    return hash;
                }
            }

            #endregion
        }
    }

    public class MapperMissingPropertyException : Exception
    {
        #region Constructors and Destructors

        public MapperMissingPropertyException(string message) : base(message)
        {
        }

        #endregion

        #region Public Methods and Operators

        public static MapperMissingPropertyException From(PropertyInfo propertyInfo)
        {
            return
                new MapperMissingPropertyException(
                    StringExt.Format("Missing property: {0} in the destination object", propertyInfo.Name));
        }

        #endregion
    }
}