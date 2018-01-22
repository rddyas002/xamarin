using Android.App;
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
using static Android.Views.View;
using Android.Views;
using System.Net.Mqtt;
using System.Reactive.Linq;
using Android.Content;

namespace TrackMe
{
    [Activity(Label = "TrackMe", MainLauncher = true)]
    public class MainActivity : Activity, ILocationListener
    {
        static readonly string TAG = "X:" + typeof(Activity).Name;
        LocationManager _locationManager;
        TextView _locationText;
        static CultureInfo ci = new CultureInfo("en-GB", true);
        private IMqttClient client;

        TextView timestamp;
        TextView latitude;
        TextView longitude;
        TextView altitude;
        TextView provider;

        EditText mqttIP;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Log.Debug(TAG, "OnCreate called");
            SetContentView(Resource.Layout.Main);

            // get textView references
            timestamp = FindViewById<TextView>(Resource.Id.txtTimestamp);
            latitude = FindViewById<TextView>(Resource.Id.txtLatitude);
            longitude = FindViewById<TextView>(Resource.Id.txtLongitude);
            altitude = FindViewById<TextView>(Resource.Id.txtAltitude);
            provider = FindViewById<TextView>(Resource.Id.txtProvider);
            mqttIP = FindViewById<EditText>(Resource.Id.webSocketIP);
            mqttIP.Text = "africanplanes.ddns.net";
            _locationText = FindViewById<TextView>(Resource.Id.locationTextView);
            _locationText.Text = "Waiting for GPS";

            // get button references
            Button connectButton = FindViewById<Button>(Resource.Id.connectWebSocket);

            //Assign The Event To Button
            connectButton.Click += delegate {
                //Call Your Method When User Clicks The Button
                btnConnectClicked();
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            Log.Debug(TAG, "OnResume called");

            _locationManager = GetSystemService(Context.LocationService) as LocationManager;
            var locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.Fine;
            locationCriteria.PowerRequirement = Power.Medium;
            string locationProvider = _locationManager.GetBestProvider(locationCriteria, true);
            Log.Debug(TAG, "Starting location updated with " + locationProvider.ToString());
            _locationManager.RequestLocationUpdates(locationProvider, 2000, 10, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            Log.Debug(TAG, "OnPause called");
            client.DisconnectAsync();
            _locationManager.RemoveUpdates(this);
        }

        public async void OnLocationChanged(Location location)
        {
            StringBuilder jsonPacket = new StringBuilder();
            if (location == null)
            {
                _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                jsonPacket.Append("{\"id\":\"S6\",");
                jsonPacket.AppendFormat(ci,"\"time\":{0},", location.Time);
                jsonPacket.Append("\"position\":[");
                jsonPacket.AppendFormat(ci, "{0},{1},{2}],", location.Latitude, location.Longitude, location.Altitude);
                jsonPacket.AppendFormat(ci, "\"accuracy\":{0}}}", location.Accuracy);
                //jsonPacket.AppendFormat(ci, "\"provider\":{0}}}", location.Provider);

                _locationText.Text = jsonPacket.ToString();
                timestamp.Text = location.Time.ToString();
                latitude.Text = location.Latitude.ToString();
                longitude.Text = location.Longitude.ToString();
                altitude.Text = location.Altitude.ToString();
                provider.Text = location.Provider.ToString();
                if (client.IsConnected)
                {
                    var message = new MqttApplicationMessage("TrackMe", Encoding.UTF8.GetBytes(jsonPacket.ToString()));
                    await client.PublishAsync(message, MqttQualityOfService.AtLeastOnce);
                }
            }
        }

        public void OnProviderDisabled(string provider)
        {
            Log.Debug(TAG, "OnProviderDisabled");
        }

        public void OnProviderEnabled(string provider)
        {
            Log.Debug(TAG, "OnProviderEnabled");
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            Log.Debug(TAG, "OnStatusChanged");
        }

        public async void btnConnectClicked()
        {
            client = await MqttClient.CreateAsync("104.196.195.27", 1883);
            await client.ConnectAsync(new MqttClientCredentials("Android", "yashren", "mqtt"));
            await client.SubscribeAsync("TrackMe", MqttQualityOfService.AtMostOnce);
            Toast.MakeText(this, "MQTT connection successful", ToastLength.Long).Show();
        }
    }
}

