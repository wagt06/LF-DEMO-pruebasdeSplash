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

namespace LIP.Droid
{
    [Activity(Theme = "@style/Theme.Splash", //Indicamos el tema que usaremos en esta actividad
               Label = "LIP PAISAS",
               MainLauncher = true, //Lo inicializamos como pantalla de inicio
               NoHistory = true //No generamos historial para que no nos regrese a esta pantalla
             , ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation,
               ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)] 
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            System.Threading.Thread.Sleep(3000); //Esperamos 3 segundos...
            this.StartActivity(typeof(MainActivity));
        }
    }
}