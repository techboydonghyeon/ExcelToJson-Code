using System;
using System.Collections.Generic;
using System.Text;

namespace DHLib
{
    public static class StringBuilderCache
    {
        private const int MAX_BUILDER_SIZE = 4096;

        [ThreadStatic]
        private static Stack<StringBuilder> CachedInstance = new Stack<StringBuilder>();

        public static StringBuilder Acquire()
        {
            if ( CachedInstance.Count == 0 )
                return new StringBuilder( MAX_BUILDER_SIZE );

            return CachedInstance.Pop();
        }

        public static void Release(StringBuilder sb)
        {
            sb.Clear();
            CachedInstance.Push( sb );
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            Release( sb );
            return result;
        }
    }
}