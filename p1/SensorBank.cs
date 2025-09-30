namespace DefaultNamespace;

using System;


/// <summary>
/// Manages a bank of integer counters with reset tracking.
/// The bank deactivates when any counter reaches its reset limit.
/// 
/// CLASS INVARIANTS:
/// - Size > 0 (at least one counter must exist)
/// - MaxResetsPerCounter > 0 (at least one reset must be allowed)
/// - counters.Length == resetCounts.Length == Size (arrays remain synchronized)
/// - For all valid indices i: counters[i] >= 0 (counters never go negative)
/// - For all valid indices i: 0 <= resetCounts[i] <= MaxResetsPerCounter
/// - IsActive == false implies exists at least one i where resetCounts[i] >= MaxResetsPerCounter
/// - IsActive == true implies for all i: resetCounts[i] < MaxResetsPerCounter
/// 
/// IMPLEMENTATION INVARIANTS:
/// - counters and resetCounts arrays are never null after construction
/// - maxResetsPerCounter is immutable (readonly) after construction
/// - Arrays maintain their size throughout object lifetime
/// - C# arrays initialize to zero by default
/// - Once deactivated, bank remains inactive until explicitly reactivated via Reactivate()
/// - State transitions: Active -> Inactive (via ResetCounter when limit reached)
///                      Inactive -> Active (via Reactivate only)
/// </summary>
public class SensorBank
{
    private readonly int[] counters;
    private readonly int[] resetCounts;
    private readonly int maxResetsPerCounter;
    private bool isActive;
    

    /// <summary>
    /// Gets whether the sensor bank is currently active.
    /// When inactive, mutator operations will throw InvalidOperationException.
    /// </summary>
    public bool IsActive 
    { 
        get { return isActive; } 
    }
    

    /// <summary>
    /// Gets the number of counters in this sensor bank.
    /// </summary>
    public int Size 
    { 
        get { return counters.Length; } 
    }
    

    /// <summary>
    /// Gets the maximum number of resets allowed per counter before deactivation.
    /// </summary>
    public int MaxResetsPerCounter 
    { 
        get { return maxResetsPerCounter; } 
    }
    

    /// <summary>
    /// Creates a new sensor bank with the specified number of counters.
    /// All counters start at zero and the bank is initially active.
    /// 
    /// Precondition: size > 0, maxResetsPerCounter > 0
    /// Postcondition: IsActive == true, Size == size, all counters == 0, all resetCounts == 0
    /// </summary>
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
    

    /// <summary>
    /// Increments the counter at the specified index by one.
    /// 
    /// Precondition: IsActive == true, 0 <= index < Size
    /// Postcondition: counters[index] == old(counters[index]) + 1, IsActive unchanged
    /// </summary>
    /// <param name="index">Zero-based index of the counter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the bank is inactive.</exception>
    public void IncrementCounter(int index)
    {
        ValidateIndex(index);
        EnsureActive();
            
        counters[index]++;
    }


    /// <summary>
    /// Gets the current value of the counter at the specified index.
    /// 
    /// Precondition: 0 <= index < Size
    /// Postcondition: Returns counters[index], state unchanged (query method)
    /// </summary>
    /// <param name="index">Zero-based index of the counter.</param>
    /// <returns>The current value of the counter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    public int QueryValue(int index)
    {
        ValidateIndex(index);
        
        return counters[index];
    }
    

    /// <summary>
    /// Gets the number of times the counter at the specified index has been reset.
    /// 
    /// Precondition: 0 <= index < Size
    /// Postcondition: Returns resetCounts[index], state unchanged (query method)
    /// </summary>
    /// <param name="index">Zero-based index of the counter.</param>
    /// <returns>The number of resets performed on this counter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    public int QueryResetCount(int index)
    {
        ValidateIndex(index);
        
        return resetCounts[index];
    }


    /// <summary>
    /// Counts how many counters currently have the specified value.
    /// 
    /// Precondition: None (value can be any integer)
    /// Postcondition: Returns count of counters where counters[i] == value, state unchanged (query method)
    /// </summary>
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


    /// <summary>
    /// Resets the counter at the specified index to zero and increments its reset count.
    /// If the reset count reaches MaxResetsPerCounter, the entire bank deactivates.
    /// 
    /// Precondition: IsActive == true, 0 <= index < Size
    /// Postcondition: counters[index] == 0, 
    ///                resetCounts[index] == old(resetCounts[index]) + 1,
    ///                IsActive == false if resetCounts[index] >= MaxResetsPerCounter
    /// </summary>
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


    /// <summary>
    /// Resets all counters and all reset counts to zero, then reactivates the bank.
    /// This provides a complete fresh start for the sensor bank.
    /// 
    /// Precondition: IsActive == true
    /// Postcondition: For all i: counters[i] == 0 AND resetCounts[i] == 0, IsActive == true
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the bank is inactive. 
    /// Use Reactivate() first if the bank is inactive.</exception>
    public void ResetSensorBank()
    {
        EnsureActive();
            
        Array.Clear(counters, 0, counters.Length);
        Array.Clear(resetCounts, 0, resetCounts.Length);
        // Reactivate since all reset counts are now zero (maintains invariant)
        isActive = true;
    }
    

    /// <summary>
    /// Reactivates an inactive sensor bank and clears all reset counts.
    /// Counter values are preserved to maintain historical data.
    /// 
    /// Precondition: None (can be called whether active or inactive)
    /// Postcondition: IsActive == true, 
    ///                For all i: resetCounts[i] == 0,
    ///                For all i: counters[i] unchanged (preserved)
    /// </summary>
    public void Reactivate()
    {
        isActive = true;
        Array.Clear(resetCounts, 0, resetCounts.Length);
    }
    

    /// <summary>
    /// Gets a string representation of the sensor bank's current state.
    /// 
    /// Precondition: None
    /// Postcondition: Returns string representation, state unchanged (query method)
    /// </summary>
    /// <returns>A string showing active status, size, and max resets.</returns>
    public override string ToString()
    {
        return "SensorBank[Active=" + isActive + ", Size=" + Size + ", MaxResets=" + maxResetsPerCounter + "]";
    }
    
    /// <summary>
    /// Validates that the provided index is within valid bounds.
    /// 
    /// Precondition: None
    /// Postcondition: If valid, returns normally; if invalid, throws exception
    /// </summary>
    /// <param name="index">Index to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= counters.Length)
            throw new ArgumentOutOfRangeException(nameof(index), 
                "Index must be between 0 and " + (counters.Length - 1));
    }
    
    /// <summary>
    /// Ensures the sensor bank is in an active state for mutator operations.
    /// 
    /// Precondition: None
    /// Postcondition: If active, returns normally; if inactive, throws exception
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the bank is inactive.</exception>
    private void EnsureActive()
    {
        if (!isActive)
            throw new InvalidOperationException("Cannot perform this operation because the sensor bank is inactive. Use Reactivate() to restore functionality.");
    }
}