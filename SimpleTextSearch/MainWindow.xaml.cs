using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleTextSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileManager fm;
        private SearchEngine se;
        private static volatile bool firstSearch_bool = true;
        private static volatile bool firstSearch_invrtd = true;
        private static volatile bool firstSearch_pos = true;
        private static volatile bool firstSearch_tfidf = true;
        public MainWindow()
        {
            InitializeComponent();
            fm = new FileManager();
            loadingImg.Visibility = Visibility.Collapsed;
        }
        private async void browseFiles_btn_Click(object sender, RoutedEventArgs e)
        {
            loading();
            msg_lbl.Content = "";
            notify_lbl.Content = "Reading files...";
            await fm.BrowseFiles();
            se = SearchEngine.Instance;
            notify_lbl.Content = "";
            firstSearch_bool = firstSearch_invrtd = firstSearch_pos = true;
            workDone();
        }
        private async void browse_btn_Click(object sender, RoutedEventArgs e)
        {
            loading();
            msg_lbl.Content = "";
            notify_lbl.Content = "Reading files...";
            await fm.BrowseFolders();
            se = SearchEngine.Instance;
            notify_lbl.Content = "";
            firstSearch_bool = firstSearch_invrtd = firstSearch_pos = true;
            workDone();
        }
        private async void boolSearch_btn_Click(object sender, RoutedEventArgs e)
        {
            if (check())
            {
                result_lstView.Items.Clear();
                loading();
                if (firstSearch_bool)
                {
                    notify_lbl.Content = "Building Search Indexes...";
                    await se.RefreshIndexDic(Algorithm.BooleanSearch);
                    firstSearch_bool = false;
                }
                else
                    notify_lbl.Content = "Searching...";
                var sw = new Stopwatch();
                sw.Start();
                var result = await se.Search(search_textBox.Text, Algorithm.BooleanSearch);
                sw.Stop();
                notify_lbl.Content = "";
                msg_lbl.Content = "About " + result.Count.ToString() + " results (" + (sw.ElapsedMilliseconds / 1000.0).ToString() + " seconds)";
                displaySearchResult(result);
                workDone();
            }
        }
        private async void inrtdIdxSearch_btn_Click(object sender, RoutedEventArgs e)
        {
            if (check())
            {
                result_lstView.Items.Clear();
                loading();
                if (firstSearch_invrtd)
                {
                    notify_lbl.Content = "Building Search Indexes...";
                    await se.RefreshIndexDic(Algorithm.InvertedIndexSearch);
                    firstSearch_invrtd = false;
                }
                else
                    notify_lbl.Content = "Searching...";
                var sw = new Stopwatch();
                sw.Start();
                var result = await se.Search(search_textBox.Text, Algorithm.InvertedIndexSearch);
                sw.Stop();
                notify_lbl.Content = "";
                msg_lbl.Content = "About " + result.Count.ToString() + " results (" + (sw.ElapsedMilliseconds / 1000.0).ToString() + " seconds)";
                displaySearchResult(result);
                workDone();
            }
        }
        private async void posIdxSearch_btn_Click(object sender, RoutedEventArgs e)
        {
            if (check())
            {
                result_lstView.Items.Clear();
                loading();
                if (firstSearch_pos)
                {
                    notify_lbl.Content = "Building Search Indexes...";
                    await se.RefreshIndexDic(Algorithm.PositionalIndexSearch);
                    firstSearch_pos = false;
                }
                else
                    notify_lbl.Content = "Searching...";
                var sw = new Stopwatch();
                sw.Start();
                var result = await se.Search(search_textBox.Text, Algorithm.PositionalIndexSearch);
                sw.Stop();
                notify_lbl.Content = "";
                msg_lbl.Content = "About " + result.Count.ToString() + " results (" + (sw.ElapsedMilliseconds / 1000.0).ToString() + " seconds)";
                displaySearchResult(result);
                workDone();
            }
        }
        private async void tfSearch_btn_Click(object sender, RoutedEventArgs e)
        {
            if (check())
            {
                result_lstView.Items.Clear();
                loading();
                if (firstSearch_tfidf)
                {
                    notify_lbl.Content = "Building Search Indexes...";
                    await se.RefreshIndexDic(Algorithm.TFIDFSearchModel);
                    firstSearch_tfidf = false;
                }
                else
                    notify_lbl.Content = "Searching...";
                var sw = new Stopwatch();
                sw.Start();
                var result = await se.Search(search_textBox.Text, Algorithm.TFIDFSearchModel);
                sw.Stop();
                notify_lbl.Content = "";
                msg_lbl.Content = "About " + result.Count.ToString() + " results (" + (sw.ElapsedMilliseconds / 1000.0).ToString() + " seconds)";
                displaySearchResult(result);
                workDone();
            }
        }
        private void displaySearchResult(IDictionary<int, string> resDic)
        {
            foreach (var row in resDic)
            {
                result_lstView.Items.Add(new ResultOutput() { docId = row.Key, docPath = row.Value });
            }
        }
        private async void viewFile(object sender, RoutedEventArgs e)
        {
            var item = (ResultOutput)(sender as ListView).SelectedItem;
            await Task.Run(() => 
            {
                if (item != null)
                {
                    Process.Start(item.docPath);
                }
            });
        }
        private void loading()
        {
            boolSearch_btn.IsEnabled =
            inrtdIdxSearch_btn.IsEnabled =
            posIdxSearch_btn.IsEnabled =
            tfSearch_btn.IsEnabled =
            browseFiles_btn.IsEnabled =
            browseFolder_btn.IsEnabled = false;
            loadingImg.Visibility = Visibility.Visible;
        }
        private void workDone()
        {
            boolSearch_btn.IsEnabled =
            inrtdIdxSearch_btn.IsEnabled =
            posIdxSearch_btn.IsEnabled =
            tfSearch_btn.IsEnabled =
            browseFiles_btn.IsEnabled =
            browseFolder_btn.IsEnabled = true;
            loadingImg.Visibility = Visibility.Collapsed;
        }
        private bool check()
        {
            if (FileManager.docsIds.Count < 1)
            {
                msg_lbl.Content = "*Please select files first!";
                return false;
            }
            else if (search_textBox.Text.Length < 1)
            {
                msg_lbl.Content = "*Please enter a query first!";
                return false;

            }
            else
            {
                msg_lbl.Content = "";
                return true;
            }
        }
    }
    internal class ResultOutput
    {
        public int docId { get; set; }
        public string docPath { get; set; }
    }
}
