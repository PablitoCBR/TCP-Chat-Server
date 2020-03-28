using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Utils
{
    public static class CollectionAssert
    {

        public static void AssertTrueForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
            foreach (T element in collection)
            {
                Assert.True(predicate(element));
            }
        }
    }
}
