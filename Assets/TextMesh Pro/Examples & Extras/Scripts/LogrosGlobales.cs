using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//pendiente ver consumo y rendimineto. sugeriria activar y desactivar componente segun el uso 
//trigger activa, si no hay pendientes el sistema se auto desactiva

public abstract class Logro
{
    public string nombre;
    public string descripcion;
    public int estado;
    public GameObject imagen;
    public string fecha;

    public abstract bool Progreso();
    public abstract bool ProgresoPrev(string fecha);

}

public class Mision
{
    public string nombre;
    //Oculto, Bloqueado, Incompleto, Completo
    public string estado;
    public string descripcion;
    public List<string> requisitosBlock = new List<string>();
    public List<string> requisitos = new List<string>();
    public List<string> requisitosHechos = new List<string>();
    public List<int> estacion = new List<int>();

    public Mision(string nombrep, string estadop, List<string> req1, List<string> req2, List<string> req3, List<int> estacionp, String descripcionp)
    {
        nombre = nombrep;
        estado = estadop;
        requisitosBlock = req1;
        requisitos = req2;
        requisitosHechos = req3;
        estacion = estacionp;
        descripcion = descripcionp;
    }

    public bool Progreso(string requisito)
    {
        Debug.Log("[Mision] ================================================");
        Debug.Log("[Mision] Progreso llamado - Mision: " + nombre);
        Debug.Log("[Mision] Requisito recibido: '" + requisito + "'");
        Debug.Log("[Mision] Estado actual: " + estado);
        Debug.Log("[Mision] Requisitos pendientes ANTES: [" + string.Join(", ", requisitos.ToArray()) + "] (Count=" + requisitos.Count + ")");
        Debug.Log("[Mision] Requisitos hechos ANTES: [" + string.Join(", ", requisitosHechos.ToArray()) + "] (Count=" + requisitosHechos.Count + ")");
        
        if (estado != "Completa")
        {
            if (estado == "Bloqueada")
            {
                requisitosBlock.Remove(requisito);
                if (requisitosBlock.Count == 0)
                {
                    estado = "Incompleta";
                    Debug.Log("[Mision] Estado cambiado a Incompleta");
                }
            }
            else
            {
                if (requisitos.Contains(requisito))
                {
                    Debug.Log("[Mision] ✓✓✓ Requisito '" + requisito + "' SÍ está en lista de requisitos");
                    requisitosHechos.Add(requisito);
                    Debug.Log("[Mision] Agregado a requisitosHechos");
                }
                else
                {
                    Debug.Log("[Mision] ✗✗✗ Requisito '" + requisito + "' NO está en lista de requisitos - NO se agregará a hechos");
                }

                bool wasRemoved = requisitos.Remove(requisito);
                Debug.Log("[Mision] requisitos.Remove('" + requisito + "') resultado: " + (wasRemoved ? "REMOVIDO" : "NO ESTABA EN LISTA"));
                Debug.Log("[Mision] Requisitos pendientes DESPUÉS: [" + string.Join(", ", requisitos.ToArray()) + "] (Count=" + requisitos.Count + ")");
                Debug.Log("[Mision] Requisitos hechos DESPUÉS: [" + string.Join(", ", requisitosHechos.ToArray()) + "] (Count=" + requisitosHechos.Count + ")");
                
                if (requisitos.Count == 0)
                {
                    estado = "Completa";
                    Debug.Log("[Mision] =====================================");
                    Debug.Log("[Mision] *** MISIÓN COMPLETADA: " + nombre + " ***");
                    Debug.Log("[Mision] =====================================");
                    return true;
                }
                else
                {
                    Debug.Log("[Mision] Misión AÚN NO completada - faltan " + requisitos.Count + " requisitos");
                }
            }
        }
        else
        {
            Debug.Log("[Mision] Misión ya estaba COMPLETA - ignorando llamada");
        }
        
        Debug.Log("[Mision] ================================================");
        return false;
    }

}

public class LogroUnico : Logro
{
    public LogroUnico(string nombreP, string descripcionP, GameObject imagenP)
    {
        nombre = nombreP;
        descripcion = descripcionP;
        estado = 0;
        imagen = imagenP;
        fecha = "";
    }
    public override bool Progreso()
    {
        if (estado == 1)
        {
            return false;
        }
        estado += 1;
        
        if (nombre != "Completo 100%")
        {
            //LogrosGlobales.Completo100.Progreso();
        }

        fecha = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;

        return true;
    }
    public override bool ProgresoPrev(string fechap)
    {
        estado += 1;
        if (nombre != "Completo 100%")

            fecha = fechap;

        return true;
    }

}

public class LogroRepetible : Logro
{
    public int contadorInicial;
    public int iteracionActual;
    public int iteracionMax;


    public LogroRepetible(string nombreP, string descripcionP, int contadorInicialP, int iteracionMaxP, GameObject imagenP)
    {
        nombre = nombreP;
        descripcion = descripcionP;
        estado = contadorInicialP;
        contadorInicial = contadorInicialP;
        iteracionActual = 0;
        iteracionMax = iteracionMaxP;
        imagen = imagenP;
        fecha = "";
    }

    public override bool Progreso()
    {
        if (estado < 1)
        {
            //Debug.Log(nombre + " en estado " + estado);
            estado += 1;
            //Debug.Log(nombre + " Progreso + 1 " + estado);
            if (estado == 1)
            {
                iteracionActual += 1;
                if (iteracionActual < iteracionMax)
                {
                    estado = contadorInicial;
                    //Debug.Log("reset de estado");
                }
                fecha = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
                LogrosGlobales.Completo100.Progreso();
                return true;
            }
        }


        return false;
    }
    public override bool ProgresoPrev(string fechap)
    {
        estado = 1;
        fecha = fechap;
        return true;

    }

}

public class LogrosGlobales : MonoBehaviour
{
    public float countdown = 0;
    public GameObject mochilaGo;

    public GameObject playerCtrl;

    public GameObject PanellistaLogros;
    public GameObject PanellistaMedallas;
    public GameObject medallas;
    public GameObject medallasdetalle;
    public GameObject medallasdetalleTitulo;
    public GameObject medallasdetalleFecha;
    public GameObject medallasdetalleDesc;
    public GameObject medallasdetalleImagen;
    public GameObject misionesLista;
    public GameObject misionesdetalle;
    public GameObject misionesdetalleTitulo;
    public GameObject misionesdetalleEstado;
    public GameObject misionesEstacion;
    public GameObject misionesRequisitos;

    public GameObject refLock;
    public GameObject refCheck;

    public GameObject puntero;

    public List<GameObject> imagenesEstados = new List<GameObject>();

    public List<GameObject> listaMedallas = new List<GameObject>();
    public List<GameObject> SlothsMedallas = new List<GameObject>();

    public List<GameObject> listaSloths = new List<GameObject>();


    public int slothPage;

    public GameObject titulomisiones;

    public bool tempResult;


    public static Logro Completo100;
    public GameObject notificaciones;

    public List<Logro> logros = new List<Logro>();
    public List<Mision> misiones = new List<Mision>();
    
    private bool misionesInicializadas = false;

    //logros individuales tienen nombre y descripcion, 
    //estado negativo faltan requisitos, 0 incompleto, 1 completo pero no mostrado, 2 completo y mostrado

    //Logro encontrar 3 especies
    //deberia ser obligatorio para seguir, para que el niño vaya aprendiendo del tutorial
    public GameObject imageLogro1;

    //Logro de salvar conejo
    //NO OBLIGATORIO (sirve como parametro de perseverancia del niño, cuanto tardó y si renunció, inlcuso si pasó por alto el conejo)
    public GameObject imageLogro2;

    //Logros de alimentar pajaro
    //NO OBLIGATORIO
    public GameObject imageLogro3;

    //Logro de apagar fogata
    //No obligatorio
    public GameObject imageLogro4;

    //Logro de reciclaje
    //No obligatorio
    public GameObject imageLogro5;

    //Logro sembrar 2 arboles
    //NO OBLIGATORIO
    public GameObject imageLogro6;

    //Logro saltar poco
    //NO OBLIGATORIO
    public GameObject imageLogro8;

    //Logro de nivel
    //state debería variar solo de 0 a 1 ya que se pueden subir varios niveles
    //definir exp
    public GameObject imageNivel;

    //Logro de animales vistos
    //states 0, 1, 2 con doble iteración para "haber visto suficientes animales para haber visto todo"
    // segunda iteración vio absolutamente todo
    public GameObject imageFauna;

    //Logro de plantas vistas
    //states 0, 1, 2 con doble iteración para "haber visto suficientes plantas para haber visto todo"
    // segunda iteración vio absolutamente todo
    public GameObject imageFlora;

    //Logro de EJuego terminado
    public GameObject imageCompletado;

    //Logro de Estaciones completas correctamente
    //Settear el numero negativo para que sumados los requisitos den 1
    public GameObject imagePerfecto;

    //checks
    public List<GameObject> checks = new List<GameObject>();
    
    // Helper para acceso seguro a checks
    private void SetCheckSafe(int index, bool active)
    {
        if (checks != null && index >= 0 && index < checks.Count)
        {
            checks[index].SetActive(active);
        }
        else
        {
            Debug.LogWarning("[LogrosGlobales] Intento de acceder a checks[" + index + "] pero la lista tiene " + (checks != null ? checks.Count.ToString() : "0") + " elementos");
        }
    }

    // Start se ejecuta después de Awake de todos los componentes, garantizando que LanguageManager esté listo
    void Start()
    {
        Debug.Log("[LogrosGlobales] ===== START INICIADO =====");
        
        // Verificar si LanguageManager está listo
        if (LanguageManager.Instancia == null)
        {
            Debug.LogWarning("[LogrosGlobales] LanguageManager no está listo en Start() - diferiendo inicialización a Update()");
            misionesInicializadas = false;
            return;
        }
        
        InicializarMisiones();
    }
    
    private void InicializarMisiones()
    {
        if (misionesInicializadas) return;
        
        Debug.Log("[LogrosGlobales] Iniciando InicializarMisiones()...");
        
        try
        {
            tempResult = false;
            //estados Bloqueado Completa Incompleta Oculto

            List<string> requisitosBlo;
            List<string> requisitosComp;
            List<string> requisitosHechos;
            List<int> reqEstaciones;
            Mision mision;

            Debug.Log("[LogrosGlobales] Intentando acceder a LanguageManager...");
            // LanguageManager
            string estacion_mision = LanguageManager.Instancia.ObtenerTexto("misiones.estacion_mision");
            string req_mision = LanguageManager.Instancia.ObtenerTexto("misiones.req_mision");
            string req_completados_mision = LanguageManager.Instancia.ObtenerTexto("misiones.req_completados_mision");

            Debug.Log("[LogrosGlobales] LanguageManager OK - Obteniendo nombres de misiones...");
            string mision_0_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_0_nombre");
            string mision_1_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_1_nombre");
            string mision_2_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_2_nombre");
            string mision_3_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_3_nombre");
            string mision_4_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_4_nombre");
            string mision_5_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_5_nombre");
            string mision_6_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_6_nombre");
            string mision_7_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_7_nombre");
            string mision_8_nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_8_nombre");

            Debug.Log("[LogrosGlobales] Nombres OK - Obteniendo descripciones...");
            string mision_0_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_0_descripcion");
            string mision_1_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_1_descripcion");
            string mision_2_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_2_descripcion");
            string mision_3_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_3_descripcion");
            string mision_4_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_4_descripcion");
            string mision_5_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_5_descripcion");
            string mision_6_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_6_descripcion");
            string mision_7_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_7_descripcion");
            string mision_8_descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_8_descripcion");

            Debug.Log("[LogrosGlobales] Descripciones OK - Checkeando playerCtrl...");
            Debug.Log("[LogrosGlobales] playerCtrl is null? " + (playerCtrl == null));
            
            // Cache Player component to avoid repeated GetComponent calls and null checks
            Player player = null;
            if (playerCtrl != null)
            {
                player = playerCtrl.GetComponent<Player>();
                Debug.Log("[LogrosGlobales] player is null? " + (player == null));
                if (player != null)
                {
                    Debug.Log("[LogrosGlobales] player.playerData is null? " + (player.playerData == null));
                }
            }
            
            Debug.Log("[LogrosGlobales] Iniciando creación de misiones...");
            
            // Validar que el array de misiones tenga el tamaño correcto (9 elementos)
            if (player != null && player.playerData != null)
            {
                if (player.playerData.misiones == null || player.playerData.misiones.Length < 9)
                {
                    Debug.LogWarning("[LogrosGlobales] Array de misiones tiene tamaño incorrecto (" + (player.playerData.misiones != null ? player.playerData.misiones.Length.ToString() : "null") + "). Reinicializando a 9 elementos.");
                    player.playerData.misiones = new bool[9];
                }
            }
            
            /*
             * aqui tambien va lo de los checks
             */
            if (player != null && player.playerData != null && player.playerData.misiones[0])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_0_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_0_descripcion);
            misiones.Add(mision);
            //checks[0].SetActive(false);
            //checks[1].SetActive(true);
            //checks[16].SetActive(false);
            //checks[17].SetActive(true);
        }
        else
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { "Iguana", "Ardilla de Guayaquil", "Pechiche" };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 1, 2 };
            mision = new Mision(mision_0_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_0_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);
        }

        if (player != null && player.playerData != null && player.playerData.misiones[1])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_1_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_1_descripcion);
            misiones.Add(mision);
            SetCheckSafe(2, false);
            SetCheckSafe(3, true);
        }
        else
        {
            requisitosBlo = new List<string>();
            requisitosComp = new List<string>() { "Devolver el conejo a su madriguera" };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 3 };

            mision = new Mision(mision_1_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_1_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);
        }


        if (player != null && player.playerData != null && player.playerData.misiones[2])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_2_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_2_descripcion);
            misiones.Add(mision);
            SetCheckSafe(4, false);
            SetCheckSafe(5, true);
        }
        else
        {
            requisitosBlo = new List<string>();
            // ACTUALIZADO: Cambiar de "Alimentar al Gavilan" (sistema antiguo) a "Test Cadenas Tróficas" (sistema nuevo)
            // Esto debe coincidir con lo que se envía en QuizGavilan.CompletarDesafio() línea ~788
            requisitosComp = new List<string>() { "Test Cadenas Tróficas" };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 4 };

            mision = new Mision(mision_2_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_2_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);
        }

        if (player != null && player.playerData != null && player.playerData.misiones[3])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_3_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_3_descripcion);
            misiones.Add(mision);
            SetCheckSafe(6, false);
            SetCheckSafe(7, true);
        }
        else
        {
            requisitosBlo = new List<string>();
            requisitosComp = new List<string>() { "Apagar la fogata" };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 5 };

            mision = new Mision(mision_3_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_3_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);
        }

        if (player != null && player.playerData != null && player.playerData.misiones[4])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_4_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_4_descripcion);
            misiones.Add(mision);
            SetCheckSafe(8, false);
            SetCheckSafe(9, true);
        }
        else
        {
            requisitosBlo = new List<string>();
            requisitosComp = new List<string>() { "reciclar objetos encontrados" };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 6 };

            mision = new Mision(mision_4_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_4_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);
        }

        if (player != null && player.playerData != null && player.playerData.misiones[5])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_5_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_5_descripcion);
            misiones.Add(mision);
            SetCheckSafe(10, false);
            SetCheckSafe(11, true);
        }
        else
        {
            requisitosBlo = new List<string>();
            requisitosComp = new List<string>() { "Semilla1", "Semilla2" };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 3 };

            mision = new Mision(mision_5_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_5_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);

        }

        if (player != null && player.playerData != null && player.playerData.misiones[6])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_6_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_6_descripcion);
            misiones.Add(mision);
            SetCheckSafe(12, false);
            SetCheckSafe(13, true);
        }
        else
        {
            requisitosBlo = new List<string>();
            requisitosComp = new List<string>() { "Ardilla de Guayaquil", "Iguana", "Pinzón Sabanero", "Tangara Azul y Gris", "Tirano Tropical", "Venado Cola Blanca", "Oso Perezoso", "Zorra Pampera" };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 6 };

            mision = new Mision(mision_6_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_6_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);
        }

        if (player != null && player.playerData != null && player.playerData.misiones[7])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_7_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_7_descripcion);
            misiones.Add(mision);
            SetCheckSafe(14, false);
            SetCheckSafe(15, true);
        }
        else
        {
            requisitosBlo = new List<string>();
            requisitosComp = new List<string>() { "Pechiche",  "Bototillo", "Fernan Sánchez", "Ceibo", "Laurel De Judea", "Jacaranda" };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 3 };

            mision = new Mision(mision_7_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_7_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);
        }

        if (player != null && player.playerData != null && player.playerData.misiones[8])
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() { };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { };
            mision = new Mision(mision_8_nombre, "Completa", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_8_descripcion);
            misiones.Add(mision);
            SetCheckSafe(16, false);
            SetCheckSafe(17, true);
        }
        else
        {
            requisitosBlo = new List<string>() { };
            requisitosComp = new List<string>() {  };
            requisitosHechos = new List<string>() { };
            reqEstaciones = new List<int>() { 1, 2 };
            mision = new Mision(mision_8_nombre, "Incompleta", requisitosBlo, requisitosComp, requisitosHechos, reqEstaciones, mision_8_descripcion);
            misiones.Add(mision);
            //Debug.Log(mision.nombre + " " + mision.estado);
        }


        slothPage = 0;
        
        Debug.Log("[LogrosGlobales] Misiones inicializadas - Total: " + misiones.Count);

        // Validar que el array de logros tenga el tamaño correcto (9 elementos)
        if (player != null && player.playerData != null)
        {
            if (player.playerData.logros == null || player.playerData.logros.Length < 9)
            {
                Debug.LogWarning("[LogrosGlobales] Array de logros tiene tamaño incorrecto (" + (player.playerData.logros != null ? player.playerData.logros.Length.ToString() : "null") + "). Reinicializando a 9 elementos.");
                player.playerData.logros = new string[9] { "", "", "", "", "", "", "", "", "" };
            }
        }

        // LanguageManager Logros
        string logro_0_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_0_nombre");
        string logro_1_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_1_nombre");
        string logro_2_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_2_nombre");
        string logro_3_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_3_nombre");
        string logro_4_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_4_nombre");
        string logro_5_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_5_nombre");
        string logro_6_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_6_nombre");
        string logro_7_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_7_nombre");
        string logro_8_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_8_nombre");
        string logro_9_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_9_nombre");
        string logro_10_nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_10_nombre");

        string logro_0_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_0_descripcion");
        string logro_1_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_1_descripcion");
        string logro_2_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_2_descripcion");
        string logro_3_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_3_descripcion");
        string logro_4_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_4_descripcion");
        string logro_5_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_5_descripcion");
        string logro_6_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_6_descripcion");
        string logro_7_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_7_descripcion");
        string logro_8_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_8_descripcion");
        string logro_9_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_9_descripcion");
        string logro_10_descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_10_descripcion");

        Logro Logro = new LogroUnico(logro_0_nombre, logro_0_descripcion, imageLogro1);
        logros.Add(Logro);
        Debug.Log(Logro.descripcion);
        if (player != null && player.playerData != null && player.playerData.logros[0] != "")
        { ProgresarLogro(0, player.playerData.logros[0]); }
        Logro = new LogroUnico(logro_1_nombre, logro_1_descripcion, imageLogro2);
        logros.Add(Logro);
        if (player != null && player.playerData != null && player.playerData.logros[1] != "")
        { ProgresarLogro(1, player.playerData.logros[1]); }
        //Debug.Log(Logro.descripcion);
        Logro = new LogroUnico(logro_2_nombre, logro_2_descripcion, imageLogro3);
        logros.Add(Logro);
        if (player != null && player.playerData != null && player.playerData.logros[2] != "")
        { ProgresarLogro(2, player.playerData.logros[2]); }
        //Debug.Log(Logro.descripcion);
        Logro = new LogroUnico(logro_3_nombre, logro_3_descripcion, imageLogro4);
        logros.Add(Logro);
        if (player != null && player.playerData != null && player.playerData.logros[3] != "")
        { ProgresarLogro(3, player.playerData.logros[3]); }
        //Debug.Log(Logro.descripcion);
        Logro = new LogroUnico(logro_4_nombre, logro_4_descripcion, imageLogro5);
        logros.Add(Logro);
        if (player != null && player.playerData != null && player.playerData.logros[4] != "")
        { ProgresarLogro(4, player.playerData.logros[4]); }
        //Debug.Log(Logro.descripcion);
        Logro = new LogroUnico(logro_5_nombre, logro_5_descripcion, imageLogro6);
        logros.Add(Logro);
        if (player != null && player.playerData != null && player.playerData.logros[5] != "")
        { ProgresarLogro(5, player.playerData.logros[5]); }
        //Debug.Log(Logro.descripcion);



        Logro = new LogroRepetible(logro_6_nombre, logro_6_descripcion, -7, 1, imageFauna);
        logros.Add(Logro);
        if (player != null && player.playerData != null && player.playerData.logros[6] != "")
        { ProgresarLogro(6, player.playerData.logros[6]); }
        //Debug.Log(Logro.descripcion);
        Logro = new LogroRepetible(logro_7_nombre, logro_7_descripcion, -4, 1, imageFlora);
        logros.Add(Logro);
        if (player != null && player.playerData != null && player.playerData.logros[7] != "")
        { ProgresarLogro(7, player.playerData.logros[7]); }
        //Debug.Log(Logro.descripcion);
        Logro = new LogroUnico(logro_8_nombre, logro_8_descripcion, imageCompletado);
        logros.Add(Logro);
        /*if (player != null && player.playerData.logros[8])
        { ProgresarLogro(8, "antes"); }*/
        //Debug.Log(Logro.descripcion);
        Completo100 = new LogroRepetible(logro_9_nombre, logro_9_descripcion, -13, 1, imagePerfecto);
        logros.Add(Completo100);
        //Debug.Log(Logro.descripcion);
        //Logro = new LogroRepetible("Lvl Up", "Subiste a nivel ", -1, 10, imageNivel);
        //logros.Add(Logro);
        //Debug.Log(Logro.descripcion);
        Debug.Log(logros.Count);
        Logro = new LogroUnico(logro_8_nombre, logro_8_descripcion, imageLogro8);
        logros.Add(Logro);
        Debug.Log(Logro.descripcion);
        if (player != null && player.playerData != null && player.playerData.logros[8] != "")
        { ProgresarLogro(8, player.playerData.logros[8]); }

        RecargarTextos(PlayerPrefs.GetString("idioma"));

        // ProgresarLogro(8);
        
            misionesInicializadas = true;
            Debug.Log("[LogrosGlobales] InicializarMisiones() completado - Total misiones: " + misiones.Count);
            for (int i = 0; i < misiones.Count; i++)
            {
                Debug.Log("[LogrosGlobales] Mision[" + i + "]: " + misiones[i].nombre + " | Estado: " + misiones[i].estado + " | Requisitos: " + string.Join(", ", misiones[i].requisitos.ToArray()));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[LogrosGlobales] ERROR en InicializarMisiones(): " + ex.Message);
            Debug.LogError("[LogrosGlobales] StackTrace: " + ex.StackTrace);
            misionesInicializadas = false; // Permitir reintentar en Update
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Si las misiones no están inicializadas y LanguageManager ya está disponible, inicializarlas
        if (!misionesInicializadas && LanguageManager.Instancia != null)
        {
            Debug.Log("[LogrosGlobales] LanguageManager ahora disponible - inicializando misiones en Update()");
            InicializarMisiones();
        }
        
        /*
        if (Input.GetKeyUp(KeyCode.N))
        {

            PanellistaLogros.SetActive(true);
            titulomisiones.SetActive(true);
            for (int i = 0; i < 6; i++)
            {
                if ((i + 4 * slothPage) < misiones.Count)
                {
                    if (misiones[i + 4 * slothPage].estado == "Oculto")
                    {
                        listaSloths[i].GetComponent<Text>().text = "???????????????????????";
                    }
                    else
                    {
                        listaSloths[i].GetComponent<Text>().text = misiones[i + 4 * slothPage].nombre;
                    }

                }


            }
        }*/
        /*
        if (Input.GetKeyUp(KeyCode.B))
        {
            PanellistaMedallas.SetActive(true);
            for (int i = 0; i < 8; i++)
            {
                if (logros[i].estado == 1)
                {
                    listaMedallas[i].GetComponent<RawImage>().color = new Color(255, 255, 255);
                }
            }
        }*/

    }
    public void abrirMedallas()
    {
        PanellistaMedallas.SetActive(true);
        mochilaGo.SetActive(false);
        puntero.SetActive(false);
        for (int i = 0; i < 11; i++)
        {
            if (logros[i].estado == 1)
            {
                listaMedallas[i].GetComponent<RawImage>().color = new Color(255, 255, 255);
                SlothsMedallas[i].GetComponent<RawImage>().color = new Color(255, 255, 0);
            }
        }
    }
    public void abrirMisiones()
    {
        PanellistaLogros.SetActive(true);
        titulomisiones.SetActive(true);
        mochilaGo.SetActive(false);
        for (int i = 0; i < 11; i++)
        {
            if ((i + 4 * slothPage) < misiones.Count)
            {
                if (misiones[i + 4 * slothPage].estado == "Oculta")
                {
                    listaSloths[i].GetComponent<Text>().text = "???????????????????????";
                }
                else
                {
                    listaSloths[i].GetComponent<Text>().text = misiones[i + 4 * slothPage].nombre;
                    if (misiones[i + 4 * slothPage].estado == "Bloqueada")
                    {
                        imagenesEstados[i].GetComponent<Image>().sprite = refLock.GetComponent<Image>().sprite;
                    }
                    else if (misiones[i + 4 * slothPage].estado == "Completa")
                    {
                        imagenesEstados[i].GetComponent<Image>().sprite = refCheck.GetComponent<Image>().sprite;
                    }
                }

            }


        }
    }

    public bool ProgresarLogro(int numeroLogro)
    {
        if (numeroLogro == 20) {
            // notificaciones.transform.Find("Titulo").gameObject.SetActive(false);
            for (int i = 0; i < notificaciones.transform.childCount; i++)
            {
                Debug.Log(notificaciones.transform.GetChild(i).name);
            }

            notificaciones.SetActive(true);
            countdown = 4;
            notificaciones.GetComponent<NotificarLogros>().Encolar("Recordatorio No Saltar", "Primer Aviso: es peligroso saltar, no saltes.", logros[8].imagen);
        }
        //Debug.Log("**********************logros:" + logros.Count);
        tempResult = logros[numeroLogro].Progreso();
        //Debug.Log("**********************en progresar logro inicio es " + tempResult);
        if (tempResult)
        {
            notificaciones.SetActive(true);
            countdown = 4;
            notificaciones.GetComponent<NotificarLogros>().Encolar(logros[numeroLogro].nombre, logros[numeroLogro].descripcion, logros[numeroLogro].imagen);
        }
        //Debug.Log("**********************en progresar envia " + tempResult);
        if (tempResult)
        {
            //Debug.Log("**********************se progresa logro " + numeroLogro);
            playerCtrl.GetComponent<Player>().regLogro(numeroLogro, DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year);
        }
        return tempResult;

    }

    public bool ProgresarLogro(int numeroLogro, string fechap)
    {
        tempResult = logros[numeroLogro].ProgresoPrev(fechap);

        return tempResult;

    }

    public void ProgresarMision(int numeromision, string cumplido)
    {
        // Verificar que las misiones estén inicializadas
        if (!misionesInicializadas || misiones == null || misiones.Count == 0)
        {
            Debug.LogWarning("[LogrosGlobales] ProgresarMision llamado pero misiones no inicializadas aún - ignorando");
            return;
        }
        
        if (numeromision < 0 || numeromision >= misiones.Count)
        {
            Debug.LogError("[LogrosGlobales] ProgresarMision: índice fuera de rango: " + numeromision + " (total misiones: " + misiones.Count + ")");
            return;
        }
        
        Debug.Log("Numero de mision: " + numeromision);

        // Debug.Log("**********************se recibe el nombre " + cumplido);
        tempResult = misiones[numeromision].Progreso(cumplido);
        Debug.Log("Tempresult de salto: " + tempResult);

        if (tempResult)
        {

            Debug.Log("Entro al tempResult");
            /*
             *aqui va lo de los checks
             **/
            SetCheckSafe(numeromision * 2, false);
            SetCheckSafe(numeromision * 2 + 1, true);
            Debug.Log("**********************se progresa mision " + numeromision);
            playerCtrl.GetComponent<Player>().regMision(numeromision);
            if (numeromision == 6)
            {
                Peticiones.instance.registerPlayerMission(misiones[6].nombre, Player.instance.playerData, Player.instance.playerData.gameStart.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                Peticiones.instance.registerPlayerPrize(logros[6].nombre, Player.instance.playerData);
                playerCtrl.GetComponent<Player>().gainEXP(6);
            }
            else if (numeromision == 7)
            {
                Peticiones.instance.registerPlayerMission(misiones[7].nombre, Player.instance.playerData, Player.instance.playerData.gameStart.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                Peticiones.instance.registerPlayerPrize(logros[7].nombre, Player.instance.playerData);
                playerCtrl.GetComponent<Player>().gainEXP(6);
            }
        }
    }
    public void NextPage()
    {

        slothPage += 1;
        for (int i = 0; i < 4; i++)
        {
            if ((i + 4 * slothPage) < misiones.Count)
            {
                if (misiones[i + 4 * slothPage].estado == "Oculta")
                {
                    listaSloths[i].GetComponent<Text>().text = "???????????????????????";
                }
                else
                {
                    listaSloths[i].GetComponent<Text>().text = misiones[i + 4 * slothPage].nombre;
                }
            }
            else
            {
                listaSloths[i].GetComponent<Text>().text = "";
            }


        }
    }
    public void PrevPage()
    {
        if (slothPage > 0)
        {
            slothPage -= 1;
            for (int i = 0; i < 4; i++)
            {
                if ((i + 4 * slothPage) < misiones.Count)
                {
                    if (misiones[i + 4 * slothPage].estado == "Oculta")
                    {
                        listaSloths[i].GetComponent<Text>().text = "???????????????????????";
                    }
                    else
                    {
                        listaSloths[i].GetComponent<Text>().text = misiones[i + 4 * slothPage].nombre;
                    }
                }


            }
        }

    }


    public void InfMed(int medalla)
    {
        string obt = LanguageManager.Instancia.ObtenerTexto("logros.info_medalla");

        //Debug.Log("Info de medalla" + medalla);
        medallas.SetActive(false);

        medallasdetalle.SetActive(true);

        medallasdetalleTitulo.GetComponent<Text>().text = logros[medalla].nombre;
        medallasdetalleFecha.GetComponent<Text>().text = obt + logros[medalla].fecha;
        medallasdetalleDesc.GetComponent<Text>().text = logros[medalla].descripcion;
        medallasdetalleImagen.GetComponent<RawImage>().texture = logros[medalla].imagen.GetComponent<RawImage>().texture;



    }
    public void DisplayMed()
    {

        medallasdetalle.SetActive(false);
        medallas.SetActive(true);
    }

    public void InfMis(int mision)
    {

        if ((mision + 4 * slothPage) < misiones.Count)
        {
            titulomisiones.SetActive(false);
            misionesLista.SetActive(false);
            misionesdetalle.SetActive(true);
            if (misiones[mision + 4 * slothPage].estado == "Oculta")
            {
                misionesdetalleTitulo.GetComponent<Text>().text = "????????????????????";
                misionesdetalleEstado.GetComponent<Text>().text = "????????????????????";
                misionesEstacion.GetComponent<Text>().text = "????????????????";
                misionesRequisitos.GetComponent<Text>().text = "????????????????\n????????????????\n????????????????\n";
            }
            else
            {
                string estacionText = LanguageManager.Instancia.ObtenerTexto("misiones.estacion_mision");  
                string reqText = LanguageManager.Instancia.ObtenerTexto("misiones.req_mision");  
                string reqCompText = LanguageManager.Instancia.ObtenerTexto("misiones.req_completados_mision");  
                
                misionesdetalleTitulo.GetComponent<Text>().text = misiones[mision + 4 * slothPage].nombre;
                misionesdetalleEstado.GetComponent<Text>().text = misiones[mision + 4 * slothPage].estado;

                misionesEstacion.GetComponent<Text>().text = estacionText;
                foreach (int est in misiones[mision + 4 * slothPage].estacion)
                {
                    misionesEstacion.GetComponent<Text>().text = misionesEstacion.GetComponent<Text>().text + est + "    ";
                }

                misionesRequisitos.GetComponent<Text>().text = misiones[mision + 4 * slothPage].descripcion + "\n\n";
                misionesRequisitos.GetComponent<Text>().text = misionesRequisitos.GetComponent<Text>().text + reqText;
                if (misiones[mision + 4 * slothPage].estado == "Bloqueada")
                {
                    foreach (string req in misiones[mision + 4 * slothPage].requisitosBlock)
                    {
                        misionesRequisitos.GetComponent<Text>().text = misionesRequisitos.GetComponent<Text>().text + "-" + req + "\n";
                    }
                }
                else
                {
                    foreach (string req in misiones[mision + 4 * slothPage].requisitos)
                    {
                        misionesRequisitos.GetComponent<Text>().text = misionesRequisitos.GetComponent<Text>().text + "-" + req + "\n";
                    }
                }

                misionesRequisitos.GetComponent<Text>().text = misionesRequisitos.GetComponent<Text>().text + reqCompText;
                if (misiones[mision + 4 * slothPage].estado == "Bloqueada")
                {
                    foreach (string req in misiones[mision + 4 * slothPage].requisitosBlock)
                    {
                        misionesRequisitos.GetComponent<Text>().text = misionesRequisitos.GetComponent<Text>().text + "-" + req + "\n";
                    }
                }
                else
                {
                    foreach (string req in misiones[mision + 4 * slothPage].requisitosHechos)
                    {
                        misionesRequisitos.GetComponent<Text>().text = misionesRequisitos.GetComponent<Text>().text + "<color=green>-" + req + "</color>\n";
                    }
                }

            }




        }
        else
        {
            misionesRequisitos.GetComponent<Text>().text = "";
        }

    }
    public void DisplayMis()
    {
        titulomisiones.SetActive(true);
        misionesdetalle.SetActive(false);
        misionesLista.SetActive(true);
    }

    private void OnEnable()
    {
        LanguageEvents.OnLanguageChanged += RecargarTextos;
    }

    private void OnDisable()
    {
        LanguageEvents.OnLanguageChanged -= RecargarTextos;
    }

    private void RecargarTextos(string idioma)
    {
        Debug.Log("♻️ Actualizando textos de misiones y logros al idioma: " + idioma);

        // Actualizar nombres y descripciones de misiones
        if (misiones.Count >= 8) // asumimos 8 misiones como en Start()
        {
            misiones[0].nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_0_nombre");
            misiones[0].descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_0_descripcion");
            misiones[1].nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_1_nombre");
            misiones[1].descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_1_descripcion");
            misiones[2].nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_2_nombre");
            misiones[2].descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_2_descripcion");
            misiones[3].nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_3_nombre");
            misiones[3].descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_3_descripcion");
            misiones[4].nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_4_nombre");
            misiones[4].descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_4_descripcion");
            misiones[5].nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_5_nombre");
            misiones[5].descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_5_descripcion");
            misiones[6].nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_6_nombre");
            misiones[6].descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_6_descripcion");
            misiones[7].nombre = LanguageManager.Instancia.ObtenerTexto("misiones.mision_7_nombre");
            misiones[7].descripcion = LanguageManager.Instancia.ObtenerTexto("misiones.mision_7_descripcion");
        }

        // Actualizar nombres y descripciones de logros
        if (logros.Count >= 10) // asumimos 10 logros como en Start()
        {
            logros[0].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_0_nombre");
            logros[0].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_0_descripcion");
            logros[1].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_1_nombre");
            logros[1].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_1_descripcion");
            logros[2].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_2_nombre");
            logros[2].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_2_descripcion");
            logros[3].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_3_nombre");
            logros[3].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_3_descripcion");
            logros[4].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_4_nombre");
            logros[4].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_4_descripcion");
            logros[5].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_5_nombre");
            logros[5].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_5_descripcion");
            logros[6].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_6_nombre");
            logros[6].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_6_descripcion");
            logros[7].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_7_nombre");
            logros[7].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_7_descripcion");
            logros[8].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_8_nombre");
            logros[8].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_8_descripcion");
            logros[9].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_9_nombre");
            logros[9].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_9_descripcion");
            logros[10].nombre = LanguageManager.Instancia.ObtenerTexto("logros.logro_10_nombre");
            logros[10].descripcion = LanguageManager.Instancia.ObtenerTexto("logros.logro_10_descripcion");
        }

        // Actualizar UI si los paneles están abiertos
        //abrirMisiones();
        //abrirMedallas();
    }

}