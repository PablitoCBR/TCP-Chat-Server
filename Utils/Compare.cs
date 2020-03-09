using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class Compare
    {
        public static bool EachElement<T>(IEnumerable<T> expected, IEnumerable<T> result)
        {
            if (expected.Count() != result.Count())
                return false;

            for(var index = 0; index < expected.Count(); index++)
            {
                if (expected.ElementAt(index).Equals(result.ElementAt(index)) == false)
                    return false;
            }

            return true;
        }
    }
}
