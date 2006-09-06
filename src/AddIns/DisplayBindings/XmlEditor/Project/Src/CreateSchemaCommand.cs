// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ICSharpCode.XmlEditor
{
	/// <summary>
	/// Creates a schema based on the xml in the currently active view.
	/// </summary>
	public class CreateSchemaCommand : AbstractMenuCommand
	{
		public CreateSchemaCommand()
		{
		}
		
		public override void Run()
		{
			// Find active XmlView.
			XmlView xmlView = XmlView.ActiveXmlView;
			if (xmlView != null) {
				
				// Create a schema based on the xml.
				SharpDevelopTextEditorProperties properties = xmlView.TextEditorControl.TextEditorProperties as SharpDevelopTextEditorProperties;
				string schema = CreateSchema(xmlView.TextEditorControl.Document.TextContent, xmlView.TextEditorControl.Encoding, properties.ConvertTabsToSpaces, properties.TabIndent);
				
				// Create a new file and display the generated schema.
				string fileName = GenerateSchemaFileName(xmlView.TitleName);
				OpenNewXmlFile(fileName, schema);
			}
		}
		
		/// <summary>
		/// Creates a schema based on the xml content.
		/// </summary>
		/// <returns>A generated schema.</returns>
		string CreateSchema(string xml, Encoding encoding, bool convertTabsToSpaces, int tabIndent)
		{
			string schema = String.Empty;
			
			using (DataSet dataSet = new DataSet()) {
				dataSet.ReadXml(new StringReader(xml), XmlReadMode.InferSchema);
				EncodedStringWriter writer = new EncodedStringWriter(encoding);
				XmlTextWriter xmlWriter = new XmlTextWriter(writer);
				
				xmlWriter.Formatting = Formatting.Indented;
				if (convertTabsToSpaces) {
					xmlWriter.Indentation = tabIndent;
					xmlWriter.IndentChar = ' ';
				} else {
					xmlWriter.Indentation = 1;
					xmlWriter.IndentChar = '\t';
				}
				
				dataSet.WriteXmlSchema(xmlWriter);
				schema = writer.ToString();
				writer.Close();
				xmlWriter.Close();
			}
			
			return schema;
		}
		
		/// <summary>
		/// Opens a new unsaved xml file in SharpDevelop.
		/// </summary>
		void OpenNewXmlFile(string fileName, string xml)
		{
			FileService.NewFile(fileName, XmlView.Language, xml);
		}
		
		/// <summary>
		/// Generates an xsd filename based on the name of the original xml
		/// file.  If a file with the same name is already open in SharpDevelop
		/// then a new name is generated (e.g. MyXml1.xsd).
		/// </summary>
		string GenerateSchemaFileName(string xmlFileName)
		{
			string baseFileName = Path.GetFileNameWithoutExtension(xmlFileName);
			string schemaFileName = String.Concat(baseFileName, ".xsd");
			
			int count = 1;
			while (FileService.IsOpen(schemaFileName)) {
				schemaFileName = String.Concat(baseFileName, count.ToString(), ".xsd");
				++count;
			}
			
			return schemaFileName;
		}
	}
}
