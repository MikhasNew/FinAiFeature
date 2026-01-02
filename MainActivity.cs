// MainActivity.cs (обновленная версия)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Preference;
using AndroidX.ViewPager2.Widget;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Services;
using EfcToXamarinAndroid.Core.ViewModels;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;
using Google.Android.Material.Tabs;
using NavigationDrawerStarter.Fragments;
using NavigationDrawerStarter.Models;
using NavigationDrawerStarter.Parsers;
using NavigationDrawerStarter.Platform.Droid;
using NavigationDrawerStarter.Settings;
using Xamarin.Essentials;

namespace NavigationDrawerStarter
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType = "application/pdf", DataPathPattern = "*.pdf")]
    public partial class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        // UI Components
        private DrawerLayout drawer;
        private RightMenu _RightMenu;
        private TabLayout tabLayout;
        private ViewPager2 pager;
        private CustomViewPager2Adapter adapter;
        private SmsReceiver smsBroadcastReceiver;

        public static int[] tabIcons = new int[]
        {
            Resource.Mipmap.ic_cash50,
            Resource.Mipmap.ic_in_deposit50,
            Resource.Mipmap.ic_cash_out111,
            Resource.Mipmap.ic_error,
        };

        // Services and ViewModel
        private MainViewModel _viewModel;
        private ISmsReader _smsReader;
        private IFileService _fileService;
        private IDataService _dataService;
        private IUIService _uiService;
        private IPermissionService _permissionService;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            // Theme setup
            SetupTheme();

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // Initialize services
            InitializeServices();

            // Initialize ViewModel
            _viewModel = new MainViewModel(_smsReader, _fileService, _dataService, _uiService, _permissionService);

            // Subscribe to realtime SMS
            _smsReader.SmsReceived += async (s, sms) =>
            {
                try
                {
                    var configManager = ConfigurationManager.ConfigManager;
                    var configuration = configManager.BankConfigurationFromJson;
                    var parser = new EfcToXamarinAndroid.Core.Parsers.Parser(sms, configuration.Banks);
                    var data = await parser.GetData();
                    if (data != null)
                    {
                        await EfcToXamarinAndroid.Core.Repository.DatesRepositorio.AddDatas(data);
                    }
                }
                catch { }
            };

            // Setup UI
            SetupUI();

            // Initialize data
            await _viewModel.InitializeAsync();
            await _viewModel.CheckPermissionsAsync();

            // Handle PDF import if needed
            if (Intent.Action == Intent.ActionView)
            {
                await _viewModel.ProcessPdfFileAsync(Intent.Data.Path);
            }
        }

        private void SetupTheme()
        {
            var config = Resources.Configuration;
            var themeMode = config.UiMode == (UiMode.NightYes | UiMode.TypeNormal);
            SetTheme(themeMode ? Resource.Style.DarkTheme : Resource.Style.LightTheme);
        }

        private void InitializeServices()
        {
            _smsReader = new AndroidSmsReader(this);
            _fileService = new AndroidFileService();
            _dataService = new DataService();
            _uiService = new AndroidUIService(this);
            _permissionService = new AndroidPermissionService(this);
        }

        private void SetupUI()
        {
            // Setup toolbar
            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            // Setup FAB
            var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            // Setup drawer
            drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, drawer, toolbar,
                Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed, GravityCompat.End);
            toggle.SyncState();

            // Setup navigation
            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            // Setup tabs
            SetupTabs();
        }

        private void SetupTabs()
        {
            tabLayout = FindViewById<TabLayout>(Resource.Id.tabLayout);
            tabLayout.InlineLabel = true;
            tabLayout.TabGravity = 0;

            pager = FindViewById<ViewPager2>(Resource.Id.pager);
            pager.OffscreenPageLimit = 3;

            adapter = new CustomViewPager2Adapter(SupportFragmentManager, Lifecycle);
            pager.Adapter = adapter;

            new TabLayoutMediator(tabLayout, pager, new CustomStrategy()).Attach();
        }

        // Event handlers
        private async void FabOnClick(object sender, EventArgs eventArgs)
        {
            var dialog = new AddItemDialog();
            dialog.AddedItem += (sender, e) => dialog.Dismiss();
            dialog.Display(SupportFragmentManager);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.nav_import:
                    _ = Task.Run(async () => await _viewModel.ProcessPdfFileAsync(await _fileService.PickFileAsync("Выберите PDF файл", new[] { ".pdf" })));
                    break;
                case Resource.Id.nav_upload:
                    _ = Task.Run(async () => await _viewModel.ExportDataAsync());
                    break;
                case Resource.Id.nav_restore:
                    _ = Task.Run(async () => await _viewModel.ImportDataAsync());
                    break;
                case Resource.Id.nav_db_clear:
                    _ = Task.Run(async () => await _viewModel.ClearDatabaseAsync());
                    break;
                case Resource.Id.nav_manage:
                    var intent = new Intent(this, typeof(SettingsActivity));
                    StartActivity(intent);
                    break;
            }

            drawer.CloseDrawer(GravityCompat.Start);
            return false;
        }

        // Lifecycle methods
        protected override async void OnResume()
        {
            base.OnResume();
            var configManager = ConfigurationManager.ConfigManager;
            var configuration = configManager.BankConfigurationFromJson;
            var granted = await _viewModel.CheckPermissionsAsync();
            if (granted)
            {
                _smsReader.StartListening(configuration.Banks);
                await _viewModel.ProcessSmsDataAsync();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            _smsReader.StopListening();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}