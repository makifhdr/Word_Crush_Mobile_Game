namespace MauiApp5.Models;

public class Cell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public char Letter { get; set; } = '\0';
    public float Scale { get; set; } = 1f;
    public float OffsetX { get; set; } = 0f;
    public float OffsetY { get; set; } = 0;
    public float Alpha { get; set; } = 255f;
    
    public PowerType Power { get; set; } = PowerType.None;
    
    public string GetPowerIcon()
    {
        return Power switch
        {
            PowerType.ClearRow => "—",
            PowerType.ClearColumn => "|",
            PowerType.ClearNeighbors => "+",
            PowerType.ClearBigArea => "*",
            _ => ""
        };
    }
}

public enum PowerType
{
    None,
    ClearRow,
    ClearColumn,
    ClearNeighbors,
    ClearBigArea
}