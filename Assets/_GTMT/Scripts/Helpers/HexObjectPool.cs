using System.Collections.Generic;

namespace GTMT
{
    public class HexObjectPool<T>
    {
        static Stack<List<T>> stack = new Stack<List<T>>();


        public static List<T> Get()
        {
            if(stack.Count > 0)
            {
                return stack.Pop();
            }

            return new List<T>();
        }

        public static void Relese(List<T> list)
        {
            list.Clear();
            stack.Push(list);
        }
    }
}

