using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class WallTrigger : MonoBehaviour
{

    public Estacion station;
    public Animator stationScreen;
    public Text stationText;
    private Inventory mochila;
    public GameObject pendiente;

    //public GameObject actionLogger;

    /*void Start()
    {
        actionLogger = GameObject.Find("ActionLogger");

    }*/

    private void Awake() {
        mochila=GameObject.FindGameObjectWithTag("Bag").GetComponent<Inventory>();
        //estaciones=GameObject.FindGameObjectWithTag("Estaciones").GetComponent<StationsHolder>();
    }

    void OnTriggerEnter(Collider obj)
    {
#if UNITY_ANDROID || UNITY_IOS
        GameObject.Find("Estacion").GetComponent<RectTransform>().localPosition = new Vector3(0f,200f,0f);
#endif
        if (obj.gameObject.tag == "Player")
        {
            Debug.Log("Acabo de pasar por un gameobj con wall trigger en estacion: " + station.ID);

            //Se setean los datos a guardar
            //El numero de la estacion
            Player.instance.EstacionActual(station.ID);
            if(Player.instance.playerData.maxStation<station.ID){
                Player.instance.playerData.maxStation=station.ID;
            }
            Player.instance.playerData.eventosEstaciones[station.ID-1]=station.activos;
            //La  mochila con sus elementos
            mochila.SaveInventory();
            //El libro con las especies desbloqueadas
            BookPages.instance.SaveBook();
            //
            GameManager.instance.SetPlayerData(Player.instance.playerData);

            /*if (Player.instance.playerData.maxStation < station.ID)
            {
                Player.instance.playerData.maxStation = station.ID;
                SaveProfile.instance.SaveGame();
            }*/
            SaveProfile.instance.SaveGame();
            //SaveProfile.instance.LoadGame();


            if (GameManager.instance.currentStation != station.ID)
            {
                GameManager.instance.currentStation = station.ID;
                stationScreen.SetBool("IsActive", true);
                if (MapManager.diccionarioNombre.ContainsKey(station.ID))
                {
                    //actionLogger.GetComponent<ActionLogger>().actionLogger.agregarAccion("Begin Bosque mision", "" + station.ID);
                    stationText.text = MapManager.diccionarioNombre[station.ID];
                    StartCoroutine(LateCall());
                    Debug.Log(MapManager.diccionarioID[station.ID]);
                    GameObject.Find("Audio").GetComponent<SoundManager>().PlayAudio(MapManager.diccionarioID[station.ID]);
                }
                else
                {
                    stationText.text = "Estación "+station.ID;
                    StartCoroutine(LateCall());
                }
            }
            
            // NUEVO: Activar esta estación (destruirá el recordatorio de la estación anterior)
            station.ActivarEstaEstacion();
            
            try{
                pendiente.SetActive(true);

            }catch (System.Exception e)
            {
                Debug.Log("paso algo activando el pendiente");
            }
        }
    }

    public IEnumerator LateCall()
    {
        yield return new WaitForSeconds(2);
        stationScreen.SetBool("IsActive", false);
    }

}