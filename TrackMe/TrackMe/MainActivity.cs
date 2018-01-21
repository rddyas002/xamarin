﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Locations;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using System.Text;
using System.Globalization;

namespace TrackMe
{
    [Activity(Label = "TrackMe", MainLauncher = true)]
    public class MainActivity : Activity, ILocationListener
    {
        static readonly string TAG = "X:" + typeof(Activity).Name;
        Location _currentLocation;
        LocationManager _locationManager;
        string _locationProvider;
        TextView _locationText;
        static CultureInfo ci = new CultureInfo("en-GB", true);

        public void OnLocationChanged(Location location)
        {
            StringBuilder jsonPacket = new StringBuilder();
            _currentLocation = location;
            if (_currentLocation == null)
            {
                _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                jsonPacket.Append("{\"id\":\"S6\",");
                jsonPacket.Append("\"position\":[");
                jsonPacket.AppendFormat(ci, "{0},{1},{2}],", _currentLocation.Latitude, _currentLocation.Longitude, _currentLocation.Altitude);
                jsonPacket.AppendFormat(ci, "\"accuracy\":{0},", _currentLocation.Accuracy);
                jsonPacket.AppendFormat(ci, "\"provider\":{0}}}", _currentLocation.Provider);
                _locationText.Text = jsonPacket.ToString();
            }
        }

        public void OnProviderDisabled(string provider)
        {
            
        }

        public void OnProviderEnabled(string provider)
        {
            
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            _locationText = FindViewById<TextView>(Resource.Id.locationTextView);
            _locationText.Text = "Waiting for GPS";

            InitializeLocationManager();
        }

        void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + _locationProvider + ".");
        }

        protected override void OnResume()
        {
            base.OnResume();
            _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }
    }
}

