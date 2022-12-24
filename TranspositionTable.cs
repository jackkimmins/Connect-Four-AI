using System;

namespace ConnectFourAI;

public class TranspositionTable
{
    private const int TABLE_SIZE = 1 << 20;
    private const int TABLE_MASK = TABLE_SIZE - 1;

    private ulong[] keys = new ulong[TABLE_SIZE];
    private int[] values = new int[TABLE_SIZE];

    public int Collisions { get; private set; }
    

    //Gets the value of the key
    public int Get(ulong key)
    {
        int index = (int)(key & TABLE_MASK);
        if (keys[index] == key)
        {
            Collisions++;
            return values[index];
        }
        else return 0;
    }

    //Add a new entry to the table
    public void Put(ulong key, int value)
    {
        int index = (int)(key & TABLE_MASK);
        keys[index] = key;
        values[index] = value;
    }

    //Resets the transposition table
    public void Reset()
    {
        Collisions = 0;
        Array.Clear(keys, 0, keys.Length);
        Array.Clear(values, 0, values.Length);
    }

    //Returns the size of the table in bytes
    public long Size()
    {
        return (long)(keys.Length * sizeof(long) + values.Length * sizeof(int));
    }
}