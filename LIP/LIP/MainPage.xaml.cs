﻿
using Android.Widget;
using Newtonsoft.Json;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace LIP
{
    public partial class MainPage : ContentPage
    {


        public Entidades.Auth Usuario = new Entidades.Auth();
        public List<object> Lista = new List<object>();
        public Boolean bEnSession = new Boolean();
        public ObservableCollection<Entidades.Lista> l = new ObservableCollection<Entidades.Lista>();
        Services.EstantesServices Servicio = new Services.EstantesServices();
        Boolean EstanteActivo = new Boolean();
        Boolean cargarCombo = new Boolean();
        public Boolean VienededeLogin = new Boolean();
        ShowToastPopUp t = new ShowToastPopUp();
        DataAccess bd = new DataAccess();

        public MainPage()
        {
            InitializeComponent();
            var c = new Conexion();
            //this..GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { TapImgScannerAsync(); }), NumberOfTapsRequired = 1 });
            this.imgPicker.GestureRecognizers.Add(new TapGestureRecognizer {  Command = new Command(()=> { EncentoPicker(); }),NumberOfTapsRequired = 1});
        }

        private void EncentoPicker()
        {
            this.Estantes.Focus();
        }

        public Boolean CargarDatos()
        {
            try
            {
                cargarCombo = true;
                Entidades.Auth item = new Entidades.Auth();
                Entidades.Respuesta Respuesta = new Entidades.Respuesta();
                BuscarProductoPage f = new BuscarProductoPage();

                var Conteo = string.Empty;
                Conteo = Servicio.Conteo(Usuario);
                Respuesta = Servicio.TraerUbicaciones(Usuario);

                if (Respuesta.Code == 1)
                {
                    Lista = Respuesta.Lista;
                }


                if (Usuario.IsCerrado)
                {
                    Usuario.Codigo_Ubicacion = 0;
                }

                if ((int.Parse(Conteo)) - 1 != Usuario.Conteo)
                {
                    Lista = Respuesta.Lista;
                    Usuario.Conteo = (int.Parse(Conteo)) - 1;
                    Usuario.Codigo_Ubicacion = 0;
                    Usuario.NombreUbicacion = "";
                    bd.EjecutarQueryScalar(string.Format("UPDATE Auth SET  Conteo={0}, isCerrado= 0 ,Codigo_Ubicacion = {2}, NombreUbicacion = '{3}'  WHERE Codigo_Usuario ={1}", (int.Parse(Conteo) - 1), Usuario.Codigo_Usuario, Usuario.Codigo_Ubicacion, Usuario.NombreUbicacion));
                }

                if (l.Count > 0) {
                    l.Clear();
                }




                foreach (object i in Lista)
                {
                    l.Add(JsonConvert.DeserializeObject<Entidades.Lista>(i.ToString()));
                }

                this.BindingContext = l;
                this.Estantes.ItemsSource = l;
                this.lblBodega.Text = Usuario.Bodega.ToString();
                this.lblSucursal.Text = Usuario.Sucursal.ToString();
                this.lblUsuario.Text = Usuario.Nombre;


                if (this.Usuario.Codigo_Ubicacion != 0)
                {
                    this.EstanteActivo = true;
                }
                else
                {
                    this.EstanteActivo = false;
                }

                if (this.EstanteActivo)
                {
                    this.btnContar.Text = "Seguir Contando ";
                    this.btnContar.IsVisible = true;
                    this.Estantes.IsVisible = false;
                    this.imgPicker.IsVisible = false;
                    this.lblEstante.Text =  Usuario.NombreUbicacion;
                }
                else
                {
                    this.Estantes.IsVisible = true;
                    this.btnContar.IsVisible = false;
                    this.imgPicker.IsVisible = true;
                    this.btnContar.Text = "Iniciar Conteo";
                    this.lblEstante.Text = "Estante No Seleccionado";
                }

                cargarCombo = false;
                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                Acr.UserDialogs.UserDialogs.Instance.Toast("Se actualizaron los datos");
                return false;
               

            }
            catch (Exception ex)
            {

                // throw;.
                cargarCombo = false;
                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("LIP", "Ocurrio un error " + ex.Message, "Aceptar");
                });

                return false;
            }

        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            this.VienededeLogin = false;
            BuscarProductoPage f = new BuscarProductoPage
            {
                Usuario = Usuario
            };
            f.Load();
            this.Navigation.PushAsync(f, true);
            if (f.isCerrardo == true)
            {
                this.Estantes.IsVisible = true;
                this.btnContar.IsVisible = false;
            }
            else
            {
                this.btnContar.IsVisible = true;

            }
        }

        private void Estantes_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (cargarCombo)
            {
                return;
            }
            Entidades.Auth item = new Entidades.Auth();
            Entidades.Respuesta Respuesta = new Entidades.Respuesta();
            var r = (Entidades.Lista)this.Estantes.SelectedItem;


            item = Usuario;
            item.NombreUbicacion = r.Nombre;
            item.Codigo_Ubicacion = r.Codigo_Ubicacion;

            Respuesta = Servicio.SeleccionarEstantes(item);
            if (Respuesta.Code == 1)
            {

                
                this.VienededeLogin = false;
                BuscarProductoPage f = new BuscarProductoPage
                {
                    Usuario = item
                };
                f.Load();
                this.Navigation.PushAsync(f, true);
            }
            else
            {
                DisplayAlert("LIP", "Ocurrio un error al seleccionar el estante, Intentelo de nuevo", "Aceptar");
                Acr.UserDialogs.UserDialogs.Instance.ShowLoading("Cerrando Sesión!", Acr.UserDialogs.MaskType.None);
                Device.BeginInvokeOnMainThread(() =>
                { 
                    var Cd = new Boolean();
                    Cd = CargarDatos();
                });
               
            }

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (!this.VienededeLogin)
                {
                    Entidades.Respuesta Respuesta = new Entidades.Respuesta();
                    Usuario = bd.GetAllLevantado(Usuario.Cedula);
                    if (Usuario.IsCerrado)
                    {
                        Usuario.Codigo_Ubicacion = 0;
                    }
                    CargarDatos();
                }

            }
            catch (Exception)
            {
                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                DisplayAlert("LIP", "Ocurrio Un error al Recuperar Datos Db Local", "Aceptar");
                return;
                //throw;
            }
        }

        private void ActEstantes_Clicked(object sender, EventArgs e)
        {
            Acr.UserDialogs.UserDialogs.Instance.ShowLoading("Cargando estantes");
            Device.BeginInvokeOnMainThread(() =>
            {
                Entidades.Respuesta Respuesta = new Entidades.Respuesta();
                Usuario = bd.GetAllLevantado(Usuario.Cedula);
                CargarDatos(); 
            });

        }

        private void CerrarSession_Clicked(object sender, EventArgs e)
        {

            Acr.UserDialogs.UserDialogs.Instance.ShowLoading("Cerrando Sesión!", Acr.UserDialogs.MaskType.None);
            Task.Run(() => CerrarSession());
        }

        private void CerrarSession()
        {
            try
            {
                var servicio = new Services.LoginServices();
                var respuesta = new Entidades.Respuesta();
                respuesta = servicio.CerrarSession(Usuario);

                if (respuesta.Code == 1)
                {
                    bd.DeleteUser(Usuario);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                        DisplayAlert("LIP - PAISAS ", "La session ha sido cerrada!", "OK");
                        Navigation.PopAsync(true);
                    });

                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                        DisplayAlert("LIP - PAISAS ", respuesta.Response != "" ? respuesta.Response : "Error de Conexion", "OK");
                        Navigation.PopAsync(true);
                    });
                }
            }
            catch (Exception)
            {
                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                DisplayAlert("LIP - PAISAS ", "Ocurrion un error", "OK");
                // throw;
            }

        }

        private async void ActualizarProductos_ClickedAsync(object sender, EventArgs e)
        {
            var respuesta = await DisplayAlert("Actualizar Productos", "Seguro que desea actualizar los productos, este proceso tomara unos minutos!", "Aceptar", "Cancelar");
            if (respuesta != true)
            {
                return;
            }
                Acr.UserDialogs.UserDialogs.Instance.ShowLoading("Cargando Items...", Acr.UserDialogs.MaskType.Clear);
                await Task.Run(() =>  CargarProductos());
        }

        private void CargarProductos() {
            try
            {
                var respuesta = bd.InsertProductos(Usuario);
                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                if (respuesta)
                {
                    Acr.UserDialogs.UserDialogs.Instance.Toast(" Items cargados Exitosamente!");
                    Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                }
                else
                {
                    Acr.UserDialogs.UserDialogs.Instance.Toast(" Ocurrio un error al cargar!");
                    Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                }
            }
            catch (Exception)
            {
                Acr.UserDialogs.UserDialogs.Instance.Toast(" Ocurrio un error al cargar!");
                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                //throw;
            }

        }
    }
}
