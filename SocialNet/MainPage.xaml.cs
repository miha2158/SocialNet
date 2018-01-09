using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace SocialNet
{

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<User> AllUsers = new ObservableCollection<User>();

        public User ActiveUser;
        public ObservableCollection<User> ActiveUserSubscriptions;

        public ObservableCollection<NewsItem> Posts;
        private bool displayingUserPosts = false;

        public NewsItem NewNewsItem = new NewsItem();

        private void UpdateCollections()
        {
            ActiveUserSubscriptions = new ObservableCollection<User>(ActiveUser.Subscriptions);
            FriendsList.ItemsSource = ActiveUserSubscriptions;
            FriendsList.UpdateLayout();
            
            Posts = new ObservableCollection<NewsItem>(displayingUserPosts? ActiveUser.News.UserPosts: ActiveUser.News.Feed);

            NewsFeed.ItemsSource = Posts;
            NewsFeed.UpdateLayout();
        }
        private void NewsFeedLabel_OnTapped(object sender, PointerRoutedEventArgs e) { }
        
        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: new friends picker
            
            User r;

            do
                r = AllUsers[Generate.Int(AllUsers.Count)];
            while (ActiveUserSubscriptions.Contains(r) || ActiveUser == r);

            ActiveUser.Subscribe(r);
            UpdateCollections();
        }
        private async void FriendItemName_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog($"Do you really wish to remove {ActiveUserSubscriptions[FriendsList.SelectedIndex]} from your friends?", "Are you sure");
            dialog.Commands.Add(new UICommand("Yes"));
            dialog.Commands.Add(new UICommand("No"));

            if((await dialog.ShowAsync()).Label == "No")
                return;
            ActiveUser.UnSubscribe(ActiveUserSubscriptions[FriendsList.SelectedIndex]);
            UpdateCollections();
        }

        private async void NewUser_Click(object sender, RoutedEventArgs e)
        {
            AllUsers.Add(new User(await Person.MakeNew()));
            
            ChooseUser.SelectedIndex = AllUsers.Count;
        }
        private void ChooseUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveUser = (User)e.AddedItems.Last();
            UserName.Content = ActiveUser.ToString();

            UpdateCollections();
        }
        
        private void PostNewNewsItem_Click(object sender, RoutedEventArgs e)
        {
            NewNewsItem.publisher = ActiveUser;
            NewNewsItem.Content = NewNewsText.Text;
            NewNewsItem.publishTime = DateTime.UtcNow;
            ActiveUser.AddPost(NewNewsItem);
            UpdateCollections();

            NewNewsText.Text = string.Empty;
            AddImageToNewNewsItem.Content = "+";
            
            NewNewsItem = new NewsItem();
        }
        private async void AddImageToNewNewsItem_Click(object sender, RoutedEventArgs e)
        {
            if (NewNewsItem.Image != null)
            {
                NewNewsItem.Image = null;
                AddImageToNewNewsItem.Content = "+";
                return;
            }
            var p = new FileOpenPicker();

            p.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            p.ViewMode = PickerViewMode.Thumbnail;
            p.FileTypeFilter.Add(".jpg");
            p.FileTypeFilter.Add(".jpeg");
            p.FileTypeFilter.Add(".png");
            p.FileTypeFilter.Add(".tiff");
            p.FileTypeFilter.Add(".gif");

            var p1 = await p.PickSingleFileAsync();
            if (p1 == null)
                return;
            NewNewsItem.Image = new BitmapImage(new Uri(p1.Path, UriKind.Absolute));
            AddImageToNewNewsItem.Content = "-";
        }
        private async void NewsPost_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Posts[NewsFeed.SelectedIndex].publisher == ActiveUser)
            {
                MessageDialog dialog = new MessageDialog($"Do you really wish to remove this post?", "Are you sure");
                dialog.Commands.Add(new UICommand("Yes"));
                dialog.Commands.Add(new UICommand("No"));

                if ((await dialog.ShowAsync()).Label == "No")
                    return;

                ActiveUser.RemovePost(Posts[NewsFeed.SelectedIndex]);
                UpdateCollections();
            }
        }

        private void UserName_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            displayingUserPosts = !displayingUserPosts;
            if (displayingUserPosts)
                NewsFeedLabel.Text = "Your Posts";
            else
                NewsFeedLabel.Text = "News Feed";
            UpdateCollections();
        }

        private async void MainGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            int UsersAdded = 6;

            for (int i = 0; i < UsersAdded; i++)
                AllUsers.Add(new User(await Person.MakeNew()));

            AllUsers[1].Subscribe(AllUsers[5]);
            AllUsers[0].Subscribe(AllUsers[1]);
            AllUsers[2].Subscribe(AllUsers[4]);
            AllUsers[4].Subscribe(AllUsers[5]);
            AllUsers[1].Subscribe(AllUsers[2]);
            AllUsers[0].Subscribe(AllUsers[3]);
            AllUsers[0].Subscribe(AllUsers[5]);
            AllUsers[2].Subscribe(AllUsers[3]);



            
            ChooseUser.SelectedIndex = 0;


            //TODO: add a testing thingy

        }
    }
}