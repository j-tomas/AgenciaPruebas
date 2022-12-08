using ServiceReference;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace AgenciaPruebas
{
    class Program
    {
        static void Main(string[] args)
        {
            SuministroLRFacturasEmitidas suministro = new SuministroLRFacturasEmitidas();
            LRfacturasEmitidasType listaFacturas = new LRfacturasEmitidasType();
            FacturaExpedidaType factura = new FacturaExpedidaType();

            // Esta informacion debe coincidir con la identificacion del certificado que se va a usar.
            PersonaFisicaJuridicaESType persona = new PersonaFisicaJuridicaESType();
            persona.NIF = "000000000";
            persona.NombreRazon = "NOMBRE O RAZON SOCIAL";

            CabeceraSii cabecera = new CabeceraSii();
            cabecera.Titular = persona;
            suministro.Cabecera = cabecera;

            RegistroSiiPeriodoLiquidacion periodo = new RegistroSiiPeriodoLiquidacion();
            periodo.Ejercicio = "2022";
            periodo.Periodo = TipoPeriodoType.Item12;
            listaFacturas.PeriodoLiquidacion = periodo;

            IDFacturaExpedidaTypeIDEmisorFactura emisor = new IDFacturaExpedidaTypeIDEmisorFactura();
            emisor.NIF = persona.NIF;
            IDFacturaExpedidaType idFactura = new IDFacturaExpedidaType();
            idFactura.IDEmisorFactura = emisor;
            idFactura.NumSerieFacturaEmisor = "1";
            idFactura.FechaExpedicionFacturaEmisor = "07-12-2022";
            listaFacturas.IDFactura = idFactura;

            PersonaFisicaJuridicaType contraparte = new PersonaFisicaJuridicaType();
            contraparte.NombreRazon = "PRUEBAS S.L.";
            // El Item debe ser un NIF bien formado.
            contraparte.Item = "000000000";
            factura.Contraparte = contraparte;
            factura.TipoFactura = ClaveTipoFacturaType.F1;
            factura.FechaOperacion = idFactura.FechaExpedicionFacturaEmisor;
            factura.ClaveRegimenEspecialOTrascendencia = IdOperacionesTrascendenciaTributariaType.Item01;
            factura.DescripcionOperacion = "Descripcion de la operacion";

            FacturaExpedidaTypeTipoDesglose desglose = new FacturaExpedidaTypeTipoDesglose();
            TipoConDesgloseType conDesglose = new TipoConDesgloseType();
            TipoSinDesglosePrestacionType sinDesglose = new TipoSinDesglosePrestacionType();
            SujetaPrestacionType sujeta = new SujetaPrestacionType();
            SujetaPrestacionTypeNoExenta noExenta = new SujetaPrestacionTypeNoExenta();
            DetalleIVAEmitidaPrestacionType detallePrestacion;
            detallePrestacion = new DetalleIVAEmitidaPrestacionType();
            detallePrestacion.BaseImponible = "10.00";
            detallePrestacion.CuotaRepercutida = "0.04";
            detallePrestacion.TipoImpositivo = "4.00";
            noExenta.DesgloseIVA = new DetalleIVAEmitidaPrestacionType[1];
            noExenta.DesgloseIVA.SetValue(detallePrestacion, 0);
            noExenta.TipoNoExenta = TipoOperacionSujetaNoExentaType.S1;
            sujeta.NoExenta = noExenta;
            sinDesglose.Sujeta = sujeta;
            conDesglose.PrestacionServicios = sinDesglose;
            desglose.Item = conDesglose;
            factura.TipoDesglose = desglose;
            factura.ImporteTotal = "10.04";

            listaFacturas.FacturaExpedida = factura;
            suministro.RegistroLRFacturasEmitidas = new LRfacturasEmitidasType[1];
            suministro.RegistroLRFacturasEmitidas.SetValue(listaFacturas, 0);

            // Cliente SII
            siiSOAPClient siiService;
            RespuestaLRFEmitidasType respuesta;
            try
            {
                siiService = new siiSOAPClient(siiSOAPClient.EndpointConfiguration.SuministroFactEmitidasPruebas);

                X509Certificate2 certificado = SeleccionarCertificado("Seleccione un certificado", StoreLocation.CurrentUser, StoreName.My);
                siiService.ClientCredentials.ClientCertificate.Certificate = certificado;
                siiService.Open();
                if (siiService.State == CommunicationState.Opened)
                {
                    respuesta = siiService.SuministroLRFacturasEmitidas(suministro);
                }
                siiService.Close();
            }
            catch (ProtocolException ex)
            {
                Console.Write(ex.Message);
            }
            catch (WebException ex)
            {
                var st = new StreamReader(ex.Response.GetResponseStream());
                Console.Write(st.ReadToEnd());
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                siiService = null;
            }
        }   // static void Main(string[] args)

        private static X509Certificate2 SeleccionarCertificado(string message, StoreLocation location, StoreName name)
        {
            X509Store store;
            X509Certificate2 certificado = null;
            try
            {
                store = new X509Store(name, location);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection certsSelect = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);
                X509Certificate2Collection certs =
                    X509Certificate2UI.SelectFromCollection(
                        certsSelect,
                        "Selección de certificado",
                        message,
                        X509SelectionFlag.SingleSelection, IntPtr.Zero);
                if (certs.Count > 0)
                {    //Almacenamos el certificado en nuestro objeto interno
                    certificado = certs[0];
                }
                store.Close();
                return certificado;
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                store = null;
            }
        }   // SeleccionarCertificado()

    }   // class Program

}       // namespace AgenciaPruebas

