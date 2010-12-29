using System;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Geometry;

namespace Rhino.Geometry
{
  public class AnnotationBase : GeometryBase
  {
    internal AnnotationBase(IntPtr native_pointer, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_pointer, parent_object, obj_ref)
    { }
    protected AnnotationBase() { }
    #region internal helper functions
    internal Point2d GetPoint(int which)
    {
      IntPtr pConstThis = ConstPointer();
      Point2d rc = new Point2d();
      UnsafeNativeMethods.ON_Annotation2_GetPoint(pConstThis, which, ref rc);
      return rc;
    }
    internal void SetPoint(int which, Point2d pt)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Annotation2_SetPoint(pThis, which, pt);
    }

    #endregion

    /// <summary>
    /// Return depends on geometry type.
    /// LinearDimension = Distance between arrow tips,
    /// RadialDimension = radius or diamater depending on type
    /// AngularDimension = angle in degrees
    /// Leader or Text = UnsetValue
    /// </summary>
    public double NumericValue
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Annotation2_NumericValue(pConstThis);
      }
    }

    public string Text
    {
      get
      {
        IntPtr pThis = ConstPointer();
        IntPtr pText = UnsafeNativeMethods.ON_Annotation2_Text(pThis, null, false);
        if (pText == IntPtr.Zero)
          return null;
        return Marshal.PtrToStringUni(pText);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Text(pThis, value, false);
      }
    }

    public string TextFormula
    {
      get
      {
        IntPtr pThis = ConstPointer();
        IntPtr pText = UnsafeNativeMethods.ON_Annotation2_Text(pThis, null,true);
        if (pText == IntPtr.Zero)
          return null;
        return Marshal.PtrToStringUni(pText);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Text(pThis, value,true);
      }
    }

    /// <summary>
    /// Text height in model units
    /// </summary>
    public double TextHeight
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Annotation2_Height(pConstThis, false, 0.0);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Height(pThis, true, value);
      }
    }

    /// <summary>
    /// Plane containing the annotation
    /// </summary>
    public Plane Plane
    {
      get
      {
        Plane rc = new Plane();
        IntPtr pThis = ConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Plane(pThis, ref rc, false);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Plane(pThis, ref value, true);
      }
    }


  }



  public class LinearDimension : AnnotationBase
  {
    public LinearDimension()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LinearDimension2_New();
      ConstructNonConstObject(ptr);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addlineardimension2.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlineardimension2.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlineardimension2.py' lang='py'/>
    /// </example>
    public LinearDimension(Plane dimensionPlane, Point2d extensionLine1End, Point2d extensionLine2End, Point2d pointOnDimensionLine)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LinearDimension2_New();
      ConstructNonConstObject(ptr);
      this.Plane = dimensionPlane;
      this.SetLocations(extensionLine1End, extensionLine2End, pointOnDimensionLine);
    }

    public static LinearDimension FromPoints(Point3d extensionLine1End, Point3d extensionLine2End, Point3d pointOnDimensionLine)
    {
      Point3d[] points = new Point3d[] { extensionLine1End, extensionLine2End, pointOnDimensionLine };
      Plane dimPlane;
      if (Plane.FitPlaneToPoints(points, out dimPlane) != PlaneFitResult.Success)
        return null;
      double s, t;
      if (!dimPlane.ClosestParameter(extensionLine1End, out s, out t))
        return null;
      Point2d ext1 = new Point2d(s, t);
      if (!dimPlane.ClosestParameter(extensionLine2End, out s, out t))
        return null;
      Point2d ext2 = new Point2d(s, t);
      if (!dimPlane.ClosestParameter(pointOnDimensionLine, out s, out t))
        return null;
      Point2d linePt = new Point2d(s, t);
      return new LinearDimension(dimPlane, ext1, ext2, linePt);
    }

    internal LinearDimension(IntPtr native_pointer, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_pointer, parent_object, obj_ref)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new LinearDimension(IntPtr.Zero, null, null);
    }

    public double DistanceBetweenArrowTips
    {
      get{ return NumericValue; }
    }

    /// <summary>
    /// Index of DimensionStyle in document DimStyle table used by the dimension
    /// </summary>
    public int DimensionStyleIndex
    {
      get
      {
        IntPtr pThis = ConstPointer();
        return UnsafeNativeMethods.ON_Annotation2_Index(pThis, false, 0);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Index(pThis, true, value);
      }
    }

    const int ext0_pt_index = 0;   // end of first extension line
    const int arrow0_pt_index = 1; // arrowhead tip on first extension line
    const int ext1_pt_index = 2;   // end of second extension line
    const int arrow1_pt_index = 3; // arrowhead tip on second extension line
    const int userpositionedtext_pt_index = 4;

    public Point2d ExtensionLine1End
    {
      get { return GetPoint(ext0_pt_index); }
      //set { SetPoint(ext0_pt_index, value); }
    }

    public Point2d ExtensionLine2End
    {
      get { return GetPoint(ext1_pt_index); }
      //set { SetPoint(ext1_pt_index, value); }
    }

    public Point2d Arrowhead1End
    {
      get { return GetPoint(arrow0_pt_index); }
      //set { SetPoint(arrow0_pt_index, value); }
    }

    public Point2d Arrowhead2End
    {
      get { return GetPoint(arrow1_pt_index); }
      //set { SetPoint(arrow1_pt_index, value); }
    }

    public Point2d TextPosition
    {
      get { return GetPoint(userpositionedtext_pt_index); }
      //set { SetPoint(userpositionedtext_pt_index, value); }
    }

    public void SetLocations(Point2d extensionLine1End, Point2d extensionLine2End, Point2d pointOnDimensionLine)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_LinearDimension2_SetLocations(pThis, extensionLine1End, extensionLine2End, pointOnDimensionLine);
    }

    public bool Aligned
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_LinearDimension2_IsAligned(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_LinearDimension2_SetAligned(pThis, value);
      }
    }
  }

  public class RadialDimension : AnnotationBase
  {
    internal RadialDimension(IntPtr native_pointer, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_pointer, parent_object, obj_ref)
    { }

    public bool IsDiameterDimension
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_RadialDimension2_IsDiameterDimension(pConstThis);
      }
    }
  }

  public class AngularDimension : AnnotationBase
  {
    internal AngularDimension(IntPtr native_pointer, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_pointer, parent_object, obj_ref)
    { }
  }

  public class OrdinateDimension : AnnotationBase
  {
    internal OrdinateDimension(IntPtr native_pointer, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_pointer, parent_object, obj_ref)
    { }
  }

  public class TextEntity : AnnotationBase
  {
    internal TextEntity(IntPtr native_pointer, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_pointer, parent_object, obj_ref)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new TextEntity(IntPtr.Zero, null, null);
    }

    /// <summary>
    /// Index of font in document font table used by the text
    /// </summary>
    public int FontIndex
    {
      get
      {
        IntPtr pThis = ConstPointer();
        return UnsafeNativeMethods.ON_Annotation2_Index(pThis, false, 0);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Index(pThis, true, value);
      }
    }
  }

  public class Leader : AnnotationBase
  {
    internal Leader(IntPtr native_pointer, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_pointer, parent_object, obj_ref)
    { }
  }

  public class TextDot : GeometryBase
  {
    internal TextDot(IntPtr native_pointer, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      :base(native_pointer, parent_object, obj_ref)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new TextDot(IntPtr.Zero, null, null);
    }

    public TextDot(string text, Point3d location)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_TextDot_New(text, location);
      ConstructNonConstObject(ptr);
    }

    public Point3d Point
    {
      get
      {
        Point3d rc = new Point3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_TextDot_GetSetPoint(ptr, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_TextDot_GetSetPoint(ptr, true, ref value);
      }
    }

    public string Text
    {
      get
      {
        IntPtr ptr = ConstPointer();
        IntPtr rc = UnsafeNativeMethods.ON_TextDot_GetSetText(ptr, false, null);
        if (rc == IntPtr.Zero)
          return null;
        return System.Runtime.InteropServices.Marshal.PtrToStringUni(rc);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_TextDot_GetSetText(ptr, true, value);
      }
    }

    // Do not wrap height, fontface and display. These are not currently used in Rhino
  }
}