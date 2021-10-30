using System;
using System.Collections;
using System.Collections.Generic;

namespace AECMIS.DAL.UnitTests.Tests
{
    public class EqualityComparer : IEqualityComparer
    {
        private readonly Dictionary<Type, Delegate> _comparers = new Dictionary<Type, Delegate>();

        public void RegisterComparer<T>(Func<T, object> comparer)
        {
            _comparers.Add(typeof(T), comparer);
        }

        public bool Equals(object x, object y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            var xType = x.GetType();
            var yType = y.GetType();
            // check subclass to handle proxies
            if (_comparers.ContainsKey(xType) && (xType == yType || yType.IsSubclassOf(xType)))
            {
                var comparer = _comparers[xType];
                var xValue = comparer.DynamicInvoke(new[] { x });
                var yValue = comparer.DynamicInvoke(new[] { y });
                return xValue.Equals(yValue);
            }
            return x.Equals(y);
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }
    }
}