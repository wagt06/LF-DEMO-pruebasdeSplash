﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Toast;
using Newtonsoft.Json;
using LIP.Services;

namespace LIP
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        ShowToastPopUp toast = new ShowToastPopUp();
        Boolean Session = new Boolean();

        public LoginPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            //this.imgPicker.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { EncentoPicker(); }), NumberOfTapsRequired = 1 });
            this.imgLogin.GestureRecognizers.Add(new  TapGestureRecognizer { Command = new Command(() => {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var respuesta = await Acr.UserDialogs.UserDialogs.Instance.PromptAsync(new Acr.UserDialogs.PromptConfig
                {
                    Message = "Ingrese la contraseña para configurar el server"
                  , InputType = Acr.UserDialogs.InputType.Name
                  , Title = "Configurar Server"
                });

                if (respuesta.Ok  && respuesta.Text == "LIP123")
                {
                    Conf();
                    return;
                }
                });
            }), NumberOfTapsRequired = 2, });
      
        }

        private void Conf() {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var f = new ConfPages.ConfPage();
                await this.Navigation.PushModalAsync(f, true);
            });
        }

        private void VerConexion()
        {
            try
            {
                if (!Services.Utilidades.ConexionServerAsync())
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                        toast.ShowToastMessage("No esta Configurado el server");
                        return;
                    });

                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                    return;
                });
          
            }
            catch (Exception)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                    toast.ShowToastMessage("No esta configurado el server!");
                    return;
                });
            }
          
        }

        private void BtnLogin_ClickedAsync(object sender, EventArgs e)
        {
            this.IsBusy = true;

           

            if (!string.IsNullOrEmpty(this.txtCedula.Text))
            {

                Acr.UserDialogs.UserDialogs.Instance.ShowLoading("Iniciando Session!", Acr.UserDialogs.MaskType.Clear);
                Task.Run(() => this.IniciarSessionAsync()
                );
            }
            else
            {
                toast.ShowToastMessage("Escriba sus datos para iniciar Sessión!");
                this.txtCedula.Focus();
            }
        }
   

        private void IniciarSessionAsync()
        {
            try
            {

                var Servicios = new Services.LoginServices();
                Entidades.Respuesta Resultado;
                Entidades.Auth usuario = new Entidades.Auth();

                if (!Services.Utilidades.ConexionServerAsync())
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                        toast.ShowToastMessage("No esta Configurado el server");
                        return;
                    });

                }

                Resultado = Servicios.LoginAsync(txtCedula.Text.ToUpper());


                if (Resultado.Code == 1) //Repuesta desde Servidor
                {

                    if (Resultado.Objeto != null)
                    {
                        usuario = JsonConvert.DeserializeObject<Entidades.Auth>(Resultado.Objeto.ToString());
                        usuario.Conteo = usuario.Conteo - 1;
                        var f = new MainPage();
                        usuario.Cedula = this.txtCedula.Text;
                        f.Usuario = usuario;
                        f.VienededeLogin = true;
                        f.Lista = Resultado.Lista;
                        f.CargarDatos();

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                            await DisplayAlert("LIP", "Bienvenido :  " + usuario.Nombre, "Aceptar");
                            await this.Navigation.PushAsync(f, true);
                        });
                    }
                    else
                    {
                        if (Resultado.Response == "Este Usuario tiene session activa")
                        {
                            var bd = new DataAccess();
                            var usu = new Entidades.Auth();
                            usu = bd.GetAllLevantado(txtCedula.Text);


                            if (usuario != null)
                            {
                                var f = new MainPage
                                {
                                    bEnSession = true,
                                    Usuario = usuario,
                                    VienededeLogin = true
                                };
                                f.CargarDatos();

                                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    await this.Navigation.PushAsync(f, true);
                                });
                            }
                            else
                            {
                                Session = false;
                                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    await DisplayAlert("LIP", "Este Usuario Tienen una session Abierta en un dispositivo, favor cerrar la sesión", "Ok");
                                    return;

                                });
                            }
                        }
                    }

                }
                if (Resultado.Code == 4)//Encontrado en BD
                {
                    var f = new MainPage
                    {
                        bEnSession = true,
                        Usuario = (Entidades.Auth)Resultado.Objeto,
                        VienededeLogin = true
                    };

                    f.CargarDatos();

                    Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await this.Navigation.PushAsync(f, true);
                    });
                }

                if (Resultado.Code == 3) //Error de Conexion BD
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("LIP", " Error de Conexion", "Aceptar");
                        return;
                    });
                    Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                }
                if (Resultado.Code == 0) //Error de Conexion Servidor
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("LIP", !string.IsNullOrEmpty(Resultado.Response) ? Resultado.Response : "Ocurrio un error de Conexion con el Servidor", "Aceptar");
                        Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                        return;
                    });
                }
                this.IsBusy = false;
            }

            catch (Exception)
            {
                Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("LIP", "Ocurrio un Error", "Ok");
                    return;

                });
                // throw;
            }
        }
       
        protected override void OnAppearing()
        {
            base.OnAppearing();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}