using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Registry()
    {
        this.entries = new T[0];
    }

    public void Sort()
    {
        Array.Sort(entries, (a, b) => a.GetId().CompareTo(b.GetId()));
    }

    public void SetValues(T[] values)
    {

        int? dupId = FindDuplicateIds(values);
        if (dupId != null)
            Debug.LogError("Duplicate id found: " + ((int)dupId).ToString());

        entries = new T[values.Length];

        Array.Copy(values, entries, values.Length);

        Sort();

    }

    private int? FindDuplicateIds(T[] values)
    {

        IEnumerable<IGrouping<int, T>> grouped = values.GroupBy(v => v.GetId());

        if (grouped.Any(g => g.Count() > 1))
        {
            return grouped.Where(g => g.Count() > 1).ToArray()[0].Key;
        }
        else
        {
            return null;
        }

    }

    public T LinearSearch(int queryId)
    {
        foreach (T entry in entries)
            if (entry.GetId() == queryId)
                return entry;
        return default;
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

        if (entries.Length <= 0)
        {
            Debug.LogError("Registry empty whilst trying to find element");
            return default;
        }

        bool? movingForwards = null;
        int nextIndexCheck = startingIndex;

        if (nextIndexCheck < 0)
        {
            nextIndexCheck = 0;
        }
        else if (nextIndexCheck >= entries.Length)
        {
            nextIndexCheck = entries.Length - 1;
        }

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
                    return default;

                nextIndexCheck--;

            }
            else if (query.GetId() < queryId)
            {

                if (movingForwards == null)
                    movingForwards = true;
                else if (movingForwards == false)
                    return default;

                nextIndexCheck++;

            }

        }

    }

    public T BinarySearch(int queryId)
    {

        int start = 0;
        int end = entries.Length - 1;

        while (true)
        {

            if (start > end)
                return default;

            int mid = (start + end) / 2;
            T midEntry = entries[mid];

            if (midEntry.GetId() == queryId)
                return midEntry;

            else if (midEntry.GetId() > queryId)
                end = mid - 1;
            else
                start = mid + 1;

        }

    }

    public T[] GetArray() => entries;

    public bool EntryWithIdExists(int id)
        => entries.Any((x) => x.GetId() == id);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return entries.GetEnumerator();
    }

}
