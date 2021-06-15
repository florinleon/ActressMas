using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Sharpen
{
    public abstract class EnumBase : IComparable<EnumBase>, IComparable
    {
        private static readonly Dictionary<Type, EnumBase[]> VALUES_MAP = new Dictionary<Type, EnumBase[]>();

        private readonly int _ordinal;
        private readonly string _name;

        protected EnumBase(int ordinal, string name)
        {
            _ordinal = ordinal;
            _name = name;
        }

        public int ordinal()
        {
            return _ordinal;
        }

        public string name()
        {
            return _name;
        }

        public override string ToString()
        {
            return _name;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((EnumBase)obj);
        }

        public int CompareTo(EnumBase other)
        {
            return this._ordinal - other._ordinal;
        }

        public static bool IsEnum(Type t)
        {
            return VALUES_MAP.ContainsKey(t);
        }

        protected static void RegisterValues<T>(EnumBase[] values) where T : EnumBase
        {
            VALUES_MAP[typeof(T)] = values;
        }

        public static EnumBase[] GetEnumValues(Type enumType)
        {
            EnumBase[] result;
            if (VALUES_MAP.TryGetValue(enumType, out result))
            {
                return result;
            }
            else
            {
                RuntimeHelpers.RunClassConstructor(enumType.TypeHandle);
                return VALUES_MAP[enumType];
            }
        }

        public static T FindByName<T>(string name) where T : EnumBase
        {
            return name == null ? null : (T)GetEnumValues(typeof(T)).FirstOrDefault(val => val.name() == name);
        }

        public static T GetByName<T>(string name) where T : EnumBase
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return (T)GetEnumValues(typeof(T)).First(val => val.name() == name);
        }
    }

    public class System
    {
        public static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int Compare(int x, int y)
        {
            return (x < y) ? -1 : ((x == y) ? 0 : 1);
        }

        public static int Compare(long x, long y)
        {
            return (x < y) ? -1 : ((x == y) ? 0 : 1);
        }

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - EPOCH).TotalMilliseconds;
        }

        public static int FloorDiv(int x, int y)
        {
            int r = x / y;
            if ((x ^ y) < 0 && (r * y != x))
            {
                r--;
            }
            return r;
        }

        public static int Round(float v)
        {
            return (int)Math.Floor(v + 0.5f);
        }

        public static long Round(double v)
        {
            return (long)Math.Floor(v + 0.5d);
        }

        public static int HighestOneBit(int i)
        {
            uint u = (uint)i;
            u |= (u >> 1);
            u |= (u >> 2);
            u |= (u >> 4);
            u |= (u >> 8);
            u |= (u >> 16);
            return (int)(u - (u >> 1));
        }
    }

    public class Arrays
    {
        public static List<T> AsList<T>(params T[] a)
        {
            return a.ToList();
        }

        public static void Fill<T>(T[] a, T val)
        {
            Fill(a, 0, a.Length, val);
        }

        public static void Fill<T>(T[] a, int from, int to, T val)
        {
            for (int i = from; i < to; i++)
            {
                a[i] = val;
            }
        }

        public static T[] CopyOf<T>(T[] a, int newSize)
        {
            T[] result = new T[newSize];
            a.CopyTo(result, 0);
            return result;
        }

        public static int HashCode<T>(T[] a)
        {
            if (a == null)
            {
                return 0;
            }

            int result = 1;
            foreach (var element in a)
            {
                result = 31 * result + element.GetHashCode();
            }
            return result;
        }

        public static string ToString<T>(T[] a)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (var i = 0; i < a.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(a[i]);
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    public static class Collections
    {
        public static object Put(IDictionary map, object key, object value)
        {
            object result = map.Contains(key) ? map[key] : null;
            map[key] = value;
            return result;
        }

        public static V Put<K, V>(IDictionary<K, V> map, K key, V value)
        {
            V result;
            if (!map.TryGetValue(key, out result))
            {
                result = default(V);
            }
            map[key] = value;
            return result;
        }

        public static V Remove<K, V>(IDictionary<K, V> map, K key)
        {
            V result;
            if (map.TryGetValue(key, out result))
            {
                map.Remove(key);
                return result;
            }
            return default(V);
        }

        public static T RemoveFirst<T>(LinkedList<T> linkedList)
        {
            var result = linkedList.First.Value;
            linkedList.RemoveFirst();
            return result;
        }

        public static T RemoveLast<T>(LinkedList<T> linkedList)
        {
            var result = linkedList.Last.Value;
            linkedList.RemoveLast();
            return result;
        }

        public static void PutAll<CK, CV, IK, IV>(IDictionary<CK, CV> collection, IDictionary<IK, IV> items)
            where IK : CK where IV : CV
        {
            foreach (var e in items)
            {
                collection.Add(e.Key, e.Value);
            }
        }

        public static void AddAll<T>(ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static T[] ToArray<T>(ICollection<T> collection)
        {
            return ToArray(collection, new T[collection.Count]);
        }

        public static T[] ToArray<T>(ICollection<T> collection, T[] array)
        {
            int i = 0;
            foreach (var item in collection)
            {
                array[i++] = item;
            }
            return array;
        }

        public static IList<T> SingletonList<T>(T item)
        {
            return new ReadOnlyCollection<T>(new T[] { item });
        }

        public static void Sort<T>(this IList<T> list)
        {
            ((List<T>)list).Sort();
        }

        public static T GetAndRemove<T>(this IList<T> list, int index)
        {
            T result = list[index];
            list.RemoveAt(index);
            return result;
        }

        public static IList<L> UnmodifiableList<L>(IList<L> list)
        {
            return new ReadOnlyCollection<L>(list);
        }
    }

    public class Runtime
    {
        public static bool IsAssignableFrom(Type baseType, Type type)
        {
            if (baseType.IsAssignableFrom(type))
            {
                return true;
            }

            var baseTypeGeneric = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;
            var typeGeneric = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            return baseTypeGeneric.IsAssignableFrom(typeGeneric);
        }

        public static bool InstanceOf(object o, Type type)
        {
            if (o == null)
            {
                return false;
            }
            if (type.IsInstanceOfType(o))
            {
                return true;
            }
            return o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(type);
        }

        public static string substring(string s, int from, int to)
        {
            return s.Substring(from, to - from);
        }

        public static FieldInfo[] GetDeclaredFields(Type clazz)
        {
            return clazz.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
        }

        public static bool HasAttribute(FieldAttributes attributes, FieldAttributes flag)
        {
            return (attributes & flag) != 0;
        }

        public static CustomAttributeData GetCustomAttribute(MemberInfo info, Type attributeType)
        {
            foreach (var a in CustomAttributeData.GetCustomAttributes(info))
            {
                if (a.Constructor.DeclaringType == attributeType)
                {
                    return a;
                }
            }
            return null;
        }

        public static long CurrentTimeMillis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static Runtime GetRuntime()
        {
            return new Runtime();
        }

        public int AvailableProcessors()
        {
            return Environment.ProcessorCount;
        }
    }

    public static class SharpenExtensions
    {
        public static string GetSimpleName(this Type t)
        {
            string name = t.Name;
            int index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }
    }

    public class IdentityHashMap<K, V> : Dictionary<K, V>
    {
        public IdentityHashMap() : base(new IdentityEqualityComparer<K>())
        {
        }
    }

    public class IdentityEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    public class UUID : IEquatable<UUID>, IComparable<UUID>
    {
        private readonly long mostSigBits;
        private readonly long leastSigBits;

        public UUID(long mostSigBits, long leastSigBits)
        {
            this.mostSigBits = mostSigBits;
            this.leastSigBits = leastSigBits;
        }

        public long getMostSignificantBits()
        {
            return mostSigBits;
        }

        public long getLeastSignificantBits()
        {
            return leastSigBits;
        }

        public override string ToString()
        {
            return ((mostSigBits >> 32) & 0xFFFFFFFF).ToString("x8") + "-" +
                   ((mostSigBits >> 16) & 0xFFFF).ToString("x4") + "-" +
                   (mostSigBits & 0xFFFF).ToString("x4") + "-" +
                   ((leastSigBits >> 48) & 0xFFFF).ToString("x4") + "-" +
                   (leastSigBits & 0xFFFFFFFFFFFF).ToString("x12");
        }

        public bool Equals(UUID other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == obj as UUID;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                long hilo = mostSigBits ^ leastSigBits;
                return ((int)(hilo >> 32)) ^ (int)hilo;
            }
        }

        public int CompareTo(UUID other)
        {
            return (mostSigBits < other.mostSigBits ? -1 :
                (mostSigBits > other.mostSigBits ? 1 :
                    (leastSigBits < other.leastSigBits ? -1 :
                        (leastSigBits > other.leastSigBits ? 1 :
                            0))));
        }

        public static bool operator ==(UUID first, UUID second)
        {
            return ReferenceEquals(first, second) || !ReferenceEquals(first, null) && !ReferenceEquals(second, null) && first.mostSigBits == second.mostSigBits && first.leastSigBits == second.leastSigBits;
        }

        public static bool operator !=(UUID first, UUID second)
        {
            return !(first == second);
        }
    }

    public static class Lists
    {
        public static void Add<T>(this IList<T> list, int index, T value)
        {
            list.Insert(index, value);
        }

        public static T RemoveAtReturningValue<T>(this IList<T> list, int index)
        {
            T value = list[index];
            list.RemoveAt(index);
            return value;
        }
    }

    public static class Maps
    {
        public static V GetOrDefault<K, V>(this IDictionary<K, V> map, K key, V defaultValue)
        {
            V result;
            return map.TryGetValue(key, out result) ? result : defaultValue;
        }

        public static V GetOrNull<K, V>(this IDictionary<K, V> map, K key) where V : class
        {
            V result;
            return map.TryGetValue(key, out result) ? result : null;
        }

        public static V? GetOrNullable<K, V>(this IDictionary<K, V> map, K key) where V : struct
        {
            V result;
            return map.TryGetValue(key, out result) ? result : new V?();
        }
    }

    public static class JavaMath
    {
        public static double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static int Compare(double? a, double? b)
        {
            return a.Value.CompareTo(b);
        }
    }

    namespace Function
    {
        public delegate O BiFunction<I1, I2, O>(I1 i1, I2 i2);
    }

    namespace Concurrent
    {
        public class ThreadPoolExecutor
        {
            //private object milliseconds;
            private int taskCount;
            private int completedTaskCount;

            public ThreadPoolExecutor(int minThreadCount, int maxThreadCount, int v, object milliseconds)
            {
                ThreadPool.SetMinThreads(minThreadCount, 1);
                ThreadPool.SetMaxThreads(maxThreadCount, 2);
            }

            public void Submit(Action p)
            {
                lock (this)
                {
                    taskCount++;
                }
                ThreadPool.QueueUserWorkItem(new WaitCallback(TaskCallBack), p);
            }

            private void TaskCallBack(Object action)
            {
                ((Action)action).Invoke();
                lock (this)
                {
                    completedTaskCount++;
                }
            }

            public int GetTaskCount()
            {
                return taskCount;
            }

            public int GetCompletedTaskCount()
            {
                return completedTaskCount;
            }

            public void Shutdown()
            {
                // nothing to do
            }

            public void AwaitTermination(int time, TimeUnit unit)
            {
                // nothing to do
            }
        }

        public enum TimeUnit
        {
            Milliseconds
        }
    }
}