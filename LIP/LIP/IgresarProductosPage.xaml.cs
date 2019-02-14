﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;
namespace LIP
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class IgresarProductosPage : ContentPage
	{
        public Entidades.Auth Usuario = new Entidades.Auth();
        DataAccess db = new DataAccess();
        public string CodigoProducto;
        public string NombreProducto;
        public Boolean ProductosDiferencias = new Boolean();

        public IgresarProductosPage ()
		{
			InitializeComponent ();
            this.imgScanner.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command (() => { TapImgScannerAsync(); }), NumberOfTapsRequired = 1 });
            
		}

        public void Cargar() {
            this.btnCodigo.Text = CodigoProducto; 
            this.lblNombre.Text = NombreProducto;
            
        }

        async void btnEscanear_Clicked(object sender, EventArgs e)
        {
            var scann = new ZXingBarcodeImageView();
            //setup options
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions
            {
                AutoRotate = false,
                UseFrontCameraIfAvailable = false,
                TryHarder = true,
                UseNativeScanning = true,
                };



            var pagina = new ZXingScannerPage();
            pagina.AutoFocus(); 
            pagina.Title = "Escaneando codigo de barra";

            await Navigation.PushAsync(pagina);

            pagina.OnScanResult += (resultado) =>
            {
                pagina.IsScanning = false;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                    lblResultado.Text = resultado.Text;
                });
            };
        }

        void btnGuardar_Clicked(object sender, EventArgs e)
        {


            var pro = new Entidades.DetalleLevantadoTemp();
            var Resp = new Entidades.Respuesta();
            Services.ProductosServices Productos = new Services.ProductosServices();

            if (string.IsNullOrEmpty(this.txtCantidad.Text))
            {
                Acr.UserDialogs.UserDialogs.Instance.Toast("Ingrese una cantidad! ");
                return;
            }

            this.txtCantidad.Text = TruncateDecimal(decimal.Parse(this.txtCantidad.Text), 2).ToString();

            pro.Codigo_Usuario = Usuario.Codigo_Usuario;
            pro.CodigoSucursal = Usuario.Sucursal;
            pro.Codigo_Ubicacion = Usuario.Codigo_Ubicacion;
            pro.Codigo_Factura = Usuario.Parcial;
            pro.Codigo_Barra = this.lblResultado.Text;
            pro.Codigo_Producto = this.CodigoProducto;
            pro.Bodega = this.Usuario.Bodega;
            pro.Cargado = true;
            pro.Tipo_Origen = 1;
          
            if (Usuario.Conteo == 0)
            {
                pro.Cantidad = double.Parse(this.txtCantidad.Text);
                pro.Resultado = double.Parse(this.txtCantidad.Text); ;
                pro.Tipo_Origen = 1;
            }
            if (Usuario.Conteo == 1)
            {
                pro.Resultado = double.Parse(this.txtCantidad.Text);
                pro.Conteo1 = double.Parse(this.txtCantidad.Text);
                pro.UC1 = Usuario.Codigo_Usuario;
                pro.Tipo_OrigenC1 = 1;
            }
            if (Usuario.Conteo == 2)
            {
                pro.Resultado = double.Parse(this.txtCantidad.Text);
                pro.Conteo2 = double.Parse(this.txtCantidad.Text);
                pro.UC2 = Usuario.Codigo_Usuario;
                pro.Tipo_OrigenC2 = 1;
            }
            if (Usuario.Conteo == 3)
            {
                pro.Resultado = double.Parse(this.txtCantidad.Text);
                pro.Conteo3 = double.Parse(this.txtCantidad.Text);
                pro.UC3 = Usuario.Codigo_Usuario;
                pro.Tipo_OrigenC3 = 1;
            }

            if (double.Parse(this.txtCantidad.Text) == 0)
            {
                pro.NoMostrarApp = 1;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                var monto = new Double();
                try
                {
                    //g = Guar
                    var g = await DisplayAlert(@"LIP Guardar Producto ", "Producto: " + this.NombreProducto + "\n\r"
                                                                  + "Cantidad :" + this.txtCantidad.Text + "\n\r"
                                                                  , "Guardar", "Cancelar");
                    //su guardar es cancelado
                    if (!g)
                    {
                        return;
                    }

                    if (!ProductosDiferencias)
                    {
                        Resp = Productos.GuardarProducto(pro);
                    }
                    else
                    {
                        Resp = Productos.ActualizarProducto(pro);
                    }

                    if (Resp.Code == 3)
                    {
                        var producto = new Entidades.DetalleLevantadoTemp();
                        producto = JsonConvert.DeserializeObject<Entidades.DetalleLevantadoTemp>(Resp.Objeto.ToString());

                        var respuesta = await Acr.UserDialogs.UserDialogs.Instance.PromptAsync(@"ya tiene conteo de " + TruncateDecimal((decimal)producto.Resultado,2) + "\n\r"
                                                                                              + "que monto desea Guardar?. Digite la Opción " + "\n\r" +
                                                                                                "1 = " + pro.Resultado + "\n\r" +
                                                                                                "2 = " + TruncateDecimal((decimal)(producto.Resultado + pro.Resultado),2) + "\n\r" +
                                                                                                "3 = Guardar lo mismo !","LIP - Los Paisas","Guardar","Cancelar", "Escribe la Opción", Acr.UserDialogs.InputType.Default);


                              
                        if (!respuesta.Ok && respuesta.Text !="") {
                            return;
                        }
                        if (respuesta.Text == "1")
                        { //true -> Sobreescribir
                            monto = (double)TruncateDecimal((decimal)pro.Resultado, 2);
                        }
                        else if (respuesta.Text == "2")
                        {// Agregarla
                           
                            monto = (pro.Resultado + producto.Resultado);
                            monto = (double)TruncateDecimal((decimal)monto,2);
                        }
                        else if (respuesta.Text == "3")
                        {// no hacer nada
                            await Navigation.PopAsync();
                        }
                        else {
                            await DisplayAlert("LIP","Ingresar Unicamente si es opción 1 , Opción 2 U Opción 3 ","Aceptar");
                            return;
                        }

                        if (monto == 0)
                        {
                            pro.NoMostrarApp = 1;
                        }

                        if (Usuario.Conteo == 0)
                        {
                            pro.Cantidad = monto;
                            pro.Resultado = monto;
                            pro.Tipo_Origen = 1;

                        }
                        if (Usuario.Conteo == 1)
                        {
                            pro.Resultado = monto;
                            pro.Conteo1 = monto;
                            pro.UC1 = Usuario.Codigo_Usuario;
                            pro.Tipo_OrigenC1 = 1;
                        }
                        if (Usuario.Conteo == 2)
                        {
                            pro.Resultado = monto;
                            pro.Conteo2 = monto;
                            pro.UC2 = Usuario.Codigo_Usuario;
                            pro.Tipo_OrigenC2 = 1;
                        }
                        if (Usuario.Conteo == 3)
                        {
                            pro.Resultado = monto;
                            pro.Conteo3 = monto;
                            pro.UC3 = Usuario.Codigo_Usuario;
                            pro.Tipo_OrigenC3 = 1;
                        }
                        var R = Productos.ActualizarProducto(pro);
                        Resp.Response = R.Response;
                        Resp.Code = R.Code;
                    }




                    if (Resp.Code == 1)
                    {
                        var lista = new List<Entidades.DetalleLevantadoTemp>();
                        lista = db.BuscarProductoDetalle(pro);
                        if (lista.Count > 0)
                        {
                            db.ActualizarProductoDetalle(pro.Codigo_Usuario, pro.Codigo_Producto, pro.Codigo_Factura, pro.CodigoSucursal, pro.Codigo_Ubicacion, pro.Bodega, monto, Usuario.Conteo);
                        }
                        else
                        {
                            db.GuardarProductoDetalle(pro);
                        }

                        await DisplayAlert("LIP", "Cantidad Guardada!", "Aceptar");
                        await Navigation.PopAsync();
                    }

                    if (Resp.Code == 5)
                    {

                        await DisplayAlert("LIP", Resp.Response, "Aceptar");
                        Usuario.Codigo_Ubicacion = 0;
                        db.UpdateLevantado(Usuario);
                        await Navigation.PopAsync();
                    }


                    if (Resp.Code == 6)
                    {
                        await DisplayAlert("LIP", Resp.Response, "Aceptar");
                        Usuario.Codigo_Ubicacion = 0;
                        Usuario.IsCerrado = true;
                        db.UpdateLevantado(Usuario);
                        await Navigation.PopAsync();
                    }
                    if (Resp.Code == 0)
                    {
                        await DisplayAlert("LIP", Resp.Response, "Aceptar");
                    }


                    if (Resp.Response == null)
                    {
                        await DisplayAlert("LIP", "Error Tiempo Espera Excedido!!", "Aceptar");
                    }

                }
                catch (Exception)
                {
                    await DisplayAlert("LIP", "Ocurrio un error al guardar!", "Aceptar");
                    return;
                    // throw;
                }

            });

        }

        public decimal TruncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }

        private void TapImgScannerAsync() {

             this.btnEscanear_Clicked(null, null);  
        }
    }
}