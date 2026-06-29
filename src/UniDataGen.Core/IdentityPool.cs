namespace UniDataGen.Core;

/// <summary>
/// A bounded, thread-safe pool of generated keys per entity so Update can target a real prior key
/// and Delete can retire one. When empty the engine warms up rather than inventing keys.
/// </summary>
public sealed class IdentityPool
{
    private readonly int _capacity;
    private readonly List<object> _keys;
    private readonly Random _rng;
    private readonly Lock _gate = new();

    public IdentityPool(int capacity = 100_000, int? seed = null)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be positive.");
        }

        _capacity = capacity;
        _keys = new List<object>(Math.Min(capacity, 1024));
        _rng = seed is { } s ? new Random(s) : new Random();
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _keys.Count;
            }
        }
    }

    /// <summary>Adds a key. When at capacity it replaces a random existing key to keep the pool bounded.</summary>
    public void Add(object key)
    {
        lock (_gate)
        {
            if (_keys.Count < _capacity)
            {
                _keys.Add(key);
            }
            else
            {
                _keys[_rng.Next(_keys.Count)] = key;
            }
        }
    }

    /// <summary>Returns a random key without removing it, for an Update. False when the pool is empty.</summary>
    public bool TryPeek(out object key)
    {
        lock (_gate)
        {
            if (_keys.Count == 0)
            {
                key = default!;
                return false;
            }

            key = _keys[_rng.Next(_keys.Count)];
            return true;
        }
    }

    /// <summary>Removes and returns a random key, for a Delete. False when the pool is empty.</summary>
    public bool TryTake(out object key)
    {
        lock (_gate)
        {
            if (_keys.Count == 0)
            {
                key = default!;
                return false;
            }

            int index = _rng.Next(_keys.Count);
            key = _keys[index];
            _keys[index] = _keys[^1];
            _keys.RemoveAt(_keys.Count - 1);
            return true;
        }
    }
}
