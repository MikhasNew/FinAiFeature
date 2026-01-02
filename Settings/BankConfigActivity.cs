using Android.App;

using Android.OS;

using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;

using System.Collections.Generic;

using ActionBar = AndroidX.AppCompat.App.ActionBar;

namespace NavigationDrawerStarter.Settings
{
    [Activity(Label = "Банки")]
    public class BankConfigActivity : AppCompatActivity, ListView.IOnItemClickListener, ListView.IOnItemLongClickListener
    {
        ListView listView;
        private List<BankConfiguration> dataItems = new List<BankConfiguration>();
        private static CustomBankConfigAdapter adapter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_bank_config);
            ActionBar actionBar = this.SupportActionBar;
            if (actionBar != null)
            {
                actionBar.SetDisplayHomeAsUpEnabled(true);
            }

            #region ConfigManager
            ConfigurationManager configManager = ConfigurationManager.ConfigManager;
            var configuration = configManager.BankConfigurationFromJson;
            #endregion

            dataItems.AddRange(configuration.Banks);

            listView = (ListView)FindViewById(Resource.Id.bank_config_dateslistView);
            adapter = new CustomBankConfigAdapter(dataItems, this.ApplicationContext);
            listView.SetAdapter(adapter);

            listView.OnItemClickListener =this;
            listView.OnItemLongClickListener =this;
        }
        protected override void OnResume()
        {
            base.OnResume();
          
        }

        protected override void OnPause()
        {
            base.OnPause();
           
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            //var dialog = new SelectItemDialog(dataItems[position]);
            //dialog.EditItemChange += (sender, e) =>
            //{
            //    dialog.Dismiss();
            //};
            //dialog.Display(Activity.SupportFragmentManager);
        }

        public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
        {
            return true;
        }
    }
}