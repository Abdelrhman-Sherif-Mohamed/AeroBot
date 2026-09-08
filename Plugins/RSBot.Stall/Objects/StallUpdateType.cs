namespace RSBot.Stall.Objects;

public enum StallUpdateType : byte
{
    ItemUpdate = 1,
    ItemAdded = 2,
    ItemRemoved = 3,
    State = 5,
    Note = 6,
    Title = 7
}
