﻿using System;
using System.IO;
using Android.App;
using Android.Widget;
using Android.OS;
using ServiceModel;
using ServiceStack;
using Shared.Client;

namespace Client.Android.Pcl
{
    [Activity(Label = "Client.Android.Pcl", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //AndroidPclExportClient.Configure();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var btnSync = FindViewById<Button>(Resource.Id.btnSync);
            var btnAsync = FindViewById<Button>(Resource.Id.btnAsync);
            var btnAwait = FindViewById<Button>(Resource.Id.btnAwait);
            var btnAuth = FindViewById<Button>(Resource.Id.btnAuth);
            var btnShared = FindViewById<Button>(Resource.Id.btnShared);
            var txtName = FindViewById<EditText>(Resource.Id.txtName);
            var lblResults = FindViewById<TextView>(Resource.Id.lblResults);

            //10.0.2.2 = loopback
            //http://developer.android.com/tools/devices/emulator.html
            var client = new JsonServiceClient("http://10.0.2.2:81/");
            var gateway = new SharedGateway("http://10.0.2.2:81/");

            btnSync.Click += delegate
            {
                try
                {
                    var response = client.Get(new Hello { Name = txtName.Text });
                    lblResults.Text = response.Result;

                    using (var ms = new MemoryStream("Contents".ToUtf8Bytes()))
                    {
                        ms.Position = 0;
                        var fileResponse = client.PostFileWithRequest<HelloResponse>(
                            "/hello", ms, "filename.txt", new Hello { Name = txtName.Text });

                        lblResults.Text = fileResponse.Result;
                    }
                }
                catch (Exception ex)
                {
                    lblResults.Text = ex.ToString();
                }
            };

            btnAsync.Click += delegate
            {
                client.GetAsync(new Hello { Name = txtName.Text })
                    .Success(response => lblResults.Text = response.Result)
                    .Error(ex => lblResults.Text = ex.ToString());
            };

            btnAwait.Click += async delegate
            {
                try
                {
                    var response = await client.GetAsync(new Hello { Name = txtName.Text });
                    lblResults.Text = response.Result;
                }
                catch (Exception ex)
                {
                    lblResults.Text = ex.ToString();
                }
            };

            btnAuth.Click += async delegate
            {
                try
                {
                    await client.PostAsync(new Authenticate
                    {
                        provider = "credentials",
                        UserName = "user",
                        Password = "pass",
                    });

                    var response = await client.GetAsync(new HelloAuth { Name = "Secure " + txtName.Text });

                    lblResults.Text = response.Result;
                }
                catch (Exception ex)
                {
                    lblResults.Text = ex.ToString();
                }
            };

            btnShared.Click += async delegate
            {
                try
                {
                    var greeting = await gateway.SayHello(txtName.Text);
                    lblResults.Text = greeting;
                }
                catch (Exception ex)
                {
                    lblResults.Text = ex.ToString();
                }
            };
        }
    }
}

