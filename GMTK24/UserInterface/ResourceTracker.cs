﻿using ExplogineMonoGame.Data;
using GMTK24.Model;

namespace GMTK24.UserInterface;

public class ResourceTracker
{
    public ResourceTracker(RectangleF textRectangle, RectangleF iconRectangle, Resource resource)
    {
        IconRectangle = iconRectangle;
        TextRectangle = textRectangle;
        Resource = resource;
        TotalRectangle = RectangleF.Union(iconRectangle, textRectangle);
    }

    public RectangleF IconRectangle { get; }
    public RectangleF TextRectangle { get; }
    public Resource Resource { get; }
    public RectangleF TotalRectangle { get; }
}
