using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace parcer
{
    public partial class MainWindow : Window
    {
        static string first_path, custom_xpath;

        public MainWindow()
        {
            InitializeComponent();
        }

        void Mainpars()
        {
            first_path = url_textbox.Text;
            custom_xpath = xpath_textbox.Text;

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            if (OneCheck.IsChecked ?? true)
            {
                doc = new HtmlWeb().Load(first_path);

                string one_result = Parced_xpath(doc);

                MessageBox.Show("Result: " + one_result);
            }

            else
            {
                List<Hrefsges> parts = new List<Hrefsges>();
                List<string> hrefs = new List<string>(), scaned_links = new List<string>(), parced_xpath_list = new List<string>();

                hrefs.Add(first_path);

                // фильтр от мусора
                string[] filter = new string[] { "@", "#", ";", "+", "!", ".jpg", ".css", ".js", ".png", ".jpeg" };

                string protocol = "https://"; // допилить проверку ответа сервака

                do
                {
                    string path = hrefs[0]; // сделать мультипоточность

                    if (scaned_links.Contains(path) == false)
                    {
                        doc = new HtmlWeb().Load(path);

                        foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                        {
                            HtmlAttribute att = link.Attributes["href"];

                            for (int i = 0; i < filter.Length; i++)
                            {
                                if (att.Value.Contains(filter[i]))
                                    goto endloop; // переход в конец цикла
                            }

                            string parced_link = protocol + Hrefs_hormalizer(att.Value);
                            if (parced_link.Contains("#"))
                                continue;

                            if (hrefs.Contains(parced_link) == false &&
                                scaned_links.Contains(parced_link) == false)
                            {
                                hrefs.Add(parced_link);
                            }

                        endloop:;
                        }

                        parts.Add(new Hrefsges() { Xpath = Parced_xpath(doc).Trim(), URL = path });
                        parced_xpath_list.Add(Parced_xpath(doc).Trim());

                        scaned_links.Add(path);
                        hrefs.Remove(path);

                    }
                } while (hrefs.Count > 0);

                var records = parts;
                using (var writer = new StreamWriter(@"C:\Users\Ilya\Desktop\asd.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }

                MessageBox.Show("Done!");
            }
        }

        public class Hrefsges
        {
            public string URL { get; set; }
            public string Xpath { get; set; }
        }

        static private string Hrefs_hormalizer(string get_link)
        {
            Regex rgx = new Regex(@"[a-z0-9_-]+(\.[a-z0-9_-]+)*\.[a-z]{2,9}");

            Match domain = rgx.Match(first_path);
            Match get_domain = rgx.Match(get_link);

            if (rgx.IsMatch(get_link))
            {
                // проверяет исходящая ли ссылка
                switch (String.Compare(get_domain.ToString(), domain.ToString()) == 0)
                {
                    case true:
                        return get_domain.ToString();
                    case false:
                        return "#";
                }
            }
            return domain + get_link;
        }

        static private string Parced_xpath(HtmlDocument doc)
        {
            try
            {
                return doc.DocumentNode
                    .SelectSingleNode(custom_xpath)
                    .InnerText;
            }
            catch (System.NullReferenceException e)
            {
                return "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        { 
            Mainpars();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            buttongo.IsEnabled = false;
            if (String.IsNullOrEmpty(url_textbox.Text) == false && 
                String.IsNullOrEmpty(xpath_textbox.Text) == false)
            {
                buttongo.IsEnabled = true;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {
            buttongo.IsEnabled = false;
            if (String.IsNullOrEmpty(url_textbox.Text) == false && 
                String.IsNullOrEmpty(xpath_textbox.Text) == false)
            {
                buttongo.IsEnabled = true;
            }
        }

    }
}
