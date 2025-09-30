namespace DefaultNamespace;

using System;


/// Manages a bank of integer counters with reset tracking.
/// The bank deactivates when any counter reaches its reset limit.

public class SensorBank
{
    private readonly int[] counters;
    private readonly int[] resetCounts;
    private readonly int maxResetsPerCounter;
    private bool isActive;
    

    /// Gets whether the sensor bank is currently active.
    /// When inactive, mutator operations will throw InvalidOperationException.
    
    public bool IsActive 
    { 
        get { return isActive; } 
    }
    

    /// Gets the number of counters in this sensor bank.
    
    public int Size 
    { 
        get { return counters.Length; } 
    }
    

    /// Gets the maximum number of resets allowed per counter before deactivation.
    
    public int MaxResetsPerCounter 
    { 
        get { return maxResetsPerCounter; } 
    }
    

    /// Creates a new sensor bank with the specified number of counters.
    /// All counters start at zero and the bank is initially active.
    
    /// <param name="size">Number of counters (must be greater than 0).</param>
    /// <param name="maxResetsPerCounter">Maximum resets per counter before deactivation (must be greater than 0).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when size or maxResetsPerCounter is less than or equal to zero.</exception>
    public SensorBank(int size, int maxResetsPerCounter)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");
        if (maxResetsPerCounter <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxResetsPerCounter), "Max resets per counter must be greater than zero.");
            
        counters = new int[size];
        resetCounts = new int[size];
        this.maxResetsPerCounter = maxResetsPerCounter;
        isActive = true;
    }
    

    /// Increments the counter at the specified index by one.
    
    /// <param name="index">Zero-based index of the counter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the bank is inactive.</exception>
    public void IncrementCounter(int index)
    {
        ValidateIndex(index);
        EnsureActive();
            
        counters[index]++;
    }


    /// Gets the current value of the counter at the specified index.
    
    /// <param name="index">Zero-based index of the counter.</param>
    /// <returns>The current value of the counter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    public int QueryValue(int index)
    {
        ValidateIndex(index);
        
        return counters[index];
    }
    

    /// Gets the number of times the counter at the specified index has been reset.
    
    /// <param name="index">Zero-based index of the counter.</param>
    /// <returns>The number of resets performed on this counter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    public int QueryResetCount(int index)
    {
        ValidateIndex(index);
        
        return resetCounts[index];
    }


    /// Counts how many counters currently have the specified value.
    
    /// <param name="value">The value to search for.</param>
    /// <returns>The number of counters with the specified value.</returns>
    public int QueryCountersWithValue(int value)
    {
        int count = 0;
        for (int i = 0; i < counters.Length; i++)
        {
            if (counters[i] == value)
                count++;
        }
        return count;
    }


    /// Resets the counter at the specified index to zero and increments its reset count.
    /// If the reset count reaches MaxResetsPerCounter, the entire bank deactivates.
    
    /// <param name="index">Zero-based index of the counter to reset.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the bank is inactive.</exception>
    public void ResetCounter(int index)
    {
        ValidateIndex(index);
        EnsureActive();
            
        counters[index] = 0;
        resetCounts[index]++;
        
        if (resetCounts[index] >= maxResetsPerCounter)
        {
            isActive = false;
        }
    }


    /// Resets all counters and all reset counts to zero.
    /// Does not change the active/inactive state.
    
    /// <exception cref="InvalidOperationException">Thrown when the bank is inactive.</exception>
    public void ResetSensorBank()
    {
        EnsureActive();
            
        Array.Clear(counters, 0, counters.Length);
        Array.Clear(resetCounts, 0, resetCounts.Length);
    }
    

    /// Reactivates an inactive sensor bank and clears all reset counts.
    /// Counter values are preserved.
    
    public void Reactivate()
    {
        isActive = true;
        Array.Clear(resetCounts, 0, resetCounts.Length);
    }
    

    /// Gets a string representation of the sensor bank's current state.
    
    /// <returns>A string showing active status, size, and max resets.</returns>
    public override string ToString()
    {
        return "SensorBank[Active=" + isActive + ", Size=" + Size + ", MaxResets=" + maxResetsPerCounter + "]";
    }
    
    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= counters.Length)
            throw new ArgumentOutOfRangeException(nameof(index), 
                "Index must be between 0 and " + (counters.Length - 1));
    }
    
    private void EnsureActive()
    {
        if (!isActive)
            throw new InvalidOperationException("Cannot perform this operation because the sensor bank is inactive. Use Reactivate() to restore functionality.");
    }
}