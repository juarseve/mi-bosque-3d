using System;
using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instancia { get; private set; }

    private Dictionary<string, string> textos = new Dictionary<string, string>();
    public event Action OnIdiomaCambiado;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        // Comprobar si ya hay un idioma guardado
        string idioma;
        if (PlayerPrefs.HasKey("idioma"))
        {
            idioma = PlayerPrefs.GetString("idioma");
        }
        else
        {
            idioma = "textos_espanol"; // idioma por defecto
            PlayerPrefs.SetString("idioma", idioma);
        }

        Debug.Log("Idioma cargado desde PlayerPrefs: " + idioma);
        CargarIdioma(idioma);
    }

    //void Start()
    //{
    //    // Solo para pruebas: eliminar idioma guardado
    //    PlayerPrefs.DeleteKey("idioma");
    //    PlayerPrefs.Save();
    //}

    public void CargarIdioma(string archivoIdioma)
    {
        PlayerPrefs.SetString("idioma", archivoIdioma);

        TextAsset archivo = Resources.Load<TextAsset>(archivoIdioma);
        if (archivo == null)
        {
            Debug.LogError("Archivo de idioma no encontrado: " + archivoIdioma);
            return;
        }

        // Limpiar el diccionario antes de cargar uno nuevo
        textos.Clear();

        // Deserializar usando JsonUtility
        DatosIdioma datos = JsonUtility.FromJson<DatosIdioma>(archivo.text);
        if (datos == null)
        {
            Debug.LogError("No se pudo deserializar el archivo de idioma.");
            return;
        }

        // Cargar textos del JSON al diccionario plano
        CargarTextos(datos);

        // Notificar a los elementos que el idioma cambió
        OnIdiomaCambiado?.Invoke();
    }

    public string ObtenerTexto(string clave)
    {
        if (textos.TryGetValue(clave, out string texto))
            return texto;
        else
            return $"[{clave}]";
    }

    private void CargarTextos(DatosIdioma datos)
    {
        if (datos.botones != null)
        {
            textos["botones.retroceder"] = datos.botones.retroceder;
            textos["botones.continuar"] = datos.botones.continuar;
            textos["botones.informacion"] = datos.botones.informacion;
            textos["botones.entendido"] = datos.botones.entendido;
            textos["botones.minimizar"] = datos.botones.minimizar;
            textos["botones.exportar"] = datos.botones.exportar;
        }

        if (datos.interfaz != null)
        {
            textos["interfaz.pausa"] = datos.interfaz.pausa;
            textos["interfaz.estacion"] = datos.interfaz.estacion;
            textos["interfaz.desafios"] = datos.interfaz.desafios;
        }

        if (datos.perfil != null)
        {
            textos["perfil.title"] = datos.perfil.title;
            textos["perfil.experiencia"] = datos.perfil.experiencia;
            textos["perfil.uni_edu"] = datos.perfil.uni_edu;
            textos["perfil.nivel"] = datos.perfil.nivel;
            textos["perfil.edad"] = datos.perfil.edad;
        }

        if (datos.menu_inventario != null)
        {
            textos["menu_inventario.perfil"] = datos.menu_inventario.perfil;
            textos["menu_inventario.misiones"] = datos.menu_inventario.misiones;
            textos["menu_inventario.descripcion_misiones"] = datos.menu_inventario.descripcion_misiones;
            textos["menu_inventario.desafios"] = datos.menu_inventario.desafios;
            textos["menu_inventario.logros"] = datos.menu_inventario.logros;
            textos["menu_inventario.preguntas"] = datos.menu_inventario.preguntas;
            textos["menu_inventario.no_desbloq"] = datos.menu_inventario.no_desbloq;
            textos["menu_inventario.respuesta"] = datos.menu_inventario.respuesta;
            textos["menu_inventario.estaciones"] = datos.menu_inventario.estaciones;
        }

        if (datos.menu_mochila != null)
        {
            textos["menu_mochila.semillas"] = datos.menu_mochila.semillas;
            textos["menu_mochila.accesorios"] = datos.menu_mochila.accesorios;
        }

        if (datos.menu_pausa != null)
        {
            var mp = datos.menu_pausa;
            textos["menu_pausa.guia"] = mp.guia;
            textos["menu_pausa.menu"] = mp.menu;
            textos["menu_pausa.ajustes"] = mp.ajustes;
            textos["menu_pausa.mapa"] = mp.mapa;
            textos["menu_pausa.lobby"] = mp.lobby;
            textos["menu_pausa.salir"] = mp.salir;
            textos["menu_pausa.diploma"] = mp.diploma;
            textos["menu_pausa.modo"] = mp.modo;
            textos["menu_pausa.continuar"] = mp.continuar;
            textos["menu_pausa.noti_diploma"] = mp.noti_diploma;

            textos["menu_pausa.ajuste_resolucion"] = mp.ajuste_resolucion;
            textos["menu_pausa.calidad_maxima"] = mp.calidad_maxima;
            textos["menu_pausa.calidad_media"] = mp.calidad_media;
            textos["menu_pausa.calidad_minima"] = mp.calidad_minima;

            textos["menu_pausa.ajuste_modo"] = mp.ajuste_modo;
            textos["menu_pausa.modo_contrarreloj"] = mp.modo_contrarreloj;
            textos["menu_pausa.modo_normal"] = mp.modo_normal;
            textos["menu_pausa.volver"] = mp.volver;
        }
        
        if (datos.menu_inicial != null)
        {
            textos["menu_inicial.boton_jugar"] = datos.menu_inicial.boton_jugar;
            textos["menu_inicial.boton_explorar"] = datos.menu_inicial.boton_explorar;
            textos["menu_inicial.boton_salir"] = datos.menu_inicial.boton_salir;
        }

        if (datos.menu_partidas != null)
        {
            if(datos.menu_partidas.nueva_partida != null)
            {
                var np = datos.menu_partidas.nueva_partida;
                textos["menu_partidas.title_avatar"] = np.title_avatar;
                textos["menu_partidas.title_datos"] = np.title_datos;
                textos["menu_partidas.label_nombre"] = np.label_nombre;
                textos["menu_partidas.label_edad"] = np.label_edad;
                textos["menu_partidas.label_genero"] = np.label_genero;
                textos["menu_partidas.genero_h"] = np.genero_h;
                textos["menu_partidas.genero_m"] = np.genero_m;
                textos["menu_partidas.genero_otro"] = np.genero_otro;
                textos["menu_partidas.label_pin"] = np.label_pin;
                textos["menu_partidas.pin_invitado"] = np.pin_invitado;
                textos["menu_partidas.invitado_1"] = np.invitado_1;
                textos["menu_partidas.invitado_2"] = np.invitado_2;

            }

            if(datos.menu_partidas.crear_cargar_partida != null)
            {
                var cnp = datos.menu_partidas.crear_cargar_partida;
                textos["menu_partidas.cargar_partida"] = cnp.cargar_partida;
                textos["menu_partidas.nueva_partida"] = cnp.nueva_partida;
            }
        }

        if (datos.mensajes_error != null)
        {
            var me = datos.mensajes_error;
            textos["mensajes_error.error_0"] = me.error_0;
            textos["mensajes_error.error_1"] = me.error_1;
            textos["mensajes_error.error_2"] = me.error_2;
            textos["mensajes_error.error_3"] = me.error_3;
            textos["mensajes_error.error_4"] = me.error_4;
            textos["mensajes_error.error_5_0"] = me.error_5_0;
            textos["mensajes_error.error_5_1"] = me.error_5_1;
        }

        if (datos.pista_luz != null)
        {
            textos["pista_luz.title"] = datos.pista_luz.title;
            textos["pista_luz.nombre"] = datos.pista_luz.nombre;
            textos["pista_luz.descripcion"] = datos.pista_luz.descripcion;
        }

        if(datos.tutorial != null)
        {
            textos["tutorial.dialogo_0"] = datos.tutorial.dialogo_0;
            textos["tutorial.dialogo_1"] = datos.tutorial.dialogo_1;
            textos["tutorial.dialogo_2"] = datos.tutorial.dialogo_2;
            textos["tutorial.dialogo_3"] = datos.tutorial.dialogo_3;
            textos["tutorial.dialogo_4"] = datos.tutorial.dialogo_4;
            textos["tutorial.dialogo_5"] = datos.tutorial.dialogo_5;
            textos["tutorial.dialogo_6"] = datos.tutorial.dialogo_6;
            textos["tutorial.dialogo_7"] = datos.tutorial.dialogo_7;
            textos["tutorial.dialogo_8"] = datos.tutorial.dialogo_8;
            textos["tutorial.dialogo_9"] = datos.tutorial.dialogo_9;
            textos["tutorial.dialogo_10"] = datos.tutorial.dialogo_10;
            textos["tutorial.dialogo_11"] = datos.tutorial.dialogo_11;
            textos["tutorial.dialogo_12"] = datos.tutorial.dialogo_12;
            textos["tutorial.son"] = datos.tutorial.son;
            textos["tutorial.vamos"] = datos.tutorial.vamos;
        }

        if (datos.lobby != null)
        {
            if (datos.lobby.bruno != null)
            {
                var br = datos.lobby.bruno;
                textos["lobby.bruno.title_0"] = br.title_0;
                textos["lobby.bruno.title_1"] = br.title_1;
                textos["lobby.bruno.title_2"] = br.title_2;
                textos["lobby.bruno.sentence_0"] = br.sentence_0;
                textos["lobby.bruno.sentence_1"] = br.sentence_1;
                textos["lobby.bruno.sentence_2"] = br.sentence_2;
            }

            if (datos.lobby.cabanas_info != null)
            {
                var ci = datos.lobby.cabanas_info;
                textos["lobby.cabanas_info.title_animales"] = ci.title_animales;
                textos["lobby.cabanas_info.sentence_animales_0"] = ci.sentence_animales_0;
                textos["lobby.cabanas_info.sentence_animales_1"] = ci.sentence_animales_1;
                textos["lobby.cabanas_info.sentence_animales_2"] = ci.sentence_animales_2;

                textos["lobby.cabanas_info.title_aves"] = ci.title_aves;
                textos["lobby.cabanas_info.sentence_aves_0"] = ci.sentence_aves_0;
                textos["lobby.cabanas_info.sentence_aves_1"] = ci.sentence_aves_1;

                textos["lobby.cabanas_info.title_flora"] = ci.title_flora;
                textos["lobby.cabanas_info.sentence_flora_0"] = ci.sentence_flora_0;
                textos["lobby.cabanas_info.sentence_flora_1"] = ci.sentence_flora_1;
                textos["lobby.cabanas_info.sentence_flora_2"] = ci.sentence_flora_2;
            }

            if (datos.lobby.excursionistas != null)
            {
                var exc = datos.lobby.excursionistas;
                textos["lobby.excursionistas.title_grupo"] = exc.title_grupo;
                textos["lobby.excursionistas.sentence_grupo_0"] = exc.sentence_grupo_0;
                textos["lobby.excursionistas.sentence_grupo_1"] = exc.sentence_grupo_1;
                textos["lobby.excursionistas.sentence_grupo_2"] = exc.sentence_grupo_2;
            }

            if (datos.lobby.cabana_animal != null)
            {
                var ca = datos.lobby.cabana_animal;
                textos["cabana_animal.title_0"] = ca.title_0;
                textos["cabana_animal.title_1"] = ca.title_1;
                textos["cabana_animal.title_2"] = ca.title_2;

                textos["cabana_animal.animal_0_sentence_0"] = ca.animal_0_sentence_0;
                textos["cabana_animal.animal_0_sentence_1"] = ca.animal_0_sentence_1;
                textos["cabana_animal.animal_0_sentence_2"] = ca.animal_0_sentence_2;

                textos["cabana_animal.animal_1_sentence_0"] = ca.animal_1_sentence_0;
                textos["cabana_animal.animal_1_sentence_1"] = ca.animal_1_sentence_1;
                textos["cabana_animal.animal_1_sentence_2"] = ca.animal_1_sentence_2;

                textos["cabana_animal.animal_2_sentence_0"] = ca.animal_2_sentence_0;
                textos["cabana_animal.animal_2_sentence_1"] = ca.animal_2_sentence_1;
                textos["cabana_animal.animal_2_sentence_2"] = ca.animal_2_sentence_2;

                textos["cabana_animal.animal_3_sentence_0"] = ca.animal_3_sentence_0;
                textos["cabana_animal.animal_3_sentence_1"] = ca.animal_3_sentence_1;
                textos["cabana_animal.animal_3_sentence_2"] = ca.animal_3_sentence_2;

                textos["cabana_animal.animal_4_sentence_0"] = ca.animal_4_sentence_0;
                textos["cabana_animal.animal_4_sentence_1"] = ca.animal_4_sentence_1;
                textos["cabana_animal.animal_4_sentence_2"] = ca.animal_4_sentence_2;

                textos["cabana_animal.animal_5_sentence_0"] = ca.animal_5_sentence_0;
                textos["cabana_animal.animal_5_sentence_1"] = ca.animal_5_sentence_1;
                textos["cabana_animal.animal_5_sentence_2"] = ca.animal_5_sentence_2;

                textos["cabana_animal.animal_6_sentence_0"] = ca.animal_6_sentence_0;
                textos["cabana_animal.animal_6_sentence_1"] = ca.animal_6_sentence_1;
                textos["cabana_animal.animal_6_sentence_2"] = ca.animal_6_sentence_2;

                textos["cabana_animal.animal_7_sentence_0"] = ca.animal_7_sentence_0;
                textos["cabana_animal.animal_7_sentence_1"] = ca.animal_7_sentence_1;
                textos["cabana_animal.animal_7_sentence_2"] = ca.animal_7_sentence_2;

                textos["cabana_animal.animal_8_sentence_0"] = ca.animal_8_sentence_0;
                textos["cabana_animal.animal_8_sentence_1"] = ca.animal_8_sentence_1;
                textos["cabana_animal.animal_8_sentence_2"] = ca.animal_8_sentence_2;

                textos["cabana_animal.animal_9_sentence_0"] = ca.animal_9_sentence_0;
                textos["cabana_animal.animal_9_sentence_1"] = ca.animal_9_sentence_1;
                textos["cabana_animal.animal_9_sentence_2"] = ca.animal_9_sentence_2;
            }

            if (datos.lobby.cabana_aves != null)
            {
                var cav = datos.lobby.cabana_aves;
                textos["cabana_aves.title_0"] = cav.title_0;
                textos["cabana_aves.title_1"] = cav.title_1;
                textos["cabana_aves.title_2"] = cav.title_2;

                textos["cabana_animal.ave_0_sentence_0"] = cav.ave_0_sentence_0;
                textos["cabana_animal.ave_0_sentence_1"] = cav.ave_0_sentence_1;
                textos["cabana_animal.ave_0_sentence_2"] = cav.ave_0_sentence_2;

                textos["cabana_animal.ave_1_sentence_0"] = cav.ave_1_sentence_0;
                textos["cabana_animal.ave_1_sentence_1"] = cav.ave_1_sentence_1;
                textos["cabana_animal.ave_1_sentence_2"] = cav.ave_1_sentence_2;

                textos["cabana_animal.ave_2_sentence_0"] = cav.ave_2_sentence_0;
                textos["cabana_animal.ave_2_sentence_1"] = cav.ave_2_sentence_1;
                textos["cabana_animal.ave_2_sentence_2"] = cav.ave_2_sentence_2;

                textos["cabana_animal.ave_3_sentence_0"] = cav.ave_3_sentence_0;
                textos["cabana_animal.ave_3_sentence_1"] = cav.ave_3_sentence_1;
                textos["cabana_animal.ave_3_sentence_2"] = cav.ave_3_sentence_2;

                textos["cabana_animal.ave_4_sentence_0"] = cav.ave_4_sentence_0;
                textos["cabana_animal.ave_4_sentence_1"] = cav.ave_4_sentence_1;
                textos["cabana_animal.ave_4_sentence_2"] = cav.ave_4_sentence_2;

                textos["cabana_animal.ave_5_sentence_0"] = cav.ave_5_sentence_0;
                textos["cabana_animal.ave_5_sentence_1"] = cav.ave_5_sentence_1;
                textos["cabana_animal.ave_5_sentence_2"] = cav.ave_5_sentence_2;

                textos["cabana_animal.ave_6_sentence_0"] = cav.ave_6_sentence_0;
                textos["cabana_animal.ave_6_sentence_1"] = cav.ave_6_sentence_1;
                textos["cabana_animal.ave_6_sentence_2"] = cav.ave_6_sentence_2;

                textos["cabana_animal.ave_7_sentence_0"] = cav.ave_7_sentence_0;
                textos["cabana_animal.ave_7_sentence_1"] = cav.ave_7_sentence_1;
                textos["cabana_animal.ave_7_sentence_2"] = cav.ave_7_sentence_2;

                textos["cabana_animal.ave_8_sentence_0"] = cav.ave_8_sentence_0;
                textos["cabana_animal.ave_8_sentence_1"] = cav.ave_8_sentence_1;
                textos["cabana_animal.ave_8_sentence_2"] = cav.ave_8_sentence_2;

                textos["cabana_animal.ave_9_sentence_0"] = cav.ave_9_sentence_0;
                textos["cabana_animal.ave_9_sentence_1"] = cav.ave_9_sentence_1;
                textos["cabana_animal.ave_9_sentence_2"] = cav.ave_9_sentence_2;

                textos["cabana_animal.ave_10_sentence_0"] = cav.ave_10_sentence_0;
                textos["cabana_animal.ave_10_sentence_1"] = cav.ave_10_sentence_1;
                textos["cabana_animal.ave_10_sentence_2"] = cav.ave_10_sentence_2;

                textos["cabana_animal.ave_11_sentence_0"] = cav.ave_11_sentence_0;
                textos["cabana_animal.ave_11_sentence_1"] = cav.ave_11_sentence_1;
                textos["cabana_animal.ave_11_sentence_2"] = cav.ave_11_sentence_2;

                textos["cabana_animal.ave_12_sentence_0"] = cav.ave_12_sentence_0;
                textos["cabana_animal.ave_12_sentence_1"] = cav.ave_12_sentence_1;
                textos["cabana_animal.ave_12_sentence_2"] = cav.ave_12_sentence_2;

                textos["cabana_animal.ave_13_sentence_0"] = cav.ave_13_sentence_0;
                textos["cabana_animal.ave_13_sentence_1"] = cav.ave_13_sentence_1;
                textos["cabana_animal.ave_13_sentence_2"] = cav.ave_13_sentence_2;

                textos["cabana_animal.ave_14_sentence_0"] = cav.ave_14_sentence_0;
                textos["cabana_animal.ave_14_sentence_1"] = cav.ave_14_sentence_1;
                textos["cabana_animal.ave_14_sentence_2"] = cav.ave_14_sentence_2;
            }

            if (datos.lobby.cabana_flora != null) 
            {
                var cf = datos.lobby.cabana_flora;
                textos["cabana_flora.title_0"] = cf.title_0;
                textos["cabana_flora.title_1"] = cf.title_1;

                textos["cabana_flora.flora_0_sentence_0"] = cf.flora_0_sentence_0;
                textos["cabana_flora.flora_0_sentence_1"] = cf.flora_0_sentence_1;

                textos["cabana_flora.flora_1_sentence_0"] = cf.flora_1_sentence_0;
                textos["cabana_flora.flora_1_sentence_1"] = cf.flora_1_sentence_1;

                textos["cabana_flora.flora_2_sentence_0"] = cf.flora_2_sentence_0;
                textos["cabana_flora.flora_2_sentence_1"] = cf.flora_2_sentence_1;

                textos["cabana_flora.flora_3_sentence_0"] = cf.flora_3_sentence_0;
                textos["cabana_flora.flora_3_sentence_1"] = cf.flora_3_sentence_1;

                textos["cabana_flora.flora_4_sentence_0"] = cf.flora_4_sentence_0;
                textos["cabana_flora.flora_4_sentence_1"] = cf.flora_4_sentence_1;
            }
        }

        if (datos.controles != null)
        {
            textos["controles.caja_ardilla"] = datos.controles.caja_ardilla;
            textos["controles.caja_iguana"] = datos.controles.caja_iguana;
            textos["controles.caja_pechiche"] = datos.controles.caja_pechiche;
        }

        if (datos.recordatorios != null)
        {
            var rec = datos.recordatorios;
            textos["recordatorios.recordatorio_3"] = rec.recordatorio_3;
            textos["recordatorios.recordatorio_4"] = rec.recordatorio_4;
            textos["recordatorios.recordatorio_5"] = rec.recordatorio_5;
            textos["recordatorios.recordatorio_6"] = rec.recordatorio_6;
            textos["recordatorios.recordatorio_7"] = rec.recordatorio_7;
            textos["recordatorios.feedback"] = rec.feedback;
            textos["recordatorios.animal_atrapado_0"] = rec.animal_atrapado_0;
            textos["recordatorios.animal_atrapado_1"] = rec.animal_atrapado_1;
            textos["recordatorios.recoge_basura"] = rec.recoge_basura;
            textos["recordatorios.fogata"] = rec.fogata;
            textos["recordatorios.basura_faltante_inicial"] = rec.basura_faltante_inicial;
            textos["recordatorios.basura_faltante"] = rec.basura_faltante;
            textos["recordatorios.basura_papel"] = rec.basura_papel;
            textos["recordatorios.basura_plastico"] = rec.basura_plastico;
            textos["recordatorios.basura_vidrio"] = rec.basura_vidrio;
            textos["recordatorios.basura_mal"] = rec.basura_mal;
            textos["recordatorios.sembrando"] = rec.sembrando;
            textos["recordatorios.help_fogata_0"] = rec.help_fogata_0;
            textos["recordatorios.help_fogata_1"] = rec.help_fogata_1;
            textos["recordatorios.help_fogata_2"] = rec.help_fogata_2;
            textos["recordatorios.help_fogata_3"] = rec.help_fogata_3;
            textos["recordatorios.help_fogata_4"] = rec.help_fogata_4;
        }

        if (datos.first_challenge != null)
        {
            var fc = datos.first_challenge;
            textos["first_challenge.title_0"] = fc.title_0;
            textos["first_challenge.title_1"] = fc.title_1;
            textos["first_challenge.title_2"] = fc.title_2;
            textos["first_challenge.title_3"] = fc.title_3;
            textos["first_challenge.title_4"] = fc.title_4;

            textos["first_challenge.sentence_0"] = fc.sentence_0;
            textos["first_challenge.sentence_1"] = fc.sentence_1;
            textos["first_challenge.sentence_2"] = fc.sentence_2;
            textos["first_challenge.sentence_3"] = fc.sentence_3;
            textos["first_challenge.sentence_4"] = fc.sentence_4;
        }

        if (datos.conseguir_libro != null)
        {
            var cl = datos.conseguir_libro;
            textos["conseguir_libro.title_0"] = cl.title_0;
            textos["conseguir_libro.title_1"] = cl.title_1;
            textos["conseguir_libro.title_2"] = cl.title_2;

            textos["conseguir_libro.sentence_0"] = cl.sentence_0;
            textos["conseguir_libro.sentence_1"] = cl.sentence_1;
            textos["conseguir_libro.sentence_2"] = cl.sentence_2;
        }

        if(datos.second_challenge != null)
        {
            var sc = datos.second_challenge;
            textos["second_challenge.title_0"] = sc.title_0;
            textos["second_challenge.title_1"] = sc.title_1;
            textos["second_challenge.title_2"] = sc.title_2;

            textos["second_challenge.sentence_0"] = sc.sentence_0;
            textos["second_challenge.sentence_1"] = sc.sentence_1;
            textos["second_challenge.sentence_2"] = sc.sentence_2;

            textos["second_challenge.title_completado"] = sc.title_completado;
            textos["second_challenge.sentence_completado"] = sc.sentence_completado;
            textos["second_challenge.title_pendiente"] = sc.title_pendiente;
            textos["second_challenge.sentence_pendiente"] = sc.sentence_pendiente;
        }

        if (datos.third_challenge != null)
        {
            var tc = datos.third_challenge;
            textos["third_challenge.title_0"] = tc.title_0;
            textos["third_challenge.title_1"] = tc.title_1;

            textos["third_challenge.sentence_0"] = tc.sentence_0;
            textos["third_challenge.sentence_1"] = tc.sentence_1;

            textos["third_challenge.title_completado"] = tc.title_completado;
            textos["third_challenge.sentence_completado"] = tc.sentence_completado;
            textos["third_challenge.title_pendiente"] = tc.title_pendiente;
            textos["third_challenge.sentence_pendiente"] = tc.sentence_pendiente;

            textos["third_challenge.recordatorio"] = tc.recordatorio;
        }

        if (datos.fourth_challenge != null)
        {
            var fc = datos.fourth_challenge;
            textos["fourth_challenge.title_suelo"] = fc.title_suelo;
            textos["fourth_challenge.sentence_suelo"] = fc.sentence_suelo;

            textos["fourth_challenge.title_0"] = fc.title_0;
            textos["fourth_challenge.title_1"] = fc.title_1;
            textos["fourth_challenge.title_2"] = fc.title_2;

            textos["fourth_challenge.sentence_0"] = fc.sentence_0;
            textos["fourth_challenge.sentence_1"] = fc.sentence_1;
            textos["fourth_challenge.sentence_2"] = fc.sentence_2;

            textos["fourth_challenge.title_completado"] = fc.title_completado;
            textos["fourth_challenge.sentence_completado"] = fc.sentence_completado;
            textos["fourth_challenge.title_pendiente"] = fc.title_pendiente;
            textos["fourth_challenge.sentence_pendiente"] = fc.sentence_pendiente;
        }

        if (datos.fifth_challenge != null)
        {
            var ffc = datos.fifth_challenge;
            textos["fifth_challenge.title_vidrio"] = ffc.title_vidrio;
            textos["fifth_challenge.sentence_vidrio"] = ffc.sentence_vidrio;

            textos["fifth_challenge.title_basura"] = ffc.title_basura;
            textos["fifth_challenge.sentence_basura"] = ffc.sentence_basura;

            textos["fifth_challenge.title_0"] = ffc.title_0;
            textos["fifth_challenge.title_1"] = ffc.title_1;

            textos["fifth_challenge.sentence_0"] = ffc.sentence_0;
            textos["fifth_challenge.sentence_1"] = ffc.sentence_1;

            textos["fifth_challenge.title_completado"] = ffc.title_completado;
            textos["fifth_challenge.sentence_completado"] = ffc.sentence_completado;
            textos["fifth_challenge.title_pendiente"] = ffc.title_pendiente;
            textos["fifth_challenge.sentence_pendiente"] = ffc.sentence_pendiente;
        }

        if (datos.sixth_challenge != null)
        {
            var sxc = datos.sixth_challenge;
            textos["sixth_challenge.title_aire"] = sxc.title_aire;
            textos["sixth_challenge.sentence_aire"] = sxc.sentence_aire;

            textos["sixth_challenge.title_0"] = sxc.title_0;
            textos["sixth_challenge.title_1"] = sxc.title_1;

            textos["sixth_challenge.sentence_0"] = sxc.sentence_0;
            textos["sixth_challenge.sentence_1"] = sxc.sentence_1;

            textos["sixth_challenge.title_completado"] = sxc.title_completado;
            textos["sixth_challenge.sentence_completado"] = sxc.sentence_completado;
            textos["sixth_challenge.title_pendiente"] = sxc.title_pendiente;
            textos["sixth_challenge.sentence_pendiente"] = sxc.sentence_pendiente;
        }

        if (datos.seventh_challenge != null)
        {
            var svc = datos.seventh_challenge;
            textos["seventh_challenge.title_0"] = svc.title_0;
            textos["seventh_challenge.title_1"] = svc.title_1;

            textos["seventh_challenge.sentence_0"] = svc.sentence_0;
            textos["seventh_challenge.sentence_1"] = svc.sentence_1;

            textos["seventh_challenge.title_completado_0"] = svc.title_completado_0;
            textos["seventh_challenge.title_completado_1"] = svc.title_completado_1;
            textos["seventh_challenge.sentence_completado_0"] = svc.sentence_completado_0;
            textos["seventh_challenge.sentence_completado_1"] = svc.sentence_completado_1;
            textos["seventh_challenge.title_pendiente_0"] = svc.title_pendiente_0;
            textos["seventh_challenge.title_pendiente_1"] = svc.title_pendiente_1;
            textos["seventh_challenge.sentence_pendiente_0"] = svc.sentence_pendiente_0;
            textos["seventh_challenge.sentence_pendiente_1"] = svc.sentence_pendiente_1;
            textos["seventh_challenge.title_fallado"] = svc.title_fallado;
            textos["seventh_challenge.sentence_fallado"] = svc.sentence_fallado;

            textos["seventh_challenge.title_wellzone_0"] = svc.title_wellzone_0;
            textos["seventh_challenge.title_wellzone_1"] = svc.title_wellzone_1;
            textos["seventh_challenge.title_wellzone_2"] = svc.title_wellzone_2;

            textos["seventh_challenge.sentence_wellzone_0"] = svc.sentence_wellzone_0;
            textos["seventh_challenge.sentence_wellzone_1"] = svc.sentence_wellzone_1;
            textos["seventh_challenge.sentence_wellzone_2"] = svc.sentence_wellzone_2;

            textos["seventh_challenge.water_logro"] = svc.water_logro;
            textos["seventh_challenge.water_falta"] = svc.water_falta;
        }

        if (datos.carteles_bosque != null)
        {
            var cb = datos.carteles_bosque;
            textos["carteles_bosque.entrada_bosque"] = cb.entrada_bosque;
            textos["carteles_bosque.primer_nivel"] = cb.primer_nivel;
            textos["carteles_bosque.segundo_nivel"] = cb.segundo_nivel;
            textos["carteles_bosque.llegada_lago"] = cb.llegada_lago;
            textos["carteles_bosque.antes_nivel_4"] = cb.antes_nivel_4;

            textos["carteles_bosque.aire_fresco"] = cb.aire_fresco;
            textos["carteles_bosque.hojarasca"] = cb.hojarasca;
            textos["carteles_bosque.basura"] = cb.basura;

            textos["carteles_bosque.lago"] = cb.lago;
            textos["carteles_bosque.pozo"] = cb.pozo;
            textos["carteles_bosque.invernadero"] = cb.invernadero;
            textos["carteles_bosque.mirador"] = cb.mirador;
        }

        if (datos.logros != null)
        {
            var lg = datos.logros;
            textos["logros.info_medalla"] = lg.info_medalla;
            textos["logros.title_logro"] = lg.title_logro;

            textos["logros.logro_0_nombre"] = lg.logro_0_nombre;
            textos["logros.logro_1_nombre"] = lg.logro_1_nombre;
            textos["logros.logro_2_nombre"] = lg.logro_2_nombre;
            textos["logros.logro_3_nombre"] = lg.logro_3_nombre;
            textos["logros.logro_4_nombre"] = lg.logro_4_nombre;
            textos["logros.logro_5_nombre"] = lg.logro_5_nombre;
            textos["logros.logro_6_nombre"] = lg.logro_6_nombre;
            textos["logros.logro_7_nombre"] = lg.logro_7_nombre;
            textos["logros.logro_8_nombre"] = lg.logro_8_nombre;
            textos["logros.logro_9_nombre"] = lg.logro_9_nombre;

            textos["logros.logro_0_descripcion"] = lg.logro_0_descripcion;
            textos["logros.logro_1_descripcion"] = lg.logro_1_descripcion;
            textos["logros.logro_2_descripcion"] = lg.logro_2_descripcion;
            textos["logros.logro_3_descripcion"] = lg.logro_3_descripcion;
            textos["logros.logro_4_descripcion"] = lg.logro_4_descripcion;
            textos["logros.logro_5_descripcion"] = lg.logro_5_descripcion;
            textos["logros.logro_6_descripcion"] = lg.logro_6_descripcion;
            textos["logros.logro_7_descripcion"] = lg.logro_7_descripcion;
            textos["logros.logro_8_descripcion"] = lg.logro_8_descripcion;
            textos["logros.logro_9_descripcion"] = lg.logro_9_descripcion;
        }

        if (datos.misiones != null)
        {
            var ms = datos.misiones;
            textos["misiones.estacion_mision"] = ms.estacion_mision;
            textos["misiones.req_mision"] = ms.req_mision;
            textos["misiones.req_completados_mision"] = ms.req_completados_mision;
            textos["misiones.mision_0_nombre"] = ms.mision_0_nombre;
            textos["misiones.mision_1_nombre"] = ms.mision_1_nombre;
            textos["misiones.mision_2_nombre"] = ms.mision_2_nombre;
            textos["misiones.mision_3_nombre"] = ms.mision_3_nombre;
            textos["misiones.mision_4_nombre"] = ms.mision_4_nombre;
            textos["misiones.mision_5_nombre"] = ms.mision_5_nombre;
            textos["misiones.mision_6_nombre"] = ms.mision_6_nombre;
            textos["misiones.mision_7_nombre"] = ms.mision_7_nombre;
            textos["misiones.mision_0_descripcion"] = ms.mision_0_descripcion;
            textos["misiones.mision_1_descripcion"] = ms.mision_1_descripcion;
            textos["misiones.mision_2_descripcion"] = ms.mision_2_descripcion;
            textos["misiones.mision_3_descripcion"] = ms.mision_3_descripcion;
            textos["misiones.mision_4_descripcion"] = ms.mision_4_descripcion;
            textos["misiones.mision_5_descripcion"] = ms.mision_5_descripcion;
            textos["misiones.mision_6_descripcion"] = ms.mision_6_descripcion;
            textos["misiones.mision_7_descripcion"] = ms.mision_7_descripcion;
        }

        if (datos.contrarreloj != null)
        {
            var ct = datos.contrarreloj;
            textos["contrarreloj.title_activo"] = ct.title_activo;
            textos["contrarreloj.contenido_activo"] = ct.contenido_activo;
            textos["contrarreloj.title_despedida"] = ct.title_despedida;
            textos["contrarreloj.contenido_despedida"] = ct.contenido_despedida;
            textos["contrarreloj.title_perdida"] = ct.title_perdida;
            textos["contrarreloj.contenido_perdida"] = ct.contenido_perdida;
        }

        if (datos.feedback_preguntas != null)
        {
            var fp = datos.feedback_preguntas;
            textos["feedback_preguntas.title_correcto"] = fp.title_correcto;
            textos["feedback_preguntas.subtitle_correcto"] = fp.subtitle_correcto;
            textos["feedback_preguntas.title_incorrecto"] = fp.title_incorrecto;
        }

        // Libro
        if (datos.libro != null)
        {
            var lb = datos.libro;
            textos["libro.nombre"] = lb.nombre;
            textos["libro.familia"] = lb.familia;
            textos["libro.estacion"] = lb.estacion;
            textos["libro.descripcion"] = lb.descripcion;
            textos["libro.latin"] = lb.latin;
            textos["libro.otros_habitats"] = lb.otros_habitats;
            textos["libro.no_datos"] = lb.no_datos;
            textos["libro.no_descubierto"] = lb.no_descubierto;
        }

        // Arboles
        if (datos.arboles != null)
        {
            var arboles = datos.arboles;
            textos["arboles.ceibo"] = arboles.ceibo;
            textos["arboles.jacaranda"] = arboles.jacaranda;
            textos["arboles.bototillo"] = arboles.bototillo;
            textos["arboles.pechiche"] = arboles.pechiche;
            textos["arboles.laurel"] = arboles.laurel;
            textos["arboles.fernan_sanchez"] = arboles.fernan_sanchez;
            textos["arboles.guasmo"] = arboles.guasmo;
            textos["arboles.guayacan"] = arboles.guayacan;
        }
    }

    // CLASES

    // Contenedor de todas las escenas
    [Serializable]
    public class DatosIdioma
    {
        public Botones botones;
        public Interfaz interfaz;
        public Perfil perfil;
        public MenuInventario menu_inventario;
        public MenuMochila menu_mochila;
        public MenuPausa menu_pausa;
        public MenuInicial menu_inicial;
        public MenuPartidas menu_partidas;
        public MensajesError mensajes_error;
        public PistaLuz pista_luz;
        public Tutorial tutorial;
        public CartelesBosque carteles_bosque;
        public Lobby lobby;
        public Controles controles;
        public Recordatorios recordatorios;
        public FirstChallenge first_challenge;
        public ConseguirLibro conseguir_libro;
        public SecondChallenge second_challenge;
        public ThirdChallengeD third_challenge;
        public FourthChallenge fourth_challenge;
        public FifthChallenge fifth_challenge;
        public SixthChallenge sixth_challenge;
        public SeventhChallenge seventh_challenge;
        public Logros logros;
        public Misiones misiones;
        public Contrarreloj contrarreloj;
        public FeedbackPreguntas feedback_preguntas;
        public Libro libro;
        public Arboles arboles;
    }

    // Botones
    [Serializable]
    public class Botones
    {
        public string continuar;
        public string retroceder;
        public string informacion;
        public string entendido;
        public string minimizar;
        public string exportar;
    }

    // Interfaz
    [Serializable]
    public class Interfaz
    {
        public string pausa;
        public string desafios;
        public string estacion;
    }

    // Perfil 
    [Serializable]
    public class Perfil
    {
        public string title;
        public string uni_edu;
        public string nivel;
        public string experiencia;
        public string edad;
    }

    // Menu Inventario
    [Serializable]
    public class MenuInventario
    {
        public string desafios;
        public string misiones;
        public string descripcion_misiones;
        public string logros;
        public string perfil;
        public string preguntas;
        public string no_desbloq;
        public string respuesta;
        public string estaciones;
    }

    // Menu Mochila
    [Serializable]  
    public class MenuMochila
    {
        public string semillas;
        public string accesorios;
    }

    // Menu pausa
    [Serializable]
    public class MenuPausa
    {
        public string guia;
        public string menu;
        public string ajustes;
        public string mapa;
        public string lobby;
        public string salir;
        public string diploma;
        public string modo;
        public string continuar;
        public string noti_diploma;
        public string ajuste_resolucion;
        public string calidad_maxima;
        public string calidad_media;
        public string calidad_minima;
        public string ajuste_modo;
        public string modo_contrarreloj;
        public string modo_normal;
        public string volver;
    }

    // Menu inicial
    [Serializable]
    public class MenuInicial
    {
        public string boton_jugar;
        public string boton_explorar;
        public string boton_salir;
    }

    // Menu partidas
    [Serializable]
    public class MenuPartidas
    {
        public NuevaPartida nueva_partida;
        public CrearCargarPartida crear_cargar_partida;
    }

    [Serializable]
    public class NuevaPartida
    {
        public string title_avatar;
        public string title_datos;
        public string label_nombre;
        public string label_edad;
        public string label_genero;
        public string genero_h;
        public string genero_m;
        public string genero_otro;
        public string label_pin;
        public string pin_invitado;
        public string invitado_1;
        public string invitado_2;
    }

    [Serializable]
    public class CrearCargarPartida
    {
        public string cargar_partida;
        public string nueva_partida;
    }

    [Serializable]
    public class MensajesError
    {
        public string error_0;
        public string error_1;
        public string error_2;
        public string error_3;
        public string error_4;
        public string error_5_0;
        public string error_5_1;
    }

    // Pista Luz 
    [Serializable]
    public class PistaLuz
    {
        public string title;
        public string nombre;
        public string descripcion;
    }

    // Tutorial
    [Serializable]
    public class Tutorial
    {
        public string dialogo_0;
        public string dialogo_1;
        public string dialogo_2;
        public string dialogo_3;
        public string dialogo_4;
        public string dialogo_5;
        public string dialogo_6;
        public string dialogo_7;
        public string dialogo_8;
        public string dialogo_9;
        public string dialogo_10;
        public string dialogo_11;
        public string dialogo_12;
        public string son;
        public string vamos;
    }

    // Lobby
    [Serializable]
    public class Lobby
    {
        public LobbyBruno bruno;
        public CabanasInfo cabanas_info;
        public Excursionistas excursionistas;
        public CabanaAnimal cabana_animal;
        public CabanaAves cabana_aves;
        public CabanaFlora cabana_flora;
    }

    [Serializable]
    public class LobbyBruno
    {
        public string title_0;
        public string title_1;
        public string title_2;
        public string sentence_0;
        public string sentence_1;
        public string sentence_2;
    }

    [Serializable]
    public class CabanasInfo
    {
        public string title_animales;
        public string sentence_animales_0;
        public string sentence_animales_1;
        public string sentence_animales_2;

        public string title_aves;
        public string sentence_aves_0;
        public string sentence_aves_1;

        public string title_flora;
        public string sentence_flora_0;
        public string sentence_flora_1;
        public string sentence_flora_2;
    }

    [Serializable]
    public class Excursionistas
    {
        public string title_grupo;
        public string sentence_grupo_0;
        public string sentence_grupo_1;
        public string sentence_grupo_2;
    }

    [Serializable]
    public class CabanaAnimal
    {
        public string title_0;
        public string title_1;
        public string title_2;
        
        public string animal_0_sentence_0;
        public string animal_0_sentence_1;
        public string animal_0_sentence_2;

        public string animal_1_sentence_0;
        public string animal_1_sentence_1;
        public string animal_1_sentence_2;

        public string animal_2_sentence_0;
        public string animal_2_sentence_1;
        public string animal_2_sentence_2;

        public string animal_3_sentence_0;
        public string animal_3_sentence_1;
        public string animal_3_sentence_2;

        public string animal_4_sentence_0;
        public string animal_4_sentence_1;
        public string animal_4_sentence_2;

        public string animal_5_sentence_0;
        public string animal_5_sentence_1;
        public string animal_5_sentence_2;

        public string animal_6_sentence_0;
        public string animal_6_sentence_1;
        public string animal_6_sentence_2;

        public string animal_7_sentence_0;
        public string animal_7_sentence_1;
        public string animal_7_sentence_2;
        
        public string animal_8_sentence_0;
        public string animal_8_sentence_1;
        public string animal_8_sentence_2;

        public string animal_9_sentence_0;
        public string animal_9_sentence_1;
        public string animal_9_sentence_2;
    }

    [Serializable]
    public class CabanaAves
    {
        public string title_0;
        public string title_1;
        public string title_2;

        public string ave_0_sentence_0;
        public string ave_0_sentence_1;
        public string ave_0_sentence_2;

        public string ave_1_sentence_0;
        public string ave_1_sentence_1;
        public string ave_1_sentence_2;

        public string ave_2_sentence_0;
        public string ave_2_sentence_1;
        public string ave_2_sentence_2;

        public string ave_3_sentence_0;
        public string ave_3_sentence_1;
        public string ave_3_sentence_2;

        public string ave_4_sentence_0;
        public string ave_4_sentence_1;
        public string ave_4_sentence_2;

        public string ave_5_sentence_0;
        public string ave_5_sentence_1;
        public string ave_5_sentence_2;

        public string ave_6_sentence_0;
        public string ave_6_sentence_1;
        public string ave_6_sentence_2;

        public string ave_7_sentence_0;
        public string ave_7_sentence_1;
        public string ave_7_sentence_2;

        public string ave_8_sentence_0;
        public string ave_8_sentence_1;
        public string ave_8_sentence_2;

        public string ave_9_sentence_0;
        public string ave_9_sentence_1;
        public string ave_9_sentence_2;

        public string ave_10_sentence_0;
        public string ave_10_sentence_1;
        public string ave_10_sentence_2;

        public string ave_11_sentence_0;
        public string ave_11_sentence_1;
        public string ave_11_sentence_2;

        public string ave_12_sentence_0;
        public string ave_12_sentence_1;
        public string ave_12_sentence_2;

        public string ave_13_sentence_0;
        public string ave_13_sentence_1;
        public string ave_13_sentence_2;

        public string ave_14_sentence_0;
        public string ave_14_sentence_1;
        public string ave_14_sentence_2;
    }

    [Serializable]
    public class CabanaFlora
    {
        public string title_0;
        public string title_1;
        public string title_2;

        public string flora_0_sentence_0;
        public string flora_0_sentence_1;

        public string flora_1_sentence_0;
        public string flora_1_sentence_1;

        public string flora_2_sentence_0;
        public string flora_2_sentence_1;

        public string flora_3_sentence_0;
        public string flora_3_sentence_1;

        public string flora_4_sentence_0;
        public string flora_4_sentence_1;
    }

    // Bosque
    [Serializable]
    public class Controles
    {
        public string caja_ardilla;
        public string caja_iguana;
        public string caja_pechiche;
    }

    [Serializable]
    public class Recordatorios
    {
        public string recordatorio_3;
        public string recordatorio_4;
        public string recordatorio_5;
        public string recordatorio_6;
        public string recordatorio_7;
        public string feedback;
        public string animal_atrapado_0;
        public string animal_atrapado_1;
        public string recoge_basura;
        public string fogata;
        public string basura_faltante_inicial;
        public string basura_faltante;
        public string basura_papel;
        public string basura_plastico;
        public string basura_vidrio;
        public string basura_mal;
        public string sembrando;
        public string help_fogata_0;
        public string help_fogata_1;
        public string help_fogata_2;
        public string help_fogata_3;
        public string help_fogata_4;
    }

    [Serializable]
    public class FirstChallenge
    {
        public string title_0;
        public string title_1;
        public string title_2;
        public string title_3;
        public string title_4;

        public string sentence_0;
        public string sentence_1;
        public string sentence_2;
        public string sentence_3;
        public string sentence_4;
    }

    [Serializable]
    public class ConseguirLibro
    {
        public string title_0;
        public string title_1;
        public string title_2;

        public string sentence_0;
        public string sentence_1;
        public string sentence_2;
    }

    [Serializable]
    public class SecondChallenge
    {
        public string title_0;
        public string title_1;
        public string title_2;

        public string sentence_0;
        public string sentence_1;
        public string sentence_2;

        public string title_completado;
        public string sentence_completado;
        public string title_pendiente;
        public string sentence_pendiente;
    }

    [Serializable]
    public class ThirdChallengeD
    {
        public string title_0;
        public string title_1;

        public string sentence_0;
        public string sentence_1;

        public string title_completado;
        public string sentence_completado;
        public string title_pendiente;
        public string sentence_pendiente;

        public string recordatorio;
    }

    [Serializable]
    public class FourthChallenge
    {
        public string title_suelo;
        public string sentence_suelo;

        public string title_0;
        public string title_1;
        public string title_2;

        public string sentence_0;
        public string sentence_1;
        public string sentence_2;

        public string title_completado;
        public string sentence_completado;
        public string title_pendiente;
        public string sentence_pendiente;
    }

    [Serializable]
    public class FifthChallenge
    {
        public string title_vidrio;
        public string sentence_vidrio;

        public string title_basura;
        public string sentence_basura;

        public string title_0;
        public string title_1;

        public string sentence_0;
        public string sentence_1;

        public string title_completado;
        public string sentence_completado;
        public string title_pendiente;
        public string sentence_pendiente;
    }

    [Serializable]
    public class SixthChallenge
    {
        public string title_aire;
        public string sentence_aire;

        public string title_0;
        public string title_1;

        public string sentence_0;
        public string sentence_1;

        public string title_completado;
        public string sentence_completado;
        public string title_pendiente;
        public string sentence_pendiente;
    }

    [Serializable]
    public class SeventhChallenge
    {
        public string title_0;
        public string title_1;

        public string sentence_0;
        public string sentence_1;

        public string title_completado_0;
        public string title_completado_1;
        public string sentence_completado_0;
        public string sentence_completado_1;
        public string title_pendiente_0;
        public string title_pendiente_1;
        public string sentence_pendiente_0;
        public string sentence_pendiente_1;
        public string title_fallado;
        public string sentence_fallado;

        public string title_wellzone_0;
        public string title_wellzone_1;
        public string title_wellzone_2;

        public string sentence_wellzone_0;
        public string sentence_wellzone_1;
        public string sentence_wellzone_2;

        public string water_logro;
        public string water_falta;
    }

    [Serializable]
    public class CartelesBosque
    {
        public string entrada_bosque;
        public string primer_nivel;
        public string segundo_nivel;
        public string llegada_lago;
        public string antes_nivel_4;

        public string aire_fresco;
        public string hojarasca;
        public string basura;

        public string lago;
        public string pozo;
        public string invernadero;
        public string mirador;
    }

    // Logros
    [Serializable]
    public class Logros
    {
        public string info_medalla;
        public string title_logro;

        public string logro_0_nombre;
        public string logro_1_nombre;
        public string logro_2_nombre;
        public string logro_3_nombre;
        public string logro_4_nombre;
        public string logro_5_nombre;
        public string logro_6_nombre;
        public string logro_7_nombre;
        public string logro_8_nombre;
        public string logro_9_nombre;

        public string logro_0_descripcion;
        public string logro_1_descripcion;
        public string logro_2_descripcion;
        public string logro_3_descripcion;
        public string logro_4_descripcion;
        public string logro_5_descripcion;
        public string logro_6_descripcion;
        public string logro_7_descripcion;
        public string logro_8_descripcion;
        public string logro_9_descripcion;
    }

    // Misiones
    [Serializable]
    public class Misiones
    {
        public string estacion_mision;
        public string req_mision;
        public string req_completados_mision;
        public string mision_0_nombre;
        public string mision_1_nombre;
        public string mision_2_nombre;
        public string mision_3_nombre;
        public string mision_4_nombre;
        public string mision_5_nombre;
        public string mision_6_nombre;
        public string mision_7_nombre;
        public string mision_0_descripcion;
        public string mision_1_descripcion;
        public string mision_2_descripcion;
        public string mision_3_descripcion;
        public string mision_4_descripcion;
        public string mision_5_descripcion;
        public string mision_6_descripcion;
        public string mision_7_descripcion;
    }

    [Serializable]
    public class Contrarreloj {
        public string title_activo;
        public string contenido_activo;
        public string title_despedida;
        public string contenido_despedida;
        public string title_perdida;
        public string contenido_perdida;
    }

    [Serializable]
    public class FeedbackPreguntas
    {
        public string title_correcto;
        public string title_incorrecto;
        public string subtitle_correcto;
    }

    // Libro
    [Serializable]
    public class Libro
    {
        public string nombre;
        public string familia;
        public string estacion;
        public string descripcion;
        public string latin;
        public string otros_habitats;
        public string no_datos;
        public string no_descubierto;
    }

    // Arboles
    [Serializable]
    public class Arboles
    {
        public string ceibo;
        public string jacaranda;
        public string bototillo;
        public string pechiche;
        public string laurel;
        public string fernan_sanchez;
        public string guasmo;
        public string guayacan;
    }
}
