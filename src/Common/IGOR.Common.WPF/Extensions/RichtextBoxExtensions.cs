using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace IGOR.Common.WPF.Extensions
{
    public class RichtextBoxExtensions
    {
        public static string GetDocumentPath(DependencyObject obj)
        {
            return (string)obj.GetValue(DocumentPathProperty);
        }

        public static void SetDocumentPath(DependencyObject obj, string value)
        {
            obj.SetValue(DocumentPathProperty, value);
        }


        public static readonly DependencyProperty DocumentPathProperty = DependencyProperty.RegisterAttached(
            "DocumentPath",
            typeof(string),
            typeof(RichtextBoxExtensions),
            new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = (s, e) =>
                {
                    var document = ((RichTextBox)s).Document;
                    var filePath = e.NewValue as string;

                    var range = new TextRange(document.ContentStart, document.ContentEnd);
                    var text = File.ReadAllText(filePath, Encoding.UTF8);
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
                    range.Load(stream, DataFormats.Text);
                    stream.Close();
                }
            });
    }
}
