using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System.Net.Mqtt;
using System.Reactive.Linq;

namespace TrackMe
{
    class MQTT
    {
        private MqttConfiguration config;
        private IMqttClient client;
        private String MQTT_topic = "TrackMe";
        private String MQTT_client_ID = "Yash_ID";

        public MQTT(String ip_address, int host)
        {
            setupAsync(ip_address, host);
        }

        private async void setupAsync(String ip_address, int host)
        {
            client = await MqttClient.CreateAsync(ip_address, host);
            await client.ConnectAsync(new MqttClientCredentials(MQTT_client_ID, "yashren", "mqtt"));
            await client.SubscribeAsync(MQTT_topic, MqttQualityOfService.AtMostOnce);
        }

        public async void MQTT_sendAsync(String data)
        {
            var message = new MqttApplicationMessage(MQTT_topic, Encoding.UTF8.GetBytes(data));
            await client.PublishAsync(message, MqttQualityOfService.AtLeastOnce);
        }

        ~MQTT()
        {
            client.DisconnectAsync();
        }

    }
}