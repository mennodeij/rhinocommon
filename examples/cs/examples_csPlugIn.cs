using System;
using Rhino;

namespace examples_cs
{
  ///<summary>
  /// Every RhinoCommon Plug-In must have one and only one PlugIn derived
  /// class. DO NOT create an instance of this class. It is the responsibility
  /// of Rhino to create an instance of this class.
  ///</summary>
  public class examples_csPlugIn : Rhino.PlugIns.PlugIn
  {
    static examples_csPlugIn m_theplugin;

    public examples_csPlugIn()
    {
      m_theplugin = this;
    }

    public static examples_csPlugIn ThePlugIn
    {
      get { return m_theplugin; }
    }

    protected override bool ShouldCallWriteDocument(Rhino.FileIO.FileWriteOptions options)
    {
      return m_dict.Count > 0;
    }
    protected override void WriteDocument(RhinoDoc doc, Rhino.FileIO.BinaryArchiveWriter archive, Rhino.FileIO.FileWriteOptions options)
    {
      archive.WriteDictionary(m_dict);
    }
    protected override void ReadDocument(RhinoDoc doc, Rhino.FileIO.BinaryArchiveReader archive, Rhino.FileIO.FileReadOptions options)
    {
      m_dict = archive.ReadDictionary();
    }

    Rhino.Collections.ArchivableDictionary m_dict = new Rhino.Collections.ArchivableDictionary(1, "ExampleCSPlugIn Dictionary");
    public void IncrementRunCommandCount()
    {
      int count = 0;
      object value;
      if (m_dict.TryGetValue("RunCommand Count", out value))
      {
        count = (int)value;
      }
      count++;
      m_dict.SetItem("RunCommand Count", count);
    }
  }
}