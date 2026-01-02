using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using NavigationDrawerStarter.Fragments;
using ActionBar = AndroidX.AppCompat.App.ActionBar;

namespace NavigationDrawerStarter.Settings
{
    [Activity(Label = "SettingsActivity", Theme = "@style/AppTheme.NoActionBar")]
    public class SettingsActivity : AppCompatActivity, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_settings);
            ActionBar actionBar = this.SupportActionBar;

            if (actionBar != null)
            {
                actionBar.SetDisplayHomeAsUpEnabled(true);
            }
            if (FindViewById(Resource.Id.idFrameLayout1) != null)
            {
                if (savedInstanceState != null)
                {
                    return;
                }
                // below line is to inflate our fragment.
                SupportFragmentManager.BeginTransaction().Add(Resource.Id.idFrameLayout1, new SettingsFragment()).Commit();

            }
        }
        
        protected override void OnResume()
        {
            base.OnResume();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            prefs.RegisterOnSharedPreferenceChangeListener(this);
            
        }

        protected override void OnPause()
        {
            base.OnPause();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            prefs.UnregisterOnSharedPreferenceChangeListener(this);
        }
        #region ISharedPreferencesOnSharedPreferenceChangeListener implementation
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
           
        }
        #endregion
    }
}