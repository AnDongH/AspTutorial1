using System;

namespace AspTutorial1.Models.Options;

public class PositionOptions
{
    public const string Position = "Position";

    public string Title { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}