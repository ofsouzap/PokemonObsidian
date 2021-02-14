using System;
using System.Collections;

public class Registry<T> : IEnumerable where T : IHasId
{

    private T[] entries;
    public int Length
    {
        get
        {
            return entries.Length;
        }
    }

    public T this[int index]
    {
        get
        {
            return entries[index];
        }
        protected set
        {
            entries[index] = value;
        }
    }

    public void Sort()
    {
        Array.Sort(entries, (a, b) => a.GetId().CompareTo(b.GetId()));
    }

    public void SetValues(T[] values)
    {

        entries = new T[values.Length];

        Array.Copy(values, entries, values.Length);

        Sort();

    }

    /// <summary>
    /// Searches the registry for an element with a specified id
    /// </summary>
    /// <param name="queryId">The id of the element to look for</param>
    /// <param name="startingIndex">The index to start at</param>
    /// <returns>The element if found or null if the element isn't present</returns>
    public T StartingIndexSearch(int queryId,
        int startingIndex)
    {

        //TODO - test this function

        bool? movingForwards = null;
        int nextIndexCheck = startingIndex;

        while (true)
        {

            T query = entries[nextIndexCheck];

            if (query.GetId() == queryId)
            {
                return query;
            }

            else if (query.GetId() > queryId)
            {

                if (movingForwards == null)
                    movingForwards = false;
                else if (movingForwards == true)
                    return default(T);

                nextIndexCheck--;

            }
            else if (query.GetId() < queryId)
            {

                if (movingForwards == null)
                    movingForwards = true;
                else if (movingForwards == false)
                    return default(T);

                nextIndexCheck++;

            }

        }

    }

    public T[] GetArray() => entries;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return entries.GetEnumerator();
    }

}
