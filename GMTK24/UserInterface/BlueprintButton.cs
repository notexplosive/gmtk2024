using GMTK24.Model;

namespace GMTK24.UserInterface;

public readonly record struct BlueprintButton(string Name, Blueprint Blueprint, bool IsLocked = false);
